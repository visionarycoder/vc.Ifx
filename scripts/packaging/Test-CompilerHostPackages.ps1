#Requires -Version 7.0
[CmdletBinding()]
param([switch] $UnderFrameworkMutex)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $UnderFrameworkMutex) { throw 'Use Invoke-FrameworkTests.ps1 -BuildOnly -Project scripts/packaging/CompilerHostSmoke.proj to hold the repository mutex.' }
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$artifacts = Join-Path $repo "TestResults/compiler-host-packages/$([Guid]::NewGuid().ToString('N'))"
$null = New-Item -ItemType Directory -Path $artifacts
Write-Host "Compiler host package artifacts: $artifacts"
Start-Transcript -Path (Join-Path $artifacts 'verification.log') | Out-Null
Import-Module (Join-Path $PSScriptRoot 'PackageValidation.psm1') -Force
$manifest = [ordered]@{ status = 'In-flight'; timestampUtc = [DateTime]::UtcNow.ToString('O'); scope = 'Four compiler-family packages plus required endpoint marker package; not global freeze'; packages = @(); compilerChecks = @(); issues = @() }

function Invoke-CheckedDotNet {
    param([string[]] $Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet failed ($LASTEXITCODE): $($Arguments -join ' ')" }
}

try {
    $manifest.sdk = (& dotnet --version).Trim()
    $manifest.branch = (& git -C $repo branch --show-current).Trim()
    $manifest.commit = (& git -C $repo rev-parse HEAD).Trim()
    $manifest.worktree = @(& git -C $repo status --short)
    $metadataPaths = @('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props', 'global.json')
    $manifest.metadataBefore = @($metadataPaths | ForEach-Object { Get-FileHash (Join-Path $repo $_) -Algorithm SHA256 } | Select-Object Path, Hash)
    $packages = @('vc.Ifx.Roslyn', 'vc.Ifx.Analyzers', 'vc.Ifx.CodeFixes', 'vc.Ifx.Generators', 'vc.Ifx.Generators.Abstractions')
    foreach ($id in $packages) {
        $project = Join-Path $repo "src/$id/$id.csproj"
        Invoke-CheckedDotNet @('pack', $project, '-c', 'Release', '-o', $artifacts, '-m:1', '-p:BuildInParallel=false', '-p:GeneratePackageOnBuild=false', '-warnaserror', '-v:minimal')
        $archives = @(Get-ChildItem -LiteralPath $artifacts -Filter '*.nupkg' | Where-Object Name -Match ('^' + [regex]::Escape($id) + '\.\d+\.\d+\.\d+.*\.nupkg$'))
        if ($archives.Count -ne 1) { throw "Expected one fresh archive for $id." }
        $required = if ($id -eq 'vc.Ifx.Roslyn') { @('Microsoft.CodeAnalysis.Common') } else { @() }
        Test-PackageArchive -Path $archives[0].FullName -PackageId $id -ReadmePath (Join-Path $repo "src/$id/README.md") -RequiredDependencies $required | Out-Host
        $extracted = Join-Path $artifacts $id
        [IO.Compression.ZipFile]::ExtractToDirectory($archives[0].FullName, $extracted)
        [xml] $nuspec = Get-Content (Join-Path $extracted "$id.nuspec") -Raw
        $metadata = $nuspec.SelectSingleNode('/*[local-name()="package"]/*[local-name()="metadata"]')
        if ($metadata.version -match '-') { throw "Expected current stable package version for $id, got $($metadata.version)." }
        $manifest.packages += [ordered]@{
            id = $id; version = $metadata.version; sha256 = (Get-FileHash $archives[0].FullName -Algorithm SHA256).Hash
            archive = $archives[0].FullName; extracted = $extracted
            dependencies = @($metadata.SelectNodes('.//*[local-name()="dependency"]') | ForEach-Object { @{ id = $_.id; version = $_.version } })
            dlls = @(Get-ChildItem $extracted -Recurse -Filter '*.dll' | ForEach-Object { @{ path = [IO.Path]::GetRelativePath($extracted, $_.FullName); sha256 = (Get-FileHash $_.FullName -Algorithm SHA256).Hash } })
        }
    }
    $extensionIds = @('vc.Ifx.Analyzers', 'vc.Ifx.CodeFixes', 'vc.Ifx.Generators')
    $sharedHash = (Get-FileHash (Join-Path $artifacts 'vc.Ifx.Roslyn/lib/netstandard2.0/vc.Ifx.Roslyn.dll') -Algorithm SHA256).Hash
    foreach ($id in $extensionIds) {
        $private = Join-Path $artifacts "$id/analyzers/dotnet/cs"
        if ((Get-FileHash (Join-Path $private 'vc.Ifx.Roslyn.dll') -Algorithm SHA256).Hash -ne $sharedHash) { throw "$id bundled shared library differs from standalone package." }
        if (Get-ChildItem $private -Filter 'Microsoft.CodeAnalysis*.dll') { throw "$id bundles a host Roslyn assembly." }
    }
    if (Test-Path (Join-Path $artifacts 'vc.Ifx.CodeFixes/analyzers/dotnet/cs/vc.Ifx.Analyzers.dll')) { throw 'CodeFixes must not bundle the separate analyzer package.' }
    & (Join-Path $PSScriptRoot 'Test-PackageValidation.ps1') -RepositoryRoot $repo -PackageDirectory $artifacts -PackageId $packages
    $manifest.archiveMutationReport = (Get-ChildItem $artifacts -Directory -Filter 'validator-tests-*' | Select-Object -First 1).FullName
    [xml] $roslynNuspec = Get-Content (Join-Path $artifacts 'vc.Ifx.Roslyn/vc.Ifx.Roslyn.nuspec') -Raw
    $versionRange = $roslynNuspec.SelectSingleNode('//*[local-name()="dependency" and @id="Microsoft.CodeAnalysis.Common"]').version
    $hostVersion = [regex]::Match($versionRange, '\d+\.\d+\.\d+').Value
    if (-not $hostVersion -or $versionRange -match '-') { throw 'Host Roslyn dependency must be stable.' }
    $hostDirectory = Join-Path $artifacts 'host'
    $null = New-Item -ItemType Directory -Path $hostDirectory
    Copy-Item (Join-Path $PSScriptRoot 'compiler-host/CompilerHost.csproj.template') (Join-Path $hostDirectory 'CompilerHost.csproj')
    Copy-Item (Join-Path $PSScriptRoot 'compiler-host/Program.cs.txt') (Join-Path $hostDirectory 'Program.cs')
    Invoke-CheckedDotNet @('build', (Join-Path $hostDirectory 'CompilerHost.csproj'), '-c', 'Release', '-m:1', '-p:BuildInParallel=false',
        '-p:ImportDirectoryBuildProps=false', '-p:ImportDirectoryBuildTargets=false', '-p:ManagePackageVersionsCentrally=false',
        "-p:HostRoslynVersion=$hostVersion", '-warnaserror', '-v:minimal')
    Invoke-CheckedDotNet @('exec', (Join-Path $hostDirectory 'bin/Release/net10.0/CompilerHost.dll'),
        (Join-Path $artifacts 'vc.Ifx.CodeFixes/analyzers/dotnet/cs'), (Join-Path $artifacts 'codefix-host.json'),
        (Join-Path $artifacts 'vc.Ifx.Generators/analyzers/dotnet/cs'), (Join-Path $artifacts 'vc.Ifx.Analyzers/analyzers/dotnet/cs'))

    $dotnetRoot = Split-Path (Get-Command dotnet).Source
    $compiler = Join-Path $dotnetRoot "sdk/$($manifest.sdk)/Roslyn/bincore/csc.dll"
    $manifest.compiler = (& dotnet exec $compiler -version).Trim()
    $references = foreach ($pack in @('Microsoft.NETCore.App.Ref', 'Microsoft.AspNetCore.App.Ref')) {
        $version = Get-ChildItem (Join-Path $dotnetRoot "packs/$pack") -Directory | Where-Object Name -Match '^10\.\d+\.\d+$' |
            Sort-Object { [version] $_.Name } -Descending | Select-Object -First 1
        Get-ChildItem (Join-Path $version.FullName 'ref/net10.0') -Filter '*.dll'
    }
    foreach ($mode in @('shared-library', 'analyzer-only', 'codefix-only', 'generator-only', 'combined', 'combined-debt')) {
        $out = Join-Path $artifacts $mode
        $null = New-Item -ItemType Directory -Path $out
        $arguments = @('/nostdlib+', '/target:library', '/langversion:14', '/nullable:enable', '/warnaserror+', '/deterministic+',
            "/out:`"$out/Consumer.dll`"", "/generatedfilesout:`"$out`"", "/errorlog:`"$out/compiler.sarif`"")
        $arguments += $references | ForEach-Object { "/reference:`"$($_.FullName)`"" }
        $ids = switch ($mode) {
            'analyzer-only' { @('vc.Ifx.Analyzers') }
            'codefix-only' { @('vc.Ifx.CodeFixes') }
            'generator-only' { @('vc.Ifx.Generators') }
            'combined' { $extensionIds }
            'combined-debt' { $extensionIds }
            default { @() }
        }
        foreach ($id in $ids) {
            $arguments += Get-ChildItem (Join-Path $artifacts "$id/analyzers/dotnet/cs") -Filter '*.dll' | ForEach-Object { "/analyzer:`"$($_.FullName)`"" }
        }
        if ($mode -eq 'shared-library') {
            $arguments += "/reference:`"$artifacts/vc.Ifx.Roslyn/lib/netstandard2.0/vc.Ifx.Roslyn.dll`""
            $arguments += "/reference:`"$hostDirectory/bin/Release/net10.0/Microsoft.CodeAnalysis.dll`""
            $source = Join-Path $PSScriptRoot 'compiler-host/RoslynConsumer.cs.txt'
        } elseif ($mode -in @('analyzer-only', 'codefix-only', 'combined-debt')) {
            $source = Join-Path $PSScriptRoot 'compiler-host/AnalyzerConsumer.cs.txt'
        } else {
            $arguments += "/reference:`"$artifacts/vc.Ifx.Generators.Abstractions/lib/net10.0/vc.Ifx.Generators.Abstractions.dll`""
            $source = Join-Path $repo 'src/vc.Ifx.Generators/verification/EndpointConsumer.cs.txt'
        }
        $arguments += "`"$source`""
        $response = Join-Path $out 'compiler.rsp'
        $arguments | Set-Content $response -Encoding utf8
        $output = & dotnet exec $compiler /noconfig "@$response" 2>&1
        $exit = $LASTEXITCODE
        $output | Tee-Object -FilePath (Join-Path $out 'compiler.log') | Out-Host
        $expected = 0
        if ($exit -ne $expected) { throw "$mode compiler exit $exit, expected $expected." }
        $diagnostics = @((Get-Content (Join-Path $out 'compiler.sarif') -Raw | ConvertFrom-Json).runs[0].results)
        if ($mode -in @('analyzer-only', 'combined-debt')) {
            if ($diagnostics.Count -ne 1 -or $diagnostics[0].ruleId -ne 'IFX1000') { throw "$mode must report exactly the intended IFX1000 diagnostic." }
        } elseif ($diagnostics.Count -ne 0) { throw "$mode compiler reported unexpected diagnostics." }
        if ($mode -in @('generator-only', 'combined')) {
            $registry = @(Get-ChildItem $out -Recurse -Filter 'IfxEndpointRouteBuilderExtensions.g.cs')
            if ($registry.Count -ne 1 -or (Get-Content $registry[0].FullName -Raw) -notmatch 'global::Handlers.Read') { throw "$mode missing generated registry." }
        }
        $manifest.compilerChecks += @{ mode = $mode; exitCode = $exit; expectedExitCode = $expected; diagnostics = @($diagnostics | ForEach-Object ruleId); response = $response }
    }
    $consumer = Join-Path $artifacts 'nuget-consumer'
    $null = New-Item -ItemType Directory -Path $consumer
    Copy-Item (Join-Path $PSScriptRoot 'compiler-host/NuGetConsumer.csproj.template') (Join-Path $consumer 'NuGetConsumer.csproj')
    Copy-Item (Join-Path $repo 'src/vc.Ifx.Generators/verification/EndpointConsumer.cs.txt') (Join-Path $consumer 'EndpointConsumer.cs')
    Copy-Item (Join-Path $PSScriptRoot 'compiler-host/RoslynConsumer.cs.txt') (Join-Path $consumer 'RoslynConsumer.cs')
    Copy-Item (Join-Path $PSScriptRoot 'compiler-host/AnalyzerConsumer.cs.txt') (Join-Path $consumer 'AnalyzerConsumer.cs')
    # Map vc.Ifx exclusively to these fresh archives, never an existing same-version cache/feed.
    $config = [xml] '<configuration><packageSources><clear/><add key="local" value=""/><add key="nuget" value="https://api.nuget.org/v3/index.json"/></packageSources><packageSourceMapping><packageSource key="local"><package pattern="vc.Ifx*"/></packageSource><packageSource key="nuget"><package pattern="*"/></packageSource></packageSourceMapping></configuration>'
    $config.configuration.packageSources.add[0].SetAttribute('value', $artifacts)
    $configPath = Join-Path $consumer 'NuGet.Config'
    $config.Save($configPath)
    $packageVersion = $manifest.packages[0].version
    if (@($manifest.packages | Where-Object version -NE $packageVersion).Count) { throw 'Fixture requires matching scoped package versions.' }
    Invoke-CheckedDotNet @('build', (Join-Path $consumer 'NuGetConsumer.csproj'), '-c', 'Release', '-m:1', '-p:BuildInParallel=false',
        '-p:ImportDirectoryBuildProps=false', '-p:ImportDirectoryBuildTargets=false', '-p:ManagePackageVersionsCentrally=false',
        "-p:CompilerPackageVersion=$packageVersion", "-p:RestoreConfigFile=$configPath", "-p:RestorePackagesPath=$artifacts/nuget-cache", "-p:ErrorLog=$consumer/compiler.sarif", '-warnaserror', '-v:minimal')
    $resolvedAnalyzers = @(Get-Content (Join-Path $consumer 'resolved-analyzers.txt'))
    foreach ($id in $extensionIds) {
        if (-not ($resolvedAnalyzers | Where-Object { $_.EndsWith("/$id.dll", [StringComparison]::OrdinalIgnoreCase) -or $_.EndsWith("\$id.dll", [StringComparison]::OrdinalIgnoreCase) })) { throw "NuGet did not resolve $id as a compiler extension." }
    }
    $resolvedReferences = @(Get-Content (Join-Path $consumer 'resolved-references.txt'))
    if ($resolvedReferences | Where-Object { [IO.Path]::GetFileNameWithoutExtension($_) -in $extensionIds }) { throw 'NuGet leaked compiler extensions into runtime compile references.' }
    $registry = @(Get-ChildItem (Join-Path $consumer 'obj/generated') -Recurse -Filter 'IfxEndpointRouteBuilderExtensions.g.cs')
    if ($registry.Count -ne 1) { throw 'NuGet consumer did not generate the endpoint registry.' }
    $consumerDiagnostics = @((Get-Content (Join-Path $consumer 'compiler.sarif') -Raw | ConvertFrom-Json).runs[0].results | Where-Object ruleId -EQ 'IFX1000')
    if ($consumerDiagnostics.Count -ne 1) { $manifest.issues += "Fresh-cache NuGet consumer reports $($consumerDiagnostics.Count) IFX1000 diagnostics for one source location; expected one." }
    $manifest.nugetConsumer = @{ buildPassed = $true; debtDiagnosticCount = $consumerDiagnostics.Count; project = (Join-Path $consumer 'NuGetConsumer.csproj'); cache = (Join-Path $artifacts 'nuget-cache'); resolvedAnalyzers = $resolvedAnalyzers; generatedRegistry = $registry[0].FullName }
    $fixOnly = Join-Path $artifacts 'nuget-codefix-only'
    $null = New-Item -ItemType Directory -Path $fixOnly
    Copy-Item (Join-Path $PSScriptRoot 'compiler-host/NuGetConsumer.csproj.template') (Join-Path $fixOnly 'CodeFixOnly.csproj')
    Copy-Item (Join-Path $PSScriptRoot 'compiler-host/AnalyzerConsumer.cs.txt') (Join-Path $fixOnly 'AnalyzerConsumer.cs')
    Invoke-CheckedDotNet @('build', (Join-Path $fixOnly 'CodeFixOnly.csproj'), '-c', 'Release', '-m:1', '-p:BuildInParallel=false',
        '-p:ImportDirectoryBuildProps=false', '-p:ImportDirectoryBuildTargets=false', '-p:ManagePackageVersionsCentrally=false', '-p:CodeFixesOnly=true',
        "-p:CompilerPackageVersion=$packageVersion", "-p:RestoreConfigFile=$configPath", "-p:RestorePackagesPath=$artifacts/codefix-only-cache", "-p:ErrorLog=$fixOnly/compiler.sarif", '-warnaserror', '-v:minimal')
    $fixOnlyAnalyzers = @(Get-Content (Join-Path $fixOnly 'resolved-analyzers.txt'))
    $fixOnlyDiagnostics = @((Get-Content (Join-Path $fixOnly 'compiler.sarif') -Raw | ConvertFrom-Json).runs[0].results | Where-Object { $_.ruleId -like 'IFX*' })
    if ($fixOnlyDiagnostics.Count -ne 0 -or ($fixOnlyAnalyzers | Where-Object { [IO.Path]::GetFileName($_) -eq 'vc.Ifx.Analyzers.dll' })) { throw 'CodeFixes-only consumer unexpectedly includes or executes IFX analyzers.' }
    if (-not ($fixOnlyAnalyzers | Where-Object { [IO.Path]::GetFileName($_) -eq 'vc.Ifx.CodeFixes.dll' })) { throw 'CodeFixes-only consumer did not discover its code-fix assembly.' }
    $fixOnlyAssets = Get-Content (Join-Path $fixOnly 'obj/project.assets.json') -Raw | ConvertFrom-Json
    if ($fixOnlyAssets.libraries.PSObject.Properties.Name -like 'vc.Ifx.Analyzers/*') { throw 'CodeFixes-only restore pulled in the analyzer package.' }
    $manifest.codeFixOnlyNugetConsumer = @{ passed = $true; project = (Join-Path $fixOnly 'CodeFixOnly.csproj'); ifxDiagnosticCount = $fixOnlyDiagnostics.Count; resolvedAnalyzers = $fixOnlyAnalyzers }
    $manifest.hostVersion = $hostVersion
    $manifest.metadataAfter = @($metadataPaths | ForEach-Object { Get-FileHash (Join-Path $repo $_) -Algorithm SHA256 } | Select-Object Path, Hash)
    $manifest.metadataChangedDuringVerification = [bool](Compare-Object $manifest.metadataBefore $manifest.metadataAfter -Property Path, Hash)
    if ($manifest.issues.Count) { throw ($manifest.issues -join [Environment]::NewLine) }
    $manifest.status = 'Passed'
    Write-Host "Scoped extracted compiler-host verification passed: $artifacts"
} catch {
    $manifest.status = 'Failed'
    $manifest.error = $_.ToString()
    throw
} finally {
    $manifest | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $artifacts 'manifest.json') -Encoding utf8
    Stop-Transcript | Out-Null
}
