namespace vc.Ifx.Roslyn.Reporting;

/// <summary>Evidence supplied by a build/report host; null artifacts are explicitly unknown.</summary>
public sealed record ReportRequest(string Revision, string Scope, string[] ExpectedPackages,
    string? CoverageJson, PackageEvidence[] Packages);

/// <summary>One package's compiler output and evaluated source inventory.</summary>
public sealed record PackageEvidence(string Package, string? SarifJson, SourceEvidence[]? Sources);

/// <summary>A repository-relative source and the symbols used by its compilation.</summary>
public sealed record SourceEvidence(string Path, string Text, string[] Symbols);

/// <summary>A versioned report, without time-dependent or machine-dependent metadata.</summary>
public sealed record FrameworkReport(int SchemaVersion, string Revision, string Scope, string MetricPolicy,
    string? CoverageSha256, bool Passed, string[] Issues, PackageReport[] Packages);

/// <summary>Independent diagnostic, coverage and source-analysis facts.</summary>
public sealed record PackageReport(string Package, string? SarifSha256, CoverageMeasure? Coverage,
    DiagnosticFact[] Diagnostics, SourceFact[] Sources, MethodMetric[] Methods);

/// <summary>Exact instrumented counts. A zero denominator has no percentage.</summary>
public sealed record CoverageMeasure(long LinesCovered, long LinesTotal, long BranchesCovered,
    long BranchesTotal, decimal? LinePercent, decimal? BranchPercent, string Status);

/// <summary>A SARIF result, including unknown severities and explicit suppressions.</summary>
public sealed record DiagnosticFact(string RuleId, string Level, string Message, bool Suppressed,
    string? MetricName, string? MetricVersion, string? MeasuredValue, string? Threshold);

/// <summary>Source provenance and any explicit exclusion.</summary>
public sealed record SourceFact(string Path, string Sha256, string? Exclusion);

/// <summary>Ordinary-method measurements; unavailable measurements remain null.</summary>
public sealed record MethodMetric(string Path, int StartLine, string Name, int? CyclomaticComplexity,
    int? NestingDepth, int? MethodLength, int ParameterCount, string Status);
