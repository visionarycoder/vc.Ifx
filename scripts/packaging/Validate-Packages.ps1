#requires -Version 7.0
[CmdletBinding()]
param(
    [string] $RepositoryRoot = (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent),
    [string[]] $PackageId = @(),
    [string] $PackageDirectory,
    [switch] $Pack,
    [switch] $MetadataOnly,
    [switch] $SkipDocsCheck
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'PackageValidation.psm1') -Force
$RepositoryRoot = [IO.Path]::GetFullPath($RepositoryRoot)
if (-not $PackageDirectory) { $PackageDirectory = Join-Path $RepositoryRoot 'artifacts/packaging' }
$PackageDirectory = [IO.Path]::GetFullPath($PackageDirectory)

function Invoke-DotNet {
    param([string[]] $Arguments)
    $output = & dotnet @Arguments -m:1 -p:BuildInParallel=false 2>&1
    if ($LASTEXITCODE -ne 0) { throw "dotnet $($Arguments -join ' ') failed:`n$($output -join [Environment]::NewLine)" }
    return ($output -join [Environment]::NewLine)
}

$projects = @(Get-ChildItem (Join-Path $RepositoryRoot 'src') -Recurse -Filter '*.csproj' |
    Where-Object FullName -NotMatch '[/\\](bin|obj)[/\\]' | Sort-Object FullName)
Assert-PackageRule ($projects.Count -gt 0) 'No source package projects discovered.'
foreach ($id in $PackageId) {
    Assert-PackageRule ($projects.BaseName -contains $id) "Unknown package: $id"
}
if ($PackageId.Count -gt 0) { $projects = @($projects | Where-Object BaseName -In $PackageId) }

foreach ($project in $projects) {
    Write-Host "Checking $($project.BaseName)"
    $evaluation = Invoke-DotNet @('msbuild', $project.FullName, '-getProperty:PackageId,PackageVersion,Description,PackageTags,PackageReadmeFile,PackageLicenseExpression,RepositoryUrl,IncludeSymbols,SymbolPackageFormat,PublishRepositoryUrl,IsPackable', '-getItem:ProjectReference,PackageReference') | ConvertFrom-Json
    $properties = $evaluation.Properties
    Assert-PackageRule ($properties.PackageId -eq $project.BaseName) "Incorrect PackageId: $($project.Name)"
    foreach ($property in @('Description', 'PackageTags', 'PackageReadmeFile', 'RepositoryUrl')) {
        Assert-PackageRule (-not [string]::IsNullOrWhiteSpace($properties.$property)) "Missing $property in $($project.Name)"
    }
    $readmePath = Join-Path $project.DirectoryName $properties.PackageReadmeFile
    Assert-PackageRule (Test-Path -LiteralPath $readmePath) "Missing source README: $readmePath"
    Assert-PackageRule ($properties.IsPackable -eq 'true') "Source package is not packable: $($project.Name)"
    Assert-PackageRule ($properties.PackageLicenseExpression -eq 'MIT') 'Expected MIT license expression.'
    Assert-PackageRule ($properties.IncludeSymbols -eq 'true' -and $properties.SymbolPackageFormat -eq 'snupkg' -and $properties.PublishRepositoryUrl -eq 'true') 'Source Link/symbol settings are disabled.'
    $publicDependencies = @()
    foreach ($reference in $evaluation.Items.ProjectReference) {
        $dependencyId = [IO.Path]::GetFileNameWithoutExtension($reference.FullPath)
        Assert-PackageRule ($project.BaseName -eq 'vc.Ifx' -or $dependencyId -ne 'vc.Ifx') 'Small package references aggregator.'
        if ($project.BaseName -notin @('vc.Ifx.Analyzers', 'vc.Ifx.CodeFixes', 'vc.Ifx.Generators')) { $publicDependencies += $dependencyId }
    }
    if ($project.BaseName -notin @('vc.Ifx.Analyzers', 'vc.Ifx.CodeFixes', 'vc.Ifx.Generators')) {
        foreach ($reference in $evaluation.Items.PackageReference) {
            $privateAssets = $reference.PSObject.Properties['PrivateAssets']
            if ($null -eq $privateAssets -or $privateAssets.Value -ne 'all') { $publicDependencies += $reference.Identity }
        }
    }
    if ($Pack) {
        Invoke-DotNet @('pack', $project.FullName, '-c', 'Release', '-o', $PackageDirectory, '-p:GeneratePackageOnBuild=false', '-v:minimal') | Write-Host
    }
    if (-not $MetadataOnly) {
        $archivePath = Join-Path $PackageDirectory "$($properties.PackageId).$($properties.PackageVersion).nupkg"
        Test-PackageArchive -Path $archivePath -PackageId $properties.PackageId -ReadmePath $readmePath -RequiredDependencies $publicDependencies
    }
}

# BenchmarkDotNet creates executable projects under bin; they are build outputs, not repository projects.
$nonPackages = @(Get-ChildItem (Join-Path $RepositoryRoot 'tests'), (Join-Path $RepositoryRoot 'performance') -Recurse -Filter '*.csproj' |
    Where-Object FullName -NotMatch '[/\\](bin|obj)[/\\]' | Sort-Object FullName)
Assert-PackageRule ($nonPackages.Count -gt 0) 'No test or benchmark projects discovered.'
foreach ($project in $nonPackages) {
    $properties = (Invoke-DotNet @('msbuild', $project.FullName, '-getProperty:IsPackable,GeneratePackageOnBuild') | ConvertFrom-Json).Properties
    Assert-PackageRule ($properties.IsPackable -eq 'false' -and $properties.GeneratePackageOnBuild -eq 'false') "Test/benchmark packaging enabled: $($project.FullName)"
}

if (-not $SkipDocsCheck) {
    $docs = Join-Path $RepositoryRoot 'docs'
    $docsProject = Join-Path $docs 'docs.csproj'
    $outputsBefore = @(Get-ChildItem $docs -Recurse -Directory -Force | Where-Object Name -In @('obj', 'bin', '.no-output'))
    Assert-PackageRule ($outputsBefore.Count -eq 0) 'Docs output directories already exist; inspect before validation.'
    foreach ($command in @('restore', 'build')) { Invoke-DotNet @($command, $docsProject, '-v:minimal') | Write-Host }
    Invoke-DotNet @('msbuild', $docsProject, '-t:Rebuild', '-v:minimal') | Write-Host
    $items = (Invoke-DotNet @('msbuild', $docsProject, '-getItem:None') | ConvertFrom-Json).Items.None
    Assert-PackageRule (@($items).Count -gt 0) 'Docs project exposes no files.'
    foreach ($item in $items) {
        Assert-PackageRule ($item.Link -eq $item.Identity -and -not [IO.Path]::IsPathRooted($item.Link)) "Docs path is not relative: $($item.Identity)"
    }
    $expected = @(Get-ChildItem $docs -Recurse -File -Force | Where-Object FullName -NE $docsProject)
    Assert-PackageRule ($expected.Count -eq @($items).Count) 'Docs recursive file inventory differs from project items.'
    $outputsAfter = @(Get-ChildItem $docs -Recurse -Directory -Force | Where-Object Name -In @('obj', 'bin', '.no-output'))
    Assert-PackageRule ($outputsAfter.Count -eq 0) 'Docs restore/build/rebuild produced output directories.'
}
Write-Host "Validated $($projects.Count) source projects and $($nonPackages.Count) non-packable test/benchmark projects."
