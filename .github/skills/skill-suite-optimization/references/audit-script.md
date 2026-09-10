---
title: Skill Suite Audit Script
description: Baseline audit commands for governed skills, prompts, and instructions during optimization passes.
doc_type: reference
status: active
last_updated: 2026-07-27
target_audience: ai
complexity: medium
estimated_tokens: 813
prerequisites:
  - skill-suite-optimization
related_skills:
  - skill-suite-optimization
appliesTo: '.github/**/*.{md,prompt.md,instructions.md}'
tags:
  - optimization
  - audit
  - powershell
---
# Skill Suite Audit Script

Use this reference when you need the full baseline audit logic outside the main skill.

## Purpose

- Count governed skills, prompts, and instructions
- Estimate tokens with `Math.Ceiling(content.Length / 4)`
- Detect metadata gaps
- Flag artifacts that are oversized, undersized, or otherwise likely optimization candidates
- Save a baseline snapshot for before/after reporting

## PowerShell Baseline Script

```powershell
$skills = Get-ChildItem -Path ".github\skills" -Recurse -Filter "SKILL.md"
$prompts = Get-ChildItem -Path ".github\prompts" -Filter "*.prompt.md"
$instructions = Get-ChildItem -Path ".github\instructions" -Filter "*.instructions.md"

$analysis = @()
foreach ($file in ($skills + $prompts + $instructions)) {
    $content = Get-Content $file.FullName -Raw
    $tokens = [Math]::Ceiling($content.Length / 4)
    $hasMetadata = $content -match 'target_audience:'

    $analysis += [PSCustomObject]@{
        Type = if ($file.Directory.Parent.Name -eq 'skills') { 'Skill' }
               elseif ($file.Name -like '*.prompt.md') { 'Prompt' }
               else { 'Instruction' }
        Name = $file.BaseName
        Tokens = $tokens
        HasMetadata = $hasMetadata
        NeedsOptimization = (-not $hasMetadata -or $tokens -gt 2500 -or $tokens -lt 500)
    }
}

$baseline = @{
    Date = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    TotalSkills = ($analysis | Where-Object Type -eq 'Skill').Count
    TotalPrompts = ($analysis | Where-Object Type -eq 'Prompt').Count
    TotalInstructions = ($analysis | Where-Object Type -eq 'Instruction').Count
    SkillTokens = ($analysis | Where-Object Type -eq 'Skill' | Measure-Object Tokens -Sum).Sum
    PromptTokens = ($analysis | Where-Object Type -eq 'Prompt' | Measure-Object Tokens -Sum).Sum
    InstructionTokens = ($analysis | Where-Object Type -eq 'Instruction' | Measure-Object Tokens -Sum).Sum
    TotalTokens = ($analysis | Measure-Object Tokens -Sum).Sum
    FilesWithMetadata = ($analysis | Where-Object HasMetadata).Count
    FilesNeedingMetadata = ($analysis | Where-Object { -not $_.HasMetadata }).Count
    FilesNeedingOptimization = ($analysis | Where-Object NeedsOptimization).Count
}

New-Item -ItemType Directory -Path ".copilot-sandbox" -Force | Out-Null
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$baseline | ConvertTo-Json | Out-File ".copilot-sandbox\\baseline-$stamp.json"
$analysis | Sort-Object Tokens -Descending | Select-Object -First 50 | Format-Table Type, Name, Tokens, HasMetadata, NeedsOptimization
```

## Follow-up Queries

```powershell
$analysis | Where-Object Tokens -gt 2500 | Sort-Object Tokens -Descending
$analysis | Where-Object Tokens -lt 500 | Sort-Object Tokens
$analysis | Where-Object { -not $_.HasMetadata } | Sort-Object Type, Name
```
