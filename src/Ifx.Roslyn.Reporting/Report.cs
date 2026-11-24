using System.Text.Json;
using Ifx.Roslyn.Reporting;

if (args.Length == 2 && args[0] == "--design")
{
    using JsonDocument input = JsonDocument.Parse(File.ReadAllText(args[1]));
    string[] issues = DesignConformance.Check(input.RootElement);
    Console.WriteLine(JsonSerializer.Serialize(new { passed = issues.Length == 0, issues, authorization_verified = false }));
    return issues.Length == 0 ? 0 : 1;
}

if (args.Length != 2) throw new ArgumentException("Expected request JSON path and output directory.");
ReportRequest request = JsonSerializer.Deserialize<ReportRequest>(File.ReadAllText(args[0]),
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new ArgumentException("Missing request.");
FrameworkReport report = ReportEngine.Create(request);
Directory.CreateDirectory(args[1]);
File.WriteAllText(Path.Combine(args[1], "report.json"), ReportEngine.ToJson(report));
File.WriteAllText(Path.Combine(args[1], "report.md"), ReportEngine.ToMarkdown(report));
Console.WriteLine($"Report: {args[1]}; passed={report.Passed}; issues={report.Issues.Length}");
return report.Passed ? 0 : 1;
