# Ifx.Roslyn.Reporting

Non-packable net10.0 executable for report input/output and source metrics. It
references the shared Ifx.Roslyn parent; compiler tooling does not reference this
executable. Physical file access belongs here rather than in analyzers or generators.

The executable reads a ReportRequest JSON file and writes report.json and report.md
to the supplied output directory. The evidence-aware entrypoint remains
scripts/reporting/Invoke-FrameworkReport.ps1.
