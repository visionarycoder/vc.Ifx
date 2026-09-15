# Util.Archiving.GZip

GZip archive helpers for listing and extracting `.gz`, `.tar.gz`, and `.tgz` files.

`Util.Archiving.GZip` models GZip as a single compressed stream rather than a multi-entry container. `ListEntriesAsync` returns one logical entry whose name is inferred from the archive filename. For `.tgz` inputs, the logical entry name ends with `.tar` because the decompressed payload is typically a TAR stream.

## Usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using Util.Archiving;
using Util.Archiving.GZip;

ServiceCollection services = new();
services.AddGZipArchiveProvider();

using ServiceProvider provider = services.BuildServiceProvider();
IArchiveProvider archiveProvider = provider.GetRequiredService<IArchiveProvider>();

ArchiveExtractionResult result = await archiveProvider.ExtractAsync(
    archivePath: @"C:\archives\sample.txt.gz",
    destinationDirectory: @"C:\output");
```

GZip is commonly paired with TAR as `.tar.gz` or `.tgz`. This package only handles the GZip compression layer.
