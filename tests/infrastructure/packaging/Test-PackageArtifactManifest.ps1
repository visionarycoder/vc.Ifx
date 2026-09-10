#Requires -Version 7.0
[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) -Parent
Import-Module (Join-Path $root 'scripts/packaging/PackageArtifactManifest.psm1') -Force
$directory = Join-Path $root "TestResults/package-artifact-tests/$([Guid]::NewGuid().ToString('N'))"
$null = New-Item -ItemType Directory -Path $directory
$script:passed = 0
$revision = 'a' * 40
$dll = [Text.Encoding]::UTF8.GetBytes('tested assembly bytes')
$pdb = [Text.Encoding]::UTF8.GetBytes('tested symbols bytes')
$payloads = @{
    'vc.Ifx.Sample.dll' = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($dll)).ToLowerInvariant()
    'vc.Ifx.Sample.pdb' = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($pdb)).ToLowerInvariant()
}
foreach ($name in @('build.json', 'coverage.json', 'global.json')) {
    '{"fixture":true}' | Set-Content -LiteralPath (Join-Path $directory $name)
}

function New-Archive([string] $Path, [string] $Id = 'vc.Ifx.Sample', [string] $Version = '1.2.3',
    [string] $Commit = $revision, [string] $PayloadName = 'vc.Ifx.Sample.dll', [byte[]] $Bytes = $dll,
    [string] $PayloadRoot = 'lib/net10.0') {
    $zip = [IO.Compression.ZipFile]::Open($Path, [IO.Compression.ZipArchiveMode]::Create)
    try {
        $entry = $zip.CreateEntry('sample.nuspec')
        $writer = [IO.StreamWriter]::new($entry.Open())
        try { $writer.Write("<package><metadata><id>$Id</id><version>$Version</version><repository commit=`"$Commit`" /></metadata></package>") }
        finally { $writer.Dispose() }
        $entry = $zip.CreateEntry("$PayloadRoot/$PayloadName")
        $stream = $entry.Open()
        try { $stream.Write($Bytes) } finally { $stream.Dispose() }
    } finally { $zip.Dispose() }
}

function New-Fixture {
    $path = Join-Path $directory ([Guid]::NewGuid().ToString('N'))
    $raw = Join-Path $path 'raw'
    $null = New-Item -ItemType Directory -Path $raw
    New-Archive (Join-Path $raw 'vc.Ifx.Sample.1.2.3.nupkg')
    New-Archive (Join-Path $raw 'vc.Ifx.Sample.1.2.3.snupkg') -PayloadName 'vc.Ifx.Sample.pdb' -Bytes $pdb
    return @{
        PackageDirectory = $raw; DestinationDirectory = (Join-Path $path 'validated')
        ExpectedPackages = @([pscustomobject]@{ id = 'vc.Ifx.Sample'; version = '1.2.3' })
        ExpectedPayloads = @{ 'vc.Ifx.Sample' = $payloads.Clone() }; Revision = $revision
        BuildManifestPath = (Join-Path $directory 'build.json'); CoverageManifestPath = (Join-Path $directory 'coverage.json')
        SdkPath = (Join-Path $directory 'global.json')
    }
}

function Test-Case([string] $Name, [scriptblock] $Body, [string] $Failure = '') {
    try { & $Body }
    catch {
        if (-not $Failure -or $_.Exception.Message -notlike "*$Failure*") { throw }
        $script:passed++
        Write-Host "PASS: $Name (rejected)"
        return
    }
    if ($Failure) { throw "Expected failure missing: $Name" }
    $script:passed++
    Write-Host "PASS: $Name"
}

function Test-Final([hashtable] $Fixture, [scriptblock] $Mutate) {
    $result = New-PackageArtifactManifest @Fixture
    & $Mutate $result.Directory
    $null = Test-PackageArtifactManifest -Directory $result.Directory -Revision $revision -ManifestSha256 $result.ManifestSha256
}

function Add-Dependency([string] $Path, [byte[]] $Bytes) {
    $zip = [IO.Compression.ZipFile]::Open($Path, [IO.Compression.ZipArchiveMode]::Update)
    try {
        $stream = $zip.CreateEntry('analyzers/dotnet/cs/Microsoft.AspNetCore.Routing.dll').Open()
        try { $stream.Write($Bytes) } finally { $stream.Dispose() }
    } finally { $zip.Dispose() }
}

Test-Case 'exact archive pair, provenance hashes and SDK survive download' {
    $fixture = New-Fixture
    $result = New-PackageArtifactManifest @fixture
    $download = Join-Path (Split-Path $result.Directory -Parent) 'download'
    Copy-Item -LiteralPath $result.Directory -Destination $download -Recurse
    $manifest = Test-PackageArtifactManifest -Directory $download -Revision $revision -ManifestSha256 $result.ManifestSha256
    if (@(Get-ChildItem $download -Force).Count -ne 4 -or $manifest.packages.Count -ne 1 -or
        $manifest.buildManifestSha256 -cne (Get-FileHash $fixture.BuildManifestPath).Hash.ToLowerInvariant()) { throw 'Incorrect manifest inventory or provenance.' }
    $fixture.DestinationDirectory = Join-Path (Split-Path $download -Parent) 'repeat'
    $repeat = New-PackageArtifactManifest @fixture
    if ($repeat.ManifestSha256 -cne $result.ManifestSha256) { throw 'Manifest is not deterministic.' }
}
Test-Case 'existing destination cannot retain stale artifacts' {
    $fixture = New-Fixture
    $null = New-PackageArtifactManifest @fixture
    $null = New-PackageArtifactManifest @fixture
} 'destination must be fresh'
Test-Case 'extra raw archive is rejected' {
    $fixture = New-Fixture
    Copy-Item (Join-Path $fixture.PackageDirectory '*.nupkg') (Join-Path $fixture.PackageDirectory 'vc.Ifx.Unvalidated.9.9.9.nupkg')
    $null = New-PackageArtifactManifest @fixture
} 'exact expected archive inventory'
Test-Case 'missing symbols is rejected' {
    $fixture = New-Fixture
    Remove-Item -LiteralPath (Join-Path $fixture.PackageDirectory 'vc.Ifx.Sample.1.2.3.snupkg')
    $null = New-PackageArtifactManifest @fixture
} 'exact expected archive inventory'
Test-Case 'stale version substituted for expected archive is rejected' {
    $fixture = New-Fixture
    Rename-Item -LiteralPath (Join-Path $fixture.PackageDirectory 'vc.Ifx.Sample.1.2.3.nupkg') -NewName 'vc.Ifx.Sample.1.2.2.nupkg'
    $null = New-PackageArtifactManifest @fixture
} 'Unexpected package directory entry'
Test-Case 'directory masquerading as archive is rejected' {
    $fixture = New-Fixture
    $path = Join-Path $fixture.PackageDirectory 'vc.Ifx.Sample.1.2.3.nupkg'
    Remove-Item -LiteralPath $path
    $null = New-Item -ItemType Directory -Path $path
    $null = New-PackageArtifactManifest @fixture
} 'Unexpected package directory entry'
Test-Case 'duplicate expected package is rejected' {
    $fixture = New-Fixture
    $fixture.ExpectedPackages += $fixture.ExpectedPackages[0]
    $null = New-PackageArtifactManifest @fixture
} 'Duplicate expected package ID'
foreach ($case in @(
    @{ Name = 'archive ID'; Arguments = @{ Id = 'vc.Ifx.Other' }; Failure = 'Archive ID mismatch' },
    @{ Name = 'archive version'; Arguments = @{ Version = '1.2.2' }; Failure = 'Archive version mismatch' },
    @{ Name = 'archive revision'; Arguments = @{ Commit = ('b' * 40) }; Failure = 'Archive revision mismatch' },
    @{ Name = 'changed untested assembly'; Arguments = @{ Bytes = ([byte[]]@(1, 2, 3)) }; Failure = 'payload differs from tested build' },
    @{ Name = 'unattested assembly'; Arguments = @{ PayloadName = 'Unknown.dll' }; Failure = 'unattested payload' },
    @{ Name = 'unsupported binary root'; Arguments = @{ PayloadRoot = 'contentFiles' }; Failure = 'Unsupported archive payload root' },
    @{ Name = 'path traversal'; Arguments = @{ PayloadName = '../vc.Ifx.Sample.dll' }; Failure = 'Invalid archive payload path' },
    @{ Name = 'redundant path segment'; Arguments = @{ PayloadName = './vc.Ifx.Sample.dll' }; Failure = 'Invalid archive payload path' },
    @{ Name = 'empty path segment'; Arguments = @{ PayloadName = '/vc.Ifx.Sample.dll' }; Failure = 'Invalid archive payload path' },
    @{ Name = 'backslash path alias'; Arguments = @{ PayloadName = 'de\vc.Ifx.Sample.dll' }; Failure = 'Unsupported archive payload root' },
    @{ Name = 'case-folded path alias'; Arguments = @{ PayloadName = 'vc.Ifx.SAMPLE.dll' }; Failure = 'unattested payload' }
)) {
    Test-Case "$($case.Name) is rejected" {
        $fixture = New-Fixture
        $path = Join-Path $fixture.PackageDirectory 'vc.Ifx.Sample.1.2.3.nupkg'
        Remove-Item -LiteralPath $path
        $arguments = $case.Arguments
        New-Archive -Path $path @arguments
        $null = New-PackageArtifactManifest @fixture
    } $case.Failure
}
Test-Case 'extra downloaded archive cannot be published' {
    Test-Final (New-Fixture) { param($path) 'decoy' | Set-Content (Join-Path $path 'vc.Ifx.Decoy.1.0.0.nupkg') }
} 'Artifact file inventory mismatch'
Test-Case 'changed downloaded package cannot be published' {
    Test-Final (New-Fixture) { param($path) 'tamper' | Add-Content (Join-Path $path 'vc.Ifx.Sample.1.2.3.nupkg') }
} 'Artifact hash mismatch'
Test-Case 'changed SDK cannot select a different publishing toolchain' {
    Test-Final (New-Fixture) { param($path) '{}' | Set-Content (Join-Path $path 'global.json') }
} 'Artifact hash mismatch'
Test-Case 'changed manifest cannot authorize a different inventory' {
    Test-Final (New-Fixture) { param($path) ' ' | Add-Content (Join-Path $path 'package-manifest.json') }
} 'Package manifest hash mismatch'
Test-Case 'different run revision cannot publish' {
    $fixture = New-Fixture
    $result = New-PackageArtifactManifest @fixture
    $null = Test-PackageArtifactManifest -Directory $result.Directory -Revision ('b' * 40) -ManifestSha256 $result.ManifestSha256
} 'Invalid package manifest provenance'
Test-Case 'captured private dependency closure remains eligible' {
    $fixture = New-Fixture
    $bytes = [Text.Encoding]::UTF8.GetBytes('captured routing dependency')
    $fixture.ExpectedPayloads['vc.Ifx.Sample']['Microsoft.AspNetCore.Routing.dll'] = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
    Add-Dependency (Join-Path $fixture.PackageDirectory 'vc.Ifx.Sample.1.2.3.nupkg') $bytes
    $null = New-PackageArtifactManifest @fixture
}
Test-Case 'dependency captured for another package cannot authorize these bytes' {
    $fixture = New-Fixture
    $bytes = [Text.Encoding]::UTF8.GetBytes('other package dependency version')
    $fixture.ExpectedPayloads['vc.Ifx.Other'] = @{ 'Microsoft.AspNetCore.Routing.dll' = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() }
    Add-Dependency (Join-Path $fixture.PackageDirectory 'vc.Ifx.Sample.1.2.3.nupkg') $bytes
    $null = New-PackageArtifactManifest @fixture
} 'unattested payload'
Test-Case 'dependency version mismatch is not hidden by same basename' {
    $fixture = New-Fixture
    $fixture.ExpectedPayloads['vc.Ifx.Sample']['Microsoft.AspNetCore.Routing.dll'] = $payloads['vc.Ifx.Sample.dll']
    Add-Dependency (Join-Path $fixture.PackageDirectory 'vc.Ifx.Sample.1.2.3.nupkg') ([byte[]]@(2, 3, 4))
    $null = New-PackageArtifactManifest @fixture
} 'payload differs from tested build'

# Execute the actual inline publish verifier; no downloaded repository script is trusted by that job.
$workflow = Get-Content -LiteralPath (Join-Path $root '.github/workflows/publish.yml') -Raw
$match = [regex]::Match($workflow, '(?ms)^          function Test-PackageArtifactManifest \{.*?^          \}')
if (-not $match.Success) { throw 'Missing inline publish verifier.' }
$inline = [regex]::Replace($match.Value, '(?m)^          ', '').Replace("`r`n", "`n")
$tokens = $null
$errors = $null
$moduleAst = [Management.Automation.Language.Parser]::ParseFile((Join-Path $root 'scripts/packaging/PackageArtifactManifest.psm1'), [ref] $tokens, [ref] $errors)
if ($errors.Count) { throw 'Manifest module failed parsing.' }
$moduleFunction = $moduleAst.Find({ param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq 'Test-PackageArtifactManifest' }, $true)
Test-Case 'publish verifier is exactly the locally tested function' {
    if ($inline -cne $moduleFunction.Extent.Text.Replace("`r`n", "`n")) { throw 'Inline publish verifier drifted from tested module.' }
}
$publishVerifier = [scriptblock]::Create($inline + "`nTest-PackageArtifactManifest @verification")
Test-Case 'actual inline publish verifier accepts an exact downloaded inventory' {
    $fixture = New-Fixture
    $result = New-PackageArtifactManifest @fixture
    $verification = @{ Directory = $result.Directory; Revision = $revision; ManifestSha256 = $result.ManifestSha256 }
    $null = & $publishVerifier
}
Test-Case 'actual inline publish verifier rejects a substituted archive before publishing' {
    $fixture = New-Fixture
    $result = New-PackageArtifactManifest @fixture
    'unvalidated' | Set-Content (Join-Path $result.Directory 'vc.Ifx.Decoy.1.0.0.nupkg')
    $verification = @{ Directory = $result.Directory; Revision = $revision; ManifestSha256 = $result.ManifestSha256 }
    $null = & $publishVerifier
} 'Artifact file inventory mismatch'

$publishBodies = [Collections.Generic.List[string]]::new()
$bodyLines = $null
foreach ($line in $workflow.Split("`n")) {
    $line = $line.TrimEnd("`r")
    if ($null -ne $bodyLines -and ($line.StartsWith('          ', [StringComparison]::Ordinal) -or $line.Length -eq 0)) {
        $bodyLines.Add($(if ($line.Length) { $line.Substring(10) } else { '' }))
        continue
    }
    if ($null -ne $bodyLines) {
        $body = [string]::Join("`n", $bodyLines)
        if ($body.Contains('dotnet nuget push', [StringComparison]::Ordinal)) { $publishBodies.Add($body) }
        $bodyLines = $null
    }
    if ($line -ceq '        run: |') { $bodyLines = [Collections.Generic.List[string]]::new() }
}
if ($null -ne $bodyLines) {
    $body = [string]::Join("`n", $bodyLines)
    if ($body.Contains('dotnet nuget push', [StringComparison]::Ordinal)) { $publishBodies.Add($body) }
}
if ($publishBodies.Count -ne 2) { throw 'Expected both real publishing scripts.' }
foreach ($publishBody in $publishBodies) {
    Test-Case 'actual publishing script uses only manifest package paths (network stubbed)' {
        $fixture = New-Fixture
        $result = New-PackageArtifactManifest @fixture
        'decoy' | Set-Content (Join-Path $result.Directory 'vc.Ifx.Decoy.1.0.0.nupkg')
        $script:pushed = [Collections.Generic.List[string]]::new()
        function dotnet {
            if ($args[0] -cne 'nuget' -or $args[1] -cne 'push') { throw 'Unexpected native command.' }
            $script:pushed.Add($args[2])
            $global:LASTEXITCODE = 0
        }
        $previousKey = $env:NUGET_API_KEY
        try {
            $env:NUGET_API_KEY = 'local-fixture-not-a-secret'
            Push-Location $result.Directory
            try { & ([scriptblock]::Create($publishBody)) } finally { Pop-Location }
        } finally { $env:NUGET_API_KEY = $previousKey }
        if ($script:pushed.Count -ne 1 -or $script:pushed[0] -cne 'vc.Ifx.Sample.1.2.3.nupkg') { throw 'Publish inventory differs from validated manifest.' }
    }
}
Write-Host "$script:passed package artifact manifest regressions passed. Evidence: $directory"
$global:LASTEXITCODE = 0
