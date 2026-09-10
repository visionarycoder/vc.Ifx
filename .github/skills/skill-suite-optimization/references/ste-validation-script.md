---
title: STE Validation Script
doc_type: reference
status: active
last_updated: 2026-07-29
---
# STE Validation Script

## Purpose

Detect STE compliance violations in `.github/**/*.md` files.

## Usage

```powershell
cd D:\WSDOT Laptop\dev\w\0724\.github
Get-ChildItem -Recurse -Filter *.md | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $violations = @()
    
    # Modal verbs
    if ($content -match '\b(should|must|could|may|might|consider|prefer|try)\b') {
        $violations += "Modal verbs"
    }
    
    # Prohibited phrases
    if ($content -match '\b(as appropriate|best practice|edge case|good enough|high quality|reasonable|relevant|various)\b') {
        $violations += "Prohibited phrases"
    }
    
    # Vague adverbs
    if ($content -match '\b(properly|correctly|appropriately)\b') {
        $violations += "Vague adverbs"
    }
    
    if ($violations.Count -gt 0) {
        Write-Host "$($_.Name): $($violations -join ', ')" -ForegroundColor Yellow
    }
}
```

## Violation Categories

| Category | Pattern | Fix |
|---|---|---|
| Modal verbs | `should`, `must`, `could`, `may`, `might` | Use imperative: "Use X" / "Do not use X" |
| Conditionals | `consider`, `prefer`, `try` | "Use X when Y" / "Use X. Use Y only when Z." |
| Vague adverbs | `properly`, `correctly`, `appropriately` | Add Test + Pass criteria |
| Prohibited phrases | `as appropriate`, `reasonable`, `relevant`, `various` | Specify explicitly or add Boolean condition |
| Passive voice | "Tests are run", "validation occurs" | "Agent runs tests", "Agent verifies input" |

## Token Verification

```powershell
$file = Get-Content "SKILL.md" -Raw
$tokens = [Math]::Ceiling($file.Length / 4)
Write-Host "Estimated tokens: $tokens"

# Verify against limits
if ($file -match 'doc_type: skill' -and $tokens -gt 2000) {
    Write-Host "FAIL: Skill exceeds 2000 tokens" -ForegroundColor Red
}
elseif ($file -match 'doc_type: instruction' -and $tokens -gt 800) {
    Write-Host "FAIL: Instruction exceeds 800 tokens" -ForegroundColor Red
}
elseif ($file -match 'doc_type: prompt' -and $tokens -gt 1500) {
    Write-Host "FAIL: Prompt exceeds 1500 tokens" -ForegroundColor Red
}
```

## Frontmatter Verification

```bash
npm run frontmatter:validate
```

## Full Audit Command

```powershell
# Run from repository root
Get-ChildItem .github -Recurse -Filter *.md | ForEach-Object {
    $path = $_.FullName
    $content = Get-Content $path -Raw
    $tokens = [Math]::Ceiling($content.Length / 4)
    
    $modals = ([regex]::Matches($content, '\b(should|must|could|may|might)\b')).Count
    $prohibited = ([regex]::Matches($content, '\b(appropriate|reasonable|relevant|various)\b')).Count
    
    [PSCustomObject]@{
        File = $_.Name
        Tokens = $tokens
        Modals = $modals
        Prohibited = $prohibited
    }
} | Format-Table -AutoSize
```

## Pass Criteria

- [ ] Zero modal verbs
- [ ] Zero prohibited phrases
- [ ] Tokens within limits
- [ ] Frontmatter validation passes
