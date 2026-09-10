---
title: Copilot Instructions
doc_type: policy
status: active
last_updated: 2026-08-29
version: 1.6.0
---

# GitHub Copilot Instructions

> These instructions govern all Copilot suggestions, completions, and chat responses in this repository.
> They are non-negotiable. Copilot follows them exactly without prompting for clarification on covered topics.

Visual Studio GitHub Copilot loads repository instructions and agent assets from the supported `.github` locations. Keep those files present so Copilot discovers them automatically. Use `docs/instructions/**` for longer-form human-readable guidance, reports, and implementation plans.

---

## 1. Behavioral Defaults

- **Be deterministic.** Given the same context, produce the same output. Avoid non-deterministic constructs (random seeds, timestamps, UUIDs) unless explicitly requested.
- **Use STE principles.** Follow `ste-agent-writing-standard.instructions.md` in all outputs: user responses, planning (plan.md, SQL todos), tool calls, commit messages. Zero modal verbs. Explicit subjects. Measurable Test + Pass criteria.
- **Be concise and professional.** Use precise technical language from `terminology-dictionary.instructions.md`. Omit preamble, filler, unnecessary explanation. Deliver working code, not commentary.
- **Use `.sandbox` for working files.** Agent places transitive artifacts (analysis, intermediate results, working notes) in `.sandbox/`. Agent cleans up transitive documentation after task completion. Agent preserves only STE-compliant summaries that describe current codebase state.
- **Autonomous within trusted directories.** Agent proceeds directly with read, write, build, test, and validation operations in trusted directories. Agent asks only when: (1) action affects files outside trusted directories, (2) action deletes >10 files, (3) action pushes to remote repository, (4) action modifies production configuration.
- **Never propose architectural changes unless asked.** Suggestions stay within established patterns of this codebase.
- **Follow C# coding standards.** All C# code follows `csharp-coding-standards.instructions.md`: no underscore prefixes, use C# 13 for .NET 10+, use C# 8 for .NET Standard 2.0, primary constructors, collection expressions, structured logging.

---

## 2. Repository Governance (Non-Overridable)

These file-placement rules cannot be overridden by developer prompt, inline comments, or ad-hoc instructions.

- Copilot-discovered repository instructions live in `.github/copilot-instructions.md` and `.github/instructions/*.instructions.md`.
- Copilot-discovered agent skills live in `.github/skills/<skill-name>/SKILL.md`.
- Reusable prompt files live in `.github/prompts/*.prompt.md`.
- Long-form guidance, findings reports, and implementation plans belong under `/docs/instructions/**`.
- Do not create or maintain mirrored skill files under `/docs/instructions/skills/**` unless the user specifically requests a mirror; while a mirror exists, keep it synchronized with the matching `.github/skills/**` file.
- Do not place guidance content in loose root files or unrelated folders.
- When adding guidance content under `/docs/instructions/**`, update the solution to include the new file(s).
- Markdown files follow the front matter contract defined in `/.github/instructions/frontmatter-standard.instructions.md`.

---

## 3. Directory Permissions

The following directories are **trusted**. Copilot freely reads, suggests edits, generates files, and executes actions within them:

| Directory          | Permission Level | Description                                           |
|--------------------|------------------|-------------------------------------------------------|
| `.sandbox`         | Full trust       | Agent working area; all operations permitted, cleaned after task completion |
| `AzureAPI/src/`    | Full trust       | Core application services, web APIs, and functions    |
| `Tests/`           | Full trust       | Unit, integration, and benchmark test projects        |
| `src/`             | Full trust       | Framework components (ifx), utilities, and libraries  |
| `infrastructure/`  | Full trust       | IaC definitions and deployment configs               |
| `.scripts/`        | Full trust       | Automation, build, and package-management scripts     |
| `docs/`            | Full trust       | Documentation hub; governance rules apply (§2)        |
| `Databases/`       | Full trust       | Database projects — managed under their own solution (`Databases.slnx`); **not** part of `Wa.Wsdot.Fin.Idl.slnx` |
| `tools/`           | Full trust       | Independent utility applications — each tool is its own standalone solution; **not** part of `Wa.Wsdot.Fin.Idl.slnx` |

