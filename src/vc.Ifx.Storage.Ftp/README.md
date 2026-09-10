# vc.Ifx.Storage.Ftp

FTP/explicit FTPS provider for .NET 10 and C# 14. The package implements both the
legacy `IStorageProvider` and the provider-neutral `IObjectStorageProvider` in
`VisionaryCoder.Framework.Storage.Ftp`. Endpoint and protocol volatility stay in
this provider; application object keys and metadata do not expose filesystem or SDK types.

## Construction

```csharp
var options = new FtpStorageOptions
{
    Host = "ftp.example.com",
    Username = username,
    Password = password,
    UseSsl = true,
    RootPath = "/application-data"
};
using var provider = new FtpStorageProvider(options, logger);
IObjectStorageProvider storage = provider;
using var input = new MemoryStream(payload);
await storage.WriteAsync(new StorageWriteRequest("reports/current.json", input), token);
using Stream content = await storage.OpenReadAsync(new("reports/current.json"), token);
```

The existing `(FtpStorageOptions, ILogger<FtpStorageProvider>)` constructor is unchanged.
Construction validates options but neither creates directories nor connects. A new overload
adds a third `Func<IAsyncFtpClient>` argument. This uses FluentFTP's shipped mockable interface,
not a duplicate SDK wrapper. Each factory result must be a fresh, disconnected, exclusively
owned client configured for the endpoint. The provider connects and disposes it for each
operation (each visited directory during object listing). Factory clients must support
`Config`, `LastReply`, and the SDK operations used below. Never return a shared live client.

`Host` is a DNS name or IP address, not a URI. Ports must be 1-65535; timeout and buffer
size must be positive. Host, credentials and root cannot be blank or contain control
characters. Passwords are not included in provider error messages. Obtain credentials
from a secret store; do not put them into source or diagnostic output.

For backward compatibility `UseSsl` defaults to false: plain FTP exposes credentials
and data on the network. Set it to true for explicit FTPS with normal certificate
validation. This is not SFTP or implicit FTPS. Port remains 21 unless explicitly set.
`UsePassive` selects PASV versus PORT; `KeepAlive` enables TCP socket keep-alive.
`TimeoutMilliseconds` applies independently to control/data connect/read operations,
not an overall deadline. Use cancellation for a caller deadline. `BufferSize` maps to
both copy buffers and SDK transfer chunks. No option disables certificate validation.

## Frozen Object Contract

The public API is the existing six-member `IObjectStorageProvider`: `Capabilities`,
`OpenReadAsync`, `WriteAsync`, `GetMetadataAsync`, `DeleteAsync`, and `ListAsync`.
No FTP-specific request or response records are added.

| Concern | FTP behavior |
| --- | --- |
| Capabilities | Read, Write, Delete, Metadata, List. Never CreateOnly. |
| Create-only | `Overwrite = false` throws `NotSupportedException` before connecting or reading input. A check followed by upload would not be atomic. |
| Paths | Slash-relative keys below the captured `RootPath`, default `/`. Reject rooted keys, backslashes, colons, controls, empty segments and `.`/`..` traversal segments. |
| Prefix | Literal ordinal matching, including partial last segments and trailing slash; no wildcard expansion. Listings return relative keys, recursively visit directories and omit links. Order is unspecified. |
| Read | Downloads into memory; returns a caller-owned stream positioned at zero, independent of the already-disposed FTP client. |
| Write | Buffers input from its current position without disposing or rewinding it. Nonseekable streams work. SDK receives a separate seekable buffer. Both object transfer directions force binary mode. |
| Write result | Returns the key with all optional metadata unknown; a completed upload establishes no atomic metadata snapshot. |
| Metadata | Positive SDK listing size maps to length. Zero/negative listing size is conservatively unknown because SDK listing formats can use zero for unavailable size. Only explicitly UTC timestamps map to `LastModified`; unspecified/local timestamps remain null. Content type and version are always null. |
| Absence | Successful parent listing without the named file yields null metadata, `FileNotFoundException` for read, and a no-op delete. No blanket conversion of FTP 550 into absence. |
| Errors | FTP 530/532 and authentication/TLS exceptions become `UnauthorizedAccessException`; other SDK, socket and timeout exceptions become `IOException` with the original exception attached. SDK failed/skipped transfer status becomes `IOException`; the SDK supplies no inner exception for status-only failures. |
| Cancellation | Tokens go to connect, transfers, listings, deletion and input copying, with checks before/after I/O and between/following yielded items. Cancellation propagates, and owned clients/buffers are disposed. |

Uploads and downloads use memory proportional to the object size; these are not constant-memory
streaming transfers. Async methods use SDK async I/O, not `Task.Run`. Synchronous legacy methods
block on the same async implementation. Cancellation or transfer failure may leave a partial
or replaced remote file; there is no transaction, rollback, conditional write or snapshot guarantee.
Concurrent calls use independent clients, but concurrent changes to the same remote key are
server-dependent. Do not concurrently mutate an input stream or share factory clients.

