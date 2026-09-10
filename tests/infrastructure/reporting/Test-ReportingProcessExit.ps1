#Requires -Version 7.0
[CmdletBinding()]
param([ValidateSet('Debug', 'Release')][string] $Configuration = 'Release')
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$testScript = Join-Path $PSScriptRoot 'Test-ReportingInfrastructure.ps1'
$pwsh = (Get-Process -Id $PID).Path

function Invoke-ModeledStep([string] $Name, [string] $Body, [bool] $ShouldPass) {
    $program = @'
$ErrorActionPreference = 'Stop'
'@ + "`n" + $Body + @'

if (Test-Path -LiteralPath variable:\LASTEXITCODE) { exit $LASTEXITCODE }
'@
    $encoded = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($program))
    $output = & $pwsh -NoProfile -NonInteractive -OutputFormat Text -EncodedCommand $encoded 2>&1
    $code = $LASTEXITCODE
    if (($code -eq 0) -ne $ShouldPass) { throw "$Name returned $code unexpectedly:`n$($output -join "`n")" }
    Write-Host "PASS: $Name; modeled GitHub pwsh process exit=$code"
}

$literal = $testScript.Replace("'", "''")
Invoke-ModeledStep 'actual reporting suite with expected native failure' "& '$literal' -Configuration $Configuration" $true

$tokens = $null
$errors = $null
$ast = [Management.Automation.Language.Parser]::ParseFile($testScript, [ref] $tokens, [ref] $errors)
if ($errors.Count) { throw 'Reporting self-test script failed parsing.' }
$function = $ast.Find({ param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq 'Test-Invocation' }, $true)
$probe = $function.Extent.Text + @'

$script:passed = 0
$runner = { & pwsh -NoProfile -Command 'exit 7'; throw 'unexpected failure' }
Test-Invocation 'broken assertion' @{} 'different expected failure'
'@ + "`n" + $ast.EndBlock.Statements[-1].Extent.Text
Invoke-ModeledStep 'unexpected failure is never cleared by success footer' $probe $false
$probe = $function.Extent.Text + @'

$script:passed = 0
$runner = { & pwsh -NoProfile -Command 'exit 0' }
Test-Invocation 'missing expected failure' @{} 'required failure'
'@ + "`n" + $ast.EndBlock.Statements[-1].Extent.Text
Invoke-ModeledStep 'missing negative-test failure remains fatal' $probe $false
Write-Host '3 reporting process-exit regressions passed.'
$global:LASTEXITCODE = 0