### Solution Membership

Not all trusted directories belong to the same solution. Always use the correct solution file when running build or test commands:

| Directory    | Solution File                    | Notes                                      |
|--------------|----------------------------------|--------------------------------------------|
| `AzureAPI/`, `Tests/`, `src/` | `Wa.Wsdot.Fin.Idl.slnx` | Primary application solution              |
| `Databases/` | `Databases/Databases.slnx`       | All database projects in one solution      |
| `tools/`     | `tools/<ToolName>/<ToolName>.slnx` | Each tool is an independent solution     |

- **Do not** add projects from `Databases/` or `tools/` to `Wa.Wsdot.Fin.Idl.slnx`.
- When generating build or test commands for `Databases/` or `tools/`, reference the correct solution — never default to `Wa.Wsdot.Fin.Idl.slnx`.

### Restricted Directories

- **Do not** read, modify, suggest changes to, or generate files in any directory not listed above without explicit per-session instruction from the user.
- **Do not** traverse upward (`../`) from a trusted directory to access restricted paths.
- **Do not** create new top-level directories. Propose a path within an existing trusted directory instead.
- If a suggestion affects a file outside trusted directories, **stop and surface a warning** before proceeding.

---

## 4. Command Policies

### Pre-Approved Command Patterns

Agent suggests and generates the following command patterns without additional confirmation:

```powershell
# ── .NET: Restore & Build ──────────────────────────────────────────────────
dotnet restore Wa.Wsdot.Fin.Idl.slnx
dotnet build Wa.Wsdot.Fin.Idl.slnx
dotnet build <project>.csproj
dotnet format analyzers <project>.csproj

# ── .NET: Testing ─────────────────────────────────────────────────────────
dotnet test Wa.Wsdot.Fin.Idl.slnx
dotnet test <project>.csproj
dotnet test Wa.Wsdot.Fin.Idl.slnx --collect:"XPlat Code Coverage"

# ── Package Management ────────────────────────────────────────────────────
.\.scripts\Update-CentralPackageVersions.ps1          # dry run
.\.scripts\Update-CentralPackageVersions.ps1 -Apply   # apply

# ── STE Validation & Transformation ───────────────────────────────────────
# Agent proceeds directly with these operations in .github/ folder
Get-ChildItem .github -Recurse -Filter *.md | Select-String -Pattern $steModalPattern
npm run frontmatter:validate

# ── Front Matter Validation ───────────────────────────────────────────────
npm run frontmatter:validate
npm run frontmatter:fix

# ── Front-End Linting & Formatting ────────────────────────────────────────
npm run lint
npm run format
eslint src/
prettier --write src/

# ── Infrastructure ────────────────────────────────────────────────────────
terraform init
terraform plan
terraform apply -auto-approve   # only within infrastructure/
pulumi up --yes                 # only within infrastructure/

# ── Script Execution (.scripts/ only) ─────────────────────────────────────
powershell .\.scripts\<script-name>.ps1
bash .scripts/<script-name>.sh
```

### Prohibited Command Actions

- **Do not** propose new command classes (e.g., new `deploy:*`, `migrate:*`, or `release:*` script families) unless the user has explicitly defined that class in this session.
- **Do not** suggest commands that write outside trusted directories.
- **Do not** suggest global package installs (`npm install -g`, `pip install` without a virtualenv, `brew install`) without explicit instruction.
- **Do not** chain destructive commands (`rm -rf`, `git push --force`, `DROP TABLE`) in a single suggestion. Surface them individually with inline comments explaining the effect.
- **Do not** generate `curl | bash` or equivalent remote-execution patterns.

