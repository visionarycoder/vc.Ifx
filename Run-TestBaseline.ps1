# Test Baseline Report Generator
$ErrorActionPreference = "Continue"
$results = @()

Write-Host "`n╔════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║     Framework Test Baseline Report Generator      ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════╝`n" -ForegroundColor Green

$packages = @(
    "Framework.Abstractions.Tests",
    "Framework.Patterns.Tests",
    "Framework.Core.Tests",
    "Framework.DataAccess.Tests",
    "Framework.EntityFrameworkCore.Tests",
    "Framework.Messaging.Tests",
    "Framework.Observability.Tests",
    "Framework.Resilience.Tests",
    "Framework.Security.Tests"
)

foreach ($pkg in $packages) {
    Write-Host "Testing $pkg..." -ForegroundColor Cyan
    
    $testPath = "tests/$pkg"
    if (-not (Test-Path $testPath)) {
        Write-Host "  ⊗ Project not found" -ForegroundColor Yellow
        continue
    }
    
    $output = dotnet test $testPath --logger "trx" --results-directory "TestResults" 2>&1 | Out-String
    
    # Parse results
    if ($output -match "Passed:\s+(\d+)") {
        $passed = [int]$matches[1]
    } else { $passed = 0 }
    
    if ($output -match "Failed:\s+(\d+)") {
        $failed = [int]$matches[1]
    } else { $failed = 0 }
    
    if ($output -match "Total:\s+(\d+)") {
        $total = [int]$matches[1]
    } else { $total = $passed + $failed }
    
    $results += [PSCustomObject]@{
        Package = $pkg -replace "\.Tests$", ""
        Total = $total
        Passed = $passed
        Failed = $failed
        PassRate = if ($total -gt 0) { [Math]::Round($passed / $total * 100, 2) } else { 0 }
    }
    
    $color = if ($failed -eq 0) { "Green" } else { "Red" }
    Write-Host "  ✓ Total: $total | Passed: $passed | Failed: $failed" -ForegroundColor $color
}

# Summary
Write-Host "`n╔════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║                    SUMMARY                         ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════╝`n" -ForegroundColor Green

$results | Format-Table -AutoSize

$grandTotal = ($results | Measure-Object -Property Total -Sum).Sum
$grandPassed = ($results | Measure-Object -Property Passed -Sum).Sum
$grandFailed = ($results | Measure-Object -Property Failed -Sum).Sum
$overallPassRate = if ($grandTotal -gt 0) { [Math]::Round($grandPassed / $grandTotal * 100, 2) } else { 0 }

Write-Host "`nGrand Totals:" -ForegroundColor Cyan
Write-Host "  Total Tests: $grandTotal"
Write-Host "  Passed: $grandPassed" -ForegroundColor Green
Write-Host "  Failed: $grandFailed" -ForegroundColor $(if ($grandFailed -eq 0) { "Green" } else { "Red" })
Write-Host "  Pass Rate: $overallPassRate%" -ForegroundColor $(if ($overallPassRate -ge 99) { "Green" } elseif ($overallPassRate -ge 90) { "Yellow" } else { "Red" })

Write-Host "`nComparison to Pre-Refactoring Baseline:" -ForegroundColor Cyan
Write-Host "  Previous: 2,081 tests, 2,080 passed (99.95%)"
Write-Host "  Current:  $grandTotal tests, $grandPassed passed ($overallPassRate%)"
Write-Host "  Delta:    $($grandTotal - 2081) tests"

# Export to file
$results | Export-Csv -Path "TestResults/baseline-report.csv" -NoTypeInformation
Write-Host "`n✓ Results exported to TestResults/baseline-report.csv" -ForegroundColor Green
