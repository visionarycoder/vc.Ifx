# Package Artifact Regressions

Run these isolated checks without building, restoring, testing or packing real
framework projects:

```powershell
pwsh -NoProfile -File tests/infrastructure/packaging/Test-PackageArtifactManifest.ps1
pwsh -NoProfile -File tests/infrastructure/packaging/Test-ValidatedPackageArtifacts.ps1
```

The wrapper suite uses a temporary repository beneath `TestResults`, with its own
repository-path mutex. Native Git/MSBuild/pack and the separately tested package
layout validator are stubbed. The wrapper, build provenance assertions, test
selection module and archive manifest module execute unchanged. This is not
evidence of a real framework build, coverage run or package validation.

The wrapper requires `--no-build --no-restore` and
`-p:BuildProjectReferences=false`. Compiler-extension package targets resolve
references to collect private assets; without the latter property the SDK can
invoke a referenced project's Build target and reject the operation with NETSDK1085.
The real first all-package attempt reproduced this failure at
`TestResults/package-artifacts/b9e8e1bc02b0445388f82933ca6bd4c3` before the
argument correction. The wrapper regression also fails if that flag is removed.

## Payload Identity

Each package's payload map retains paths relative to its captured `TargetDir`.
For example, `cs/Microsoft.CodeAnalysis.resources.dll` and
`de/Microsoft.CodeAnalysis.resources.dll` are distinct entries even though their
basenames match. Unbundled outputs do not require a matching archive entry.

Every DLL/PDB actually present in either archive must match the exact relative
path and SHA256 from that package's tested outputs. Only the known NuGet roots
`lib/<framework>/` and `analyzers/dotnet/cs/` are stripped. Culture directories
remain significant; there is no basename fallback or acceptance of any hash from
another path. Unsupported roots, path traversal, empty segments, backslashes,
case aliases and unattested payloads fail closed. The existing layout validator
continues to enforce target frameworks and prohibit bundled host Roslyn assets.

The wrapper fixtures include 24 private dependency DLLs, private PDBs and different
hashes for both Roslyn satellite basenames across `cs`, `de` and `ja`. They prove
that unbundled collisions do not block packaging, exact packaged satellites are
still checked, cross-culture substitution or flattening fails, and changed
private DLL/PDB bytes cannot pass. The packaged host-satellite fixture exercises
the manifest layer only; the stubbed layout validator does not authorize that
layout for a real compiler package.

GitHub's published artifact manifest format and inline download verifier are
unchanged by this path-mapping correction. The orchestrator owns the final real
28-package verification against the captured full build and coverage evidence.
