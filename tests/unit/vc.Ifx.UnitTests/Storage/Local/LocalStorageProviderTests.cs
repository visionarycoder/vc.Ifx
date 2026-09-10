using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VisionaryCoder.Framework.Storage;
using VisionaryCoder.Framework.Storage.Local;

namespace VisionaryCoder.Framework.Tests.Storage.Local;

[TestClass]
public sealed class LocalStorageProviderTests
{
    private DirectoryInfo temporaryDirectory = null!;
    private LocalStorageProvider provider = null!;
    private string root = null!;

    [TestInitialize]
    public void Initialize()
    {
        temporaryDirectory = Directory.CreateTempSubdirectory("vc-ifx-local-");
        root = Path.Combine(temporaryDirectory.FullName, "objects");
        provider = CreateProvider(root);
    }

    [TestCleanup]
    public void Cleanup()
    {
        string temporaryPath = Path.GetFullPath(temporaryDirectory.FullName);
        Assert.IsTrue(temporaryPath.StartsWith(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase));
        Directory.Delete(temporaryPath, recursive: true);
    }

    [TestMethod]
    public void OptionsAndConstructorsValidateWithoutCreatingRoot()
    {
        var options = new LocalStorageOptions();
        Assert.AreEqual(Directory.GetCurrentDirectory(), options.RootPath);
        options.Validate();
        Assert.IsInstanceOfType<IObjectStorageProvider>(new LocalStorageProvider(NullLogger<LocalStorageProvider>.Instance));
        Assert.Throws<ArgumentNullException>(() => new LocalStorageProvider(null!));
        Assert.Throws<ArgumentNullException>(() => new LocalStorageProvider(null!, NullLogger<LocalStorageProvider>.Instance));
        Assert.Throws<ArgumentNullException>(() => new LocalStorageProvider(options, null!));
        Assert.Throws<ArgumentException>(() => CreateProvider(Path.Combine(root, "bad\0root")));
        Assert.IsFalse(Directory.Exists(root));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("relative")]
    public void OptionsRejectInvalidRoots(string? path)
    {
        Assert.Throws<ArgumentException>(() => new LocalStorageOptions { RootPath = path! }.Validate());
    }

    [TestMethod]
    public async Task ObjectRootIsCapturedAndDoesNotScopeLegacyPaths()
    {
        var options = new LocalStorageOptions { RootPath = root };
        var configured = new LocalStorageProvider(options, NullLogger<LocalStorageProvider>.Instance);
        options.RootPath = Path.Combine(temporaryDirectory.FullName, "changed");
        using var content = new MemoryStream([7]);
        await configured.WriteAsync(new("stored", content));
        Assert.IsTrue(File.Exists(Path.Combine(root, "stored")));
        Assert.IsFalse(Directory.Exists(options.RootPath));

        string outside = Path.Combine(temporaryDirectory.FullName, "legacy.txt");
        configured.WriteAllText(outside, "legacy");
        Assert.AreEqual("legacy", configured.ReadAllText(outside));
    }

    [TestMethod]
    public async Task DependencyInjectionSelectsCompatibleConstructors()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILogger<LocalStorageProvider>>(NullLogger<LocalStorageProvider>.Instance);
        services.AddSingleton(new LocalStorageOptions { RootPath = root });
        services.AddTransient<IStorageProvider, LocalStorageProvider>();
        services.AddKeyedTransient<IStorageProvider, LocalStorageProvider>("named");
        using ServiceProvider container = services.BuildServiceProvider();
        IStorageProvider local = container.GetRequiredService<IStorageProvider>();
        IStorageProvider named = container.GetRequiredKeyedService<IStorageProvider>("named");
        Assert.IsInstanceOfType<LocalStorageProvider>(local);
        Assert.IsInstanceOfType<LocalStorageProvider>(named);
        using var content = new MemoryStream([1]);
        await ((IObjectStorageProvider)local).WriteAsync(new("resolved", content));
        Assert.IsNotNull(await ((IObjectStorageProvider)named).GetMetadataAsync(new("resolved")));

