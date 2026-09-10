---
name: antipattern-detection-bundle
title: Anti-Pattern Detection Bundle
description: Route anti-pattern detection to specialized skills (performance, code smell, design smell, architectural debate) and consolidate findings.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: high
estimated_tokens: 1800
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - performance-antipatterns
  - code-smell-detection
  - design-smell-detection
  - architectural-decision-debate
  - refactor
  - dotnet-code-quality-standards
  - analyzing-dotnet-performance
related_docs:
  - https://learn.microsoft.com/en-us/azure/architecture/antipatterns/
  - https://en.wikipedia.org/wiki/Anti-pattern
  - https://en.wikipedia.org/wiki/Code_smell
  - https://en.wikipedia.org/wiki/Design_smell
appliesTo: '**/*.{cs,csproj,md,json}'
tags:
  - antipatterns
  - code-smell
  - design-smell
  - refactoring
  - performance
  - quality
  - bundle
---

# Anti-Pattern Detection Bundle

Agent uses this skill to identify, challenge, and remediate anti-patterns before creation or during refactoring across performance, code quality, design, and architectural dimensions.

## When to Use

| Condition | Use |
|---|---|
| Agent reviews code or design for quality issues | Use this skill |
| Developer proposes new pattern or architecture | Use this skill |
| Refactoring work targets existing codebase | Use this skill |
| Performance or scalability concerns exist | Use this skill |
| Code review identifies potential anti-pattern | Use this skill |

## When Not to Use

| Condition | Alternative |
|---|---|
| Specific Azure anti-pattern already identified | Use `performance-antipatterns` directly |
| Code smell analysis only | Use `code-smell-detection` directly |
| Design debate without anti-pattern context | Use `architectural-decision-debate` directly |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Code, design, or architecture description | Yes | Context for anti-pattern detection |
| Scope | Yes | New code, existing code, or design proposal |
| Performance or quality concerns | No | Specific symptoms guide detection |

## Anti-Pattern Detection Framework

Agent applies layered anti-pattern detection by delegating to specialized skills:

### Detection Layer Routing

| Layer | Skill | Token Cost | Coverage |
|---|---|---|---|
| Performance & Scalability | `performance-antipatterns` | ~1200 | 10 Azure anti-patterns with references |
| Code Quality | `code-smell-detection` | ~1100 | 10 code smells with references |
| Structural Design | `design-smell-detection` | ~900 | 7 design smells |
| Architectural Decisions | `architectural-decision-debate` | ~3200 | 3-position debate framework |

**Total bundle token cost:** ~6400 tokens (excluding reference files)

### When to Use Sub-Skills Directly

- **Known anti-pattern category:** Use specific skill directly
- **Broad quality review:** Use this bundle to scan all dimensions
- **Decision debate needed:** Use `architectural-decision-debate`

## Quick Reference

**Azure Performance Anti-Patterns:**
- Busy Database, Busy Front End, Chatty I/O, Extraneous Fetching
- Improper Instantiation, Monolithic Persistence, No Caching
- Noisy Neighbor, Retry Storm, Synchronous I/O

**Code Smells:**
- Duplicated Code, Long Method, Large Class, Too Many Parameters
- Data Clump, Shotgun Surgery, Feature Envy, Magic Numbers
- Primitive Obsession, Refused Bequest

**Design Smells:**
- Cyclic Dependencies, Tight Coupling, Insufficient Modularization
- Broken Hierarchy, Missing Abstraction, Unstable Dependencies

**Detailed guidance:** Each sub-skill contains `references/` directory with specific anti-pattern remediation examples.

## Workflow

1. Agent identifies scope (new code, existing code, design proposal).
2. Agent routes to appropriate detection skill(s):
   - Performance concerns → `performance-antipatterns`
   - Code quality concerns → `code-smell-detection`
   - Structural concerns → `design-smell-detection`
   - Decision debate → `architectural-decision-debate`
3. Agent consolidates findings from sub-skills.
4. Agent reports detected anti-patterns with severity, evidence, and impact.
5. Agent proposes refactoring alternatives or design changes.
6. Agent validates proposed changes eliminate anti-pattern without introducing new issues.

Test: Run static analysis, build, and tests after remediation.
Pass: Zero new defects. Anti-pattern eliminated. All tests pass.

## Detection Rules Matrix

