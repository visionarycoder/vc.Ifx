---
title: Input Validation Hardening Reference
doc_type: reference
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: low
estimated_tokens: 300
---
# Input Validation Hardening Reference

Agent standardizes request validation behavior for DTOs, route/query parameters, model-state failures.

## Use For

Use when:
- Hardening DTO constraints, request guards
- Ensuring predictable 400 responses for invalid input
- Normalizing validation behavior across endpoints

## Workflow

Agent performs:

1. Agent identifies DTO, route, query inputs per endpoint
2. Agent applies explicit validation rules, guard clauses
3. Agent aligns invalid-input responses to consistent error contracts
Test: POST invalid payload
Pass: Status = 400. Response body = RFC 9457 Problem Details.
4. Agent adds or updates targeted tests for invalid, boundary payloads
Test: Run `dotnet test [ValidationTests].csproj`
Pass: All validation tests pass.

## Outputs

Agent produces:
- Validation guard changes
- Predictable invalid-input response behavior
- Test impact summary