## Server And Security Limits

File lookup and listing use FluentFTP `GetListing(..., FtpListOption.UseStat, token)` and
require a successful path-status response (212 or 213). Each listing is the first listing
on its fresh client; stale login/TYPE replies are rejected. This avoids treating the SDK's
null-returning `GetObjectInfo` as proof of absence. Servers must support parseable `STAT path`
directory listings for object lookup/read/delete/list and legacy file lookup/list operations.
FTP 550 can mean missing, permission denied or unsupported operation. An inaccessible or
missing parent/root with an ambiguous response remains an I/O error, not an empty result.
SDK parsing and the server's visible directory entries determine what can be discovered.

Root/path checks are lexical only. They are not a chroot, ACL boundary or proof against
server aliases and symbolic links. Use a least-privileged server account rooted by server
policy when confinement is required. Listing rejects out-of-directory or malformed child
paths, and does not intentionally follow link entries. Direct keys can still traverse
server-resolved aliases. No symlink security claim or real-server interoperability claim is made.

## Legacy Compatibility

All `IStorageProvider` signatures are retained, including `FileInfo` and `DirectoryInfo`.
These remain filesystem-shaped compatibility tokens, not remote metadata. Prefer string
paths and the object interface. In particular, a Windows drive-qualified `FileInfo.FullName`
is rejected; an accepted UNC-shaped value is interpreted as remote path segments.

Legacy relative paths are server-root-relative and ignore `RootPath`. Backslashes are
converted to slashes and outer slashes normalized. Hardening intentionally rejects URI/drive
paths, control characters, repeated internal separators and traversal instead of silently
retargeting an arbitrary URI to the configured host. `GetFullPath` produces an escaped,
credential-free endpoint URI; it is descriptive output, not an accepted input path.
UTF-8 text writes omit a BOM. `UseBinary = false` affects legacy transfers only.
Legacy search supports `*` and `?` case-insensitively; blank search patterns mean all entries.
`DirectoryExists` retains the SDK's boolean semantics (server failures may be reported false).
Nonrecursive directory deletion uses `RMD`, never the SDK's recursive deletion routine;
server rejection, including absence, is an error. Recursive deletion delegates to the SDK.
Deleting the server root is rejected. Disposal rejects subsequent operations; it does not
cancel already-running calls. Client disposal may perform synchronous socket cleanup.

## Retry Ownership And SDK Evidence

There is no Polly policy or provider retry loop. FluentFTP owns its internal recovery and
resume behavior. `RetryAttempts = 1` concerns SDK verification retries, not a promise of
one protocol command or a total disabling of SDK reconnect/resume. Avoid wrapping mutating
operations in another retry policy unless duplicate/partial effects are acceptable.

Verified against the shipped FluentFTP 53.0.2 `IAsyncFtpClient`, `IBaseFtpClient` and `FtpConfig`
APIs using dotnet-inspect, and its NuGet source commit
`074f222240ad8e1db7768697db0a4d0a6c3a92cf`:

- [Upload internals](https://github.com/robinrodricks/FluentFTP/blob/074f222240ad8e1db7768697db0a4d0a6c3a92cf/FluentFTP/Client/AsyncClient/UploadFileInternal.cs): seek/reset behavior and SDK resume/status handling motivate input buffering.
- [DownloadStream](https://github.com/robinrodricks/FluentFTP/blob/074f222240ad8e1db7768697db0a4d0a6c3a92cf/FluentFTP/Client/AsyncClient/DownloadStream.cs): stream-based high-level download and token signature.
- [GetObjectInfo](https://github.com/robinrodricks/FluentFTP/blob/074f222240ad8e1db7768697db0a4d0a6c3a92cf/FluentFTP/Client/AsyncClient/GetObjectInfo.cs): null can represent an error, so the provider does not use it.
- [GetListing](https://github.com/robinrodricks/FluentFTP/blob/074f222240ad8e1db7768697db0a4d0a6c3a92cf/FluentFTP/Client/AsyncClient/GetListing.cs): STAT handling, unavailable metadata and data-channel error suppression.

## Verification

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -TestSourceScope Storage/Ftp -TestPackage vc.Ifx.Storage.Ftp -CoveragePackage vc.Ifx.Storage.Ftp -Filter FullyQualifiedName~Storage.Ftp
```

The serialized runner isolates this package's test sources and inward dependencies. Tests
use the SDK interface mock for every operation that could connect. Tests of the real default
client inspect construction/configuration only and never invoke `Connect`. They cover legacy
and object operations, validation, truthful metadata, client/buffer disposal, failure/status
translation, cancellation, current input position, nonseekable streams and retry ownership.
Measured results and the explicit Orchestrator handoff are recorded in the FTP section of
`docs/planning/framework-upgrade-parallel-plan.md`. Unit coverage is not live-server, full-suite,
Release, package-publication or repository-wide warning/security-gate evidence.
