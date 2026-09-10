#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $BuildDirectory,
    [Parameter(Mandatory)][string] $CoverageRunDirectory,
    [Parameter(Mandatory)][ValidatePattern('\A[0-9a-f]{40}\z')][string] $Revision,
    [ValidateRange(1, 3600)][int] $LockTimeoutSeconds = 1200
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
Import-Module (Join-Path $root 'scripts/testing/BuildProvenance.psm1') -Force
Import-Module (Join-Path $root 'scripts/testing/TestSourceSelection.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'PackageArtifactManifest.psm1') -Force
$BuildDirectory = [IO.Path]::GetFullPath($BuildDirectory)
$CoverageRunDirectory = [IO.Path]::GetFullPath($CoverageRunDirectory)
$identity = if ($IsWindows) { $root.ToUpperInvariant() } else { $root }
$digest = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($identity)))
$mutex = [Threading.Mutex]::new($false, "vc.Ifx.FrameworkTests.$digest")
$acquired = $false
try {
    Write-Host 'Waiting for exclusive framework package/build access...'
    try { $acquired = $mutex.WaitOne([TimeSpan]::FromSeconds($LockTimeoutSeconds)) }
    catch [Threading.AbandonedMutexException] {
        $acquired = $true
        throw 'Previous worker abandoned the build/test mutex. Capture and test a fresh build before packaging.'
    }
    if (-not $acquired) { throw 'Timed out waiting for framework build/test access.' }
    $projects = @(Get-ChildItem -LiteralPath (Join-Path $root 'src') -Recurse -Filter '*.csproj' -File |
        Where-Object FullName -NotMatch '[/\\](bin|obj)[/\\]' | Sort-Object FullName)
    $tests = @(Get-ChildItem -LiteralPath (Join-Path $root 'tests') -Recurse -Filter '*.csproj' -File |
        Where-Object FullName -NotMatch '[/\\](bin|obj)[/\\]')
    if ($projects.Count -eq 0 -or $tests.Count -ne 2) { throw 'Expected source packages and both centralized test projects.' }
    $selection = Resolve-IfxTestSelection -ProjectPath $tests[0].FullName
    $assertion = @{ BuildDirectory = $BuildDirectory; CoverageRunDirectory = $CoverageRunDirectory
        Configuration = 'Release'; RequiredProject = @($projects.FullName) + @($tests.FullName); Selection = $selection }
    $build = Assert-IfxBuildProvenance @assertion
    if ($build.manifest.revision -cne $Revision -or $build.manifest.repositoryRoot -cne $root) { throw 'Package build revision or repository mismatch.' }
    $contextPath = Join-Path $CoverageRunDirectory 'run-context.json'
    $contextHash = (Get-FileHash -LiteralPath $contextPath).Hash
    $context = Get-Content -LiteralPath $contextPath -Raw | ConvertFrom-Json
    Assert-IfxTestSelectionContext -Context $context -RequireFull
    if (-not $context.full -or $context.filter -or $context.testSourceScope -or $context.testPackage -or
        $context.configuration -cne 'Release' -or $context.revision -cne $Revision) { throw 'Publishing requires full Release coverage of this revision.' }
    $summaryPath = Join-Path $CoverageRunDirectory 'summary.json'
    $summaryHash = (Get-FileHash -LiteralPath $summaryPath).Hash
    $summary = Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json
    if (-not $summary.passed -or $summary.threshold -ne 100 -or $summary.packages.Count -ne $projects.Count -or
        @(Compare-Object @($projects.BaseName | Sort-Object) @($summary.packages.package | Sort-Object) -CaseSensitive).Count -or
        @($summary.packages | Where-Object { $_.status -cne 'Passed' -or $_.linesTotal -le 0 -or
            $_.linesCovered -ne $_.linesTotal -or $_.branchesCovered -ne $_.branchesTotal }).Count) { throw 'Exact full-package coverage gate has not passed.' }
    $payloads = @{}
    foreach ($project in $projects) {
        $record = @($build.manifest.projects | Where-Object projectPath -CEQ $project.FullName)
        if ($record.Count -ne 1) { throw "Missing or duplicate tested package project: $($project.Name)" }
        $packagePayloads = [hashtable]::new([StringComparer]::Ordinal)
        foreach ($output in @($record[0].outputs | Where-Object path -Match '\.(dll|pdb)$')) {
            # Culture satellites share basenames; retain their location under the captured output root.
            $relative = [IO.Path]::GetRelativePath($record[0].settings.TargetDir, $output.path).Replace('\', '/')
            if ([IO.Path]::IsPathRooted($relative) -or $relative.Split('/') -contains '..') {
                throw "Tested payload is outside its package output directory: $($output.path)"
            }
            if ($packagePayloads.ContainsKey($relative) -and $packagePayloads[$relative] -cne $output.sha256) {
                throw "Conflicting tested payload identities in $($project.BaseName): $relative"
            }
            $packagePayloads[$relative] = $output.sha256
        }
        foreach ($path in @($record[0].targetPath, [IO.Path]::ChangeExtension($record[0].targetPath, '.pdb'))) {
            if ($path -cnotin $record[0].outputs.path) { throw "Missing tested primary package payload: $path" }
        }
        $payloads[$project.BaseName] = $packagePayloads
    }
    $run = Join-Path $root "TestResults/package-artifacts/$([Guid]::NewGuid().ToString('N'))"
    $raw = Join-Path $run 'raw'
    $null = New-Item -ItemType Directory -Path $raw
    foreach ($project in $projects) {
        # Compiler-package reference resolution must not build referenced projects either.
        $arguments = @('pack', $project.FullName, '--no-build', '--no-restore', '-c', 'Release', '-o', $raw,
            '-m:1', '-p:BuildInParallel=false', '-p:BuildProjectReferences=false', '-p:GeneratePackageOnBuild=false', '-v:minimal')
        & dotnet @arguments
        if ($LASTEXITCODE -ne 0) { throw "No-build packaging failed: $($project.Name)" }
    }
    $validated = @(& (Join-Path $PSScriptRoot 'Validate-Packages.ps1') -RepositoryRoot $root -PackageDirectory $raw)
    if ($validated.Count -ne $projects.Count -or
        @(Compare-Object @($projects.BaseName | Sort-Object) @($validated.PackageId | Sort-Object) -CaseSensitive).Count) { throw 'Validator did not attest every expected package.' }
    $expected = @(foreach ($package in $validated) {
        $archive = [IO.Compression.ZipFile]::OpenRead($package.Archive)
        try {
            $spec = @($archive.Entries | Where-Object FullName -CLike '*.nuspec')
            if ($spec.Count -ne 1) { throw 'Validated archive must contain one nuspec.' }
            $reader = [IO.StreamReader]::new($spec[0].Open())
            try { [xml] $document = $reader.ReadToEnd() } finally { $reader.Dispose() }
            $version = $document.SelectSingleNode('/*[local-name()="package"]/*[local-name()="metadata"]/*[local-name()="version"]').InnerText
            [pscustomobject]@{ id = $package.PackageId; version = $version }
        } finally { $archive.Dispose() }
    })
    $after = Assert-IfxBuildProvenance @assertion
    if ($after.sha256 -cne $build.sha256) { throw 'Tested build identity changed during packaging.' }
    if ((Get-FileHash -LiteralPath $summaryPath).Hash -cne $summaryHash -or
        (Get-FileHash -LiteralPath $contextPath).Hash -cne $contextHash) { throw 'Coverage evidence changed during packaging.' }
    $result = New-PackageArtifactManifest -PackageDirectory $raw -DestinationDirectory (Join-Path $run 'validated') `
        -ExpectedPackages $expected -ExpectedPayloads $payloads -Revision $Revision -BuildManifestPath $build.path `
        -CoverageManifestPath $summaryPath -SdkPath (Join-Path $root 'global.json')
    if ($env:GITHUB_ENV) {
        @("IFX_PACKAGE_DIRECTORY=$raw", "IFX_PACKAGE_UPLOAD_DIRECTORY=$($result.Directory)") |
            Add-Content -LiteralPath $env:GITHUB_ENV -Encoding utf8
    }
    if ($env:GITHUB_OUTPUT) { "manifest-sha256=$($result.ManifestSha256)" | Add-Content -LiteralPath $env:GITHUB_OUTPUT -Encoding utf8 }
    Write-Host "Validated $($result.PackageCount) tested archive pairs: $($result.Directory)"
    $result
}
finally {
    if ($acquired) { $mutex.ReleaseMutex() }
    $mutex.Dispose()
}