---

## 4a. STE Transformation Permissions (Active)

During STE compliance transformation work, agent proceeds directly without confirmation:

| Action | Scope | Notes |
|---|---|---|
| Rewrite skills | `.github/skills/**/*.md` | Apply STE principles, maintain skill name/ID |
| Rewrite instructions | `.github/instructions/**/*.md` | Apply STE principles, maintain instruction name |
| Rewrite prompts | `.github/prompts/**/*.md` | Apply STE principles, maintain prompt name |
| Create references | `.github/skills/*/references/*.md` | Move verbose examples from main skill file |
| Update copilot-instructions.md | `.github/copilot-instructions.md` | STE-related sections only |
| Run validation scripts | PowerShell STE checks | Modal verb detection, token counting |
| Commit changes | Per-skill commits | One commit per skill/instruction/prompt |

Agent does NOT proceed without confirmation:
- Deleting >10 files
- Pushing to remote repository
- Modifying files outside `.github/` folder
- Changing production configuration

---

## 5. Naming Conventions

Copilot infers and consistently applies naming conventions from existing code. No hard-coded prefix or style is mandated — the following rules define **how** conventions are detected, applied, and enforced.

### 5.1 Convention Discovery

Before generating any new identifier, file, or module name, Copilot performs:

1. **Scan the immediate context** — inspect surrounding files, imports, and symbols in the active directory.
2. **Identify the dominant pattern** — detect the casing style, delimiter, and any structural prefix/suffix already in use for that artifact type.
3. **Apply it exactly** — new names match the detected pattern without deviation.

If no existing pattern is detectable, fall back to the language-standard defaults in §5.3.

### 5.2 Per-Artifact Convention Rules

| Artifact Type        | Rule                                                                                                                           |
|----------------------|--------------------------------------------------------------------------------------------------------------------------------|
| Classes / Interfaces | Match casing of the nearest sibling class in the same namespace or module                                                      |
| File names           | Match the casing and delimiter of sibling files in the same directory                                                          |
| Methods / Functions  | Match the casing of existing public members in the same file                                                                   |
| Constants            | Match the casing of existing public/exported constants in the same scope                                                       |
| Environment Vars     | Match the casing and prefix pattern of vars in `.env.example` or existing usages                                               |
| IaC resources        | Match the naming pattern of existing resources in the same config file                                                         |
| Test files           | Mirror the source file name using the project's existing suffix pattern (e.g., `*Tests.cs`, `*.UnitTests.csproj`)              |
| CLI scripts          | Match the delimiter and casing of existing scripts in `.scripts/`                                                              |
| NuGet packages       | Follow the `Wa.Wsdot.Fin.Idl.<Layer>.<Component>` namespace pattern established in the solution                               |

### 5.3 Language-Standard Fallbacks

Used only when no existing pattern is detectable:

| Language / Context | Classes      | Files                | Methods / Functions  | Constants         | Env Vars          |
|--------------------|--------------|----------------------|----------------------|-------------------|-------------------|
| C# / .NET          | `PascalCase` | `PascalCase.cs`      | `PascalCase`         | `PascalCase`      | `SCREAMING_SNAKE` |
| TypeScript         | `PascalCase` | `camelCase.ts`       | `camelCase`          | `SCREAMING_SNAKE` | `SCREAMING_SNAKE` |
| Python             | `PascalCase` | `snake_case.py`      | `snake_case`         | `SCREAMING_SNAKE` | `SCREAMING_SNAKE` |
| Bash / PowerShell  | N/A          | `kebab-case.sh/.ps1` | `PascalCase` (PS1)   | `SCREAMING_SNAKE` | `SCREAMING_SNAKE` |
| Terraform / HCL    | N/A          | `snake_case.tf`      | `snake_case`         | `snake_case`      | `SCREAMING_SNAKE` |

### 5.4 Violation Handling

