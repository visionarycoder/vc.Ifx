# Ifx.Archiving

> Package migration (2026-11-08): this module is now included in `Ifx`.
> Separate-package installation and assembly references below are historical.


Archive contracts and BCL-backed GZip, TAR, and ZIP providers for vc.Ifx.
`Ifx.Archiving` does not depend on third-party archive libraries. The optional
`Ifx.Archiving.SevenZip` package adds 7z support and keeps its `SharpCompress`
dependency outside this package.

## Providers

| Format | Provider | Registration |
| --- | --- | --- |
| GZip | `GZipArchive` | `AddGZipArchiveProvider()` |
| TAR | `TarArchive` | `AddTarArchiveProvider()` |
| ZIP | `ZipArchive` | `AddZipArchiveProvider()` |

Each provider implements `IArchiveProvider` and supports listing and extraction.
GZip represents one compressed stream as one logical entry; `.tgz` output is
named as a `.tar` payload. TAR and ZIP preserve their archive entry paths and
reject extraction paths that resolve outside the destination directory.

```csharp
using Microsoft.Extensions.DependencyInjection;
using vc.Ifx.Archiving;
using vc.Ifx.Archiving.Zip;

ServiceCollection services = new();
services.AddZipArchiveProvider();

using ServiceProvider provider = services.BuildServiceProvider();
IArchiveProvider archiveProvider = provider.GetRequiredService<IArchiveProvider>();
ArchiveExtractionResult result = await archiveProvider.ExtractAsync(
    archivePath: @"C:\archives\sample.zip",
    destinationDirectory: @"C:\output");
```
