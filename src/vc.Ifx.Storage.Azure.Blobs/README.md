# vc.Ifx.Storage.Azure.Blobs

Azure block blob storage for .NET 10 and C# 14, implementing both `IStorageProvider`
and `IObjectStorageProvider`. Namespace: `VisionaryCoder.Framework.Storage.Azure.Blob`.
The provider owns Azure protocol/configuration volatility; requests and facts stay in
the inward-only storage abstractions package. There is no duplicate SDK adapter interface.

## Construction And Authentication

```csharp
var options = new AzureBlobStorageOptions
{
    ContainerName = "application-data",
    UseManagedIdentity = true,
    StorageAccountUri = "https://youraccount.blob.core.windows.net",
    CreateContainerIfNotExists = false
};
using var provider = new AzureBlobStorageProvider(options, logger);
IObjectStorageProvider storage = provider;
using var input = new MemoryStream(payload);
StorageObjectMetadata result = await storage.WriteAsync(
    new StorageWriteRequest("reports/current.json", input, overwrite: false), token);
using Stream content = await storage.OpenReadAsync(new("reports/current.json"), token);
```

The existing `(AzureBlobStorageOptions, ILogger<AzureBlobStorageProvider>)` constructor
is retained. It builds SDK clients and validates configuration without contacting Azure.
`CreateContainerIfNotExists` still defaults to true, but creation now occurs before each
write, not in the constructor or on read/list/delete operations. Existing container access
settings are not changed. Set the option to false for pre-provisioned, least-privilege use.

Despite its legacy name, `UseManagedIdentity = true` uses `DefaultAzureCredential`:
that chain also includes supported developer credentials. It is not a guarantee that
managed identity was selected. The account URI must be HTTPS, without a container path,
userinfo, query or fragment. With the option false, supply an SDK connection string.
The SDK validates its syntax; the development-storage connection string is supported.
Do not commit connection strings or SAS tokens to source control.

The new overload adds a third `BlobContainerClient` argument. Inject a real SDK client
with your chosen token credential/transport/retry settings, or use the SDK's virtual
methods and model factories in tests. Its `Name` must match `ContainerName`. Endpoint
and credential fields are ignored in this overload and need not be supplied; behavioral
options are still validated. Client transport/retry settings are not overwritten. Azure
clients are reusable and not disposable; the provider does not own their transport.
An application-created `HttpClient`/transport remains application-owned.

## Frozen Object Semantics

| Concern | Behavior |
| --- | --- |
| Capabilities | Read, Write, Delete, Metadata, List and CreateOnly. These do not imply permissions or server availability. |
| Object names | Exact container-relative names; case, repeated slashes and internal whitespace are preserved. Reject leading slash, backslash, controls, dot traversal segments and names longer than 1024 UTF-16 code units. Remaining Azure account/service-specific restrictions are enforced by Azure. |
| Prefix | Literal ordinal prefix, empty for all; supports partial segments and wildcard characters as literal text. Uses paged flat blob listing, not virtual-directory entries. |
| Read | Uses `DownloadStreamingAsync`; the returned content stream is caller-owned. The opening token governs opening, and later reads accept their own token. The provider does not buffer the complete object. |
| Write | Passes the original readable stream from its current position to the SDK and leaves it open on success, failure and cancellation. Nonseekable input is supported by SDK partitioning/buffering. Do not concurrently mutate the input. |
| Conditional create | `Overwrite = false` sets `BlobRequestConditions.IfNoneMatch = ETag.All` on the upload. Azure evaluates this at creation/commit; no preflight existence check. A conflict is an `IOException` retaining the SDK exception. |
| Replacement | `Overwrite = true` has no conditional header. Replacing a block blob replaces its content and can reset existing metadata/headers according to SDK upload semantics. |
| Metadata lookup | Uses stored content length (including zero), last-modified time, content type and the original opaque ETag representation. Nothing is inferred from extension or name. |
| Write result | Contains the key plus upload-response last-modified and ETag. Length and content type remain null because that response does not supply them. No follow-up read introduces a racing metadata snapshot. |
| Listing metadata | Maps the SDK listing's nullable stored length, timestamp, content type and ETag. Unknown fields remain null. The sequence is not a consistent snapshot. |
| Missing | Only SDK 404 with `BlobNotFound` or `ContainerNotFound` means absence: read throws `FileNotFoundException`, metadata returns null, deletion succeeds, and a missing-container listing yields nothing. Other 404 responses remain errors. |
| Delete | Idempotent `DeleteIfExistsAsync`. Snapshots are not silently deleted; a snapshot/lease/retention conflict remains an error. |
| Errors | 401/403 or Azure credential authentication failures become `UnauthorizedAccessException`; other SDK/aggregated transfer failures become `IOException`, preserving the SDK cause. Cancellation propagates as `OperationCanceledException`. |

Download stream reads after opening retain SDK stream behavior, including SDK exceptions
and any retry/resume behavior; the provider does not wrap that stream merely to rename
exceptions. Legacy whole-content reads copy it and dispose it. Cancellation or transfer
failure does not promise rollback. Large writes can leave uncommitted blocks, and container
creation can have succeeded before a write fails. An ambiguous response after commit is
not proof that no object was created. Retry policies do not turn writes into exactly-once
operations. Server leases, versioning, immutability and soft deletion still apply.

