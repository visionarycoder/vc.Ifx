# Ifx.Roslyn.Reporting

Non-packable net10.0 executable for report input/output and source metrics. It
references the shared Ifx.Roslyn parent; compiler tooling does not reference this
executable. Physical file access belongs here rather than in analyzers or generators.

The executable reads a ReportRequest JSON file and writes report.json and report.md
to the supplied output directory. The evidence-aware entrypoint remains
scripts/reporting/Invoke-FrameworkReport.ps1.

The `--design <request.json>` mode performs semantic component-reference checks
for [consumer design baseline verification](../../docs/onboarding/design-baseline.md).
The portable runner supplies evaluated compilation inputs and the reviewed policy.
The result reports structural issues and never authenticates an approval.
