# vc.Ifx.Observability

Standard System.Diagnostics ActivitySource/Meter adapters for .NET 10 / C# 14.
Pipeline remains the only direct package dependency. ITracer, ISpan and IMetrics,
existing parameterless constructors, and the compatibility Observibility
namespace are unchanged. No exporter, hosting framework or new DI package is
introduced.

## Published Spelling Compatibility

The package is named `vc.Ifx.Observability`, but its published public namespace
retains the historical spelling `VisionaryCoder.Framework.Pipeline.Observibility`.
That spelling is intentional compatibility, not a second implementation. Existing
using directives and fully qualified type names must retain `Observibility`;
this upgrade does not rename the public namespace or require a source migration.

## Trace Ownership

OpenTelemetryTracer uses the existing source name PipelineInvoker by default.
The ActivitySource constructor overload borrows a host-owned source, allowing
isolated listeners and DI configuration. StartSpan creates an Internal child of
the caller's current activity; it never adopts or disposes that caller activity.
No-listener and sampling-rejected spans are no-ops, not wrappers around the
current activity. Normal Activity context propagation flows across await and
Task.Run without a second AsyncLocal store.

End and Dispose share one atomic lifetime transition. Only the activity created
by StartSpan is disposed, once, including concurrent/repeated End/Dispose calls.
Ending the current span restores its prior activity; ending a non-current span
preserves the caller's current activity. Dispose remains safe after End and does
not repeat stop events. Finish nested spans in reverse order in their owning
logical async flow; ending in a different execution context cannot rewrite the
originating context's AsyncLocal value. Complete tag writes before ending a span.

Names/tag keys are nonblank and values are non-null (empty values are allowed).
Tags after End are ignored. Pipeline status tags remain intact; success maps to
ActivityStatusCode.Ok, error to Error, other statuses including canceled to
Unset. error.message sets the Error description. No extra exception events or
logs are synthesized. Error descriptions are trusted diagnostics, not client
responses. Listener exceptions propagate; owned activity cleanup restores ambient
context even when a stop callback fails, and the stop is not retried.
Start callback failures also restore ambient context. If the SDK throws before
returning an activity, the adapter cannot safely reclaim an unreturned activity
without risking ownership of a listener-created activity; listener code must own
that cleanup. SDK listeners should not throw. Instrument publication failures
are cached by the corresponding Lazy instrument; replacing the host meter is
required to retry a failed definition. No listener exception is silently ignored.

## Metrics

The default meter name remains PipelineInvoker.Metrics. The Meter constructor
overload borrows a host-owned meter. Each nonblank metric name identifies one
Counter<long> (count) or Histogram<double> (ms) per meter and instrument kind,
shared across adapter instances and concurrent first use. Lazy publication avoids
duplicate creation callbacks. Labels are nonblank request tag values, not part
of instrument identity or descriptions; colon-containing names/labels cannot
collide. Keep names static and labels low-cardinality; the adapter does not impose
an application-specific whitelist or eviction policy.

Each IncrementCounter emits exactly +1; each ObserveHistogram emits one
nonnegative millisecond sample using the existing double-valued SDK histogram.
Zero is valid. No timer, observable callback, retry, second event or hidden
aggregation is introduced. Without listeners, SDK measurement recording is a
no-op. Listener failures propagate without retry; the SDK owns listener delivery.

Pipeline's MetricsInterceptor owns completion semantics: one requests_total and
one request_duration_ms sample per entered operation, plus requests_failed_total
or requests_canceled_total when appropriate. Pre-canceled calls rejected before
interceptors produce no diagnostics. The adapter does not recalculate duration or
infer cancellation. Pipeline's logging interceptor remains the logging owner.

## DI and Migration

Register adapters as singleton ITracer/IMetrics services. Default constructors
use process-lifetime SDK objects. For host-scoped sources/meters, inject the
ActivitySource and Meter (or resolve a Meter through IMeterFactory) using explicit
DI factories. The host owns their disposal; adapters never dispose them or
caller activities, including when a DI scope ends. Dispose hosts after in-flight
operations finish. No global listener or exporter is installed by the package.

Migration: instrument descriptions are now label-independent and duplicate
per-label instruments are removed. Negative durations and invalid arguments now
fail early. End/Dispose is idempotent, completed tags are ignored, and Pipeline
status tags also populate standard Activity status. Metric names, units, numeric
types, request tag keys and default source/meter names are retained.

## Verification

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Observability -TestSourceScope Observability -TestPackage vc.Ifx.Observability -Filter FullyQualifiedName~Tests.Observability -WarningsAsErrors
```

Strict run verified 2026-09-09: 19/19 tests, 70/70 lines and 16/16 branches (100%),
with warnings treated as errors. Evidence:
`TestResults/coverage/vc.Ifx.Observability/a0ef05be9bfc4d559725313b4fc35608/summary.json`
and the adjacent `vc.Ifx.UnitTests/tests.trx`. Both handwritten adapter files are
included; no package exclusions were added. Tests use ActivityListener/MeterListener
and deterministic Pipeline clocks, without exporters, network requests or
wall-clock delays. DI integration covers success, failure and cancellation,
exact metric/log counts, activity ownership, and borrowed SDK lifetimes.

Status remains In-flight for final integration, packaging and repository-wide
gates owned by the orchestrator. No whole-solution check was run by this worker.

Primary references: [.NET metrics](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/metrics-instrumentation)
and [Activity lifetime](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Diagnostics.DiagnosticSource/src/System/Diagnostics/Activity.cs).
