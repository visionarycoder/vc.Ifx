# vc.Ifx.Storage.Abstractions

Storage contracts for .NET 10 and stable C# 14. The package has no package or
project dependencies. Storage mechanisms stay behind Access contracts; these
types describe infrastructure requests and facts, not shared domain entities.

## Frozen contracts

`IStorageProvider` remains source-compatible with the local, FTP, blob, and
aggregator consumers. Its `FileInfo` and `DirectoryInfo` overloads are a documented
compatibility exception: they describe local filesystem concepts and must not be
used as the portable model for remote object storage. No signature was removed,
and no new member is required from existing implementations.

`IObjectStorageProvider` is an independent, optional contract for object storage.
The Local, FTP and Azure Blobs providers now implement it alongside their legacy
interface. Their READMEs and conformance suites describe implemented capabilities
and provider-specific limits. It requires these members:

| Member | Result and semantics |
| --- | --- |
| `Capabilities` | Supported `StorageCapabilities` flags; no guarantee of authorization or availability. |
| `OpenReadAsync(StorageObjectRequest, CancellationToken)` | `Task<Stream>`; caller disposes the returned stream. Missing objects throw `FileNotFoundException`. |
| `WriteAsync(StorageWriteRequest, CancellationToken)` | `Task<StorageObjectMetadata>`; reads from the input's current position and leaves it open, including on failure. |
| `GetMetadataAsync(StorageObjectRequest, CancellationToken)` | `Task<StorageObjectMetadata?>`; null means absent. This also provides an existence check without another operation. |
| `DeleteAsync(StorageObjectRequest, CancellationToken)` | `Task`; succeeds when deleted or already absent, without an atomic existence result. |
| `ListAsync(StorageListRequest, CancellationToken)` | `IAsyncEnumerable<StorageObjectMetadata>`; unordered listing, empty for no matches, no directory entries, no snapshot guarantee. |

Every cancellation token defaults to `default`. There are no default interface
implementations and no DI registrations in this package.

## Requests and metadata

Paths are nonblank, opaque, provider-relative strings. Records preserve path case,
slashes, and whitespace within a nonblank path. Each provider validates its own
path restrictions and confinement; records do not call local filesystem APIs.

- `StorageObjectRequest(string path)` identifies an object.
- `StorageWriteRequest(string path, Stream content, bool overwrite = true)`
  requires a readable stream; it need not be seekable. The caller must keep the
  stream open and avoid concurrent use until the write finishes.
- `StorageListRequest(string prefix = "")` uses literal ordinal prefix matching.
  Empty selects all objects; null is invalid. Whitespace and wildcard characters
  are literal. Providers may use a broader native query and filter its results.
- `StorageObjectMetadata(string path, long? length = null,
  DateTimeOffset? lastModified = null, string? contentType = null,
  string? version = null)` represents immutable facts. Negative lengths are invalid;
  zero is valid. Null means unavailable. Version values remain opaque.

These sealed records have get-only properties and value equality. Stream equality
in write requests is reference equality, not content equality; copying a request
does not copy or transfer ownership of its stream. No request normalizes data or
performs I/O.

`StorageCapabilities` flags are `None=0`, `Read=1`, `Write=2`, `Delete=4`,
`Metadata=8`, `List=16`, and `CreateOnly=32`. `CreateOnly` supplements `Write`:
`overwrite: false` must either reject an existing object atomically or throw
`NotSupportedException`. A check-then-write sequence does not satisfy this promise.

## Failures and cancellation

New object providers reject null requests with `ArgumentNullException` and invalid
provider paths with `ArgumentException`. Record constructors reject null required
values, blank object paths, unreadable input streams, and negative content lengths.
Unsupported operations throw `NotSupportedException`; authorization failures throw
`UnauthorizedAccessException`. Other storage failures throw `IOException`, retaining
the underlying provider exception as `InnerException` when present. A create-only
conflict is an `IOException`. Errors must never be converted into absence or an
empty listing.

Providers honor cancellation before I/O, during pending I/O, and throughout async
enumeration, throwing `OperationCanceledException` with the supplied token. The
opening token covers opening a read stream; subsequent reads take their own tokens.
Failure or cancellation during a write does not promise rollback. Error mapping and
cancellation of later stream reads are responsibilities of the concrete provider's
returned stream and must be documented there.

