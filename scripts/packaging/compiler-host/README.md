# Extracted compiler-host package verification

Bounded Gate 2 fixture. The runner does not edit framework source, package metadata
or shared build configuration. The separately authorized 2026-09-10 remediation
removed the unused analyzer dependency from CodeFixes packaging, as recorded below.
Final repository acceptance remains with the orchestrator.

## Run

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -BuildOnly -Project scripts/packaging/CompilerHostSmoke.proj -WarningsAsErrors
```

The wrapper owns the repository mutex; all child builds/packs use one MSBuild node
and disable parallel project builds. Do not invoke the internal script directly.
Each invocation creates fresh `TestResults/compiler-host-packages/<id>` artifacts,
retains failures, and publishes nothing. Templates are copied into that directory;
the host/consumer projects have no repository project references and disable shared
build imports. NuGet's vc.Ifx source mapping and a fresh cache prevent old same-version
packages from masking a packaging regression.

## Contract Checked

- Fresh Release archives for Roslyn, Analyzers, CodeFixes, Generators, and the required
  endpoint marker package. Validate README, nuspec, stable dependency versions,
  portable symbols and commit-pinned Source Link through the existing validator.
- Ordinary Roslyn library under `lib/netstandard2.0`, exposing only its documented
  Common dependency. Compiler extensions under `analyzers/dotnet/cs`, without
  runtime assets, public dependencies or bundled host Roslyn assemblies.
- Bundled shared DLL hashes match the standalone archive. CodeFixes contains only
  its own DLL and the shared Roslyn DLL; its binary references must not include
  Analyzers. The archive validator rejects an analyzer DLL anywhere in CodeFixes.
- An isolated .NET workspace host loads private vc.Ifx DLLs only from the extracted
  CodeFixes directory. Host output contains none of these DLLs. Host-provided Roslyn
  and composition services compose exactly three active CodeFixes exports; all 15
  retired types remain constructible, unexported and unadvertised.
- Four actual packaged debt actions run against diagnostics from the separately
  extracted analyzer package, loaded in its own context. Line/block comments,
  pragmas and suppression attributes compile after
  application and retain unresolved TODO debt. Three analyzer exports and four
  constructible incremental generator exports match their frozen contracts.
- SDK compiler consumes the extracted shared library, analyzer alone, CodeFixes
  alone, generator alone, and combined packages. Generated endpoint registration
  compiles with current ASP.NET reference assemblies. A combined diagnostic probe
  rejects duplicate analyzer execution.
- A genuine NuGet consumer restores all five fresh packages, compiles generated
  endpoints, records resolved analyzer/reference paths and checks diagnostic count.
- A second fresh-cache consumer references only CodeFixes. It must resolve the
  fix assembly without an analyzer package dependency or IFX diagnostics.
- Archive mutation tests run against these same fresh packages, including deliberate
  reintroduction of the duplicated analyzer DLL. `Test-PackageValidation.ps1` accepts
  optional `-PackageDirectory` and `-PackageId`; its existing defaults are retained.

## Green Evidence: 2026-09-10

Passed run: `TestResults/compiler-host-packages/b096e65d29104d539734b3e9247c8502`.
The command above completed with zero warnings/errors and exit code zero.

- Five fresh stable `1.0.0` archives pass metadata, layout, symbols and Source Link.
- 19 archive checks pass: five valid archives and fourteen rejected mutations.
  Mutation report: `validator-tests-bd48aea7b0504b05b8b18883c2d24675/summary.json`.
- `codefix-host.json`: 48 checks pass, with the unchanged three MEF exports,
  all 15 retired providers still unexported, no binary analyzer reference, and
  exactly two private CodeFixes-context assemblies. Separate analyzer loading,
  applied debt actions and four generator exports are verified.
- All six SDK compiler probes pass. `combined-debt/compiler.sarif` contains
  exactly one IFX1000 result; `codefix-only/compiler.sarif` has no diagnostics.
- Actual fresh-cache NuGet `nuget-consumer/compiler.sarif` likewise has exactly
  one IFX1000 result, and generated endpoint registration compiles.
- `nuget-codefix-only` builds with the fix assembly discovered, no restored
  Analyzers package and no IFX diagnostics. The separate package is now an explicit
  consumer choice, not an implicit diagnostic emitter inside CodeFixes.
- `manifest.json` contains all hashes, paths, identities, compiler results and
  both NuGet-consumer results. No shared metadata changed during the run.
- No executable provider source changed. The earlier accepted 96-test strict
  CodeFixes result remains historical evidence; framework coverage was not rerun
  or remeasured by this packaging remediation.

## Red Evidence: 2026-09-10

Prior failing run: `TestResults/compiler-host-packages/dbcd31f85ca74b71879bc1e252cc526b`.
This remains the retained duplication reproduction, not current green evidence.

- All five stable `1.0.0` archives pass layout/symbol/Source Link validation.
  Roslyn exposes Common `5.9.0`; the three compiler extensions expose no public
  dependencies. Generator has 24 private DLLs, including its Routing parser closure.
- SDK `10.0.401`, compiler `5.9.0-1.26423.113`, workspace/Common assembly version
  `5.9.0.0`, runtime `.NET 10.0.12`. These are measured hosts, not a claim about all IDEs.
- `codefix-host.json`: 45 checks pass, including exact MEF exports, retirement,
  private loading, applied actions, analyzer exports and generator discovery.
- Shared-library, analyzer-only, CodeFixes-only and generator/combined endpoint
  consumer compilations succeed. Intended IFX1000 diagnostic is informational;
  warnings-as-errors does not promote its default severity. No warnings are suppressed.
- `combined-debt/compiler.sarif`: **two IFX1000 results for one source location**.
- `nuget-consumer/compiler.sarif`: the same duplication through actual fresh-cache
  NuGet restore. Its build completes with zero warnings/errors, but the verification
  script deliberately exits nonzero because diagnostic multiplicity is wrong.
- `manifest.json` records package/DLL SHA256 hashes, exact paths, dependency ranges,
  compiler outcomes, source mapping context and unchanged before/after shared metadata
  hashes. `verification.log`, response files, generated registries, resolved NuGet
  analyzer/reference paths and the projects used are retained.
- `codefix-assembly-references.json` records direct PE metadata inspection: CodeFixes
  references Roslyn/Common/CSharp/Workspaces, composition, immutable collections and
  netstandard; **it does not reference vc.Ifx.Analyzers**.

## Applied Remediation

Both packages previously contributed their own analyzer DLL path.
Roslyn ran both even though their bytes matched. This was proven in raw compiler and
actual NuGet consumers, not inferred from archive layout alone.

Orchestrator explicitly authorized removal of CodeFixes' unused Analyzers project
reference and bundled DLL. PE references were reconfirmed before editing. The
production package now depends only on the private Roslyn helper; the centralized
unit-test project already references Analyzers directly for its debt harness.
Existing CodeFixes-only users must add the separate Analyzers package for IFX
diagnostics. The package README records this compatibility change; provider APIs,
actions and diagnostic behavior are unchanged.

Validator/test expectations and these consumers now enforce the separate-install
contract. No global Directory props/packages/targets, Validate-Packages wrapper,
publishing files, provider executable source, or Nash-owned catalog/support-matrix/
generator-contract documents were edited by this remediation. Required documentation
details were relayed through Orchestrator. IFX1000 remains unsuppressed.

Earlier retained fixture runs: `7dea21799e4e4768bba90bad552cbfd1` stopped on a
PowerShell fixture variable name; `e12eb7b73d1c440790948628329af17e` corrected the
fixture's mistaken expectation that informational IFX1000 fails compilation;
`7c20d6ef850b4f74a21dddb99a13dcc9` passed the narrower checks before the combined-debt
probe was added; `e7d4b87b96884b89b61b3c7d7b40690d` first exposed the duplication.
These precede the red reproduction and subsequent green remediation above.

## Remaining Host Limits

This is real SDK compiler execution, .NET workspace execution and MEF composition,
not a Visual Studio/VS Code installation test. IDE menu discovery, light-bulb UI,
remote process loading, live Fix All UX, legacy .NET Framework hosts and older Roslyn
versions remain unverified. Host binaries must satisfy the packaged assembly
references; netstandard2.0 alone is not proof of older IDE compatibility.
The fixture does not rerun framework coverage, all 28 packages or hosted CI, and it
does not replace the already-accepted endpoint/WebApi runtime integration tests.
