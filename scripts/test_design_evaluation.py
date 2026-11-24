"""Opt-in end-to-end fixture; no fixture record grants operational approval."""
import copy
import hashlib
import json
import os
from pathlib import Path
import tempfile
import unittest
import subprocess
import sys
from unittest.mock import patch

from design_evaluation import evaluate, run_json


@unittest.skipUnless(os.environ.get('IFX_DESIGN_REPORTER'), 'Set IFX_DESIGN_REPORTER to a built reporting host')
class EvaluatedDesignTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.temporary = tempfile.TemporaryDirectory(prefix='ifx-consumer-fixture-')
        cls.root = Path(cls.temporary.name).resolve()
        cls.reporter = Path(os.environ['IFX_DESIGN_REPORTER']).resolve()
        cls.repo = Path(__file__).resolve().parents[1]
        files = {
            'global.json': (cls.repo / 'global.json').read_text(encoding='utf-8'),
            'NuGet.Config': '<configuration><packageSources><clear/><add key="local" value="'
                + str(cls.repo / '.nupkgs') + '"/><add key="nuget" value="https://api.nuget.org/v3/index.json"/></packageSources></configuration>',
            'Directory.Build.props': '<Project><PropertyGroup><TargetFramework>net10.0</TargetFramework>'
                '<LangVersion>14.0</LangVersion><ImplicitUsings>enable</ImplicitUsings>'
                '<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles></PropertyGroup></Project>',
            'Directory.Build.targets': '<Project><ItemGroup Condition="\'$(MSBuildProjectName)\' == \'Manager\' and \'$(IFX_FIXTURE_EXTRA)\' == \'true\'">'
                '<PackageReference Include="Ifx" Version="0.1.0"/></ItemGroup></Project>',
            'Engine/Engine.csproj': '<Project Sdk="Microsoft.NET.Sdk"><ItemGroup>'
                '<PackageReference Include="Ifx" Version="0.1.0"/></ItemGroup></Project>',
            'Engine/IPrice.cs': 'namespace Pricing; public interface IPrice { Task<int> GetAsync(CancellationToken token); }',
            'Engine/Price.cs': 'namespace Pricing; public sealed class Price : IPrice {'
                ' public Task<int> GetAsync(CancellationToken token) => Task.FromResult(1); }',
            'Engine/Reuse.cs': 'namespace Pricing; public static class Reuse {'
                ' public static Ifx.Proxy.ProxyContext Context() => new(); }',
            'Manager/Manager.csproj': '<Project Sdk="Microsoft.NET.Sdk"><ItemGroup>'
                '<ProjectReference Include="../Engine/Engine.csproj"/></ItemGroup></Project>',
            'Manager/Order.cs': 'public class Order(Pricing.IPrice price) {'
                ' public Task<int> Run(CancellationToken token) => price.GetAsync(token); }',
        }
        for name, content in files.items():
            path = cls.root / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text(content, encoding='utf-8')
        cls.lock = {'version': 1, 'files': {}, 'projects': {},
                    'reuse': [{'capability': 'invocation context', 'decision': 'reuse',
                               'selection': 'Ifx 0.1.0 / Ifx.Proxy.ProxyContext',
                               'evidence': 'fictional fixture compiles against the actual local package'}],
                    'call_policy': {'assemblies': {'Engine': 'pricing', 'Manager': 'orders'},
                                    'allowed_contracts': {'pricing': [], 'orders': ['Pricing.IPrice']}},
                    'builds': [{'properties': {'Configuration': 'Release'}, 'projects': {}}]}
        cls.lock['files'] = {name: hashlib.sha256((cls.root / name).read_bytes()).hexdigest()
                             for name in files if not name.endswith('.cs') or name.endswith('IPrice.cs')}
        # Calibrate ONLY this known fictional seed, before any mutation. This is not
        # an operational baseline-generation command and authenticates no design.
        for name in ('Engine/Engine.csproj', 'Manager/Manager.csproj'):
            actual = run_json(['dotnet', 'msbuild', str(cls.root / name), '-nologo', '-verbosity:quiet',
                               '-restore', '-target:Rebuild', '-property:Configuration=Release',
                               '-getItem:ProjectReference,PackageReference,ReferencePath',
                               '-getProperty:AssemblyName,TargetFramework,DefineConstants,LangVersion,OutputType,AllowUnsafeBlocks,Nullable,CheckForOverflowUnderflow,ProjectAssetsFile'], cls.root)
            references = [Path(p['FullPath']).relative_to(cls.root).as_posix() for p in actual['Items']['ProjectReference']]
            packages = [p['Identity'] for p in actual['Items']['PackageReference']]
            cls.lock['projects'][name] = {'references': references, 'packages': packages}
            assets = json.loads(Path(actual['Properties']['ProjectAssetsFile']).read_text(encoding='utf-8'))
            cls.lock['builds'][0]['projects'][name] = {
                'references': references, 'packages': packages,
                'properties': {key: actual['Properties'][key] for key in ('AssemblyName', 'TargetFramework', 'DefineConstants', 'LangVersion', 'OutputType', 'AllowUnsafeBlocks', 'Nullable', 'CheckForOverflowUnderflow')},
                'resolved_packages': {identity.rsplit('/', 1)[0]: identity.rsplit('/', 1)[1]
                                      for identity, item in assets['libraries'].items() if item['type'] == 'package'},
                'resolved_references': {Path(p['FullPath']).name: hashlib.sha256(Path(p['FullPath']).read_bytes()).hexdigest()
                                        for p in actual['Items']['ReferencePath']}}

    @classmethod
    def tearDownClass(cls):
        cls.temporary.cleanup()

    def test_valid_consumer_reuses_actual_ifx(self):
        errors, evidence = evaluate(self.root, self.lock, self.reporter)
        self.assertEqual([], errors)
        self.assertEqual(2, len(evidence))
        self.assertEqual('0.1.0', evidence[0]['resolved_packages']['Ifx'])

    def test_imported_conditional_package_is_rejected(self):
        with patch.dict(os.environ, {'IFX_FIXTURE_EXTRA': 'true'}):
            errors, _ = evaluate(self.root, self.lock, self.reporter)
        self.assertIn('Evaluated package dependencies differ: Manager/Manager.csproj', errors)

    def test_concrete_call_is_rejected(self):
        path = self.root / 'Manager/Order.cs'
        original = path.read_bytes()
        try:
            path.write_text('public class Order { public Task<int> Run(CancellationToken token) => new Pricing.Price().GetAsync(token); }')
            errors, _ = evaluate(self.root, self.lock, self.reporter)
            self.assertTrue(any('Forbidden component reference' in error for error in errors))
        finally:
            path.write_bytes(original)

    def test_missing_profile_project_is_rejected(self):
        lock = copy.deepcopy(self.lock)
        del lock['builds'][0]['projects']['Manager/Manager.csproj']
        with self.assertRaisesRegex(ValueError, 'full project inventory'):
            evaluate(self.root, lock, self.reporter)

    def test_command_emits_hashed_evidence_without_authority(self):
        lock = self.root / 'fixture-lock.json'
        report = self.root / 'fixture-evidence.json'
        lock.write_text(json.dumps(self.lock), encoding='utf-8')
        result = subprocess.run([sys.executable, str(self.repo / 'scripts/Test-DesignBaseline.py'),
                                 '--root', str(self.root), '--lock', str(lock), '--evaluate',
                                 '--reporter', str(self.reporter), '--report', str(report)],
                                text=True, capture_output=True, timeout=300)
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)
        evidence = json.loads(report.read_text(encoding='utf-8'))
        self.assertFalse(evidence['authorization_verified'])
        self.assertTrue(evidence['evaluated'])
        self.assertEqual(2, len(evidence['evidence']))
        self.assertTrue(all(len(digest) == 64 for project in evidence['evidence']
                            for digest in project['inputs'].values()))


if __name__ == '__main__':
    unittest.main()
