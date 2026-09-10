# Framework benchmarks

Run from the repository root in PowerShell 7 with SDK 10.0.401:

```powershell
./scripts/Invoke-FrameworkBenchmarks.ps1 -ListOnly
./scripts/Invoke-FrameworkBenchmarks.ps1 -Project Core,Filtering,Proxy,Storage -Job Dry
./scripts/Invoke-FrameworkBenchmarks.ps1 -Project Filtering -Filter '*FilteringExecutionBenchmarks*' -Job Short
```

The runner shares the framework test mutex, builds Release with one MSBuild node,
disables automatic packaging, and checks stable framework/language and
non-packable metadata. BenchmarkDotNet child builds receive the same concurrency
settings. It does not change the machine power plan. Empty selections, failed
benchmarks, and missing measurements fail the command.

Each run retains its arguments, SDK/runtime details, revision, before/after
worktree inventory, build logs, BenchmarkDotNet logs, JSON, Markdown, CSV and HTML
under a fresh `TestResults/benchmarks/<run-id>` directory. No repository cleanup
is performed. BenchmarkDotNet generated builds live under benchmark `bin` paths.
For reproducible comparisons, use a frozen source revision, record any dirty
changes separately, use matching runtime/hardware/power settings, and reserve an
idle machine. A shared working tree and host cannot provide a release baseline.

`Dry` is one cold-start measurement for execution/setup checks; its timing and
allocation results are not steady-state performance estimates. `Short` uses
BenchmarkDotNet's short job (three warmup and three measured iterations).
`Default` uses its normal adaptive policy and can take substantially longer.
Both longer jobs need deliberate scheduling around other workers. Full job and
environment settings are included in each report. MemoryDiagnoser records managed
allocations, not native memory or durable storage throughput.

## Measurement boundaries

- Core measures success/failure results, typed identifier construction, parsing,
  equality, and warmed JSON converter round trips.
- Filtering separates predicate construction, portable translation, compilation
  plus execution, already-compiled execution, and JSON serialization. Fixed data
  includes matching and nonmatching records; setup compares execution results.
- Querying measures serialization and deserialization of composite query filters.
- Proxy measures header-dependent cache keys and hit/miss/replacement overhead.
  Cached byte arrays are referenced, not copied; payload size is not bandwidth.
- Pipeline compares direct local dispatch with zero, one, and eight pass-through
  interceptors, including routing/correlation overhead. HTTP adapter measurements
  consume serialized requests and deserialize a fixed in-memory response; they
  include no sockets, DNS, server, or live service latency.
- Storage uses the actual local provider with 1 KiB/64 KiB files. Read consumes the
  stream, overwrite writes a fixed path, list enumerates 16 seeded objects, and
  create/delete measures a complete lifecycle. Setup is outside timing. Each
  process owns a unique temporary directory with guarded cleanup. OS file caching
  is allowed; no flush-to-durable-media claim is made.

See [performance audit](../docs/packaging/performance-audit.md) for the scenario
inventory, exact runs and open measurement gaps. These executables do not replace
unit/integration tests or the per-package coverage gate.
