using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using VisionaryCoder.Framework.Storage;
using VisionaryCoder.Framework.Storage.Local;

namespace vc.Ifx.Storage.Benchmarks;

[MemoryDiagnoser]
public class StorageBenchmarks
{
    private IObjectStorageProvider provider = null!;
    private byte[] payload = [];
    private string root = string.Empty;
    private readonly StorageObjectRequest read = new("seed/0.bin");
    private readonly StorageObjectRequest cycle = new("cycle.bin");

    [Params(1024, 65536)] public int PayloadBytes { get; set; }

    [GlobalSetup]
    public async Task Setup()
    {
        root = Path.Combine(Path.GetTempPath(), "vc.Ifx-benchmarks", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        provider = new LocalStorageProvider(new LocalStorageOptions { RootPath = root }, NullLogger<LocalStorageProvider>.Instance);
        payload = Enumerable.Range(0, PayloadBytes).Select(index => (byte)(index % 251)).ToArray();
        for (int index = 0; index < 16; index++)
        {
            using var content = new MemoryStream(payload, writable: false);
            await provider.WriteAsync(new StorageWriteRequest($"seed/{index}.bin", content));
        }
        await Overwrite();
        if (await ReadAndConsume() != PayloadBytes || await ListSeedObjects() != 16)
            throw new InvalidOperationException("Storage fixture is incomplete.");
    }

    [Benchmark]
    public async Task<long> ReadAndConsume()
    {
        await using Stream content = await provider.OpenReadAsync(read);
        await content.CopyToAsync(Stream.Null);
        return content.Position;
    }

    [Benchmark]
    public async Task<StorageObjectMetadata> Overwrite()
    {
        using var content = new MemoryStream(payload, writable: false);
        return await provider.WriteAsync(new StorageWriteRequest("replace.bin", content));
    }

    [Benchmark]
    public async Task<int> ListSeedObjects()
    {
        int count = 0;
        await foreach (StorageObjectMetadata item in provider.ListAsync(new StorageListRequest("seed/"))) count++;
        return count;
    }

    [Benchmark]
    public async Task CreateAndDelete()
    {
        using var content = new MemoryStream(payload, writable: false);
        await provider.WriteAsync(new StorageWriteRequest(cycle.Path, content, overwrite: false));
        await provider.DeleteAsync(cycle);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        string parent = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "vc.Ifx-benchmarks")) + Path.DirectorySeparatorChar;
        string owned = Path.GetFullPath(root);
        if (!owned.StartsWith(parent, StringComparison.Ordinal) || Path.GetFileName(owned).Length != 32)
            throw new InvalidOperationException("Refusing to remove a directory outside the owned benchmark fixture.");
        Directory.Delete(owned, recursive: true);
    }
}