| Rule | Dimension | Detection Pattern | Remediation |
|---|---|---|---|
| ANTI-001 | Azure | Database performs business logic | Move logic to application tier |
| ANTI-002 | Azure | Request threads perform CPU-intensive work | Use background processing |
| ANTI-003 | Azure | Multiple small HTTP calls in loop | Batch requests or use bulk operations |
| ANTI-004 | Azure | Fetching entire dataset when subset needed | Add filtering, projection, pagination |
| ANTI-005 | Azure | Creating DbContext or HttpClient per request | Use DI with correct lifetime |
| ANTI-006 | Azure | Single database for OLTP and analytics | Separate read and write stores |
| ANTI-007 | Azure | No caching for frequently read static data | Add caching layer with expiration |
| ANTI-008 | Azure | Synchronous I/O in async method | Use true async I/O operations |
| ANTI-009 | Code | Duplicated logic blocks >10 lines | Extract to shared method or class |
| ANTI-010 | Code | Method exceeds 50 lines | Split into smaller focused methods |
| ANTI-011 | Code | Class has >10 public methods | Apply Single Responsibility Principle |
| ANTI-012 | Code | Method has >5 parameters | Introduce parameter object or builder |
| ANTI-013 | Code | Magic numbers without explanation | Replace with named constants |
| ANTI-014 | Code | Primitive types for domain concepts | Create value objects |
| ANTI-015 | Design | Circular namespace or project references | Break cycle with abstraction or event |
| ANTI-016 | Design | Concrete class depends on concrete class | Introduce interface or abstraction |
| ANTI-017 | Arch | No clear layer separation | Define and enforce architectural boundaries |
| ANTI-018 | Arch | Dead code remains uncommented | Remove or document as intentional |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Static analysis | `dotnet format analyzers` or static analyzer | Zero new violations |
| Code metrics | Analyze complexity, maintainability index | Metrics improve or stay within thresholds |
| Build verification | `dotnet build` | Zero compile errors |
| Test verification | `dotnet test` | All tests pass |
| Performance baseline | Compare before/after performance | Performance improves or remains stable |

## Prevention Guidance

Agent provides anti-pattern prevention when generating new code:

- **Before adding database logic:** Consider if logic belongs in application tier
- **Before synchronous I/O:** Use async/await throughout
- **Before caching bypass:** Check if data is frequently read and static
- **Before concrete dependency:** Introduce abstraction for testability
- **Before inline logic duplication:** Extract to reusable component
- **Before large method:** Split into focused single-purpose methods

## Remediation Workflow

When anti-pattern detected:

1. Agent classifies anti-pattern by dimension and severity.
2. Agent collects evidence (code snippets, metrics, references).
3. Agent explains why pattern is problematic (performance, maintainability, testability).
4. Agent proposes specific refactoring with before/after examples.
5. Agent estimates impact (lines changed, test changes, risk).
6. Agent validates remediation eliminates issue.

## Integration with Other Skills

| Anti-Pattern Type | Primary Skill | Secondary Skills |
|---|---|---|
| Azure performance anti-patterns | `performance-antipatterns` | `analyzing-dotnet-performance`, `optimizing-ef-core-queries` |
| Code smells | `code-smell-detection` | `refactor`, `dotnet-code-quality-standards` |
| Design smells | `design-smell-detection` | `dotnet-architectural-layers`, `dependency-injection-patterns` |
| Architectural decisions | `architectural-decision-debate` | `technology-selection`, `api-gateway-patterns` |

## Output Format

Agent reports detected anti-patterns in structured format:

```
Anti-Pattern Detection Report
━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Found: 3 anti-patterns

1. [HIGH] Busy Database
   Location: CustomerRepository.cs, lines 45-78
   Evidence: Stored procedure performs complex business calculations
   Impact: Database CPU contention under load
   Remediation: Move calculation logic to CustomerEngine service
   Effort: ~4 hours (extract logic, add unit tests)

2. [MEDIUM] Duplicated Code
   Location: OrderService.cs (lines 120-135), InvoiceService.cs (lines 88-103)
   Evidence: Identical tax calculation logic in both classes
   Impact: Maintenance burden, inconsistency risk
   Remediation: Extract to TaxCalculator utility class
   Effort: ~1 hour

3. [LOW] Magic Numbers
   Location: DiscountService.cs, lines 23, 45, 67
   Evidence: Unexplained numeric literals (0.15, 0.20, 0.25)
   Impact: Reduced readability
   Remediation: Replace with named constants (StandardDiscount, PremiumDiscount, VipDiscount)
   Effort: ~30 minutes
```

## Verification Checklist

Agent verifies:
- [ ] All four detection dimensions scanned
- [ ] Detected anti-patterns classified by severity
- [ ] Evidence collected for each detection
- [ ] Remediation proposed with effort estimate
- [ ] No new anti-patterns introduced by remediation
- [ ] Performance and quality metrics remain stable or improve
