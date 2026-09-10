# vc.Ifx.Abstractions

Foundational request/result contracts, deterministic clock access, and the
compatible service lifecycle base for .NET 10 / C# 14.

## Stable Contracts

Existing public signatures and namespaces are retained. ServiceRequest and
ServiceRequest<T> remain empty, mutable, inheritable classes with parameterless
construction and reference identity; the generic type is a marker, not a new
payload property. No correlation, diagnostics, options, or shared domain-object
contracts are invented by this change. IClock keeps its existing
vc.Ifx.Abstractions.Time namespace and UtcNow property.

ServiceResult and ServiceResult<T> are immutable result containers with reference
identity, not value records. Payloads and exceptions are retained by reference,
without deep cloning. IsSuccess is the sole success discriminator.
Success(null) is legitimate for nullable reference or nullable value types.
Match, Map, and MapAsync pass that null value to the success delegate, and a
mapper returning null also produces success. Non-nullable value types continue
to use their ordinary values, including zero; failed results expose default(T).
Use ServiceResult<string?> when null is part of the successful contract.

Both Match delegates and all mapper delegates must be non-null, even when the
current result would not invoke one. Failure(string) and Failure(string,
Exception) require a nonblank message. Exception arguments must be non-null.
Failure(Exception) preserves the exception's message verbatim, including an
empty message. Error messages are not automatically safe for HTTP clients.
ServiceResultBase remains a compatible extensibility base; external derived
types remain responsible for their own state invariants.

Map/MapAsync copy failure metadata unchanged without calling the mapper. An
ordinary exception thrown by a mapper becomes a failed result retaining that
exception instance. Match delegate exceptions propagate. OperationCanceledException
and subclasses always propagate from mapping and exception-based failure
factories; cancellation is never converted to an ordinary failed result.
MapAsync awaits the operation without requiring a captured synchronization
context. Its existing delegate signature is preserved: capture the caller's
cancellation token in the delegate and pass it to the underlying operation.
A null task returned by an invalid async mapper becomes an ordinary failed result.

```csharp
var result = ServiceResult<string?>.Success(null);
var length = result.Map(value => value?.Length ?? 0); // success, value 0
var loaded = await result.MapAsync(value => LoadAsync(value, cancellationToken));
```

LoadAsync is an application operation that accepts a nullable value and a
cancellation token.

## Lifecycle and Dependency Boundary

ServiceBase<T> retains its ILogger<T> constructor, protected Logger,
ThrowIfDisposed, IDisposable, and protected virtual Dispose(bool) hook.
The injected logger is borrowed and is never disposed by the base.
Dispose invokes Dispose(true) and suppresses finalization; overrides must call
base.Dispose(disposing) and keep their own cleanup idempotent. Repeat Dispose
calls remain supported. ThrowIfDisposed succeeds before disposal and throws
ObjectDisposedException with the concrete service type name afterward.
Disposal is not a synchronization primitive; concurrent use/disposal requires
application coordination.

The base no longer has a finalizer: it owns no unmanaged resource and cannot
perform useful finalizer cleanup. Keeping a finalizer on every service delayed
collection and invoked derived virtual cleanup from the finalizer thread.
Derived types owning unmanaged resources should use SafeHandle or implement
their own finalization. The protected Dispose(false) hook remains for source
compatibility and marks the base disposed without assuming managed cleanup.
GC.SuppressFinalize remains in Dispose for derived classes that own finalizers.

Microsoft.Extensions.Logging remains an explicit compatibility exception in this
abstractions package because existing ServiceBase<T> consumers depend on the
typed logger contract. There are no HTTP, cloud SDK, persistence, or other
provider dependencies. Moving the base or reducing the logging dependency is a
separate package migration; package metadata and dependencies are unchanged here.

## Migration

- Replace reliance on null success being treated as failure with explicit
  validation in the application. Match/Map/MapAsync now agree with IsSuccess.
- Supply both Match delegates and non-null mapping delegates, even for
  short-circuited failure results.
- Replace blank explicit failure messages with meaningful messages. Exception-only
  failures retain the original message, including empty messages.
- Handle cancellation through the normal cancellation path; code expecting
  IsFailure after cancellation must await/catch cancellation instead.
- Dispose derived services deterministically. Do not rely on the former base
  finalizer to run Dispose(false).
- Existing root FrameworkResultTests contains two obsolete null-success tests;
  updating that unassigned file is an orchestrator migration requirement.

## Verification

Package tests are under tests/unit/vc.Ifx.UnitTests/Abstractions, with lifecycle
tests in the existing root ServiceBaseTests.cs. Use the serialized infrastructure
wrapper with filter
`FullyQualifiedName~Tests.Abstractions|FullyQualifiedName~ServiceBaseTests`.
Measured coverage and remaining global gates are recorded in the package's
[planning section](https://github.com/visionarycoder/vc.Ifx/blob/main/docs/planning/framework-upgrade-parallel-plan.md).

Verified 2026-09-09:

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Abstractions -Filter 'FullyQualifiedName~Tests.Abstractions|FullyQualifiedName~ServiceBaseTests' -ReportOnly
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -BuildOnly -Project src/vc.Ifx.Abstractions/vc.Ifx.Abstractions.csproj -NoRestore
```

28 tests passed; measured coverage is 95/95 lines and 18/18 branches (both 100%),
with 100% methods. The fresh report also passed strict Test-CoverageReport.ps1
validation without ReportOnly. Evidence:
`TestResults/coverage/vc.Ifx.Abstractions/cde87fa6abca4fc18cd2f20ecb8d9c14/`.
Package build passed with zero warnings/errors. No additional coverage exclusions.

The existing result regression suite passed 25/27 tests; the two null-success
expectations identified above remain an orchestrator-owned migration. This
package remains In-flight for that migration and the whole-solution build,
full-suite/global coverage, and final package gates.
