#Requires -Version 7.0
[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$evidence = Join-Path $repository "TestResults/package-wrapper-tests/$([Guid]::NewGuid().ToString('N'))"
$revision = 'a' * 40
$passed = 0

# Only native Git/MSBuild/pack and the independently owned archive-layout validator are stubbed.
# The actual wrapper, F2 file/provenance assertion, selection and artifact modules execute unchanged.
function global:git { $global:LASTEXITCODE = 0; return ('a' * 40) }
function global:dotnet {
    $global:LASTEXITCODE = 0
    if ($args[0] -ceq 'msbuild') {
        return (@{ Items = @{ IfxBuildSetting = @(); Compile = @(@{ FullPath = $global:packFixture.Source }); IfxDeclaredProjectReference = @(); EmbeddedResource = @(); Protobuf = @() } } | ConvertTo-Json -Depth 5)
    }
    if ($args[0] -cne 'pack') { throw 'Unexpected native command in isolated package test.' }
    foreach ($flag in @('--no-build', '--no-restore', '-m:1', '-p:BuildInParallel=false', '-p:BuildProjectReferences=false', '-p:GeneratePackageOnBuild=false')) {
        if ($flag -cnotin $args) { throw "Unsafe pack arguments: missing $flag" }
    }
    $global:packFixture.PackCalls++
    $destination = $args[[Array]::IndexOf($args, '-o') + 1]
    Copy-Item -Path (Join-Path $global:packFixture.Templates '*') -Destination $destination
    if ($global:packFixture.Mode -ceq 'pack-failure') { $global:LASTEXITCODE = 7 }
    if ($global:packFixture.Mode -ceq 'source-change') { 'changed after test' | Add-Content -LiteralPath $global:packFixture.Source }
    if ($global:packFixture.Mode -ceq 'extra-archive') { 'decoy' | Set-Content -LiteralPath (Join-Path $destination 'vc.Ifx.Decoy.1.0.0.nupkg') }
    if ($global:packFixture.Mode -ceq 'coverage-change') { ' ' | Add-Content -LiteralPath (Join-Path $global:packFixture.Coverage 'summary.json') }
}

function New-WrapperFixture([string] $Mode) {
    $root = Join-Path $evidence $Mode
    foreach ($directory in @('scripts/packaging', 'scripts/testing', 'src/vc.Ifx.Sample', 'tests/unit', 'tests/integration', 'build', 'coverage', 'outputs', 'templates')) {
        $null = New-Item -ItemType Directory -Path (Join-Path $root $directory) -Force
    }
    foreach ($file in @('scripts/packaging/Invoke-ValidatedPackageArtifacts.ps1', 'scripts/packaging/PackageArtifactManifest.psm1',
        'scripts/testing/BuildProvenance.psm1', 'scripts/testing/TestSourceSelection.psm1')) {
        Copy-Item -LiteralPath (Join-Path $repository $file) -Destination (Join-Path $root $file)
    }
    @'
param($RepositoryRoot, $PackageDirectory)
[pscustomobject]@{ PackageId = 'vc.Ifx.Sample'; Archive = (Join-Path $PackageDirectory 'vc.Ifx.Sample.1.2.3.nupkg') }
'@ | Set-Content -LiteralPath (Join-Path $root 'scripts/packaging/Validate-Packages.ps1')
    $source = Join-Path $root 'Fixture.cs'
    'class Fixture {}' | Set-Content -LiteralPath $source
    '{"sdk":{"version":"10.0.401"}}' | Set-Content -LiteralPath (Join-Path $root 'global.json')
    $outputDirectory = Join-Path $root 'outputs'
    $satellites = @(foreach ($culture in @('cs', 'de', 'ja')) {
        "$culture/Microsoft.CodeAnalysis.resources.dll"
        "$culture/Microsoft.CodeAnalysis.CSharp.resources.dll"
    })
    $outputs = @(foreach ($name in @('vc.Ifx.Sample.dll', 'vc.Ifx.Sample.pdb', 'PrivateDependency1.pdb') + @(1..24 | ForEach-Object { "PrivateDependency$_.dll" }) + $satellites) {
        $path = Join-Path $outputDirectory $name
        $null = New-Item -ItemType Directory -Path (Split-Path $path -Parent) -Force
        [IO.File]::WriteAllBytes($path, [Text.Encoding]::UTF8.GetBytes("tested $name"))
        @{ path = $path; sha256 = (Get-FileHash -LiteralPath $path).Hash.ToLowerInvariant() }
    })
    $projects = @(foreach ($name in @('src/vc.Ifx.Sample/vc.Ifx.Sample.csproj', 'tests/unit/vc.Ifx.UnitTests.csproj', 'tests/integration/vc.Ifx.IntegrationTests.csproj')) {
        $path = Join-Path $root $name
        '<Project />' | Set-Content -LiteralPath $path
        @{ name = [IO.Path]::GetFileNameWithoutExtension($path); projectPath = $path; targetPath = (Join-Path $outputDirectory 'vc.Ifx.Sample.dll')
            inputs = @(@{ path = $source; sha256 = (Get-FileHash -LiteralPath $source).Hash.ToLowerInvariant() }); outputs = $outputs
            settings = @{ TargetDir = $outputDirectory; DebugType = 'portable' }; absentInputs = @(); compilePaths = @($source); nonCodeInputPaths = @(); projectReferences = @() }
    })
    $selection = @{ Version = 1; SourceScope = ''; Scopes = @(); TestPackage = ''; OutputKey = ''; Fingerprint = ''; Arguments = @() }
    $build = Join-Path $root 'build'
    $identityPath = Join-Path $build 'build-identity.json'
    @{ schemaVersion = 1; repositoryRoot = $root; revision = $revision; configuration = 'Release'; testSelection = $selection; projects = $projects } |
        ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $identityPath
    $hash = (Get-FileHash -LiteralPath $identityPath).Hash.ToLowerInvariant()
    @{ buildIdentity = @{ sha256 = $hash } } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $build 'build-complete.json')
    $coverage = Join-Path $root 'coverage'
    Copy-Item -LiteralPath $identityPath -Destination (Join-Path $coverage 'consumed-build-identity.json')
    @{ sha256 = $hash; restoredOutputsVerified = $true } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $coverage 'build-identity-verified.json')
    @{ full = $true; noBuild = $true; filter = ''; testSourceScope = ''; testPackage = ''; testSelection = $selection
        configuration = 'Release'; revision = $revision; buildIdentity = @{ sha256 = $hash } } |
        ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $coverage 'run-context.json')
    @{ passed = $true; threshold = 100; packages = @(@{ package = 'vc.Ifx.Sample'; status = 'Passed'; linesTotal = 1; linesCovered = 1; branchesTotal = 0; branchesCovered = 0 }) } |
        ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $coverage 'summary.json')
    foreach ($extension in @('nupkg', 'snupkg')) {
        $zip = [IO.Compression.ZipFile]::Open((Join-Path $root "templates/vc.Ifx.Sample.1.2.3.$extension"), [IO.Compression.ZipArchiveMode]::Create)
        try {
            $writer = [IO.StreamWriter]::new($zip.CreateEntry('vc.Ifx.Sample.nuspec').Open())
            try { $writer.Write("<package><metadata><id>vc.Ifx.Sample</id><version>1.2.3</version><repository commit=`"$revision`" /></metadata></package>") } finally { $writer.Dispose() }
            foreach ($output in $outputs) {
                if (($extension -ceq 'snupkg') -ne $output.path.EndsWith('.pdb', [StringComparison]::Ordinal)) { continue }
                $relative = [IO.Path]::GetRelativePath($outputDirectory, $output.path).Replace('\', '/')
                if ($relative.Contains('/') -and $Mode -notin @('packaged-satellites', 'swapped-satellite', 'flattened-satellite')) { continue }
                $bytes = [IO.File]::ReadAllBytes($output.path)
                if ($Mode -ceq 'swapped-satellite' -and $relative -ceq 'de/Microsoft.CodeAnalysis.resources.dll') {
                    $bytes = [IO.File]::ReadAllBytes((Join-Path $outputDirectory 'cs/Microsoft.CodeAnalysis.resources.dll'))
                }
                if ($Mode -ceq 'flattened-satellite' -and $relative -ceq 'cs/Microsoft.CodeAnalysis.resources.dll') { $relative = 'Microsoft.CodeAnalysis.resources.dll' }
                if (($Mode -ceq 'changed-private-dll' -and $relative -ceq 'PrivateDependency1.dll') -or
                    ($Mode -ceq 'changed-private-pdb' -and $relative -ceq 'PrivateDependency1.pdb')) { $bytes = [Text.Encoding]::UTF8.GetBytes('not the tested payload') }
                $stream = $zip.CreateEntry("analyzers/dotnet/cs/$relative").Open()
                try { $stream.Write($bytes) } finally { $stream.Dispose() }
            }
        } finally { $zip.Dispose() }
    }
    return @{ Root = $root; Source = $source; Templates = (Join-Path $root 'templates'); PackCalls = 0; Mode = $Mode; Build = $build; Coverage = $coverage }
}

$previousEnvironment = @{}
foreach ($key in @('GITHUB_ENV', 'GITHUB_OUTPUT')) { $previousEnvironment[$key] = [Environment]::GetEnvironmentVariable($key) }
try {
    foreach ($case in @(
        @{ Mode = 'success'; Error = '' },
        @{ Mode = 'packaged-satellites'; Error = '' },
        @{ Mode = 'swapped-satellite'; Error = 'Archive payload differs from tested build: de/Microsoft.CodeAnalysis.resources.dll' },
        @{ Mode = 'flattened-satellite'; Error = 'Archive contains an unattested payload: Microsoft.CodeAnalysis.resources.dll' },
        @{ Mode = 'changed-private-dll'; Error = 'Archive payload differs from tested build: PrivateDependency1.dll' },
        @{ Mode = 'changed-private-pdb'; Error = 'Archive payload differs from tested build: PrivateDependency1.pdb' },
        @{ Mode = 'stale-source'; Error = 'Changed build input/output' },
        @{ Mode = 'unpaired-coverage'; Error = 'Coverage is not bound' },
        @{ Mode = 'pack-failure'; Error = 'No-build packaging failed' },
        @{ Mode = 'source-change'; Error = 'Changed build input/output' },
        @{ Mode = 'extra-archive'; Error = 'exact expected archive inventory' },
        @{ Mode = 'coverage-change'; Error = 'Coverage evidence changed during packaging' }
    )) {
        $global:packFixture = New-WrapperFixture $case.Mode
        $env:GITHUB_ENV = Join-Path $global:packFixture.Root 'github-env.txt'
        $env:GITHUB_OUTPUT = Join-Path $global:packFixture.Root 'github-output.txt'
        if ($case.Mode -ceq 'stale-source') { 'changed before pack' | Add-Content -LiteralPath $global:packFixture.Source }
        if ($case.Mode -ceq 'unpaired-coverage') { '{}' | Set-Content -LiteralPath (Join-Path $global:packFixture.Coverage 'consumed-build-identity.json') }
        $failure = ''
        try {
            $result = & (Join-Path $global:packFixture.Root 'scripts/packaging/Invoke-ValidatedPackageArtifacts.ps1') `
                -BuildDirectory $global:packFixture.Build -CoverageRunDirectory $global:packFixture.Coverage -Revision $revision
        } catch { $failure = $_.Exception.Message }
        if ($case.Error) {
            if (-not $failure.Contains($case.Error, [StringComparison]::Ordinal)) { throw "$($case.Mode): expected $($case.Error), received $failure" }
            if ((Test-Path -LiteralPath $env:GITHUB_ENV) -or (Test-Path -LiteralPath $env:GITHUB_OUTPUT)) { throw 'Failed wrapper exported publishable artifacts.' }
            if ($case.Mode -in @('stale-source', 'unpaired-coverage') -and $global:packFixture.PackCalls -ne 0) { throw 'Wrapper packed before validating tested provenance.' }
        }
        else {
            if ($failure) { throw $failure }
            if ($result.PackageCount -ne 1 -or $global:packFixture.PackCalls -ne 1 -or @(Get-ChildItem $result.Directory).Count -ne 4) { throw 'Incorrect wrapper result.' }
            if ((Get-Content -LiteralPath $env:GITHUB_OUTPUT -Raw).Trim() -cne "manifest-sha256=$($result.ManifestSha256)") { throw 'Missing exact trusted manifest output.' }
        }
        $passed++
        Write-Host "PASS wrapper: $($case.Mode)"
    }
} finally {
    foreach ($key in $previousEnvironment.Keys) { [Environment]::SetEnvironmentVariable($key, $previousEnvironment[$key]) }
}
Write-Host "$passed package wrapper regressions passed, including 24 private dependencies, private symbols and culture satellite collisions. Native tools/layout validator stubbed; real F2 assertions/modules. Evidence: $evidence"
$global:LASTEXITCODE = 0
