# Phase 5: Namespace and Using Statement Update Script
# Run this AFTER creating Contracts projects and moving files

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Phase 5: Namespace Update Script" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$basePath = "C:\Dev\VisionaryCoder\App.Framework\main\src"

# Define namespace replacements
$namespaceReplacements = @{
    'VisionaryCoder.Framework.Abstractions.Proxy' = 'VisionaryCoder.Framework.Resilience.Contracts.Proxy'
    'VisionaryCoder.Framework.Abstractions.Proxy.Exceptions' = 'VisionaryCoder.Framework.Resilience.Contracts.Proxy.Exceptions'
    'VisionaryCoder.Framework.Abstractions.Pipeline' = 'VisionaryCoder.Framework.Resilience.Contracts.Pipeline'
    'VisionaryCoder.Framework.Abstractions.Secrets' = 'VisionaryCoder.Framework.Security.Contracts.Secrets'
    'VisionaryCoder.Framework.Abstractions.Messaging' = 'VisionaryCoder.Framework.Messaging.Contracts'
}

# Define using statement replacements (order matters - most specific first!)
$usingReplacements = @(
    @{
        Old = 'using VisionaryCoder.Framework.Abstractions.Proxy.Exceptions;'
        New = 'using VisionaryCoder.Framework.Resilience.Contracts.Proxy.Exceptions;'
    },
    @{
        Old = 'using VisionaryCoder.Framework.Abstractions.Proxy;'
        New = 'using VisionaryCoder.Framework.Resilience.Contracts.Proxy;'
    },
    @{
        Old = 'using VisionaryCoder.Framework.Abstractions.Pipeline;'
        New = 'using VisionaryCoder.Framework.Resilience.Contracts.Pipeline;'
    },
    @{
        Old = 'using VisionaryCoder.Framework.Abstractions.Secrets;'
        New = 'using VisionaryCoder.Framework.Security.Contracts.Secrets;'
    },
    @{
        Old = 'using VisionaryCoder.Framework.Abstractions.Messaging;'
        New = 'using VisionaryCoder.Framework.Messaging.Contracts;'
    }
)

# Packages that need updating
$packages = @(
    "Framework.Observability",
    "Framework.Resilience",
    "Framework.Security",
    "Framework.Messaging",
    "Framework.Resilience.Contracts",
    "Framework.Observability.Contracts",
    "Framework.Security.Contracts",
    "Framework.Messaging.Contracts"
)

$totalFiles = 0
$updatedFiles = 0
$errors = 0

Write-Host "Scanning packages for namespace updates..." -ForegroundColor Green
Write-Host ""

foreach ($package in $packages) {
    $packagePath = Join-Path $basePath $package
    
    if (-not (Test-Path $packagePath)) {
        Write-Host "  ⊘ Skipped: $package (not found)" -ForegroundColor Gray
        continue
    }
    
    Write-Host "Processing: $package" -ForegroundColor Cyan
    
    $files = Get-ChildItem -Path $packagePath -Filter "*.cs" -Recurse -ErrorAction SilentlyContinue | 
             Where-Object { $_.FullName -notmatch '\\obj\\|\\bin\\' }
    
    foreach ($file in $files) {
        $totalFiles++
        $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
        
        if ($null -eq $content) {
            continue
        }
        
        $originalContent = $content
        $modified = $false
        
        # Update using statements (most specific first)
        foreach ($replacement in $usingReplacements) {
            if ($content -match [regex]::Escape($replacement.Old)) {
                $content = $content -replace [regex]::Escape($replacement.Old), $replacement.New
                $modified = $true
            }
        }
        
        # Update namespace declarations
        foreach ($old in $namespaceReplacements.Keys) {
            $new = $namespaceReplacements[$old]
            $pattern = "namespace\s+$([regex]::Escape($old))"
            if ($content -match $pattern) {
                $content = $content -replace $pattern, "namespace $new"
                $modified = $true
            }
        }
        
        if ($modified) {
            try {
                Set-Content $file.FullName $content -NoNewline -ErrorAction Stop
                $updatedFiles++
                $relativePath = $file.FullName.Replace($basePath + "\", "")
                Write-Host "  ✓ Updated: $relativePath" -ForegroundColor Green
            }
            catch {
                $errors++
                Write-Host "  ✗ FAILED: $($file.Name) - $($_.Exception.Message)" -ForegroundColor Red
            }
        }
    }
}

Write-Host ""
Write-Host "Adding Missing Using Statements..." -ForegroundColor Green
Write-Host "-----------------------------------" -ForegroundColor Green

# Add missing using VisionaryCoder.Framework.Resilience.Contracts.Proxy; where needed
$packagesToFix = @("Framework.Observability", "Framework.Resilience", "Framework.Security")

foreach ($package in $packagesToFix) {
    $packagePath = Join-Path $basePath $package
    
    if (-not (Test-Path $packagePath)) {
        continue
    }
    
    $files = Get-ChildItem -Path $packagePath -Filter "*.cs" -Recurse -ErrorAction SilentlyContinue | 
             Where-Object { $_.FullName -notmatch '\\obj\\|\\bin\\' }
    
    foreach ($file in $files) {
        $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
        
        if ($null -eq $content) {
            continue
        }
        
        # Check if file uses proxy types but doesn't have the using statement
        $usesProxyTypes = $content -match '\b(IProxyInterceptor|IOrderedProxyInterceptor|ProxyContext|ProxyResponse|ProxyDelegate)\b'
        $hasProxyUsing = $content -match 'using VisionaryCoder\.Framework\.Resilience\.Contracts\.Proxy;'
        
        if ($usesProxyTypes -and -not $hasProxyUsing) {
            # Find where to insert the using statement (after other usings)
            if ($content -match '(using [^;]+;\r?\n)+') {
                $lastUsingMatch = [regex]::Matches($content, 'using [^;]+;') | Select-Object -Last 1
                $insertPosition = $lastUsingMatch.Index + $lastUsingMatch.Length
                
                $newUsing = "`r`nusing VisionaryCoder.Framework.Resilience.Contracts.Proxy;"
                $content = $content.Insert($insertPosition, $newUsing)
                
                try {
                    Set-Content $file.FullName $content -NoNewline -ErrorAction Stop
                    $relativePath = $file.FullName.Replace($basePath + "\", "")
                    Write-Host "  ✓ Added using statement: $relativePath" -ForegroundColor Green
                    $updatedFiles++
                }
                catch {
                    Write-Host "  ✗ FAILED: $($file.Name) - $($_.Exception.Message)" -ForegroundColor Red
                    $errors++
                }
            }
        }
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Namespace Update Complete!" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Statistics:" -ForegroundColor Yellow
Write-Host "  Total files scanned: $totalFiles" -ForegroundColor White
Write-Host "  Files updated: $updatedFiles" -ForegroundColor Green
Write-Host "  Errors: $errors" -ForegroundColor $(if ($errors -gt 0) { "Red" } else { "Green" })
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Build solution: dotnet build" -ForegroundColor White
Write-Host "2. Check for remaining errors" -ForegroundColor White
Write-Host "3. Run tests: dotnet test" -ForegroundColor White
Write-Host ""
