"""Mutation checks for consumer design locks."""
import copy
import hashlib
import importlib.util
from pathlib import Path
import tempfile
import unittest

spec = importlib.util.spec_from_file_location('baseline', Path(__file__).with_name('Test-DesignBaseline.py'))
baseline = importlib.util.module_from_spec(spec)
spec.loader.exec_module(baseline)


class DesignBaselineTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        (self.root / 'design.json').write_bytes(b'accepted design fixture')
        (self.root / 'A.csproj').write_text('<Project><ItemGroup><PackageReference Include="Ifx" /></ItemGroup></Project>')
        self.lock = {'version': 1, 'files': {'design.json': hashlib.sha256(b'accepted design fixture').hexdigest()},
                     'projects': {'A.csproj': {'references': [], 'packages': ['Ifx']}},
                     'reuse': [{'capability': 'proxy', 'evidence': 'fixture API review',
                                'decision': 'reuse', 'selection': 'Ifx.Proxy'}]}

    def test_valid_structure(self):
        self.assertEqual([], baseline.validate(self.root, self.lock))

    def test_changed_input(self):
        (self.root / 'design.json').write_text('changed')
        self.assertIn('Input hash mismatch: design.json', baseline.validate(self.root, self.lock))

    def test_added_project(self):
        (self.root / 'B.csproj').write_text('<Project/>')
        self.assertTrue(baseline.validate(self.root, self.lock))

    def test_dependency_bypass(self):
        (self.root / 'A.csproj').write_text('<Project><ItemGroup><ProjectReference Include="B.csproj"/></ItemGroup></Project>')
        self.assertIn('Project dependencies differ: A.csproj', baseline.validate(self.root, self.lock))

    def test_package_substitution(self):
        self.lock['projects']['A.csproj']['packages'] = ['Other']
        self.assertIn('Package dependencies differ: A.csproj', baseline.validate(self.root, self.lock))

    def test_reuse_requires_justification(self):
        self.lock['reuse'][0]['selection'] = ''
        self.assertTrue(baseline.validate(self.root, self.lock))

    def test_path_escape(self):
        self.lock['files'] = {'../outside': '0' * 64}
        with self.assertRaises(ValueError):
            baseline.validate(self.root, self.lock)

    def test_unknown_version(self):
        lock = copy.deepcopy(self.lock)
        lock['version'] = 2
        self.assertTrue(baseline.validate(self.root, lock))

    def test_malformed_records_fail(self):
        for lock in (None, [], {'version': True}, {'version': 1, 'files': []},
                     dict(self.lock, reuse=[{}]),
                     dict(self.lock, projects={'A.csproj': {'references': 'B', 'packages': []}})):
            with self.subTest(lock=lock):
                self.assertTrue(baseline.validate(self.root, lock))

    def test_missing_input(self):
        (self.root / 'design.json').unlink()
        self.assertIn('Input hash mismatch: design.json', baseline.validate(self.root, self.lock))

    def test_absolute_path_rejected(self):
        self.lock['files'] = {str(self.root.parent / 'outside'): '0' * 64}
        with self.assertRaises(ValueError):
            baseline.validate(self.root, self.lock)


if __name__ == '__main__':
    unittest.main()
