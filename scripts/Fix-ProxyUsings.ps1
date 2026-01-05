# Fix-ProxyUsings.ps1
# Adds missing using statements for Framework.Proxy.Abstractions to all interceptor files

$ErrorActionPreference = "Stop"
$fixed = 0
$skipped = 0

Write-Host "Scanning for files that need fixing..." -ForegroundColor Cyan

$files = Get-ChildItem "src\Framework.Proxy\Interceptors" -Recurse -Filter "*.cs"

foreach ($file in $files) {
    try {
        $content = Get-Content $file.FullName -Raw
        
        # Check if file uses proxy types
        $usesProxyTypes = $content -match "IProxyInterceptor|IOrderedProxyInterceptor|ProxyContext|ProxyDelegate|ProxyResponse"
        $hasUsing = $content -match "using VisionaryCoder\.Framework\.Abstractions\.Proxy;"
        
        if ($usesProxyTypes -and -not $hasUsing) {
            Write-Host "  Fixing: $($file.Name)" -ForegroundColor Yellow
            
            # Find the position after the last using statement or after the license
            if ($content -match "(?s)(.*?)(using [^;]+;)(\r?\n)(\r?\n)(namespace)") {
                # Add after the last using statement
                $newContent = $content -replace "(using [^;]+;)(\r?\n)(\r?\n)(namespace)", "`$1`$2using VisionaryCoder.Framework.Abstractions.Proxy;`$3`$4"
            }
            elseif ($content -match "(?s)(// Licensed.*?\r?\n)(\r?\n)(namespace)") {
                # Add after license if no using statements
                $newContent = $content -replace "(// Licensed.*?\r?\n)(\r?\n)(namespace)", "`$1`$2using VisionaryCoder.Framework.Abstractions.Proxy;`$2`$3"
            }
            else {
                Write-Host "    Skipped - couldn't find insertion point" -ForegroundColor Red
                $skipped++
                continue
            }
            
            if ($newContent -ne $content) {
                Set-Content -Path $file.FullName -Value $newContent -NoNewline
                $fixed++
                Write-Host "    ✓ Fixed" -ForegroundColor Green
            }
        }
    }
    catch {
        Write-Host "  Error processing $($file.Name): $_" -ForegroundColor Red
        $skipped++
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Fixed: $fixed files" -ForegroundColor Green
Write-Host "Skipped: $skipped files" -ForegroundColor Yellow
Write-Host "========================================`n" -ForegroundColor Cyan
