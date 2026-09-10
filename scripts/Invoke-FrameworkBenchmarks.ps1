#Requires -Version 7.0
[CmdletBinding()]
param(
    [ValidateSet('Core', 'Filtering', 'Proxy', 'Storage')][string[]] $Project = @('Core', 'Filtering', 'Proxy', 'Storage'),
    [ValidateSet('Dry', 'Short', 'Default')][string] $Job = 'Dry',
    [string[]] $Filter = @('*'),
    [switch] $ListOnly,
    [ValidateRange(1, 3600)][int] $LockTimeoutSeconds = 1200
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$identity = if ($IsWindows) { $root.ToUpperInvariant() } else { $root }
$digest = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($identity)))
$mutex = [Threading.Mutex]::new($false, "vc.Ifx.FrameworkTests.$digest")
$acquired = $false
$run = Join-Path $root "TestResults/benchmarks/$([Guid]::NewGuid().ToString('N'))"
try {
    Write-Host 'Waiting for exclusive framework build/test/benchmark access...'
    try { $acquired = $mutex.WaitOne([TimeSpan]::FromSeconds($LockTimeoutSeconds)) }
    catch [Threading.AbandonedMutexException] { $acquired = $true; throw 'Previous owner abandoned the mutex. Rebuild before measuring.' }
    if (-not $acquired) { throw 'Timed out waiting for framework build/test access.' }
    $null = New-Item -ItemType Directory -Path $run
    $revision = & git -C $root rev-parse HEAD
    if ($LASTEXITCODE -ne 0) { throw 'Cannot identify benchmark revision.' }
    @{ revision = $revision; projects = $Project; job = $Job; filter = $Filter; configuration = 'Release'; listOnly = [bool] $ListOnly;
        startedUtc = [DateTime]::UtcNow.ToString('O'); caveat = 'Dry is execution smoke only. Shared-machine runs are not release baselines.' } |
        ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $run 'run-context.json') -Encoding utf8
    & dotnet --info | Set-Content -LiteralPath (Join-Path $run 'dotnet-info.txt') -Encoding utf8
    & git -C $root status --short | Set-Content -LiteralPath (Join-Path $run 'worktree-before.txt') -Encoding utf8
    Write-Host "Benchmark artifacts: $run"
    $inventory = [Collections.Generic.List[object]]::new()
    foreach ($name in $Project) {
        $projectName = "vc.Ifx.$name.Benchmarks"
        $projectPath = Join-Path $root "performance/$projectName/$projectName.csproj"
        $destination = Join-Path $run $name
        $null = New-Item -ItemType Directory -Path $destination
        $propertiesJson = & dotnet msbuild $projectPath '-getProperty:TargetFramework,LangVersion,IsPackable,GeneratePackageOnBuild' '-m:1' '-p:BuildInParallel=false'
        if ($LASTEXITCODE -ne 0) { throw "Benchmark evaluation failed: $name" }
        $properties = ($propertiesJson | ConvertFrom-Json).Properties
        if ($properties.TargetFramework -ne 'net10.0' -or $properties.LangVersion -ne '14.0' -or $properties.IsPackable -ne 'false' -or $properties.GeneratePackageOnBuild -ne 'false') {
            throw "Benchmark metadata violates stable/non-packable rules: $name"
        }
        & dotnet build $projectPath '-c' 'Release' '-m:1' '-p:BuildInParallel=false' '-p:GeneratePackageOnBuild=false' '-warnaserror' '-v:minimal' 2>&1 |
            Tee-Object -FilePath (Join-Path $destination 'build.log') | Out-Host
        if ($LASTEXITCODE -ne 0) { throw "Benchmark build failed: $name" }
        $assembly = Join-Path $root "performance/$projectName/bin/Release/net10.0/$projectName.dll"
        $arguments = @($assembly, '--filter') + $Filter + @('--job', $Job, '--artifacts', $destination,
            '--exporters', 'json', '--stopOnFirstError', '--keepFiles')
        if ($ListOnly) { $arguments += @('--list', 'flat') }
        $arguments | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $destination 'arguments.json') -Encoding utf8
        & dotnet @arguments 2>&1 | Tee-Object -FilePath (Join-Path $destination 'console.log') | Out-Host
        if ($LASTEXITCODE -ne 0) { throw "Benchmark execution failed: $name" }
        if ($ListOnly) {
            $names = @(Get-Content (Join-Path $destination 'console.log') | Where-Object { $_ -match '^vc\.Ifx\..*Benchmarks\.' })
            if ($names.Count -eq 0) { throw "Zero benchmarks discovered: $name" }
            $inventory.Add(@{ project = $name; discoveredMethods = $names.Count; names = $names })
        }
        else {
            $reports = @(Get-ChildItem $destination -Recurse -Filter '*-report-full*.json' -File)
            if ($reports.Count -eq 0) { throw "No JSON benchmark results: $name" }
            $count = 0
            foreach ($report in $reports) {
                $document = Get-Content -LiteralPath $report.FullName -Raw | ConvertFrom-Json
                foreach ($benchmark in $document.Benchmarks) {
                    if ($null -eq $benchmark.Statistics -or $benchmark.Statistics.N -lt 1) { throw "Missing measurements: $($benchmark.FullName)" }
                    $count++
                }
            }
            if ($count -eq 0) { throw "Zero benchmark cases executed: $name" }
            $inventory.Add(@{ project = $name; measuredCases = $count; reports = @($reports.FullName) })
        }
    }
    @($inventory) | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $run 'summary.json') -Encoding utf8
    & git -C $root status --short | Set-Content -LiteralPath (Join-Path $run 'worktree-after.txt') -Encoding utf8
    Write-Host "Benchmark run completed: $run"
}
finally {
    if ($acquired) { $mutex.ReleaseMutex() }
    $mutex.Dispose()
}