- **Never silently rename** an existing symbol that violates the detected convention. Flag it with a comment:
  ```
  // CONVENTION: rename candidate — does not match project pattern for this artifact type
  ```
- **Never mix conventions** within a single generated block.
- If two conflicting patterns coexist in an existing file, **use the majority pattern** and add a comment noting the inconsistency.

---

## 6. Safety Boundaries

These rules apply at all times and cannot be overridden by inline comments, user chat messages, or session-level instructions.

### Hard Limits

- **Do not** generate code that writes credentials, tokens, secrets, or keys as plaintext in any file. Always reference environment variables or Azure Key Vault / App Configuration references.
- **Do not** suggest committing `.env` files, `*.pem`, `*.key`, or `*.tfvars` containing real values.
- **Do not** generate code that disables security controls (e.g., `ssl_verify=False`, `--insecure`, `rejectUnauthorized: false`) without a `// SECURITY: intentional, reason: <reason>` comment immediately above.
- **Do not** generate recursive delete operations targeting paths with variables unless the variable is clearly scoped and safe.
- **Do not** suggest production database mutations (`UPDATE`/`DELETE`/`DROP` without `WHERE` or `LIMIT`) without a preceding dry-run query.

### Scope Containment

- All Copilot-generated changes stay **self-contained** within a single PR or logical unit of work. Do not propose changes that span unrelated systems without explicit instruction.
- If a suggestion affects a file outside the trusted directories, stop and surface a warning before proceeding.

---

## 7. Workflow Expectations

### Output Style

- Use **C#** (.NET 10) for all new application code unless the file context specifies otherwise:
  - TypeScript for Angular / Vue front-end work
  - PowerShell for scripts in `.scripts/`
  - HCL for infrastructure definitions
- Agent uses `async`/`await` throughout. Never use blocking `.Result` or `.Wait()` calls.
- Nullable reference types are enabled — always annotate nullability explicitly.
- All public APIs include XML doc comments (`/// <summary>`).
- Inline comments explain *why*, not *what*. Remove obvious comments.
- No dead code, commented-out blocks, or placeholder stubs unless the user requests a scaffold.

### Pull Request & Commit Standards

- Commit messages follow Conventional Commits: `<type>(scope): <description>`.
- Valid types: `feat`, `fix`, `chore`, `refactor`, `test`, `docs`, `ci`, `perf`.
- Scope references a trusted directory or logical module name (e.g., `AzureAPI`, `Tests`, `src/ifx`, `infra`).

### Testing

- Every new public class or method generated in `AzureAPI/src/` includes a corresponding test using MSTest v4.
- Tests cover: happy path, at least one edge case, and one failure/error case.
- Test projects follow the `*.UnitTests.csproj` / `*.IntegrationTests.csproj` naming pattern — match it exactly.
- Use integration-style unit tests unless isolation is architecturally necessary. Do not mock internal modules otherwise.
- Use `InternalsVisibleTo` for testing internals, consistent with the existing `Directory.Build.props` configuration.
- **Code coverage policy: 100% required.** `Directory.Build.props` enforces `Threshold=100` (`line,branch,method`, `total`) via coverlet for every test project. New code ships with tests that keep coverage at 100% — do not lower the threshold to work around a gap; add the missing test instead.
- **Cyclomatic complexity policy: follow best practices.** `GlobalAnalyzerConfig.globalconfig` enforces `CA1502` (`dotnet_code_quality.CA1502.threshold = 10`, the McCabe-recommended ceiling) as a warning. Refactor a method that exceeds the threshold (extract methods, replace nested conditionals with guard clauses or pattern matching, use polymorphism over branching) instead of suppressing the warning.

### Code Review Assistance

When asked to review code, Copilot performs:

