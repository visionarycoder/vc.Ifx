"""Evaluate assigned builds and inspect resolved calls using the existing reporting host."""
import hashlib
import json
from pathlib import Path
import subprocess
import tempfile


def run_json(command, root):
    result = subprocess.run(command, cwd=root, text=True, capture_output=True, timeout=300)
    if result.returncode:
        raise ValueError('Build or semantic verification failed: ' + result.stdout[-4000:] + result.stderr[-4000:])
    return json.loads(result.stdout)


def evaluate(root, lock, reporter):
    builds = lock.get('builds')
    policy = lock.get('call_policy')
    if not isinstance(builds, list) or not builds or not isinstance(policy, dict):
        raise ValueError('Evaluated verification requires builds and call_policy')
    assignments = policy['assemblies']
    allowed = policy['allowed_contracts']
    if not isinstance(assignments, dict) or not assignments:
        raise ValueError('Missing assembly/component assignments')
    errors, evidence = [], []
    protected_names = {'global.json', 'nuget.config', 'packages.lock.json', '.editorconfig',
                       'globalanalyzerconfig.globalconfig'}
    for path in root.rglob('*'):
        relative = path.relative_to(root)
        if not path.is_file() or {'obj', 'bin', '.git'}.intersection(relative.parts):
            continue
        if (path.suffix.lower() in ('.csproj', '.props', '.targets', '.globalconfig')
                or path.name.lower() in protected_names):
            if relative.as_posix() not in lock['files']:
                errors.append('Build input is not hash-bound: ' + relative.as_posix())
    if errors:
        return errors, evidence
    for build in builds:
        if set(build['projects']) != set(lock['projects']):
            raise ValueError('Every build profile must map the full project inventory')
        properties = build['properties']
        if not properties or any(not isinstance(k, str) or not isinstance(v, str)
                                 or any(c in k + v for c in ';\r\n') for k, v in properties.items()):
            raise ValueError('Build properties must be explicit single string values')
        seen_assemblies = set()
        for name, expected in build['projects'].items():
            required_properties = {'AssemblyName', 'TargetFramework', 'DefineConstants', 'LangVersion',
                                   'OutputType', 'AllowUnsafeBlocks', 'Nullable', 'CheckForOverflowUnderflow'}
            if not required_properties.issubset(expected['properties']):
                raise ValueError('Build profile lacks expected compiler properties: ' + name)
            command = ['dotnet', 'msbuild', str(root / name), '-nologo', '-m:1', '-verbosity:quiet',
                       '-restore', '-target:Rebuild',
                       '-getItem:ProjectReference,PackageReference,Compile,ReferencePath,Analyzer',
                       '-getProperty:AssemblyName,TargetFramework,DefineConstants,LangVersion,OutputType,AllowUnsafeBlocks,Nullable,CheckForOverflowUnderflow,ProjectAssetsFile,MSBuildAllProjects,EmitCompilerGeneratedFiles,CompilerGeneratedFilesOutputPath']
            command.extend('-property:' + k + '=' + v for k, v in properties.items())
            actual = run_json(command, root)
            items, effective = actual['Items'], actual['Properties']
            refs = set()
            for entry in items['ProjectReference']:
                target = Path(entry['FullPath']).resolve()
                if not target.is_relative_to(root):
                    raise ValueError('Evaluated project reference leaves consumer root')
                refs.add(target.relative_to(root).as_posix())
            if refs != set(expected['references']):
                errors.append('Evaluated project dependencies differ: ' + name)
            if {p['Identity'] for p in items['PackageReference']} != set(expected['packages']):
                errors.append('Evaluated package dependencies differ: ' + name)
            for key, value in expected['properties'].items():
                if effective.get(key) != value:
                    errors.append('Effective property differs: ' + name + ':' + key)
            assets = json.loads(Path(effective['ProjectAssetsFile']).read_text(encoding='utf-8'))
            versions = {identity.rsplit('/', 1)[0]: identity.rsplit('/', 1)[1]
                        for identity, info in assets['libraries'].items() if info['type'] == 'package'}
            if versions != expected['resolved_packages']:
                errors.append('Resolved package versions differ: ' + name)
            assembly = effective['AssemblyName']
            if assembly in seen_assemblies:
                raise ValueError('Consumer projects have duplicate assembly identities: ' + assembly)
            seen_assemblies.add(assembly)
            if assembly not in assignments:
                raise ValueError('Unmapped evaluated assembly: ' + assembly)
            sources = sorted({p['FullPath'] for p in items['Compile']})
            if items['Analyzer']:
                if effective['EmitCompilerGeneratedFiles'].lower() != 'true':
                    raise ValueError('Baseline must emit generator output for semantic verification: ' + name)
                generated = Path(effective['CompilerGeneratedFilesOutputPath'])
                if not generated.is_absolute():
                    generated = (root / name).parent / generated
                generated = generated.resolve()
                if not generated.is_relative_to(root) or generated == root:
                    raise ValueError('Generated-source output must be inside the consumer checkout')
                sources = sorted(set(sources) | {str(p) for p in generated.rglob('*.cs')})
            references = sorted({p['FullPath'] for p in items['ReferencePath']})
            reference_hashes = {Path(p).name: hashlib.sha256(Path(p).read_bytes()).hexdigest()
                                for p in references}
            if len(reference_hashes) != len(references):
                errors.append('Ambiguous resolved assembly names: ' + name)
            if reference_hashes != expected['resolved_references']:
                errors.append('Resolved assembly bytes differ: ' + name)
            if not sources or not references:
                raise ValueError('Missing compilation inputs: ' + name)
            if any(not Path(p).resolve().is_relative_to(root) for p in sources):
                raise ValueError('Compilation source leaves consumer root: ' + name)
            request = {'assembly': assembly, 'component': assignments[assembly],
                       'assemblies': assignments, 'allowed_contracts': allowed[assignments[assembly]],
                       'allowed_data_types': policy.get('allowed_data_types', {}).get(assignments[assembly], []),
                       'symbols': [s for s in effective['DefineConstants'].split(';') if s],
                       'language': effective['LangVersion'], 'sources': sources, 'references': references}
            request['output_type'] = effective['OutputType']
            request['allow_unsafe'] = effective['AllowUnsafeBlocks'].lower() == 'true'
            request['check_overflow'] = effective['CheckForOverflowUnderflow'].lower() == 'true'
            with tempfile.TemporaryDirectory(prefix='ifx-design-') as temporary:
                request_path = Path(temporary) / 'request.json'
                request_path.write_text(json.dumps(request), encoding='utf-8')
                result = subprocess.run(['dotnet', str(reporter), '--design', str(request_path)],
                                        cwd=root, text=True, capture_output=True, timeout=300)
                if result.returncode not in (0, 1):
                    raise ValueError('Semantic checker execution failed: ' + result.stderr[-4000:])
                report = json.loads(result.stdout)
                errors.extend(report['issues'])
                if not report['passed'] and not report['issues']:
                    errors.append('Semantic checker returned failure without evidence')
            evidence.append({'project': name, 'profile': properties, 'properties': effective,
                             'resolved_packages': versions,
                             'inputs': {p: hashlib.sha256(Path(p).read_bytes()).hexdigest()
                                        for p in sources + references + [a['FullPath'] for a in items['Analyzer']]}})
    return errors, evidence