These are requirements of the object contract, not a change to the behavior of
legacy filesystem overloads. This package introduces no SDK wrappers or retry policy.

## Provider Implementations

Local, FTP, and Blob implement `IObjectStorageProvider` alongside their legacy
interface. The five operations cover the plan's read/write/exists/
list/delete/metadata needs; metadata absence supplies the exists result. Separate
directory, copy, move, retry, locking, and conditional-version operations are not
part of this contract.

| Fact or behavior | Local | FTP | Azure Blobs |
| --- | --- | --- | --- |
| `Path` | Object path relative to the configured root | Object path relative to the configured remote root | Blob name relative to the configured container |
| `Length` | File byte length | Server-reported size, otherwise null | Blob content length |
| `LastModified` | File's UTC modification time | Server timestamp only when available and its timezone is known, otherwise null | Blob last-modified timestamp |
| `ContentType` | Null; do not infer from extension | Null unless genuinely supplied by the server | Stored blob content type |
| `Version` | Null; modification time is not a version token | Null unless the server supplies a version token | Opaque ETag including its original representation |
| `CreateOnly` | Advertise only after implementing and testing exclusive creation | Omit unless the chosen server/client mechanism guarantees atomic create-only writes | Advertise only after implementing and testing the SDK's conditional create operation |

Workers must advertise only implemented and tested capabilities. Creation-only
support cannot be inferred from an ordinary overwrite option. Provider roots,
credentials, timeouts, and SDK-specific options remain in the provider packages.
SDK clients with existing mocking seams should be injected directly; this contract
does not require an additional SDK adapter layer.

The aggregator's existing storage extension methods intentionally register only
legacy `IStorageProvider`. At the application composition boundary, resolve that
service once and cast the built-in Local/FTP/Blob instance to
`IObjectStorageProvider`; pass that same instance to the consuming component.
Do not register or resolve a second transient alias for the same provider, and do
not dispose a DI-owned provider separately. Custom legacy providers need not
implement the optional interface. See the aggregator README's portable composition
example. This abstraction package adds no DI or changes to frozen requests/results.

## Factory compatibility

`StorageFactoryOptions.Implementations` is a live, read-only dictionary view;
casting the view cannot mutate registrations. Names use ordinal, case-sensitive
comparison and later registrations replace an exact matching name. Configure once
before concurrent use; concurrent registration and reading are unsupported.

The aggregator owns registration. `Validate()` explicitly rejects blank names and
types that are not closed, concrete classes. It accepts an empty collection and
does not require `IStorageProvider`, because existing registrations include queue
and table providers. Provider options and activation dependencies belong to their
owners. Validation is opt-in to preserve existing builders that permit blank names;
applications should call it before activation. Null names and types are rejected
at registration and leave existing entries intact.

`StorageImplementation(Type ImplementationType, object? Options = null)` retains
its positional constructor, deconstruction, init properties, and record equality.
It is a passive descriptor, so callers can construct descriptors with abstract or
interface types. The options object is retained by reference; equality follows that
object's equality implementation. The descriptor itself performs no validation.

## Verification

Focused tests live in `tests/unit/vc.Ifx.UnitTests/Storage/Abstractions` and cover
record defaults, guards, equality, stream ownership, registration validation,
read-only exposure, compatibility, and the shape of the portable interface.
Existing factory and descriptor tests provide additional compatibility coverage.
Provider I/O, DI, cancellation execution, and SDK failure mapping belong to provider
and aggregator suites; an interface-only test cannot prove those implementations.

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -BuildOnly -Project src/vc.Ifx.Storage.Abstractions/vc.Ifx.Storage.Abstractions.csproj -NoRestore
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Storage.Abstractions -Filter 'FullyQualifiedName~Storage.Abstractions|FullyQualifiedName~StorageFactoryOptionsTests|FullyQualifiedName~StorageImplementationTests'
```

Project completion also requires Orchestrator's whole-solution build, full-suite,
coverage policy, and packaging verification. Targeted coverage does not prove those
global gates. See the owned planning section for measured results and handoff.
The infrastructure wrapper serializes builds, tests, and coverage against other
workers using the wrapper, preventing shared test assembly instrumentation races.
Its coverage report check enforces the package thresholds. Do not run ad hoc builds
or test commands concurrently with shared test assembly instrumentation.