        var legacyServices = new ServiceCollection();
        legacyServices.AddSingleton<ILogger<LocalStorageProvider>>(NullLogger<LocalStorageProvider>.Instance);
        legacyServices.AddTransient<IStorageProvider, LocalStorageProvider>();
        using ServiceProvider legacyContainer = legacyServices.BuildServiceProvider();
        Assert.IsInstanceOfType<LocalStorageProvider>(legacyContainer.GetRequiredService<IStorageProvider>());
    }

    [TestMethod]
    public async Task ObjectRoundTripPreservesOwnershipPositionAndMetadata()
    {
        using var content = new MemoryStream([0, 1, 2, 3]);
        content.Position = 1;
        StorageObjectMetadata written = await provider.WriteAsync(new("folder/object.bin", content));
        Assert.AreEqual("folder/object.bin", written.Path);
        Assert.AreEqual(3L, written.Length);
        Assert.AreEqual(4L, content.Position);
        Assert.IsTrue(content.CanRead);
        Assert.IsNotNull(written.LastModified);
        Assert.AreEqual(TimeSpan.Zero, written.LastModified.Value.Offset);
        Assert.IsNull(written.ContentType);
        Assert.IsNull(written.Version);

        StorageObjectMetadata? metadata = await provider.GetMetadataAsync(new("folder/object.bin"));
        Assert.AreEqual(written, metadata);
        await using Stream read = await provider.OpenReadAsync(new("folder/object.bin"));
        using var result = new MemoryStream();
        await read.CopyToAsync(result);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, result.ToArray());
        Assert.IsTrue(read.CanRead);
        Assert.AreEqual(StorageCapabilities.Read | StorageCapabilities.Write | StorageCapabilities.Delete |
            StorageCapabilities.Metadata | StorageCapabilities.List | StorageCapabilities.CreateOnly, provider.Capabilities);
    }

    [TestMethod]
    public async Task ObjectWritesReplaceAndCreateOnlyRejectsExistingContent()
    {
        using var first = new MemoryStream([1, 2, 3]);
        await provider.WriteAsync(new("object", first, overwrite: false));
        using var replacement = new MemoryStream([4]);
        await provider.WriteAsync(new("object", replacement));
        using var rejected = new MemoryStream([5]);
        await Assert.ThrowsAsync<IOException>(() => provider.WriteAsync(new("object", rejected, overwrite: false)));
        CollectionAssert.AreEqual(new byte[] { 4 }, File.ReadAllBytes(Path.Combine(root, "object")));
        Assert.IsTrue(rejected.CanRead);
        Assert.AreEqual(0L, rejected.Position);
    }

    [TestMethod]
    public async Task ConcurrentCreateOnlyWritesHaveExactlyOneWinner()
    {
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<bool>[] writes = Enumerable.Range(0, 8).Select(index => Task.Run(async () =>
        {
            await ready.Task;
            using var content = new MemoryStream([(byte)index]);
            try { await provider.WriteAsync(new("contended", content, overwrite: false)); return true; }
            catch (IOException) { return false; }
        })).ToArray();
        ready.SetResult();
        bool[] results = await Task.WhenAll(writes);
        Assert.AreEqual(1, results.Count(success => success));
        Assert.AreEqual(1L, (await provider.GetMetadataAsync(new("contended")))!.Length);
    }

    [TestMethod]
    public async Task MissingObjectsAndMissingParentsHaveExplicitSemantics()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(() => provider.OpenReadAsync(new("missing/child")));
        Assert.IsNull(await provider.GetMetadataAsync(new("missing/child")));
        await provider.DeleteAsync(new("missing/child"));
        Assert.AreEqual(0, (await ReadAll(provider.ListAsync(new()))).Count);
        Directory.CreateDirectory(root);
        await Assert.ThrowsAsync<FileNotFoundException>(() => provider.OpenReadAsync(new("missing")));
        Assert.IsNull(await provider.GetMetadataAsync(new("missing")));
        await provider.DeleteAsync(new("missing"));
        Directory.CreateDirectory(Path.Combine(root, "directory"));
        Assert.IsNull(await provider.GetMetadataAsync(new("directory")));
        Assert.AreEqual(0, (await ReadAll(provider.ListAsync(new()))).Count);
    }

    [TestMethod]
    public async Task DeeplyMissingRootListsEmpty()
    {
        var nested = CreateProvider(Path.Combine(root, "missing", "deeper"));
        Assert.AreEqual(0, (await ReadAll(nested.ListAsync(new()))).Count);
    }

    [TestMethod]
    public async Task ObjectDeleteIsIdempotent()
    {
        using var content = new MemoryStream([1]);
        await provider.WriteAsync(new("object", content));
        await provider.DeleteAsync(new("object"));
        await provider.DeleteAsync(new("object"));
        Assert.IsNull(await provider.GetMetadataAsync(new("object")));
    }

    [TestMethod]
    public async Task ListsRecurseAndUseLiteralOrdinalPrefixes()
    {
        foreach (string name in new[] { "A/one", "A/two", "B/three", "other" })
        {
            using var content = new MemoryStream([1]);
            await provider.WriteAsync(new(name, content));
        }

        CollectionAssert.AreEquivalent(new[] { "A/one", "A/two", "B/three", "other" },
            (await ReadAll(provider.ListAsync(new()))).Select(item => item.Path).ToArray());
        CollectionAssert.AreEquivalent(new[] { "A/one", "A/two" },
            (await ReadAll(provider.ListAsync(new("A/")))).Select(item => item.Path).ToArray());
        Assert.AreEqual(0, (await ReadAll(provider.ListAsync(new("*")))).Count);
        Assert.AreEqual(0, (await ReadAll(provider.ListAsync(new("a/")))).Count);
    }

    [TestMethod]
    [DataRow("../escape")]
    [DataRow("nested/../../escape")]
    [DataRow(".")]
    [DataRow("..")]
    [DataRow("/absolute")]
    [DataRow("trailing/")]
    [DataRow("double//slash")]
    [DataRow("back\\slash")]
    [DataRow("C:relative")]
    [DataRow("object:stream")]
    [DataRow("trailing.")]
    [DataRow("trailing ")]
    [DataRow("wild*card")]
    [DataRow("control\tcharacter")]
    [DataRow("null\0character")]
    public async Task ObjectPathsRejectTraversalAndAmbiguousNames(string path)
    {
        using var content = new MemoryStream();
        await Assert.ThrowsAsync<ArgumentException>(() => provider.OpenReadAsync(new(path)));
        await Assert.ThrowsAsync<ArgumentException>(() => provider.WriteAsync(new(path, content)));
        await Assert.ThrowsAsync<ArgumentException>(() => provider.GetMetadataAsync(new(path)));
        await Assert.ThrowsAsync<ArgumentException>(() => provider.DeleteAsync(new(path)));
        Assert.IsFalse(Directory.Exists(root));
    }

    [TestMethod]
    public async Task ObjectOperationsRejectNullRequests()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.OpenReadAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.WriteAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.GetMetadataAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.DeleteAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => ReadAll(provider.ListAsync(null!)));
    }

    [TestMethod]
    public async Task PreCanceledObjectOperationsDoNotCreateRootOrDisposeInput()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        using var content = new MemoryStream([1]);
        CancellationToken token = cancellation.Token;
        OperationCanceledException failure = await Assert.ThrowsAsync<OperationCanceledException>(() => provider.WriteAsync(new("object", content), token));
        Assert.AreEqual(token, failure.CancellationToken);
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.OpenReadAsync(new("object"), token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.GetMetadataAsync(new("object"), token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.DeleteAsync(new("object"), token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => ReadAll(provider.ListAsync(new(), token)));
        Assert.IsFalse(Directory.Exists(root));
        Assert.IsTrue(content.CanRead);
    }

    [TestMethod]
    public async Task CancellationDuringStreamingDisposesOutputButLeavesInputOpen()
    {
        using var cancellation = new CancellationTokenSource();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int reads = 0;
        using var content = new TestReadStream(async (buffer, token) =>
        {
            if (reads++ == 0)
            {
                buffer.Span[0] = 7;
                return 1;
            }

            entered.SetResult();
            await Task.Delay(Timeout.Infinite, token);
            return 0;
        });
        Task write = provider.WriteAsync(new("object", content), cancellation.Token);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        cancellation.Cancel();
        OperationCanceledException failure = await Assert.ThrowsAsync<OperationCanceledException>(() => write);
        Assert.AreEqual(cancellation.Token, failure.CancellationToken);
        Assert.IsFalse(content.Disposed);
        using FileStream exclusive = File.Open(Path.Combine(root, "object"), FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Assert.AreEqual(1L, exclusive.Length);
        Assert.AreEqual(7, exclusive.ReadByte());
    }

    [TestMethod]
    public async Task ReadFailureDisposesOutputAndPreservesOriginalException()
    {
        var failure = new IOException("Input failure");
        using var content = new TestReadStream((_, _) => throw failure);
        Assert.AreSame(failure, await Assert.ThrowsAsync<IOException>(() => provider.WriteAsync(new("object", content))));
        Assert.IsFalse(content.Disposed);
        using FileStream exclusive = File.Open(Path.Combine(root, "object"), FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    }

    [TestMethod]
    public async Task CancellationAtEndOfInputPreventsSuccessfulWriteResult()
    {
        using var cancellation = new CancellationTokenSource();
        using var content = new TestReadStream((_, _) =>
        {
            cancellation.Cancel();
            return ValueTask.FromResult(0);
        });
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.WriteAsync(new("object", content), cancellation.Token));
        Assert.IsFalse(content.Disposed);
        using FileStream exclusive = File.Open(Path.Combine(root, "object"), FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    }

    [TestMethod]
    public async Task DisposedInputAfterRequestCreationIsRejectedBeforeRootCreation()
    {
        var content = new MemoryStream();
        var request = new StorageWriteRequest("object", content);
        content.Dispose();
        await Assert.ThrowsAsync<ArgumentException>(() => provider.WriteAsync(request));
        Assert.IsFalse(Directory.Exists(root));
    }

    [TestMethod]
    public async Task CancellationBetweenListItemsStopsBothEnumerators()
    {
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "one"), "1");
        File.WriteAllText(Path.Combine(root, "two"), "2");
        using var cancellation = new CancellationTokenSource();
        await using IAsyncEnumerator<StorageObjectMetadata> objects = provider.ListAsync(new()).GetAsyncEnumerator(cancellation.Token);
        await using IAsyncEnumerator<string> files = provider.EnumerateFilesAsync(root).GetAsyncEnumerator(cancellation.Token);
        Assert.IsTrue(await objects.MoveNextAsync());
        Assert.IsTrue(await files.MoveNextAsync());
        cancellation.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => objects.MoveNextAsync().AsTask());
        await Assert.ThrowsAsync<OperationCanceledException>(() => files.MoveNextAsync().AsTask());
    }

    [TestMethod]
    public async Task FilesystemFailuresAreNotConvertedToMissingObjects()
    {
        File.WriteAllText(root, "not a directory");
        await Assert.ThrowsAsync<IOException>(() => ReadAll(provider.ListAsync(new())));
        await Assert.ThrowsAsync<IOException>(() => provider.GetMetadataAsync(new("object")));
        await Assert.ThrowsAsync<IOException>(() => provider.OpenReadAsync(new("object")));
        await Assert.ThrowsAsync<IOException>(() => provider.DeleteAsync(new("object")));
        using var content = new MemoryStream([1]);
        await Assert.ThrowsAsync<IOException>(() => provider.WriteAsync(new("object", content)));
        Assert.IsTrue(content.CanRead);
    }

    [TestMethod]
    public async Task NonSeekableContentAndEmptyObjectsRoundTrip()
    {
        int reads = 0;
        using var content = new TestReadStream((buffer, _) =>
        {
            if (reads++ != 0) { return ValueTask.FromResult(0); }
            buffer.Span[0] = 9;
            return ValueTask.FromResult(1);
        });
        Assert.AreEqual(1L, (await provider.WriteAsync(new("streamed", content))).Length);
        Assert.IsFalse(content.Disposed);
        using var empty = new MemoryStream();
        Assert.AreEqual(0L, (await provider.WriteAsync(new("empty", empty))).Length);
        await using Stream read = await provider.OpenReadAsync(new("empty"));
        Assert.AreEqual(-1, read.ReadByte());
    }

    [TestMethod]
    public async Task ObjectAccessFailuresPropagateAndInputStaysOpen()
    {
        Directory.CreateDirectory(root);
        string directory = Path.Combine(root, "directory");
        Directory.CreateDirectory(directory);
        using var content = new MemoryStream([1]);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => provider.OpenReadAsync(new("directory")));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => provider.WriteAsync(new("directory", content)));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => provider.DeleteAsync(new("directory")));
        Assert.IsTrue(content.CanRead);
        Assert.IsTrue(Directory.Exists(directory));
    }

    [TestMethod]
    public async Task NativeFileSharingPreservesHeldContentWithPlatformDeleteSemantics()
    {
        using var content = new MemoryStream([1]);
        await provider.WriteAsync(new("locked", content));
        using FileStream held = File.Open(Path.Combine(root, "locked"), FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        await Assert.ThrowsAsync<IOException>(() => provider.OpenReadAsync(new("locked")));
        using var replacement = new MemoryStream([2]);
        await Assert.ThrowsAsync<IOException>(() => provider.WriteAsync(new("locked", replacement)));
        if (OperatingSystem.IsWindows())
        {
            await Assert.ThrowsAsync<IOException>(() => provider.DeleteAsync(new("locked")));
            Assert.IsTrue(File.Exists(Path.Combine(root, "locked")));
        }
        else
        {
            await provider.DeleteAsync(new("locked"));
            Assert.IsFalse(File.Exists(Path.Combine(root, "locked")));
        }
        Assert.AreEqual(1, held.ReadByte());
        Assert.IsTrue(replacement.CanRead);
    }

    [TestMethod]
    public async Task ReadStreamRemainsUsableAndSupportsCancellationAfterOpen()
    {
        using var content = new MemoryStream([1]);
        await provider.WriteAsync(new("object", content));
        using var opening = new CancellationTokenSource();
        Stream read = await provider.OpenReadAsync(new("object"), opening.Token);
        await using (read)
        {
            opening.Cancel();
            Assert.AreEqual(1, read.ReadByte());
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(() => read.ReadAsync(new byte[1], cancellation.Token).AsTask());
        }
        Assert.IsFalse(read.CanRead);
    }

    [TestMethod]
    public async Task LegacyFileAndDirectoryOperationsRemainCompatible()
    {
        Assert.IsFalse(provider.DirectoryExists(root));
        Assert.AreEqual(root, provider.CreateDirectory(root).FullName);
        Assert.IsTrue(provider.DirectoryExists(root));
        string child = Path.Combine(root, "child");
        Assert.AreEqual(child, (await provider.CreateDirectoryAsync(child)).FullName);
        string file = Path.Combine(root, "file.txt");
        var info = new FileInfo(file);
        Assert.IsFalse(provider.FileExists(info));
        Assert.IsFalse(provider.FileExists(file));
        provider.WriteAllText(file, "hello");
        Assert.IsTrue(provider.FileExists(info));
        Assert.IsTrue(provider.FileExists(file));
        Assert.AreEqual("hello", provider.ReadAllText(file));
        await provider.WriteAllTextAsync(file, "world");
        Assert.AreEqual("world", await provider.ReadAllTextAsync(file));
        provider.WriteAllBytes(file, [1, 2]);
        CollectionAssert.AreEqual(new byte[] { 1, 2 }, provider.ReadAllBytes(file));
        await provider.WriteAllBytesAsync(file, [3]);
        CollectionAssert.AreEqual(new byte[] { 3 }, await provider.ReadAllBytesAsync(file));
        CollectionAssert.AreEqual(new[] { file }, provider.GetFiles(root, "*.txt"));
        CollectionAssert.AreEqual(new[] { child }, provider.GetDirectories(root));
        CollectionAssert.AreEqual(new[] { file }, (await ReadAll(provider.EnumerateFilesAsync(root))).ToArray());
        Assert.AreEqual(Path.GetFullPath(file), provider.GetFullPath(file));
        Assert.AreEqual(root, provider.GetDirectoryName(file));
        Assert.IsNull(provider.GetDirectoryName(Path.GetPathRoot(root)!));
        Assert.AreEqual("file.txt", provider.GetFileName(file));
        await Assert.ThrowsAsync<IOException>(() => provider.DeleteDirectoryAsync(root, recursive: false));
        provider.DeleteFile(file);
        await provider.DeleteFileAsync(file);
        provider.DeleteFile(Path.Combine(root, "absent", "file"));
        Assert.IsFalse(provider.FileExists(info));
        Assert.AreEqual(0, (await ReadAll(provider.EnumerateFilesAsync(root))).Count);
        provider.DeleteDirectory(child, recursive: false);
        await provider.DeleteDirectoryAsync(root);
        provider.DeleteDirectory(root);
        await provider.DeleteDirectoryAsync(root);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("bad\0path")]
    public async Task LegacyPathsAreValidatedConsistently(string? path)
    {
        Assert.Throws<ArgumentException>(() => provider.FileExists(path!));
        Assert.Throws<ArgumentException>(() => provider.ReadAllText(path!));
        await Assert.ThrowsAsync<ArgumentException>(() => provider.ReadAllTextAsync(path!));
        Assert.Throws<ArgumentException>(() => provider.ReadAllBytes(path!));
        await Assert.ThrowsAsync<ArgumentException>(() => provider.ReadAllBytesAsync(path!));
        Assert.Throws<ArgumentException>(() => provider.WriteAllText(path!, ""));
        await Assert.ThrowsAsync<ArgumentException>(() => provider.WriteAllTextAsync(path!, ""));
        Assert.Throws<ArgumentException>(() => provider.WriteAllBytes(path!, []));
        await Assert.ThrowsAsync<ArgumentException>(() => provider.WriteAllBytesAsync(path!, []));
        Assert.Throws<ArgumentException>(() => provider.DeleteFile(path!));
        await Assert.ThrowsAsync<ArgumentException>(() => provider.DeleteFileAsync(path!));
        Assert.Throws<ArgumentException>(() => provider.DirectoryExists(path!));
        Assert.Throws<ArgumentException>(() => provider.CreateDirectory(path!));
        await Assert.ThrowsAsync<ArgumentException>(() => provider.CreateDirectoryAsync(path!));
        Assert.Throws<ArgumentException>(() => provider.DeleteDirectory(path!));
        await Assert.ThrowsAsync<ArgumentException>(() => provider.DeleteDirectoryAsync(path!));
        Assert.Throws<ArgumentException>(() => provider.GetFiles(path!));
        Assert.Throws<ArgumentException>(() => provider.GetDirectories(path!));
        await Assert.ThrowsAsync<ArgumentException>(() => ReadAll(provider.EnumerateFilesAsync(path!)));
        Assert.Throws<ArgumentException>(() => provider.GetFullPath(path!));
        Assert.Throws<ArgumentException>(() => provider.GetDirectoryName(path!));
        Assert.Throws<ArgumentException>(() => provider.GetFileName(path!));
    }

    [TestMethod]
    public async Task LegacyArgumentsAndFailuresPropagate()
    {
        string file = Path.Combine(root, "file");
        Assert.Throws<ArgumentNullException>(() => provider.FileExists((FileInfo)null!));
        Assert.Throws<ArgumentNullException>(() => provider.WriteAllText(file, null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.WriteAllTextAsync(file, null!));
        Assert.Throws<ArgumentNullException>(() => provider.WriteAllBytes(file, null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.WriteAllBytesAsync(file, null!));
        Assert.Throws<ArgumentException>(() => provider.GetFiles(root, ""));
        Assert.Throws<ArgumentException>(() => provider.GetDirectories(root, ""));
        await Assert.ThrowsAsync<ArgumentException>(() => ReadAll(provider.EnumerateFilesAsync(root, "")));
        Assert.Throws<DirectoryNotFoundException>(() => provider.GetFiles(root));
        Assert.Throws<DirectoryNotFoundException>(() => provider.GetDirectories(root));
        await Assert.ThrowsAsync<DirectoryNotFoundException>(() => ReadAll(provider.EnumerateFilesAsync(root)));
        Directory.CreateDirectory(root);
        Assert.Throws<FileNotFoundException>(() => provider.ReadAllText(file));
        await Assert.ThrowsAsync<FileNotFoundException>(() => provider.ReadAllTextAsync(file));
        Assert.Throws<FileNotFoundException>(() => provider.ReadAllBytes(file));
        await Assert.ThrowsAsync<FileNotFoundException>(() => provider.ReadAllBytesAsync(file));
        Assert.Throws<UnauthorizedAccessException>(() => provider.WriteAllText(root, ""));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => provider.WriteAllBytesAsync(root, []));
        Assert.Throws<UnauthorizedAccessException>(() => provider.DeleteFile(root));
    }

    [TestMethod]
    public async Task PreCanceledLegacyOperationsDoNotMutateFilesystem()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        CancellationToken token = cancellation.Token;
        string file = Path.Combine(root, "object");
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.ReadAllTextAsync(file, token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.ReadAllBytesAsync(file, token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.WriteAllTextAsync(file, "", token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.WriteAllBytesAsync(file, [], token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.CreateDirectoryAsync(root, token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.DeleteFileAsync(file, token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.DeleteDirectoryAsync(root, cancellationToken: token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => ReadAll(provider.EnumerateFilesAsync(root, cancellationToken: token)));
        Assert.IsFalse(Directory.Exists(root));
    }

    private static LocalStorageProvider CreateProvider(string path) =>
        new(new LocalStorageOptions { RootPath = path }, NullLogger<LocalStorageProvider>.Instance);

    private static async Task<List<T>> ReadAll<T>(IAsyncEnumerable<T> items)
    {
        var result = new List<T>();
        await foreach (T item in items) { result.Add(item); }
        return result;
    }

    private sealed class TestReadStream(Func<Memory<byte>, CancellationToken, ValueTask<int>> read) : Stream
    {
        public bool Disposed { get; private set; }
        public override bool CanRead => !Disposed;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => read(buffer, cancellationToken);
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
    }
}