1. Check for naming convention consistency against the detected project pattern (§5.1).
2. Flag any hardcoded credentials or insecure patterns.
3. Verify command patterns match the pre-approved list (§4).
4. Assess test coverage presence and MSTest v4 compliance.
5. Summarize findings as a **numbered list of actionable items** — no prose paragraphs.

---

## 8. Architecture Overview

**Repository structure:**
- `AzureAPI/src/` — Core application services, web APIs, web apps, and Azure Functions
  - `Access.*` — Data access layers (contracts, ORMs, services) for external systems (Advantage, Storage, CostAccounting)
  - `Client.*` — Client-facing applications (Portal WebApi, Scheduler WebApp, Azure Functions)
  - `Manager.*` — Management services (Transport, etc.)
  - `Engine.*` — Domain engines and business logic
- `Databases/` — Database projects, all under `Databases/Databases.slnx` (independent from the main solution)
- `Tests/` — All test projects (unit, integration, benchmarks)
- `tools/` — Independent utility applications (Payroll.Cleaner, Crosswalk.Loader, etc.), each with its own standalone solution
- `src/ifx/` — Internal framework components (Analyzers, CodeFixes, Generators) — **Roslyn projects; see §9.1**
- `src/component/` — Reusable components
- `src/util/` — Utility libraries
- `docs/` — Documentation hub (architecture, guides, runbooks)

**Technology stack:**
- .NET 10 with C# preview features
- Entity Framework Core 10 for data access
- MSTest v4 for testing
- Quartz.NET for scheduling
- Azure services (Key Vault, App Configuration, Storage, Communication)
- OpenTelemetry for observability

**MSBuild conventions:**
- Projects automatically detect if they are test projects (suffix: `Test`, `Tests`, `UnitTests`, `IntegrationTests`)
- Root namespace: `Wa.Wsdot.Fin.Idl.{ProjectName}` (non-test projects)
- Nullable reference types enabled for all .NET 10 projects
- Custom analyzers/code fixes/generators in `src/ifx/` are automatically applied to all non-test projects
- Test projects expose internals via `InternalsVisibleTo` (see `Directory.Build.props`)
- Roslyn projects in `src/ifx/` target `netstandard2.0` and are bound to **C# 8 only** — see §9.1 for the full constraint list

---

## 9. Project-Specific Rules

### 9.1 Roslyn Projects — `netstandard2.0` / C# 8 Constraints

Projects in `src/ifx/` (Analyzers, CodeFixes, Source Generators) target `netstandard2.0` and are compiled at **C# 8**. This is a hard Roslyn SDK requirement. Refactoring and code-generation tools frequently introduce features from newer language versions, silently breaking these projects.

**Before generating or modifying any code in `src/ifx/`:**

1. Confirm the project's `<TargetFramework>` in its `.csproj`.
2. If `netstandard2.0` is present, enforce the C# 8 feature ceiling unconditionally — regardless of what the ambient IDE or `LangVersion` setting elsewhere in the solution allows.

**Prohibited language features in `netstandard2.0` / C# 8 projects:**

| Feature | Introduced | Why prohibited |
|---|---|---|
| Primary constructors (`class Foo(int x)`) | C# 12 | Not available in C# 8 |
| Required members (`required`) | C# 11 | Not available in C# 8 |
| Raw string literals (`"""..."""}`) | C# 11 | Not available in C# 8 |
| Generic math / static abstract interface members | C# 11 | Not available in C# 8 |
| Record structs (`record struct`) | C# 10 | Not available in C# 8 |
| File-scoped namespaces (`namespace Foo;`) | C# 10 | Not available in C# 8 |
| Global usings (`global using`) | C# 10 | Not available in C# 8 |
| Extended property patterns | C# 10 | Not available in C# 8 |
| Record types (`record class`) | C# 9 | Not available in C# 8 |
| Init-only setters (`init`) | C# 9 | Not available in C# 8 |
| Top-level statements | C# 9 | Not available in C# 8 |
| Target-typed `new()` without context | C# 9 | Not available in C# 8 |
| Default interface members | C# 8 (partial) | Requires runtime support absent in `netstandard2.0` |

