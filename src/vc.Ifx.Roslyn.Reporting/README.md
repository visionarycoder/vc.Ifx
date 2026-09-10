# vc.Ifx.Roslyn.Reporting

net10.0 library for deterministic, versioned SARIF, exact Coverlet coverage and
Roslyn ordinary-method metric reports. No analyzer dependencies or report file I/O.

```csharp
FrameworkReport report = ReportEngine.Create(request, cancellationToken);
string json = ReportEngine.ToJson(report);
string markdown = ReportEngine.ToMarkdown(report);
```

The caller supplies a `ReportRequest`: revision, full/scoped label, explicit package
inventory, merged raw Coverlet JSON, per-package compiler SARIF 2.1.0 and evaluated
source text/symbols. Missing artifacts produce null measurements and failed reports;
malformed artifacts throw. No historical artifact discovery or rounded thresholds.

`ifx-method-source-v1` counts syntax decisions, control-flow nesting, physical body
line span and declared parameters. It is not a substitute for semantic CFG metrics
or CA1502. Only ordinary methods are measured; abstract/extern bodies are unknown.
Nesting >4 is a review classification, not an invented quality failure. Generated
exclusions and source hashes are explicit; handwritten records/async remain.

The script host is `scripts/reporting/Invoke-FrameworkReport.ps1` in the repository.
The authoritative schema, policies, exclusions, CI provenance and limitations are
documented in [report-contract.md](https://github.com/visionarycoder/vc.Ifx/blob/main/docs/reporting/report-contract.md).
