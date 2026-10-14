#Requires -Version 7.0
<#
.SYNOPSIS
Formats the vc.Ifx solution according to repository .editorconfig rules.

.DESCRIPTION
Runs dotnet format whitespace and dotnet format style against vc.Ifx.slnx.
Use -VerifyNoChanges to check formatting without changing files.

.PARAMETER VerifyNoChanges
Returns a nonzero exit code when formatting changes are required.

.PARAMETER NoRestore
Prevents dotnet format from running an implicit restore.

.PARAMETER Include
Formats only the specified repository-relative files or directories.
#>
[CmdletBinding()]
param(
    [switch] $VerifyNoChanges,
    [switch] $NoRestore,
    [string[]] $Include = @()
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$solutionPath = Join-Path $repositoryRoot 'vc.Ifx.slnx'
if (-not (Test-Path -LiteralPath $solutionPath -PathType Leaf)) {
    throw "Solution file was not found: $solutionPath"
}
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'The .NET SDK command `dotnet` was not found on PATH.'
}

$includePaths = [Collections.Generic.List[string]]::new()
$repositoryPrefix = $repositoryRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
foreach ($path in $Include) {
    if ([string]::IsNullOrWhiteSpace($path) -or [IO.Path]::IsPathRooted($path)) {
        throw "Include paths must be non-empty and repository-relative: '$path'"
    }

    $fullPath = [IO.Path]::GetFullPath((Join-Path $repositoryRoot $path))
    if (-not $fullPath.StartsWith($repositoryPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Include path must remain inside the repository: '$path'"
    }
    if (-not (Test-Path -LiteralPath $fullPath)) {
        throw "Include path does not exist: '$path'"
    }

    $relativePath = [IO.Path]::GetRelativePath($repositoryRoot, $fullPath).Replace('\', '/')
    $includePaths.Add($relativePath)
}

$formatPhases = @('whitespace', 'style')
Push-Location -LiteralPath $repositoryRoot
try {
    foreach ($phase in $formatPhases) {
        $arguments = @('format', $phase, $solutionPath, '--verbosity', 'minimal')
        if ($NoRestore) {
            $arguments += '--no-restore'
        }
        if ($VerifyNoChanges) {
            $arguments += '--verify-no-changes'
        }
        if ($includePaths.Count -gt 0) {
            $arguments += '--include'
            $arguments += $includePaths.ToArray()
        }

        Write-Information "Running dotnet format $phase for vc.Ifx.slnx." -InformationAction Continue
        & dotnet @arguments
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet format $phase failed with exit code $LASTEXITCODE."
        }
    }
}
finally {
    Pop-Location
}
