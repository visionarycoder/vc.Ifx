---
name: cwe-input-validation-fixes
title: CWE Input Verification & Buffer Safety Fixes
description: >
  Consolidated guidance for input-verification and bounds CWEs (CWE-20, CWE-119, 
  CWE-120, CWE-121, CWE-122, CWE-125, CWE-787) with detection patterns, fix patterns, 
  and measurable verification.
doc_type: skill
status: active
last_updated: 2026-08-31
target_audience: ai
complexity: medium
estimated_tokens: 1580
prerequisites:
  - Buildable repo (dotnet or native toolchain)
  - Sanitizers available in CI (ASan/UBSan) for native code
  - Static analyzers (clang-tidy, Coverity) and Roslyn analyzers
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - cwe-authentication-authz-fixes
  - cwe-injection-attack-fixes
  - cwe-data-protection-fixes
  - cwe-resource-management-fixes
  - cwe-miscellaneous-security-fixes
  - cwe-format-external-fixes
  - security-controller
appliesTo: '**/*.{cs,c,cpp,h,hpp,rs}'
tags:
  - cwe
  - input-validation
  - buffer-safety
  - fuzzing
---

# CWE Input Verification & Buffer Safety Fixes

Agent uses this bundle for unchecked input, unsafe copy logic, and bounds defects that expose memory or crash services.

## Activation

**USE FOR:** CWE-20, CWE-119, CWE-120, CWE-121, CWE-122, CWE-125, CWE-787.  
**DO NOT USE FOR:** Authentication defects, deserialization defects, or unrelated business rules.

## Critical Rules

| Rule | Test | Pass |
|---|---|---|
| Agent verifies type, length, format, and range at every trust boundary. | Run unit tests with null, empty, max, and malformed payloads. | Every invalid payload returns rejection before business logic runs. |
| Agent canonicalizes paths, URLs, and Unicode text before bounds logic. | Run path and Unicode boundary tests. | Canonical form is used in all path decisions. |
| Agent verifies size arithmetic before allocation or copy. | Run analyzers and boundary tests. | Zero new overflow findings. |
| Agent uses bounded APIs or safe containers for copy operations. | Run `rg -n "\b(strcpy|strcat|sprintf|gets|memcpy\(|alloca\()"`. | Zero new unsafe-copy hits in touched files. |
| Agent uses typed DTOs in managed code. Agent avoids raw `dynamic` parsing on untrusted input. | Run API tests with malformed JSON. | Invalid payloads return 400 or explicit reject result. |

## CWE Coverage

| CWE | Concern | Trigger Pattern | Fix Pattern | Verification |
|---|---|---|---|---|
| 20 | Improper input verification | `Request.Query`, `Request.Form`, `JsonElement`, `dynamic` on untrusted data | Use typed DTOs, binder types, attributes, or explicit parsers | Run invalid payload tests. Pass: Invalid input is rejected. |
| 119 | Memory-bounds violation | Manual index math, unsafe pointers, raw buffer copies | Use bounded APIs, spans, vectors, or explicit capacity guards | Run analyzers. Pass: Zero targeted findings. |
| 120 | Classic buffer overflow | `strcpy`, `sprintf`, `gets`, unbounded format strings | Replace with `snprintf`, `strlcpy`, `memcpy_s`, or length-guarded copy | Run grep. Pass: Zero targeted APIs remain in touched files. |
| 121 | Stack-based overflow | `alloca`, VLA, fixed stack buffers from user length | Cap length or move to heap allocation | Run boundary tests. Pass: Oversize input is rejected. |
| 122 | Heap-based overflow | `malloc(len)` from untrusted size, overflowed count math | Guard `count * sizeof(T)`, use `calloc` or safe containers | Run sanitizer tests. Pass: Zero heap overflow reports. |
| 125 | Out-of-bounds read | `offset + len` without bounds guard | Guard offset and remaining length before read | Run parser tests. Pass: Out-of-range reads return error. |
| 787 | Out-of-bounds write | Unchecked write offset or unchecked `Array.Copy` | Guard offset, length, and capacity before write | Run sanitizer tests. Pass: Zero write violations. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Inventory | Agent runs repo search for DTO bypasses, unsafe copies, stack allocation, and parser code. | Run `rg -n "Request\.(Query|Form)|JsonElement|dynamic" -g "**/*.cs"` and `rg -n "\b(strcpy|memcpy|memmove|gets|scanf|alloca)\b" -g "**/*.{c,cpp,h,hpp}"`. | Hotspot list exists for every touched file. |
| 2. Classify | Agent maps each finding to a CWE row in this bundle. | Review hotspot list. | Every finding has one CWE label or explicit out-of-scope note. |
| 3. Fix | Agent replaces unsafe copy logic, adds bounds guards, and adds typed parsing. | Run targeted build or compile command for touched component. | Touched component builds with zero new errors. |
| 4. Run tests | Agent runs boundary unit tests, parser tests, and sanitizer jobs when available. | Run existing unit tests and sanitizer jobs for touched component. | All targeted tests pass. |
| 5. Re-scan | Agent runs grep and analyzer checks again. | Re-run inventory commands and existing analyzers. | Zero remaining targeted hits in touched files, or each remaining hit has explicit rationale. |

## Decision Matrix

| Condition | Use |
|---|---|
| Managed code parses HTTP or JSON input | Use typed DTOs and framework model binding |
| Native code copies bytes or strings | Use bounded APIs or safe containers |
| Untrusted length controls allocation | Use explicit overflow guards before allocation |
| Untrusted length controls stack allocation | Reject oversize input or use heap allocation |
| Parser handles binary payloads | Run sanitizer and fuzz coverage after fix |

## Language Notes

| Language | Agent Uses | Agent Avoids |
|---|---|---|
| C# | DTOs, validation attributes, `Span<T>`, checked arithmetic | Raw `dynamic`, unchecked `Array.Copy`, unchecked `Buffer.BlockCopy` |
| C/C++ | `snprintf`, `strlcpy`, `memcpy_s`, `std::vector`, `std::string` | `strcpy`, `sprintf`, `gets`, unchecked `malloc` math |
| Rust | `Vec`, slices, checked indexing, `checked_*` math | `unsafe` copy paths without bounds proof |

## Verification Checklist

Agent verifies:
- [ ] Boundary tests cover null, empty, max, and malformed payloads
- [ ] Existing analyzers report zero new targeted findings
- [ ] Existing sanitizer jobs report zero out-of-bounds defects
- [ ] Touched parsers reject oversize length fields
- [ ] Touched files contain zero new unsafe-copy APIs

## Inputs

| Input | Required | Default |
|---|---|---|
| Project or repo path | Yes | - |
| Language scope | No | All matching files |
| CWE subset | No | All 7 |

## Outputs

Agent generates:
- Hotspot list with CWE labels
- Patch set that adds guards or safe APIs
- Boundary unit tests or parser tests when the repo already contains them
- Verification notes with grep, analyzer, and sanitizer results

## References

- CWE: https://cwe.mitre.org/
- Microsoft secure coding guidance: https://learn.microsoft.com/security/develop/secure-coding
- AddressSanitizer: https://clang.llvm.org/docs/AddressSanitizer.html
- LibFuzzer: https://llvm.org/docs/LibFuzzer.html
