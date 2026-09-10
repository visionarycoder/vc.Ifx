using System.Text.Json;
using vc.Ifx.Roslyn.Reporting;

if (args.Length != 2) throw new ArgumentException("Expected request JSON path and output directory.");
ReportRequest request = JsonSerializer.Deserialize<ReportRequest>(File.ReadAllText(args[0]),
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new ArgumentException("Missing request.");
FrameworkReport report = ReportEngine.Create(request);
Directory.CreateDirectory(args[1]);
File.WriteAllText(Path.Combine(args[1], "report.json"), ReportEngine.ToJson(report));
File.WriteAllText(Path.Combine(args[1], "report.md"), ReportEngine.ToMarkdown(report));
Console.WriteLine($"Report: {args[1]}; passed={report.Passed}; issues={report.Issues.Length}");
return report.Passed ? 0 : 1;
