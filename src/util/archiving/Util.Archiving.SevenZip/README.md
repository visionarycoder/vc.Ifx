# Util.Archiving.SevenZip

7z archive helpers for listing and extracting `.7z` files.

`Util.Archiving.SevenZip` exposes read-only 7z operations for `Util.Archiving`. The provider supports archive listing and extraction through `IArchiveProvider`. It does not add 7z archive creation APIs to this package.

## Usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using Util.Archiving;
using Util.Archiving.SevenZip;

ServiceCollection services = new();
services.AddSevenZipArchiving();

using ServiceProvider provider = services.BuildServiceProvider();
IArchiveProvider archiveProvider = provider.GetRequiredService<IArchiveProvider>();

ArchiveExtractionResult result = await archiveProvider.ExtractAsync(
    archivePath: @"C:\archives\sample.7z",
    destinationDirectory: @"C:\output");
```
