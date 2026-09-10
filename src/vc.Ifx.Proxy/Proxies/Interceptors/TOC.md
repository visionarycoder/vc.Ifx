---
title: Interceptors Framework Table of Contents
doc_type: reference
status: active
last_updated: 2026-08-19
summary: Complete index of interceptor implementations, supporting files, and documentation
tags:
  - interceptors
  - toc
  - reference
audience: developer
---

# Interceptors Framework — Table of Contents

## Quick Navigation

| Need | Link |
|---|---|
| **Getting Started** | [QUICK-START.md](QUICK-START.md) |
| **Integration Patterns** | [USAGE.md](USAGE.md) |
| **Complete Reference** | [README.md](README.md) |
| **Historical Changes** | [REFACTORING-SUMMARY.md](REFACTORING-SUMMARY.md) |
| **Structure Verification** | [VERIFICATION-STRUCTURE.md](VERIFICATION-STRUCTURE.md) |

---

## Interceptor Implementations (13 total)

### Security Category (3 interceptors)

#### 1. Authentication

**File:** [Authentication/JwtAuthenticationInterceptor.cs](Authentication/JwtAuthenticationInterceptor.cs)

**Supporting Files:**
- [Authentication/IJwtTokenValidator.cs](Authentication/IJwtTokenValidator.cs) — Contract for JWT token validation
- [Authentication/RequireAuthenticationAttribute.cs](Authentication/RequireAuthenticationAttribute.cs) — Method-level authentication marker

**Documentation:** [Authentication/README.md](Authentication/README.md)

**Purpose:** Establish authenticated identity from JWT token and populate ClaimsPrincipal

**Key Features:**
- JWT token extraction from multiple sources (MethodContext.Items, method arguments)
- Token validation via IJwtTokenValidator contract
- Principal establishment in MethodContext.Items
- Attribute-based opt-in/opt-out

---

#### 2. Authorization

**File:** [Authorization/AuthorizationInterceptor.cs](Authorization/AuthorizationInterceptor.cs)

**Supporting Files:**
- [Authorization/IAuthorizationService.cs](Authorization/IAuthorizationService.cs) — Contract for policy evaluation
- [Authorization/RequireAuthorizationAttribute.cs](Authorization/RequireAuthorizationAttribute.cs) — Method-level authorization marker

**Documentation:** [Authorization/README.md](Authorization/README.md)

**Purpose:** Enforce access policies based on authenticated principal

**Key Features:**
- Depends on principal from JwtAuthenticationInterceptor
- Policy-based authorization decision
- Attribute-based configuration
- Graceful handling of unauthenticated requests

---

#### 3. Redaction

**File:** [Redaction/RedactionInterceptor.cs](Redaction/RedactionInterceptor.cs)

**Supporting Files:** None (uses standard types)

**Documentation:** [Redaction/README.md](Redaction/README.md)

**Purpose:** Mask sensitive method arguments before logging/auditing

**Key Features:**
- Stores safe (redacted) arguments in MethodContext.Items
- Preserves original arguments for actual invocation
- Works with audit trail capture

---

### Observability Category (4 interceptors)

#### 4. Correlation

**File:** [Correlation/CorrelationInterceptor.cs](Correlation/CorrelationInterceptor.cs)

**Supporting Files:** None (uses standard types)

**Documentation:** [Correlation/README.md](Correlation/README.md)

**Purpose:** Track distributed operation flow across service boundaries

**Key Features:**
- Correlation ID generation and propagation
- Integration with OpenTelemetry traces
- W3C Trace Context support

---

#### 5. Audit

**File:** [Auditing/AuditInterceptor.cs](Auditing/AuditInterceptor.cs)

**Supporting Files:**
- [Auditing/IAuditSink.cs](Auditing/IAuditSink.cs) — Contract for audit storage
- [Auditing/AuditEntry.cs](Auditing/AuditEntry.cs) — Audit record model

**Documentation:** [Auditing/README.md](Auditing/README.md)

**Purpose:** Record operation execution history for compliance and debugging

**Key Features:**
- Captures who did what, when
- Uses safe arguments from RedactionInterceptor
- Pluggable audit sink (implement IAuditSink)
- Captures invocation metadata

---

#### 6. Telemetry (OTLP)

**File:** [Telemetry/TelemetryInterceptor.cs](Telemetry/TelemetryInterceptor.cs)

**Supporting Files:** None (uses standard types)

**Documentation:** [Telemetry/README.md](Telemetry/README.md)

**Purpose:** Export OpenTelemetry metrics and traces

**Key Features:**
- Automatic span creation
- Metrics emission (latency, success rate)
- Exception recording
- Integration with OTel collectors

---

#### 7. Status