Object keys are not local filesystem paths. This provider exposes the full configured
container, not a per-prefix security sandbox. Use container-scoped credentials/RBAC and
server policy for isolation. It does not introduce a shared domain model, public URL
generation, SAS signing, ETag-match update API, copy/move API or extra retry abstraction.

## Options And Retry Ownership

Container names are 3-63 ASCII lowercase letters, digits or single interior hyphens.
Special service containers such as `$root` are outside this provider's supported options.
Block upload tiers are Hot, Cool, Cold or Archive. Archive can make subsequent reads
unavailable until rehydration; the provider does not rehydrate automatically. Public
container access defaults to `None` and applies only during optional creation.

`TimeoutMilliseconds` defaults to 30000 and configures SDK network timeout per attempt,
not an operation deadline. `BufferSize` defaults to 4 MiB and sets initial/maximum SDK
upload chunk sizes and the legacy byte-read copy buffer; SDK upload concurrency is one.
Both must be positive. Caller cancellation is the operation-level deadline mechanism.

The default SDK client uses exponential retry with `MaxRetries = 3`,
`RetryDelayMilliseconds = 800`, and `MaxRetryDelayMilliseconds = 8000`. Retry counts
and delays cannot be negative, and maximum delay cannot be less than initial delay.
Zero retries disables SDK retries. These settings apply only to provider-created clients.
There is no Polly policy, provider retry loop or stacked retry budget. The SDK also owns
transfer partitioning and recovery. Independently wrapping writes in retries requires
accounting for conditional conflicts and ambiguous outcomes.

## Legacy Compatibility

All `IStorageProvider` signatures remain, including filesystem-shaped `FileInfo` and
`DirectoryInfo` compatibility tokens. They do not describe remote filesystem metadata.
`FileInfo.FullName` is interpreted as a blob name, including drive segments, not as a
local file to open. Prefer strings and the object interface for portable callers.

Legacy paths normalize backslashes, leading slashes and repeated slashes; object paths
do not. Dot traversal and invalid names are now rejected. `GetFullPath` returns an escaped
blob URI with its query removed so it does not disclose SAS credentials. The URI is
descriptive output, not an alternate endpoint for future operations.

Virtual directories retain the real zero-length `.directory` marker convention.
`CreateDirectory` writes that marker. Object listing includes it because it is a real blob;
legacy file lists hide names ending in `/.directory`. Legacy file listing remains recursive
under a prefix, while directory listing returns immediate distinct directory prefixes.
Search supports case-insensitive `*`/`?`; other regex characters are escaped and literal.
Blank search patterns are rejected.

Nonrecursive deletion rejects any item other than that directory's exact marker, including
a single child file or a nested directory marker. It does not delete racing new children.
Recursive deletion lists then removes observed blobs; neither operation is transactional
or guaranteed to leave the prefix empty under concurrent writes. Missing directories are
no-ops. Synchronous legacy methods block on asynchronous SDK operations; no `Task.Run`
is used. Disposing the provider rejects new operations but does not cancel calls already
in progress or invalidate a caller-owned download stream.

## SDK Evidence And Verification

Verified Azure.Storage.Blobs 12.26.0 virtual `BlobClient`/`BlobContainerClient` APIs and
model-factory signatures with dotnet-inspect, plus the matching official SDK source:

- [BlobClient upload implementation](https://github.com/Azure/azure-sdk-for-net/blob/Azure.Storage.Blobs_12.26.0/sdk/storage/Azure.Storage.Blobs/src/BlobClient.cs) supplies conditional-create semantics.
- [PartitionedUploader](https://github.com/Azure/azure-sdk-for-net/blob/Azure.Storage.Blobs_12.26.0/sdk/storage/Azure.Storage.Common/src/Shared/PartitionedUploader.cs) handles stream windows at the current position and buffers nonseekable partitions.
- [Azure naming rules](https://learn.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata) define service-side naming constraints; this provider intentionally supports a documented subset.

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -TestSourceScope Storage/Azure/Blobs -TestPackage vc.Ifx.Storage.Azure.Blobs -CoveragePackage vc.Ifx.Storage.Azure.Blobs -Filter FullyQualifiedName~Storage.Azure.Blobs
```

The serialized source/package-scoped run passed 32 tests with 262/262 lines, 114/114
branches and 100% methods. Evidence:
`TestResults/coverage/vc.Ifx.Storage.Azure.Blobs/ebbf9f25fcd041cca1846a3814126445/summary.json`.
Tests mock SDK virtual methods or use the real SDK with an in-memory HTTP handler;
none contact Azure or an emulator. The transport tests verify the actual conditional
header, one winner under simulated concurrent create, seekable/nonseekable stream
ownership and pending-I/O cancellation. They are not evidence of a live Azure deployment.
Default-client tests inspect construction and URI/configuration only.

The Orchestrator owns final full-suite, zero-warning solution/Release, repack, global
coverage/security and live-environment integration gates. This scoped result does not
prove those gates. No aggregator registration or shared build configuration was changed.
