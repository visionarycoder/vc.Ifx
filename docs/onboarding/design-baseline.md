---
title: Consumer design baseline checks
doc_type: guide
status: active
last_updated: 2026-11-09
---

# Consumer design baseline checks

Use existing Ifx packages and compiler tooling before creating infrastructure.
The Application Architect records a reuse assessment for each assigned capability:
existing package/API and version, examined source or consumer evidence, selected
mechanism, and any gap that requires new implementation. The Solution Architect
reviews that selection with the detailed design. A similar name alone does not
establish suitability. New reusable mechanisms belong in Ifx.

The portable checker reads a separately controlled JSON lock. It never creates
its own expected baseline from the submitted code. The design owner prepares the
expected project graph from the accepted detailed design; independent integration
retrieves that lock from its protected baseline, outside worker write authority.
Do not accept a lock path chosen by the submitting worker as trusted evidence.

```json
{
  "version": 1,
  "files": {"governance/system-design.json": "replace-with-exact-sha256"},
  "projects": {
    "src/Example.csproj": {"references": [], "packages": ["Ifx"]}
  },
  "reuse": [{
    "capability": "component proxy",
    "decision": "reuse",
    "selection": "reviewed Ifx.Proxy API and approved package version",
    "evidence": "exact API review and consumer verification references"
  }]
}
```

`files` contains exact SHA-256 hashes of the requirements, system design, detailed
design, assignment, selected SDP, decision/review records and technical inputs
used for this action. Include package locks, central versions, SDK selection,
build imports, analyzer settings and suppressions. Paths are relative to the
consumer checkout; independently verified external records can be staged there
as exact bytes. The lock is a structural projection, not a replacement governance
artifact. Its authority comes from the existing review and decision routes.

```text
python scripts/Test-DesignBaseline.py --root <consumer-checkout> --lock <protected-lock.json>
python -m unittest discover -s scripts -p test_design_baseline.py
```

The checker rejects changed/missing inputs, project inventory drift, declared
project/package dependency changes, root escapes and incomplete reuse entries.
Malformed records fail closed. All projects must be mapped, including tests.
It reports structural results and always reports authorization as unverified.

## Evaluated builds and component references

For stronger portable checks, add `builds` and `call_policy` to the protected lock.
Each build profile specifies explicit MSBuild properties and the full project
inventory. Multi-target projects need a profile for each approved target framework.
Run from the controlled checker checkout:

```text
python scripts/Test-DesignBaseline.py --root <consumer-checkout> --lock <protected-lock.json> --evaluate --reporter <built-Ifx.Roslyn.Reporting.dll> --report <evidence.json>
```

Each profile has this shape. Replace the illustration with architect-reviewed
expected values; do not derive the operational expected graph from a submission.

```json
{
  "properties": {"Configuration": "Release", "TargetFramework": "net10.0"},
  "projects": {
    "src/Example.csproj": {
      "references": [],
      "packages": ["Ifx"],
      "properties": {
        "AssemblyName": "Example",
        "TargetFramework": "net10.0",
        "DefineConstants": "exact evaluated approved symbols",
        "LangVersion": "14.0",
        "OutputType": "Library",
        "AllowUnsafeBlocks": "false",
        "Nullable": "enable",
        "CheckForOverflowUnderflow": "false"
      },
      "resolved_packages": {"Ifx": "0.1.0"},
      "resolved_references": {"Ifx.dll": "exact-approved-sha256"}
    }
  }
}
```

The dictionaries above are abbreviated. List all resolved packages, including
transitive packages, and all resolved assembly file names and exact byte hashes,
including framework reference assemblies. Binding assembly bytes also binds the
contract metadata consumed by compilation. Hash approved contract source files
in `files` to reject changes to their reviewed surface. Include every project,
props/targets file, package lock, NuGet configuration, SDK selection and analyzer
configuration in `files`. Evaluated mode rejects unbound build files in the checkout.
The approved baseline must enable `EmitCompilerGeneratedFiles` so generator outputs
can be included; workers must not change settings to make their own work pass.

`call_policy` maps each consumer assembly to one approved component and lists the
contracts its callers may reference. Keep separate assemblies for components
when using this checker; it cannot distinguish multiple components in one assembly.
This limitation does not mandate a new layout for an already accepted system.

```json
{
  "assemblies": {"Orders": "orders", "Pricing.Contracts": "pricing"},
  "allowed_contracts": {"orders": ["Pricing.IPrice"], "pricing": []},
  "allowed_data_types": {"orders": ["Pricing.PriceRequest"], "pricing": []}
}
```

The reviewed data-type list permits exact DTO/enum references separately from
service interfaces. The architect must verify that these are data types; a type
name does not establish that classification. Expected contract file bytes and
resolved reference hashes preserve the reviewed operations and signatures.

Evaluated mode restores and rebuilds every project/profile, then queries actual
MSBuild items. It checks imported and conditional project/package dependencies,
effective compiler properties, resolved package versions and assembly hashes.
The existing Reporting executable reconstructs Roslyn compilations from resolved
sources, references and symbols, including emitted generator output. It rejects
unresolved compilations, cross-component concrete types, unapproved interfaces
and dynamic dispatch. It examines symbol references as well as invocations, so
aliases, method groups, constructors and property accesses remain visible.
The report records hashes of compilation inputs and compiler extensions.
Protected input bytes are checked again after build execution.

MSBuild executes project code. Run it only in an appropriately authorized worker;
portable checks do not isolate that code. The selected platform must protect the
checker, expected lock, reporting executable and evidence from worker amendment.

## Verification and limits

The fictional two-component fixture reuses the actual local Ifx 0.1.0 package.
It checks a valid consumer, an imported conditional dependency, a direct concrete
call and an incomplete build profile. It calibrates only its known test seed before
mutations. That test setup does not produce an operational approval or design lock.
The C# tests additionally run the existing Ifx interceptor generator and verify
its actual generated source against both allowed and forbidden contract policies.

```powershell
$env:IFX_DESIGN_REPORTER = (Resolve-Path src/Ifx.Roslyn.Reporting/bin/Release/net10.0/Ifx.Roslyn.Reporting.dll).Path
python -m unittest discover -s scripts -p test_design_evaluation.py
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -Configuration Release -WarningsAsErrors -Filter FullyQualifiedName~DesignConformanceTests
```

These checks do not prove runtime DI resolves an approved proxy, that reflection
or external binaries cannot bypass a chain, or that a permitted interface call
uses the approved transport. Run contract and proxy-chain composition tests in
the actual consuming project. Existing Ifx tests verify reusable mechanisms, not
an arbitrary application's registrations. Semantic design review still establishes
volatility, behavior and cohesion. Identity, approval authenticity, freshness,
revocation and protected integration remain separate evidence. This is portable
structural verification, not a complete architecture or authorization gate.
