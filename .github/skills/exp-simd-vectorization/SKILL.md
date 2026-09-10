---
name: exp-simd-vectorization
description: Experimental optimization workflow for .NET SIMD and TensorPrimitives changes that stay benchmark-gated.
license: MIT
title: SIMD Vectorization
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1200
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - microbenchmarking
  - eval-performance
appliesTo: '**/*.{cs,csproj}'
tags:
  - exp
  - experimental
  - simd
  - performance
  - ste
---
# SIMD Vectorization

This skill remains experimental.
This skill changes code only when measurements justify complexity and correctness stays explicit.

## Decision Table

| Hot-path shape | Preferred first move | Escalation path |
|---|---|---|
| Bulk numeric math | `TensorPrimitives` or BCL vectorized helpers | Manual intrinsics only when helpers are insufficient |
| Byte or char scanning | Span helpers or vectorized BCL APIs | `Vector128` or `Vector256` intrinsics |
| Bitwise or fused loops | `Vector128` or `Vector256` intrinsics | `Vector512` only when hardware and benchmarks justify it |
| Mixed-width conversion | Library helper or simple scalar baseline | Intrinsics with clear fallback |

## Workflow

| Step | Agent action | Output |
|---|---|---|
| 1. Baseline | Agent captures benchmarks and alloc profiles for the exact hotspot. | Baseline data |
| 2. Screen | Agent confirms the path is hot, CPU-bound, and stable enough for low-level optimization. | Go or no-go decision |
| 3. Implement | Agent applies the smallest vectorized change that preserves readability and fallback behavior. | Experimental optimization |
| 4. Verify | Agent runs correctness tests over edge sizes and remainder loops. | Correctness evidence |
| 5. Compare | Agent reruns benchmarks on the same workload. | Before and after metrics |

## Quality Gate

| Check | Test | Pass criteria |
|---|---|---|
| Experimental labeling | Review touched docs or comments. | The optimization path stays labeled experimental when it is not standardized. |
| Correctness | Run targeted tests over empty, short, exact-width, and tail-length inputs. | Outputs match the scalar baseline. |
| Benchmark gain | Compare before and after benchmark results. | The vectorized version improves the chosen metric by a meaningful margin. |
| Fallback path | Review hardware-guard logic. | A safe scalar or narrower-vector fallback exists when hardware support is absent. |
