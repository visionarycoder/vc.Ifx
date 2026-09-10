using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace vc.Ifx.Roslyn.Reporting;

/// <summary>Report generation from explicit build/test facts. Never reads or writes report files.</summary>
public static class ReportEngine
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

    /// <summary>Builds a deterministic report. Malformed input throws; incomplete evidence fails closed.</summary>
    public static FrameworkReport Create(ReportRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Revision);
        if (request.Scope is not ("full" or "scoped")) throw new ArgumentException("Scope must be full or scoped.");
        if (request.ExpectedPackages.Length == 0 || request.ExpectedPackages.Any(string.IsNullOrWhiteSpace) ||
            request.ExpectedPackages.Distinct(StringComparer.Ordinal).Count() != request.ExpectedPackages.Length)
            throw new ArgumentException("Expected packages must be nonempty and unique.");
        Dictionary<string, PackageEvidence> evidence = request.Packages.ToDictionary(package => package.Package, StringComparer.Ordinal);
        if (evidence.Keys.Except(request.ExpectedPackages, StringComparer.Ordinal).Any()) throw new ArgumentException("Unexpected package evidence.");
        List<string> issues = [];
        List<PackageReport> packages = [];
        using JsonDocument? coverage = request.CoverageJson is null ? null : JsonDocument.Parse(request.CoverageJson);
        foreach (string package in request.ExpectedPackages.Order(StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            CoverageMeasure? measure = ReadCoverage(coverage, package);
            if (measure?.Status != "Passed") issues.Add($"Coverage incomplete or below 100%: {package}");
            evidence.TryGetValue(package, out PackageEvidence? input);
            DiagnosticFact[] diagnostics = [];
            if (input?.SarifJson is null) issues.Add($"Missing SARIF: {package}");
            else
            {
                diagnostics = ReadSarif(input.SarifJson);
                if (diagnostics.Any(diagnostic => !diagnostic.Suppressed && diagnostic.Level is "error" or "unknown"))
                    issues.Add($"SARIF contains errors or unknown severity: {package}");
            }
            List<SourceFact> sources = [];
            List<MethodMetric> methods = [];
            if (input?.Sources is null || input.Sources.Length == 0) issues.Add($"Missing source inventory: {package}");
            else
            {
                if (input.Sources.Select(source => source.Path.Replace('\\', '/')).Distinct(StringComparer.Ordinal).Count() != input.Sources.Length)
                    throw new ArgumentException("Duplicate source paths.");
                foreach (SourceEvidence source in input.Sources.OrderBy(source => source.Path, StringComparer.Ordinal))
                    SourceMetrics.Analyze(source, sources, methods, issues, cancellationToken);
            }
            packages.Add(new(package, input?.SarifJson is null ? null : Hash(input.SarifJson), measure, diagnostics,
                sources.OrderBy(source => source.Path, StringComparer.Ordinal).ToArray(),
                methods.OrderBy(method => method.Path, StringComparer.Ordinal).ThenBy(method => method.StartLine).ThenBy(method => method.Name, StringComparer.Ordinal).ToArray()));
        }
        return new(1, request.Revision, request.Scope, "ifx-method-source-v1",
            request.CoverageJson is null ? null : Hash(request.CoverageJson), issues.Count == 0,
            issues.Order(StringComparer.Ordinal).ToArray(), packages.ToArray());
    }

    internal static string Hash(string text) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    private static CoverageMeasure? ReadCoverage(JsonDocument? document, string package)
    {
        if (document is null) return null;
        JsonProperty[] modules = document.RootElement.EnumerateObject().Where(module =>
            Path.GetFileNameWithoutExtension(module.Name.Replace('\\', '/')) == package).ToArray();
        if (modules.Length > 1) throw new ArgumentException($"Duplicate coverage module: {package}");
        if (modules.Length == 0) return null;
        long lines = 0, coveredLines = 0, branches = 0, coveredBranches = 0;
        foreach (JsonProperty file in modules[0].Value.EnumerateObject())
        foreach (JsonProperty type in file.Value.EnumerateObject())
        foreach (JsonProperty method in type.Value.EnumerateObject())
        {
            foreach (JsonProperty line in method.Value.GetProperty("Lines").EnumerateObject())
            {
                long hits = ReadHits(line.Value);
                lines++;
                if (hits > 0) coveredLines++;
            }
            foreach (JsonElement branch in method.Value.GetProperty("Branches").EnumerateArray())
            {
                long hits = ReadHits(branch.GetProperty("Hits"));
                branches++;
                if (hits > 0) coveredBranches++;
            }
        }
        string status = lines == 0 ? "NoExecutableLines" :
            lines == coveredLines && branches == coveredBranches ? "Passed" : "BelowThreshold";
        return new(coveredLines, lines, coveredBranches, branches,
            lines == 0 ? null : 100m * coveredLines / lines,
            branches == 0 ? null : 100m * coveredBranches / branches, status);
    }

    private static long ReadHits(JsonElement value)
    {
        long hits = value.GetInt64();
        if (hits < 0) throw new ArgumentException("Negative coverage hit count.");
        return hits;
    }

    private static DiagnosticFact[] ReadSarif(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;
        if (root.GetProperty("version").GetString() != "2.1.0") throw new ArgumentException("Only SARIF 2.1.0 is supported.");
        JsonElement runs = root.GetProperty("runs");
        if (runs.GetArrayLength() == 0) throw new ArgumentException("SARIF contains no runs.");
        List<DiagnosticFact> facts = [];
        foreach (JsonElement run in runs.EnumerateArray())
        {
            _ = run.GetProperty("tool").GetProperty("driver").GetProperty("name").GetString();
            if (run.TryGetProperty("invocations", out JsonElement invocations) &&
                invocations.EnumerateArray().Any(invocation => !invocation.GetProperty("executionSuccessful").GetBoolean()))
                throw new ArgumentException("SARIF contains an unsuccessful invocation.");
            foreach (JsonElement result in run.GetProperty("results").EnumerateArray())
            {
                string level = "unknown";
                if (result.TryGetProperty("level", out JsonElement severity)) level = severity.GetString()!;
                if (level is not ("error" or "warning" or "note" or "none")) level = "unknown";
                bool suppressed = result.TryGetProperty("suppressions", out JsonElement suppressions) &&
                    suppressions.EnumerateArray().Any(suppression => !suppression.TryGetProperty("status", out JsonElement status) || status.GetString() == "accepted");
                JsonElement properties = default;
                if (result.TryGetProperty("properties", out properties) && properties.TryGetProperty("customProperties", out JsonElement custom)) properties = custom;
                facts.Add(new(result.GetProperty("ruleId").GetString()!, level, result.GetProperty("message").GetProperty("text").GetString()!, suppressed,
                    Property(properties, "MetricName"), Property(properties, "MetricVersion"), Property(properties, "MeasuredValue"), Property(properties, "Threshold")));
            }
        }
        return facts.OrderBy(fact => fact.RuleId, StringComparer.Ordinal).ThenBy(fact => fact.Level, StringComparer.Ordinal)
            .ThenBy(fact => fact.Message, StringComparer.Ordinal).ThenBy(fact => Serialize(fact), StringComparer.Ordinal).ToArray();
    }

    private static string? Property(JsonElement properties, string name) =>
        properties.ValueKind == JsonValueKind.Object && properties.TryGetProperty(name, out JsonElement value) ? value.ToString() : null;

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    /// <summary>Serializes schema v1 with explicit nulls and LF endings.</summary>
    public static string ToJson(FrameworkReport report) => Serialize(report).Replace("\r\n", "\n") + "\n";

    /// <summary>Renders exact counts and all metrics for review with invariant formatting.</summary>
    public static string ToMarkdown(FrameworkReport report)
    {
        StringBuilder text = new();
        text.Append("# Framework Report v1\n\nRevision: ").Append(Escape(report.Revision))
            .Append("\n\nScope: ").Append(report.Scope).Append("\n\nResult: ").Append(report.Passed ? "Passed" : "Failed").Append("\n\n");
        foreach (string issue in report.Issues) text.Append("- ").Append(Escape(issue)).Append('\n');
        text.Append("\n| Package | Lines | Branches | Coverage |\n| --- | --- | --- | --- |\n");
        foreach (PackageReport package in report.Packages)
        {
            CoverageMeasure? coverage = package.Coverage;
            text.Append("| ").Append(Escape(package.Package)).Append(" | ")
                .Append(coverage is null ? "Unknown" : $"{Number(coverage.LinesCovered)}/{Number(coverage.LinesTotal)}")
                .Append(" | ").Append(coverage is null ? "Unknown" : $"{Number(coverage.BranchesCovered)}/{Number(coverage.BranchesTotal)}")
                .Append(" | ").Append(coverage?.Status ?? "Unknown").Append(" |\n");
        }
        foreach (PackageReport package in report.Packages)
        {
            text.Append("\n## ").Append(Escape(package.Package)).Append("\n\n")
                .Append("| Method | Cyclomatic | Nesting | Length | Parameters | Status |\n| --- | --- | --- | --- | --- | --- |\n");
            foreach (MethodMetric method in package.Methods)
                text.Append("| ").Append(Escape($"{method.Path}:{Number(method.StartLine)} {method.Name}"))
                    .Append(" | ").Append(Number(method.CyclomaticComplexity)).Append(" | ").Append(Number(method.NestingDepth))
                    .Append(" | ").Append(Number(method.MethodLength)).Append(" | ").Append(Number(method.ParameterCount))
                    .Append(" | ").Append(method.Status).Append(" |\n");
            foreach (DiagnosticFact diagnostic in package.Diagnostics)
                text.Append("\n- ").Append(Escape($"{diagnostic.RuleId} ({diagnostic.Level}, suppressed={diagnostic.Suppressed}): {diagnostic.Message}")).Append('\n');
            foreach (SourceFact source in package.Sources.Where(source => source.Exclusion is not null))
                text.Append("\n- Excluded: ").Append(Escape(source.Path)).Append(" (").Append(source.Exclusion).Append(")\n");
        }
        return text.ToString();
    }

    private static string Number(long? value) => value?.ToString(CultureInfo.InvariantCulture) ?? "Unknown";
    private static string Escape(string value) => value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
        .Replace("|", "&#124;").Replace("`", "&#96;").Replace("\r", " ").Replace("\n", " ");
}
