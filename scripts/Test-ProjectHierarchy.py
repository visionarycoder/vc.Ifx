"""Validate the consolidated project inventory and upward dependency rule."""
from pathlib import Path
import xml.etree.ElementTree as ET
import re

root = Path(__file__).resolve().parents[1]
projects = {p.resolve(): ET.parse(p).getroot() for area in ('src', 'tests', 'performance', 'scripts')
            for p in (root / area).rglob('*.csproj') if not {'obj', 'bin'}.intersection(p.relative_to(root).parts)}
source = {p.stem: p for p in projects if p.is_relative_to(root / 'src')}
listed = {(root / n.attrib['Path']).resolve() for n in ET.parse(root / 'vc.Ifx.slnx').findall('.//Project')}
errors = []
if listed != set(projects):
    errors.append(f'Solution inventory mismatch: missing={set(projects) - listed}, stale={listed - set(projects)}')
graph = {}
for path, tree in projects.items():
    references = {(path.parent / n.attrib['Include'].replace('\\', '/')).resolve() for n in tree.findall('.//ProjectReference')}
    graph[path] = references
    for target in references:
        if target not in projects:
            errors.append(f'Missing project: {path.stem} -> {target}')
        if path.stem in source and target.stem.startswith(path.stem + '.') and 'Abstractions' not in target.stem.split('.'):
            errors.append(f'Parent references child: {path.stem} -> {target.stem}')
    if path.stem in source and path.stem != 'Ifx' and 'Abstractions' not in path.stem.split('.'):
        ancestors = [p for name, p in source.items() if path.stem.startswith(name + '.')]
        if ancestors:
            parent = max(ancestors, key=lambda p: len(p.stem))
            framework = tree.findtext('.//TargetFramework') or 'net10.0'
            parent_framework = projects[parent].findtext('.//TargetFramework') or 'net10.0'
            # A netstandard compiler host cannot reference the net10 runtime root.
            if not (framework == 'netstandard2.0' and parent_framework == 'net10.0') and parent not in references:
                errors.append(f'Missing parent reference: {path.stem} -> {parent.stem}')

def visit(path, active, done):
    if path in active:
        errors.append(f'Project cycle through {path.stem}')
        return
    if path in done:
        return
    for target in graph.get(path, ()):
        visit(target, active | {path}, done)
    done.add(path)

done = set()
for path in projects:
    visit(path, set(), done)
if errors:
    raise SystemExit('\n'.join(errors))
expected = {'Ifx', 'Ifx.Integrations', 'Ifx.Integrations.Azure', 'Ifx.Integrations.Windows',
            'Ifx.Roslyn', 'Ifx.Roslyn.Analyzers', 'Ifx.Roslyn.Generators', 'Ifx.Roslyn.CodeFixes'}
libraries = {name for name, path in source.items() if projects[path].findtext('.//IsPackable') != 'false'}
if libraries != expected:
    raise SystemExit(f'Production library set differs from the adopted layout: {libraries ^ expected}')
if any(path.parent.parent != root / 'src' for path in source.values()):
    raise SystemExit('Source projects must reside directly under src, without an extra grouping directory.')
benchmarks = {path.stem: path for path in projects if path.is_relative_to(root / 'performance')}
expected_benchmarks = {'Ifx.Benchmarks': 'Ifx', 'Ifx.Integrations.Benchmarks': 'Ifx.Integrations'}
if set(benchmarks) != set(expected_benchmarks):
    raise SystemExit('Benchmark project names must follow the measured production projects.')
for name, library in expected_benchmarks.items():
    if graph[benchmarks[name]] != {source[library]}:
        raise SystemExit(f'{name} must reference only its production counterpart {library}.')
reporting = source.get('Ifx.Roslyn.Reporting')
if reporting is None or projects[reporting].findtext('.//OutputType') != 'Exe' or projects[reporting].findtext('.//IsPackable') != 'false':
    raise SystemExit('Ifx.Roslyn.Reporting must remain a non-packable executable under src/ifx.')
for name in ['Ifx', 'Ifx.Integrations']:
    for item in projects[source[name]].findall('.//PackageReference'):
        package = item.attrib['Include']
        if package.startswith('Azure.') or 'AzureAppConfiguration' in package or package == 'Experimental.System.Messaging':
            raise SystemExit(f'Platform dependency escaped its project: {name} -> {package}')
for name in ['Ifx.Roslyn', 'Ifx.Roslyn.Analyzers', 'Ifx.Roslyn.Generators']:
    for item in projects[source[name]].findall('.//PackageReference'):
        if 'Workspaces' in item.attrib['Include']:
            raise SystemExit(f'Workspace dependency in compiler-safe project: {name}')
for path in (root / 'src').rglob('*'):
    if {'obj', 'bin'}.intersection(path.relative_to(root).parts):
        continue
    if path.is_dir() and 'Abstractions' in path.name:
        raise SystemExit(f'Abstractions folder must be merged into its parent: {path}')
    if path.suffix == '.cs':
        namespaces = re.findall(r'^\s*namespace\s+([\w.]+)', path.read_text(encoding='utf-8-sig'), re.M)
        if any('Abstractions' in namespace.split('.') for namespace in namespaces):
            raise SystemExit(f'Abstractions namespace must be merged into its parent: {path}')
print(f'PASS: {len(libraries)} source libraries, one reporting executable, {len(projects)} solution projects; upward dependencies, existing references, and no cycles.')
