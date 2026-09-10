#requires -Version 7.0
[CmdletBinding()]
param(
    [string] $RepositoryRoot = (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent),
    [string] $PackageDirectory,
    [string[]] $PackageId = @('vc.Ifx.Analyzers', 'vc.Ifx.Generators', 'vc.Ifx.CodeFixes', 'vc.Ifx.Roslyn', 'vc.Ifx.Filtering')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'PackageValidation.psm1') -Force
$packages = if ($PackageDirectory) { [IO.Path]::GetFullPath($PackageDirectory) } else { Join-Path $RepositoryRoot 'artifacts/packaging' }
$fixtureRoot = Join-Path $packages ('validator-tests-' + [guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($fixtureRoot) | Out-Null
$passed = 0

foreach ($id in $PackageId) {
    $archive = Get-ChildItem $packages -Filter "$id.*.nupkg" | Where-Object Name -Match ('^' + [regex]::Escape($id) + '\.\d') | Select-Object -First 1
    Assert-PackageRule ($null -ne $archive) "First pack $id into $packages."
    Test-PackageArchive -Path $archive.FullName -PackageId $id -ReadmePath (Join-Path $RepositoryRoot "src/$id/README.md") | Out-Null
    $passed++
}

function Edit-Archive {
    param([string] $Path, [string] $Remove, [string] $Add, [string] $Text, [scriptblock] $EditNuspec)
    $archive = [IO.Compression.ZipFile]::Open($Path, [IO.Compression.ZipArchiveMode]::Update)
    try {
        if ($Remove) { $archive.GetEntry($Remove).Delete() }
        if ($Add) {
            $writer = [IO.StreamWriter]::new($archive.CreateEntry($Add).Open())
            try { $writer.Write($Text) } finally { $writer.Dispose() }
        }
        if ($EditNuspec) {
            $entry = $archive.GetEntry('vc.Ifx.Analyzers.nuspec')
            $reader = [IO.StreamReader]::new($entry.Open())
            try { [xml] $document = $reader.ReadToEnd() } finally { $reader.Dispose() }
            & $EditNuspec $document
            $entry.Delete()
            $writer = [IO.StreamWriter]::new($archive.CreateEntry('vc.Ifx.Analyzers.nuspec').Open())
            try { $document.Save($writer) } finally { $writer.Dispose() }
        }
    } finally { $archive.Dispose() }
}

$cases = @(
    @{ Name='duplicate-codefix-analyzer'; PackageId='vc.Ifx.CodeFixes'; Expected='Code fixes bundle vc.Ifx.Analyzers'; Add='analyzers/dotnet/cs/vc.Ifx.Analyzers.dll'; Text='unexpected analyzer fixture' },
    @{ Name='missing-readme'; Expected='Missing archive entry: README.md'; Remove='README.md' },
    @{ Name='missing-private-dependency'; Expected='missing vc.Ifx.Roslyn'; Remove='analyzers/dotnet/cs/vc.Ifx.Roslyn.dll' },
    @{ Name='missing-analyzer'; Expected='not in analyzers/dotnet/cs'; Remove='analyzers/dotnet/cs/vc.Ifx.Analyzers.dll' },
    @{ Name='runtime-asset'; Expected='runtime or reference assets'; Add='lib/netstandard2.0/leaked.dll'; Text='fixture' },
    @{ Name='host-assembly'; Expected='bundles host Roslyn'; Add='analyzers/dotnet/cs/Microsoft.CodeAnalysis.dll'; Text='fixture' },
    @{ Name='test-assembly'; Expected='output leaked'; Add='analyzers/dotnet/cs/vc.Ifx.UnitTests.dll'; Text='fixture' },
    @{ Name='wrong-readme'; Expected='README differs'; Remove='README.md'; Add='README.md'; Text='wrong README' },
    @{ Name='missing-symbols'; Expected='Missing snupkg'; NoSymbols=$true },
    @{ Name='missing-tags'; Expected='Missing nuspec tags'; EditNuspec={ param($doc) $node=$doc.SelectSingleNode('//*[local-name()="tags"]'); $node.ParentNode.RemoveChild($node) | Out-Null } },
    @{ Name='missing-commit'; Expected='Missing repository commit'; EditNuspec={ param($doc) $doc.SelectSingleNode('//*[local-name()="repository"]').RemoveAttribute('commit') } },
    @{ Name='mismatched-commit'; Expected='Source Link commit differs'; EditNuspec={ param($doc) $doc.SelectSingleNode('//*[local-name()="repository"]').SetAttribute('commit', ('0' * 40)) } },
    @{ Name='prerelease-dependency'; Expected='Stable package exposes a prerelease'; EditNuspec={
        param($doc)
        $metadata=$doc.SelectSingleNode('//*[local-name()="metadata"]')
        $dependencies=$doc.CreateElement('dependencies', $metadata.NamespaceURI)
        $dependency=$doc.CreateElement('dependency', $metadata.NamespaceURI)
        $dependency.SetAttribute('id', 'Example.Dependency')
        $dependency.SetAttribute('version', '1.0.0-preview.1')
        $dependencies.AppendChild($dependency) | Out-Null
        $metadata.AppendChild($dependencies) | Out-Null
    } },
    @{ Name='aggregator-leak'; Expected='Aggregator dependency leaked'; EditNuspec={
        param($doc)
        $metadata=$doc.SelectSingleNode('//*[local-name()="metadata"]')
        $dependencies=$doc.CreateElement('dependencies', $metadata.NamespaceURI)
        $dependency=$doc.CreateElement('dependency', $metadata.NamespaceURI)
        $dependency.SetAttribute('id', 'vc.Ifx')
        $dependency.SetAttribute('version', '1.0.0')
        $dependencies.AppendChild($dependency) | Out-Null
        $metadata.AppendChild($dependencies) | Out-Null
    } }
)
foreach ($case in $cases) {
    $id = if ($case.ContainsKey('PackageId')) { $case['PackageId'] } else { 'vc.Ifx.Analyzers' }
    $source = Get-ChildItem $packages -Filter "$id.*.nupkg" | Where-Object Name -Match ('^' + [regex]::Escape($id) + '\.\d') | Select-Object -First 1
    Assert-PackageRule ($null -ne $source) "First pack $id into $packages."
    $path = Join-Path $fixtureRoot ($case['Name'] + '.nupkg')
    Copy-Item -LiteralPath $source.FullName -Destination $path
    if (-not $case.ContainsKey('NoSymbols')) {
        Copy-Item -LiteralPath ([IO.Path]::ChangeExtension($source.FullName, '.snupkg')) -Destination ([IO.Path]::ChangeExtension($path, '.snupkg'))
    }
    Edit-Archive -Path $path -Remove $case['Remove'] -Add $case['Add'] -Text $case['Text'] -EditNuspec $case['EditNuspec']
    $failure = $null
    try { Test-PackageArchive -Path $path -PackageId $id -ReadmePath (Join-Path $RepositoryRoot "src/$id/README.md") | Out-Null }
    catch { $failure = $_.Exception.Message }
    Assert-PackageRule ($null -ne $failure -and $failure -match [regex]::Escape($case['Expected'])) "Case $($case['Name']) did not fail as expected: $failure"
    $passed++
}
@{ passed = $passed; realPackages = $PackageId; rejectedMutations = @($cases | ForEach-Object { $_['Name'] }) } | ConvertTo-Json -Depth 4 | Set-Content (Join-Path $fixtureRoot 'summary.json') -Encoding utf8
Write-Host "$passed validator checks passed ($($PackageId.Count) real packages, $($cases.Count) rejected mutations)."
Write-Host "Mutation evidence retained at $fixtureRoot"
