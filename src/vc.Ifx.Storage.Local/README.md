# vc.Ifx.Storage.Local

Native filesystem storage for .NET 10 and stable C# 14. The provider implements
both the compatible `IStorageProvider` file/directory API and the provider-neutral
`IObjectStorageProvider` API. It depends inward on Storage.Abstractions and uses
Microsoft logging; no filesystem adapter or shared domain model is introduced.

## Construction and root scope

```csharp
var storage = new LocalStorageProvider(
    new LocalStorageOptions { RootPath = absoluteApplicationDataPath }, logger);
IObjectStorageProvider objects = storage;
```

The original `LocalStorageProvider(ILogger<LocalStorageProvider>)` constructor
remains available. It captures the current working directory as its object root.
`LocalStorageOptions.RootPath` also defaults to the current directory when the
options are created. Roots must be fully qualified and syntactically valid. The
provider captures a normalized absolute root; later option changes do not affect
the instance. Construction does not create a directory or verify permissions.

**The root applies only to object operations.** All legacy file/directory methods
still accept native relative or absolute paths, regardless of the configured root.
The package does not add DI dependencies or registrations. Existing aggregator
`AddLocalStorage(options)` registration selects the options-and-logger constructor;
DI can still use the logger-only constructor when no options are registered.
Applications can register an explicitly constructed instance under either interface
when using the new object API. Current aggregator named registrations share an
unkeyed options singleton: do not assume different named registrations receive
different roots. Per-name options/factory registration requires an aggregator change.

## Object paths and facts

Object keys use `/` between nonempty relative segments. Absolute paths, empty
segments, `.` and `..`, backslashes, colons, control characters, trailing dots or
spaces, and `< > " | ? *` are rejected with `ArgumentException`. This also rejects
Windows drive paths, UNC paths, and alternate data stream syntax. Other native
filename restrictions still apply; path validation is not a universal filename
or device-name validator.

Accepted object keys are combined with the root without case folding. Actual
lookup sensitivity follows the filesystem, so Windows normally treats differently
cased keys as the same file. Listings return the filesystem's stored spelling,
using `/`, and apply literal ordinal prefix matching even on a case-insensitive
filesystem. Prefixes are not wildcard patterns or directory paths.

Metadata contains relative path, file length, and UTC last-write time. Content type
and version are null: file extensions and timestamps are not authoritative media
types or version tokens. A lookup returns null for an absent file, including a
missing parent; directories are not objects. Metadata is a filesystem observation,
not a transactional snapshot.
The metadata reader uses the native
[`FileInfo.Length` missing-file semantics](https://github.com/dotnet/runtime/blob/v10.0.0/src/libraries/System.Private.CoreLib/src/System/IO/FileInfo.cs#L30-L39);
it does not suppress general `IOException` or authorization errors.

## I/O behavior

- `OpenReadAsync` returns a caller-owned readable `FileStream`. The caller must
  dispose it. Missing objects throw `FileNotFoundException`, including when a
  parent directory is absent. The stream allows concurrent readers; its subsequent
  reads use their own cancellation tokens and retain native filesystem errors.
- `WriteAsync` creates missing parent directories, consumes content from its
  current position, and leaves input open on success, error, or cancellation.
  Non-seekable readable streams are accepted. The destination is always disposed.
- Ordinary writes create or truncate the target. `overwrite: false` uses
  `FileMode.CreateNew`, so competing creates cannot replace an existing file.
  Conflicts and sharing violations remain `IOException`. Reads/writes may fail
  while another operation holds an incompatible native file share.
- `DeleteAsync` succeeds for an already absent object or parent directory. It does
  not delete directories or return a race-sensitive existence result.
- `ListAsync` recursively returns files, includes hidden/system files, skips
  reparse-point entries via native enumeration options, and propagates access
  failures. A missing root or no matches produces an empty sequence. There is no
  ordering or snapshot guarantee; concurrent changes can cause an I/O failure.

The provider advertises Read, Write, Delete, Metadata, List, and CreateOnly. These
describe implemented operations, not permission or availability guarantees.

Cancellation is checked before object operations, passed to asynchronous stream
copies/flushes, and checked between enumeration entries. Opening handles and native
metadata/directory/delete calls are synchronous OS operations; cancellation cannot
interrupt a call once the OS is executing it. They are not dispatched through
`Task.Run`. Cancellation during writes may leave an empty or partially written
file and created directories. There is no rollback, atomic replace, or durable
disk-flush promise. Canceling the token used to open a returned read stream does
not close that stream.

## Security boundary

The root is a naming scope for trusted application data, **not a security sandbox**.
Lexical path checks prevent direct `..` and rooted-key traversal. The provider does
not resolve, lock, or verify every filesystem component against symbolic links,
junctions, mount points, or concurrent path replacement. Direct operations can
follow links outside the root. Skipping reparse-point entries during listing does
not secure other operations or the root itself. No symlink confinement guarantee
is claimed or tested. Restrict filesystem permissions and use a root whose directory
structure is controlled by trusted code. The legacy API has no root restriction.

## Legacy compatibility

Legacy synchronous/asynchronous file reads and writes preserve native filesystem
behavior and do not create missing parent directories. Null/blank paths and embedded
null characters are rejected consistently. Null contents and blank search patterns
are rejected. File and directory deletes remain idempotent for absence; errors such
as access denial or nonempty nonrecursive directory deletion propagate. Existence
checks retain native `File.Exists`/`Directory.Exists` false-on-error behavior; new
object metadata lookups do not use those checks to hide operational errors.

Async directory/delete methods check cancellation before their synchronous native
operation. Async enumeration is lazy instead of materializing the full directory,
and observes cancellation even for an empty directory. Some native-operation errors
can be thrown before a task is returned. Native exceptions propagate without redundant
catch/rethrow layers; path operations emit debug logs without content or credentials.

## Verification

Tests under `tests/unit/vc.Ifx.UnitTests/Storage/Local` use uniquely owned temporary
directories and deterministic stream controls. They cover both interfaces, root
capture, validation, metadata, missing paths, ownership, cancellation, failure
propagation, prefix filtering, and competing create-only writes. No live service,
random timing delay, or symlink privilege is required.

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -TestSourceScope Storage/Local -TestPackage vc.Ifx.Storage.Local -CoveragePackage vc.Ifx.Storage.Local -Filter FullyQualifiedName~Storage.Local
```

Use the infrastructure wrapper for builds and tests so coverage instrumentation is
serialized. The command compiles all local-storage tests, including the DI constructor
selection check, and measures the complete local package; it is not full-suite
evidence. Measured coverage and remaining global gates are recorded in this
package's section of `docs/planning/framework-upgrade-parallel-plan.md`. Filesystem
behavior is verified on the test host's operating system; passing Windows tests does
not establish Unix permission behavior or network-filesystem consistency guarantees.