**File:** [Status/StatusInterceptor.cs](Status/StatusInterceptor.cs)

**Supporting Files:**
- [Status/IStatusSink.cs](Status/IStatusSink.cs) — Contract for status storage
- [Status/Status.cs](Status/Status.cs) — Status value type
- [Status/StatusUpdated.cs](Status/StatusUpdated.cs) — Status change event

**Documentation:** [Status/README.md](Status/README.md)

**Purpose:** Track execution status and report completion events

**Key Features:**
- Execution state tracking (Started, Completed, Failed)
- Pluggable status sink
- Integrates with timeout/retry tracking
- Event notifications on state changes

---

### Fault Tolerance Category (3 interceptors)

#### 8. Retry

**File:** [Retry/RetryInterceptor.cs](Retry/RetryInterceptor.cs)

**Supporting Files:**
- [Retry/RetryAttribute.cs](Retry/RetryAttribute.cs) — Configuration for retry policy

**Documentation:** [Retry/README.md](Retry/README.md)

**Purpose:** Exponential backoff retry logic for transient failures

**Key Features:**
- Configurable max attempts and backoff
- Transient exception detection (ITransientExceptionDetector)
- Jitter support
- Integrates with TimeoutInterceptor

---

#### 9. Timeout

**File:** [Timeout/TimeoutInterceptor.cs](Timeout/TimeoutInterceptor.cs)

**Supporting Files:**
- [Timeout/TimeoutAttribute.cs](Timeout/TimeoutAttribute.cs) — Configuration for timeout policy

**Documentation:** [Timeout/README.md](Timeout/README.md)

**Purpose:** Enforce operation time limits with graceful cancellation

**Key Features:**
- Configurable timeout duration per method
- CancellationToken injection
- Timeout exception on deadline exceeded
- Integrates with retry and circuit breaker

---

#### 10. CircuitBreaker

**File:** [CircuitBreaker/CircuitBreakerInterceptor.cs](CircuitBreaker/CircuitBreakerInterceptor.cs)

**Supporting Files:**
- [CircuitBreaker/CircuitBreakerAttribute.cs](CircuitBreaker/CircuitBreakerAttribute.cs) — Configuration
- [CircuitBreaker/CircuitBreakerState.cs](CircuitBreaker/CircuitBreakerState.cs) — State tracking
- [CircuitBreaker/CircuitBreakerOpenException.cs](CircuitBreaker/CircuitBreakerOpenException.cs) — Exception type

**Documentation:** [CircuitBreaker/README.md](CircuitBreaker/README.md)

**Purpose:** Prevent cascading failures by stopping calls to failing services

**Key Features:**
- Three states: Closed (normal), Open (failing), Half-Open (recovering)
- Configurable thresholds and reset timeout
- Thread-safe state management
- Works with transient exception detector

---

### Data Integrity Category (3 interceptors)

#### 11. Validation

**File:** [Validation/ValidationInterceptor.cs](Validation/ValidationInterceptor.cs)

**Supporting Files:** None (uses standard types)

**Documentation:** [Validation/README.md](Validation/README.md)

**Purpose:** Enforce input parameter constraints before invocation

**Key Features:**
- Data annotations support (DataAnnotations)
- Custom validation rules
- Error aggregation and reporting
- Fails fast before business logic execution

---

#### 12. Idempotency

**File:** [Idempotency/IdempotencyInterceptor.cs](Idempotency/IdempotencyInterceptor.cs)

**Supporting Files:**
- [Idempotency/IIdempotencyKeyResolver.cs](Idempotency/IIdempotencyKeyResolver.cs) — Extracts idempotency key
- [Idempotency/IIdempotencyStore.cs](Idempotency/IIdempotencyStore.cs) — Result cache storage
- [Idempotency/IdempotentAttribute.cs](Idempotency/IdempotentAttribute.cs) — Configuration marker

**Documentation:** [Idempotency/README.md](Idempotency/README.md)

**Purpose:** Cache and reuse results for repeated operations (same key)

**Key Features:**
- Configurable cache duration
- Pluggable key resolution strategy
- Pluggable result storage (implement IIdempotencyStore)
- Prevents duplicate processing

---

#### 13. Exception Handling

**File:** [Exceptions/ExceptionInterceptor.cs](Exceptions/ExceptionInterceptor.cs)

**Supporting Files:**
- [Exceptions/IExceptionHandler.cs](Exceptions/IExceptionHandler.cs) — Contract for exception transformation

**Documentation:** [Exceptions/README.md](Exceptions/README.md)

**Purpose:** Transform and wrap exceptions for consistent error handling

**Key Features:**
- Pluggable exception transformation (implement IExceptionHandler)
- Preserves stack traces
- Enables custom error mappings
- Integrates with status/telemetry tracking

