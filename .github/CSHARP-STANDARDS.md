---
title: C# Coding Standards - Quick Reference
doc_type: reference
status: active
last_updated: 2026-09-10
---

# C# Coding Standards - Quick Reference

## Overview

C# coding standards are now enforced repository-wide through GitHub Copilot.

**Full Documentation:** `.github/instructions/csharp-coding-standards.instructions.md`

---

## Quick Rules

### 1. No Underscore Prefixes ❌

```csharp
// ❌ WRONG
private readonly ILogger _logger;

// ✅ CORRECT
private readonly ILogger logger;
```

### 2. Use Latest C# Features

| Target Framework | C# Version | Key Features |
|------------------|------------|--------------|
| .NET 10+ | C# 14+ | Primary constructors, collection expressions `[]` |
| .NET Standard 2.0 | C# 14+ | Nullable reference types, using declarations |

### 3. Modern Patterns (.NET 10+)

```csharp
// Primary constructor
public sealed class ExampleEngine(ILogger<Service> logger, ITelemetryService telemetryService) 
    : ServiceBase<ExampleEngine>(logger) IExampleEngine
{
    // Parameters become fields automatically
    public void Method() => logger.LogInformation("Info");
}

// Collection expressions
List<Material> materials = [];
IReadOnlyCollection<string> errors = [ex.Message];

// Target-typed new
return new ConvertResponse
{
    Result = new() { IsSuccess = true }
};
```

### 4. Structured Logging

```csharp
// ✅ CORRECT
logger.LogInformation("Processing {FileName} with {Count} records", fileName, count);

// ❌ WRONG
logger.LogInformation($"Processing {fileName} with {count} records");
```

---

## Enforcement

- **Copilot Integration:** Standards referenced in `.github/copilot-instructions.md`
- **Automatic Application:** All Copilot suggestions follow these rules
- **Non-Negotiable:** No exceptions without explicit override

---

## Files

```
.github/
├── copilot-instructions.md              (main Copilot config - references standards)
└── instructions/
    └── csharp-coding-standards.instructions.md  (full documentation)
```

---

**Updated:** 2026-08-19  
**Scope:** All C# code in repository  
**Status:** Active and enforced
