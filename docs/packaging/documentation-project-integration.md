# Documentation project integration

Local implementation and CLI verification completed 2026-09-10. Actual Visual
Studio loading/refresh remains unverified; this is not a claim of IDE acceptance.

## Implemented

`vc.Ifx.slnx` now contains one real `Project Path="docs/docs.csproj"` entry under
`/docs/`. The 31 manual documentation File entries and redundant solution folders
were removed. Documentation contents were not removed. Recursive None/Link items
provide relative paths automatically, including documents added after evaluation.

The project explicitly imports the installed `Microsoft.NET.Sdk` props/targets,
targets net10.0/C#14, and declares no Compile items, package dependencies, project
references or package output. Root library/test/central-package imports are
disabled for this dedicated documentation project. Standard SDK project-system
imports remain available; explicit no-op Build/Restore/Clean/Pack/Rebuild,
CompileDesignTime and target-path targets prevent assembly and CLI build output.

`RestoreProjectStyle=Unknown` is NuGet's non-NuGet project graph style, not warning
suppression. The project has no restore assets to write. A mixed solution restores
its real libraries without trying to produce docs package assets. No external
project SDK or new package dependency was introduced. The NoTargets SDK considered
in the earlier assessment was not needed.

## Verification

`./tests/infrastructure/docs/Test-DocumentationProject.ps1` passed 14 checks using
the repository mutex, one MSBuild node, parallel building disabled, and warnings
as errors. Logs, fixture solution and summary:
`TestResults/docs-project/97a0183382a744cb9ffcb6d700d3678c`.

- Exactly one real solution Project entry and no manual docs File entries.
- 65 automatic relative None/Link items matched every on-disk document at the
  checkpoint. Subsequent documentation changes naturally alter that count.
- SDK/framework/language/non-packable metadata; zero Compile items and zero
  package/project references.
- Standalone restore, build, clean, pack and Rebuild passed without warnings.
- CLI invocation of the no-op CompileDesignTime target passed. This is not the
  complete Visual Studio design-time target pipeline.
- A copied project discovered `new/nested/added.md` after its first evaluation
  without edits to the project or solution.
- An isolated solution containing that docs project and a real SDK runtime
  project restored/built Release with zero warnings.
- No docs obj/bin/.no-output directories existed after all tested operations.

The earlier plain-MSBuild probe remains retained in
`TestResults/docs-solution-probe/build.log`: it emitted NU1503 and an empty-restore
warning. It predates the SDK-based correction and is not current success evidence.
The existing packaging documentation check also remains applicable to standalone
recursive items and no-output targets.

## Integration handoff

The documentation project adds a non-library solution entry, not a 29th package.
The dependency validator's audited directories must include `docs`; that file is
owned by Orchestrator and the required inventory update was explicitly relayed.
Framework package/source/test counts and coverage expectations remain unchanged.
No shared Directory.Build settings, source packages or compiler-host verification
files were changed by this correction.

Orchestrator reported the fresh warning-free Release solution build
`global-20260910-01` and paired full coverage `3c71dc66f5aa4fe9a29a5f20af0f7554`
passing 3,617 unit plus 15 integration tests and all 28 package thresholds.
Those are Orchestrator's global results, not executions by this workstream; the
earlier failed Tables coverage checkpoint remains preserved separately.

## Remaining IDE verification

`vswhere -all -products * -format json` found only SQL Server Management Studio 22,
not a development Visual Studio installation. No IDE was launched, installed or
configured. SDK imports and CLI item evaluation support project integration but
cannot establish actual Solution Explorer behavior.

On the target Visual Studio installation, open the real solution, verify that the
docs project loads, add/remove a nested document and observe automatic relative
tree updates, then confirm there are no docs build/design-time output directories.
Do not claim that final IDE check until performed. Final packaging and hosted gates
remain with Orchestrator; the CLI implementation is ready for the stable-source
handoff independently of this explicit IDE limitation.
