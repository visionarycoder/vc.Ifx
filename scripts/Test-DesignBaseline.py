"""Check a reviewed design lock against a consumer checkout; never issue approval."""
import argparse
import hashlib
import json
from pathlib import Path
import xml.etree.ElementTree as ET
import subprocess

from design_evaluation import evaluate


def validate(root, lock):
    errors = []
    if not isinstance(lock, dict) or type(lock.get('version')) is not int or lock['version'] != 1:
        return ['Unsupported design lock version']
    if (not isinstance(lock.get('files'), dict)
            or not isinstance(lock.get('projects'), dict)
            or not isinstance(lock.get('reuse'), list)):
        return ['Invalid design lock collections']
    for name, digest in lock['files'].items():
        if (not isinstance(name, str) or not isinstance(digest, str)
                or len(digest) != 64 or any(c not in '0123456789abcdef' for c in digest)):
            return ['Invalid input path or SHA-256']
    for name, project in lock['projects'].items():
        if not isinstance(name, str) or not isinstance(project, dict):
            return ['Invalid project record']
        for key in ('references', 'packages'):
            values = project.get(key)
            if (not isinstance(values, list) or any(not isinstance(v, str) or not v for v in values)
                    or len(values) != len(set(values))):
                return ['Invalid project dependency list']
    for item in lock['reuse']:
        if not isinstance(item, dict) or any(not isinstance(item.get(k), str)
                for k in ('capability', 'decision', 'selection', 'evidence')):
            return ['Invalid reuse record']

    def local(name):
        if Path(name).is_absolute() or '\\' in name or '..' in Path(name).parts:
            raise ValueError('Expected repository-relative forward-slash path: ' + name)
        path = (root / name).resolve()
        if not path.is_relative_to(root.resolve()):
            raise ValueError('Path escapes consumer root: ' + name)
        return path

    files = lock['files']
    if not files or not lock['projects'] or not lock['reuse']:
        errors.append('Files, projects and reuse assessment must be nonempty')
    for name, digest in files.items():
        path = local(name)
        if not path.is_file() or hashlib.sha256(path.read_bytes()).hexdigest() != digest:
            errors.append('Input hash mismatch: ' + name)
    projects = lock['projects']
    actual = {p.relative_to(root).as_posix() for p in root.rglob('*.csproj')
              if not {'obj', 'bin', '.git'}.intersection(p.relative_to(root).parts)}
    if actual != set(projects):
        errors.append('Project inventory differs from design lock')
    for name, spec in projects.items():
        path = local(name)
        if not path.is_file():
            errors.append('Missing project: ' + name)
            continue
        tree = ET.parse(path)
        refs = set()
        packages = set()
        for node in tree.iter():
            tag = node.tag.split('}')[-1]
            if tag == 'ProjectReference':
                target = (path.parent / node.attrib['Include'].replace('\\', '/')).resolve()
                if not target.is_relative_to(root.resolve()):
                    errors.append('External project reference: ' + name)
                else:
                    refs.add(target.relative_to(root).as_posix())
            if tag == 'PackageReference':
                packages.add(node.attrib.get('Include', node.attrib.get('Update', '')))
        if refs != set(spec['references']):
            errors.append('Project dependencies differ: ' + name)
        if packages != set(spec['packages']):
            errors.append('Package dependencies differ: ' + name)
        for target in refs:
            if target not in projects:
                errors.append('Unmapped dependency: ' + target)
    for item in lock['reuse']:
        if not item['capability'].strip() or not item['evidence'].strip():
            errors.append('Reuse assessment lacks capability or evidence')
        if item['decision'] not in ('reuse', 'new', 'not-applicable'):
            errors.append('Unknown reuse decision')
        if not item['selection'].strip():
            errors.append('Reuse assessment lacks selected API or gap justification')
    return errors


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--root', type=Path, required=True)
    parser.add_argument('--lock', type=Path, required=True,
                        help='Separately controlled, reviewed design lock')
    parser.add_argument('--evaluate', action='store_true', help='Build explicit profiles and check resolved component references')
    parser.add_argument('--reporter', type=Path, help='Built Ifx.Roslyn.Reporting.dll from the controlled checker revision')
    parser.add_argument('--report', type=Path, help='Write inspectable verification evidence')
    args = parser.parse_args()
    evidence = []
    try:
        lock = json.loads(args.lock.read_text(encoding='utf-8'))
        errors = validate(args.root.resolve(), lock)
        if args.evaluate and not errors:
            if args.reporter is None or not args.reporter.is_file():
                raise ValueError('Evaluated verification requires a built reporting host')
            evaluated, evidence = evaluate(args.root.resolve(), lock, args.reporter.resolve())
            errors.extend(evaluated)
            # A build can run arbitrary targets. Recheck protected bytes after it.
            errors.extend(validate(args.root.resolve(), lock))
    except (OSError, ValueError, KeyError, TypeError, AttributeError, subprocess.TimeoutExpired, ET.ParseError) as error:
        errors = ['Invalid or unresolved design lock: ' + str(error)]
    report = {'structural_result': 'fail' if errors else 'pass', 'evaluated': args.evaluate,
              'errors': errors, 'authorization_verified': False, 'evidence': evidence}
    rendered = json.dumps(report, indent=2)
    if args.report:
        args.report.write_text(rendered, encoding='utf-8')
    print(json.dumps({k: v for k, v in report.items() if k != 'evidence'}, indent=2))
    return bool(errors)


if __name__ == '__main__':
    raise SystemExit(main())
