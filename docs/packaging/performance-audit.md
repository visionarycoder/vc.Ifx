# Performance project audit

Local implementation checkpoint: 2026-09-09. The original inventory contained
five methods, including a storage type-name lookup that did not exercise storage.
The four existing projects now contain 31 real benchmark methods / 54 parameterized
cases. No new packable project or public framework API was introduced.

| Project | Implemented scenarios | Methods / cases |
| --- | --- | --- |
| Core | Success/failure results; typed identifier construction, valid/invalid parsing, equality, JSON serialization/deserialization | 10 / 10 |
| Filtering | Simple and parameterized nested collection predicates; portable translation and JSON; direct/compiled/translate-compile-execute filtering at 100 and 10,000 records; composite Querying JSON at 1/16 predicates | 10 / 20 |
| Proxy | Cache keys with 0/16 headers; 64 B/64 KiB cache hit/miss/replacement; direct local dispatch vs 0/1/8 interceptors; in-memory HTTP serialization/dispatch/deserialization at two payload sizes | 7 / 16 |
| Storage | Actual local provider consuming reads, overwriting an existing file, listing 16 objects, create/delete lifecycle at 1 KiB/64 KiB | 4 / 8 |

The existing solution already contains all four projects. Filtering benchmarks now
reference Querying; Proxy benchmarks reference Pipeline. All four evaluate to
`net10.0`, C# `14.0`, `IsPackable=false`, `GeneratePackageOnBuild=false`.
Shared `performance/BenchmarkProgram.cs` propagates one-node build settings to
BenchmarkDotNet and rejects failed or empty execution. The wrapper shares the
repository build/test mutex, preserves fresh run-specific evidence, and refuses
missing JSON measurements. Power-plan changes are disabled.

See [benchmark README](../../performance/README.md) for setup, timing boundaries,
warmup policy, cleanup, commands and interpretation. Correctness and unit coverage
remain separate gates; successful benchmark setup assertions are not unit coverage.

## Retained execution evidence

All commands below run from the repository root, use the shared mutex, build
Release with `-m:1 -p:BuildInParallel=false`, and disable automatic packaging.
Artifact IDs are subdirectories of `TestResults/benchmarks/` and contain exact
arguments, build/console logs, JSON/Markdown/CSV/HTML results and SDK/worktree data.

| Command arguments to `./scripts/Invoke-FrameworkBenchmarks.ps1` | Result | Artifact ID |
| --- | --- | --- |
| `-Project Core -Filter '*SuccessfulResult' -Job Dry` | Passed one case; verified actual Dry job, one measurement, no power-plan change | `dfabe04dd17642608bedcc8510b116a7` |
| `-Project Core,Filtering,Storage -Job Dry` | Passed 38 cases: Core 10, Filtering/Querying 20, Storage 8; zero build warnings | `787f99bf01244249ab9af4200a554e51` |
| `-Project Proxy -Job Dry` | Passed 16 cases; zero build warnings after package owner fixed correlation CS8601 | `af7ffce467914ffd8185950d9880a8ca` |
| `-Project Proxy,Storage -Filter '*ProxyCacheBenchmarks*','*StorageBenchmarks*' -Job Dry` | Passed 14 cases after prepopulating replacement/overwrite fixtures; zero build warnings | `cb3a6d8023e04878baec1eae46334dcf` |
| `-Project Core -Filter '*IdentifierBenchmarks.Serialize' -Job Short` | Passed one case, three measurements; zero build warnings | `90f0df68c6164939a36909782a1162c8` |

Replacement/overwrite setup was tightened to prepopulate the target before timing,
including a single-iteration Dry run. The affected fixture rerun supersedes those
fixtures' earlier Dry evidence. All 54 distinct cases have successful execution
evidence; reruns are not counted as additional scenarios.

The bounded Short serialization run observed mean 181.812 ns, standard deviation
25.077 ns, and 104 managed bytes/op. Its 99.9% confidence interval was
[-275.682, 639.306] ns: uncertainty exceeds the mean, so no regression threshold or
reliable latency claim follows. This run proves repeated measurement/export works;
it does not establish a release baseline.

The observed host was Windows 11, Intel Core i7-8809G (4 physical / 8 logical cores),
SDK 10.0.401, runtime 10.0.12, BenchmarkDotNet 0.15.6, workstation GC. This is a
concurrently edited worktree on a shared machine, not a frozen release baseline.
Dry intentionally emits short-iteration warnings and unavailable confidence
intervals: it is execution evidence, not a useful latency comparison.

### Earlier unsuccessful attempts

- `d050b37e505a4e7cbd156a4a2a6bfd15`: discovery build exposed shared benchmark
  entry-point API mistakes; corrected before the successful runs above.
- `d7e8a203d72945dc8d3c682518583f1d`: Core/Filtering discovery succeeded;
  Proxy failed on the active package owner's CS8601. No warning suppression or
  package behavior edit was made by this worker.
- `e4b25095870649c68d0ff6f001102e94`: initial job configuration incorrectly
  overrode the requested Dry job with Default. One case finished; the next owned
  benchmark child was deliberately stopped. Parent exited nonzero, restored the
  prior Balanced power plan and released the mutex. This incomplete run is not
  a benchmark baseline. The configuration now uses a job mutator, preserves the
  selected job, and explicitly disables power-plan enforcement.

## Remaining gates and gaps

No release performance threshold or before/after baseline is inferred from smoke
runs. Repeated comparisons require an idle machine and stable source snapshot.
Storage allows OS caching and does not claim durable-media throughput. HTTP uses
an in-memory handler; FTP/Azure latency, credentials and live service cleanup are
outside these deterministic scenarios. Cache payloads are referenced, not copied.
gRPC, secrets, WebApi, observability, analyzer/generator execution and contention
still have no dedicated performance scenarios.

Orchestrator owns final whole-solution build, complete tests/coverage and a fresh
28-package snapshot once source stabilizes. Hosted execution is a separate future
integration check requiring remote changes, not a prerequisite for this local
milestone. No full-suite rerun, final pack, push, publish or hosted settings change
was performed by this performance follow-through.