---

## Core Infrastructure

### Core Types and Contracts

**File:** [Core/README.md](Core/README.md)

**Supporting Files:**
- [Exceptions/ITransientExceptionDetector.cs](Exceptions/ITransientExceptionDetector.cs) — Contract for transient exception detection
- [Exceptions/DefaultTransientExceptionDetector.cs](Exceptions/DefaultTransientExceptionDetector.cs) — Default implementation (TimeoutException, IOException)
- [Core/MethodContext.cs](Core/MethodContext.cs) — Execution context passed to all interceptors
- [Core/MethodArgument.cs](Core/MethodArgument.cs) — Represents method argument metadata
- [Core/HandlerDelegate.cs](Core/HandlerDelegate.cs) — Next-in-chain delegate type

**Purpose:** Shared infrastructure used by all interceptors

**Key Contracts:**
- `IInvocationInterceptor` — Base interface implemented by all interceptors
- `MethodContext` — Shared execution context (Items dictionary, method info)
- `ITransientExceptionDetector` — Identifies exceptions safe to retry (used by Retry, CircuitBreaker)

---

### Security Support

**File:** [Security/README.md](Security/README.md)

**Supporting Files:**
- [Security/ICurrentPrincipalAccessor.cs](Security/ICurrentPrincipalAccessor.cs) — Contract for accessing authenticated principal

**Purpose:** Helper contracts for security-related functionality

---

## Documentation Structure

### Root-Level Guides

| File | Purpose |
|---|---|
| [README.md](README.md) | Main overview and quick reference |
| [QUICK-START.md](QUICK-START.md) | 5-minute getting started guide |
| [USAGE.md](USAGE.md) | Detailed integration and usage patterns |
| [TOC.md](TOC.md) | This file — complete reference index |

### Historical/Reference

| File | Purpose |
|---|---|
| [REFACTORING-SUMMARY.md](REFACTORING-SUMMARY.md) | Complete changelog of renaming and organization changes |
| [VERIFICATION-STRUCTURE.md](VERIFICATION-STRUCTURE.md) | Folder structure verification and file checklist |

---

## Search by Category

### By Execution Order

1. [Authentication](Authentication/README.md) — Establish identity
2. [Authorization](Authorization/README.md) — Check permissions
3. [Redaction](Redaction/README.md) — Prepare safe arguments
4. [Validation](Validation/README.md) — Validate inputs
5. [Correlation](Correlation/README.md) — Start trace context
6. [Audit](Auditing/README.md) — Record operation
7. [Telemetry](Telemetry/README.md) — Export metrics
8. [Status](Status/README.md) — Track execution state
9. [Retry](Retry/README.md) — Prepare retry logic
10. [Timeout](Timeout/README.md) — Set time limits
11. [CircuitBreaker](CircuitBreaker/README.md) — Prevent cascading failures
12. [Idempotency](Idempotency/README.md) — Check cache
13. [Exception](Exceptions/README.md) — Transform exceptions

### By Pluggability (requires implementation)

| Interceptor | Contract to Implement |
|---|---|
| Authentication | `IJwtTokenValidator` |
| Authorization | `IAuthorizationService` |
| Audit | `IAuditSink` |
| Status | `IStatusSink` |
| Idempotency | `IIdempotencyKeyResolver`, `IIdempotencyStore` |
| Exception | `IExceptionHandler` |

### By Attribute Configuration

| Interceptor | Attribute |
|---|---|
| Authentication | `RequireAuthenticationAttribute` |
| Authorization | `RequireAuthorizationAttribute` |
| Retry | `RetryAttribute` |
| Timeout | `TimeoutAttribute` |
| Idempotency | `IdempotentAttribute` |
| CircuitBreaker | `CircuitBreakerAttribute` |

---

## File Manifest

### Total Files

- **13** Interceptor implementations
- **24** Supporting files (contracts, attributes, types)
- **7** Core infrastructure files
- **14** Documentation files (1 per interceptor + root guides)
- **1** Base interface (IInvocationInterceptor.cs)

### By Type

| Type | Count |
|---|---|
| Interceptor implementations | 13 |
| Interfaces/Contracts | 10 |
| Attributes | 6 |
| Value types (Status, etc.) | 5 |
| Exception types | 1 |
| State management | 1 |
| Core types | 6 |
| Documentation (.md) | 14 |

---

## See Also

- [BoundaryProxy Infrastructure](../README.md) — Dynamic proxy using DispatchProxy
- [Proxies/README.md](../README.md) — Integration patterns with DI
- [Core/README.md](Core/README.md) — Base contracts and types

---

**Last Verified:** 2026-08-19  
**Total Interceptors:** 13  
**Status:** Complete