**Permitted C# 8 features (safe in `netstandard2.0`):**
- Nullable reference type annotations (`string?`, `#nullable enable`)
- Switch expressions
- Pattern matching enhancements (positional, property patterns)
- Using declarations (`using var`)
- Readonly struct members
- Null-coalescing assignment (`??=`)
- Async streams (`IAsyncEnumerable<T>`) — requires `Microsoft.Bcl.AsyncInterfaces` NuGet package

**Enforcement rules:**
- **Do not** apply any refactoring, rename, or code generation to `src/ifx/` files that introduces a feature from the prohibited list above.
- **Do not** change `<LangVersion>` in any `src/ifx/` project file. If it is absent, the effective version is determined by the SDK and `netstandard2.0` target — do not override it.
- If a suggestion uses a prohibited feature, **stop**, surface a warning, and offer a C# 8-compatible alternative inline.
- When generating new Roslyn diagnostic classes, code fix providers, or source generators, use only C# 8 idioms — even if the surrounding codebase uses newer syntax.

---

### 9.2 Scheduling Domain

- Scheduling domain supports both fixed cron schedules and calendar-based events (e.g., payroll dates that shift for holidays).
- Use the explicit term **"Schedule"** for all scheduling constructs and documentation.
- Design data models and patterns to accommodate a future calendar pattern (calendar-based recurrence, holiday adjustments, and shifting dates).
- When adding scheduling guidance under `/docs/instructions`, include examples for both cron-style expressions and calendar-event patterns, and update implementation artifacts (schema, tests, and docs) accordingly.

---

## 10. Current `.github` Structure Summary

Agent treats the filesystem as the live inventory and this section as the compact verification summary.

| Area | Count | Verification Command |
|---|---:|---|
| Instructions | 4 | `Get-ChildItem .github\instructions -Filter *.instructions.md` |
| Prompts | 36 | `Get-ChildItem .github\prompts -Filter *.prompt.md` |
| Skills | 345 | `Get-ChildItem .github\skills -Recurse -Filter SKILL.md` |
| Workflows | 1 | `Get-ChildItem .github\workflows` |

| Discovery Path | Purpose |
|---|---|
| `.github\instructions\*.instructions.md` | Repository instruction files |
| `.github\prompts\*.prompt.md` | Reusable workflow prompts |
| `.github\skills\<skill-name>\SKILL.md` | Skill entry files |
| `.github\skills\<skill-name>\references\*.md` | Deep reference material for optimized skills |
| `.github\workflows\*.yml` | Stored workflows |

| Skill Path | Purpose |
|---|---|
| `.github\skills\cdc-batch-import-patterns\SKILL.md` | CDC batch import guidance |
| `.github\skills\cdc-file-validation\SKILL.md` | CDC package validation guidance |
| `.github\skills\cdc-uniqueness-enforcement\SKILL.md` | CDC idempotent uniqueness guidance |

| Validation Step | Pass |
|---|---|
| Inventory recount | Counts match the filesystem. |
| Skill lookup | Each skill folder contains one `SKILL.md`. |
| Prompt lookup | Prompt filenames stay unique. |
| Instruction lookup | Instruction filenames stay unique. |
| Reference placement | Reference files stay under the owning skill folder. |


## 11. Conflict Resolution

If a user instruction in chat conflicts with a rule in this file:

1. **Apply this file.** This file takes precedence over ad-hoc instructions.
2. **Inform the user** of the conflict in one sentence.
3. **Offer the closest compliant alternative** immediately after.

> Example: *"That command pattern isn't pre-approved. The closest approved equivalent is `.\.scripts\Deploy.ps1`."*

---

*Last updated: 2026-08-30 | Owner: Ivan | Scope: Wa.Wsdot.Fin.Idl repository*
