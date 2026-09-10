#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $CoveragePath,
    [Parameter(Mandatory)][string[]] $ExpectedPackage,
    [Parameter(Mandatory)][string] $OutputDirectory,
    [switch] $ReportOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$coverage = Get-Content -LiteralPath $CoveragePath -Raw | ConvertFrom-Json -AsHashtable
if ($coverage -isnot [System.Collections.IDictionary] -or $coverage.Count -eq 0) {
    throw 'Coverage document contains no modules.'
}

$rows = foreach ($package in ($ExpectedPackage | Sort-Object -Unique)) {
    $modules = @($coverage.Keys | Where-Object { [IO.Path]::GetFileNameWithoutExtension($_) -ceq $package })
    if ($modules.Count -gt 1) { throw "Duplicate module for $package." }
    [long] $lines = 0
    [long] $coveredLines = 0
    [long] $branches = 0
    [long] $coveredBranches = 0
    foreach ($module in $modules) {
        foreach ($document in $coverage[$module].Values) {
            foreach ($type in $document.Values) {
                foreach ($method in $type.Values) {
                    foreach ($hits in $method.Lines.Values) {
                        if ($hits -isnot [long] -and $hits -isnot [int]) { throw 'Invalid line hit count.' }
                        if ($hits -lt 0) { throw 'Negative line hit count.' }
                        $lines++
                        if ($hits -gt 0) { $coveredLines++ }
                    }
                    foreach ($branch in $method.Branches) {
                        if (-not $branch.Contains('Hits') -or ($branch.Hits -isnot [long] -and $branch.Hits -isnot [int])) {
                            throw 'Invalid branch hit count.'
                        }
                        if ($branch.Hits -lt 0) { throw 'Negative branch hit count.' }
                        $branches++
                        if ($branch.Hits -gt 0) { $coveredBranches++ }
                    }
                }
            }
        }
    }
    $status = if ($modules.Count -eq 0) { 'MissingModule' }
        elseif ($lines -eq 0) { 'NoExecutableLines' }
        elseif ($lines -eq $coveredLines -and $branches -eq $coveredBranches) { 'Passed' }
        else { 'BelowThreshold' }
    [pscustomobject][ordered]@{
        package = $package
        linesCovered = $coveredLines
        linesTotal = $lines
        linePercent = $(if ($lines -gt 0) { [Math]::Round(100.0 * $coveredLines / $lines, 6) } else { $null })
        branchesCovered = $coveredBranches
        branchesTotal = $branches
        branchPercent = $(if ($branches -gt 0) { [Math]::Round(100.0 * $coveredBranches / $branches, 6) } else { $null })
        status = $status
    }
}
if (@($rows).Count -eq 0) { throw 'No expected packages supplied.' }
$failures = @($rows | Where-Object status -ne Passed)
$report = [ordered]@{
    schemaVersion = 1
    coverageFile = [IO.Path]::GetFullPath($CoveragePath)
    threshold = 100
    passed = ($failures.Count -eq 0)
    packages = @($rows)
}
$null = New-Item -ItemType Directory -Path $OutputDirectory -Force
$report | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'summary.json') -Encoding utf8
$markdown = @('# Package Coverage', '', '| Package | Lines | Branches | Status |', '| --- | --- | --- | --- |')
$markdown += $rows | ForEach-Object { "| $($_.package) | $($_.linesCovered)/$($_.linesTotal) | $($_.branchesCovered)/$($_.branchesTotal) | $($_.status) |" }
$markdown | Set-Content -LiteralPath (Join-Path $OutputDirectory 'summary.md') -Encoding utf8
$rows | Format-Table package, linesCovered, linesTotal, branchesCovered, branchesTotal, status | Out-Host
if ($failures.Count -gt 0 -and -not $ReportOnly) {
    throw "Coverage gate failed for $($failures.Count) package(s). See $OutputDirectory/summary.md."
}
