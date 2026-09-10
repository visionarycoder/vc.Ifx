using FluentFTP;
using FluentFTP.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using VisionaryCoder.Framework.Storage;
using VisionaryCoder.Framework.Storage.Ftp;

namespace VisionaryCoder.Framework.Tests.Storage.Ftp;

[TestClass]
public sealed class FtpStorageProviderTests
{
    private Mock<IAsyncFtpClient> client = null!;
    private FtpStorageProvider provider = null!;
    private FtpConfig config = null!;
    private FtpListItem[] listing = [];
    private FtpReply reply;
    private int created;

    [TestInitialize]
    public void Initialize()
    {
        client = new Mock<IAsyncFtpClient>(MockBehavior.Strict);
        config = new FtpConfig { UploadDataType = FtpDataType.ASCII, DownloadDataType = FtpDataType.ASCII };
        reply = new FtpReply { Code = "213", Message = "Status" };
        client.SetupGet(value => value.Config).Returns(config);
        client.SetupGet(value => value.LastReply).Returns(() => reply);
        client.Setup(value => value.Connect(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        client.Setup(value => value.Dispose());
        client.Setup(value => value.GetListing(It.IsAny<string>(), FtpListOption.UseStat, It.IsAny<CancellationToken>())).ReturnsAsync(() => listing);
        provider = new FtpStorageProvider(Options(), NullLogger<FtpStorageProvider>.Instance, () => { created++; return client.Object; });
    }

    private static FtpStorageOptions Options(string root = "/") => new() { Host = "ftp.example.test", Username = "user", Password = "secret", RootPath = root };
    private static FtpListItem Item(string path, FtpObjectType type = FtpObjectType.File, long size = 5, DateTime modified = default)
        => new() { FullName = path, Name = path[(path.LastIndexOf('/') + 1)..], Type = type, Size = size, Modified = modified };

    [TestMethod]
    public void ConstructorsAndRealDefaultClientAreNetworkFree()
    {
        using var defaultProvider = new FtpStorageProvider(Options(), NullLogger<FtpStorageProvider>.Instance);
        var factory = (Func<IAsyncFtpClient>)typeof(FtpStorageProvider).GetField("clientFactory", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(defaultProvider)!;
        using IAsyncFtpClient actual = factory();
        Assert.IsFalse(actual.IsConnected);
        Assert.AreEqual("ftp.example.test", actual.Host);
        Assert.AreEqual(21, actual.Port);
        Assert.AreEqual("user", actual.Credentials.UserName);
        Assert.AreEqual("secret", actual.Credentials.Password);
        Assert.AreEqual(FtpEncryptionMode.None, actual.Config.EncryptionMode);
        Assert.AreEqual(FtpDataConnectionType.PASV, actual.Config.DataConnectionType);
        Assert.AreEqual(FtpDataType.Binary, actual.Config.UploadDataType);
        Assert.AreEqual(FtpDataType.Binary, actual.Config.DownloadDataType);
        Assert.AreEqual(8192, actual.Config.TransferChunkSize);
        Assert.AreEqual(30000, actual.Config.ConnectTimeout);
        Assert.AreEqual(30000, actual.Config.ReadTimeout);
        Assert.AreEqual(30000, actual.Config.DataConnectionConnectTimeout);
        Assert.AreEqual(30000, actual.Config.DataConnectionReadTimeout);
        Assert.AreEqual(1, actual.Config.RetryAttempts);
        Assert.AreEqual(FtpDate.ServerTime, actual.Config.TimeConversion);
        Assert.IsFalse(actual.Config.ValidateAnyCertificate);
        Assert.Throws<ArgumentNullException>(() => new FtpStorageProvider(null!, NullLogger<FtpStorageProvider>.Instance));
        Assert.Throws<ArgumentNullException>(() => new FtpStorageProvider(Options(), null!));
        Assert.Throws<ArgumentNullException>(() => new FtpStorageProvider(Options(), NullLogger<FtpStorageProvider>.Instance, null!));
        Assert.IsInstanceOfType<IStorageProvider>(defaultProvider);
        Assert.AreEqual(StorageCapabilities.Read | StorageCapabilities.Write | StorageCapabilities.Delete | StorageCapabilities.Metadata | StorageCapabilities.List, provider.Capabilities);
    }

    [TestMethod]
    public void AlternateOptionsMapRealSdkConfiguration()
    {
        var options = new FtpStorageOptions { Host = "::1", Port = 2121, Username = "user", Password = "secret", UseSsl = true, UsePassive = false, UseBinary = false, KeepAlive = true, TimeoutMilliseconds = 42, BufferSize = 1234 };
        options.Validate();
        Assert.AreEqual("ftps://[::1]:2121", options.ServerUri);
        Assert.AreEqual("ftp://ftp.example.test", Options().ServerUri);
        using IAsyncFtpClient actual = FtpStorageProvider.CreateClient(options);
        Assert.AreEqual(FtpEncryptionMode.Explicit, actual.Config.EncryptionMode);
        Assert.AreEqual(FtpDataConnectionType.PORT, actual.Config.DataConnectionType);
        Assert.AreEqual(FtpDataType.ASCII, actual.Config.UploadDataType);
        Assert.AreEqual(FtpDataType.ASCII, actual.Config.DownloadDataType);
        Assert.IsTrue(actual.Config.SocketKeepAlive);
        Assert.AreEqual(42, actual.Config.ConnectTimeout);
        Assert.AreEqual(1234, actual.Config.TransferChunkSize);
    }

    [TestMethod]
    public void OptionsValidateValuesRatherThanPropertyNames()
    {
        foreach (string? host in new[] { null, "", " ", "bad host", "ftp://host", "host/path", "host\r\nUSER admin" })
            Assert.Throws<ArgumentException>(() => new FtpStorageOptions { Host = host!, Username = "u", Password = "p" }.Validate());
        foreach (string? credential in new[] { null, "", " ", "u\nPASS p" })
        {
            Assert.Throws<ArgumentException>(() => new FtpStorageOptions { Host = "host", Username = credential!, Password = "p" }.Validate());
            Assert.Throws<ArgumentException>(() => new FtpStorageOptions { Host = "host", Username = "u", Password = credential! }.Validate());
        }
        foreach (int port in new[] { 0, 65536 })
            Assert.Throws<ArgumentOutOfRangeException>(() => new FtpStorageOptions { Host = "host", Username = "u", Password = "p", Port = port }.Validate());
        Assert.Throws<ArgumentOutOfRangeException>(() => new FtpStorageOptions { Host = "host", Username = "u", Password = "p", TimeoutMilliseconds = 0 }.Validate());
        Assert.Throws<ArgumentOutOfRangeException>(() => new FtpStorageOptions { Host = "host", Username = "u", Password = "p", BufferSize = -1 }.Validate());
        foreach (string root in new[] { "", "relative", "//root", "/a\\b", "/a:b", "/a//b", "/a/./b", "/a/../b" })
            Assert.Throws<ArgumentException>(() => Options(root).Validate());
        Options("/root/nested/").Validate();
    }

    [TestMethod]
    public async Task InvalidArgumentsDoNotCreateClients()
    {
        Assert.Throws<ArgumentNullException>(() => provider.FileExists((FileInfo)null!));
        Assert.Throws<ArgumentException>(() => provider.FileExists(new FileInfo(@"C:\file")));
        Assert.Throws<ArgumentNullException>(() => provider.OpenReadAsync(null!));
        Assert.Throws<ArgumentNullException>(() => provider.GetMetadataAsync(null!));
        Assert.Throws<ArgumentNullException>(() => provider.DeleteAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.WriteAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => { await foreach (var item in provider.ListAsync(null!)) { } });
        Assert.Throws<ArgumentNullException>(() => provider.WriteAllText("file", null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.WriteAllBytesAsync("file", null!));
        foreach (string path in new[] { "", " ", "a\r\nDELE b", "a//b", "a/./b", "a/../b", "ftp://host/a", "C:\\a" })
            Assert.Throws<ArgumentException>(() => provider.GetFullPath(path));
        foreach (string key in new[] { "/file", "a\\b", "a:b", "a//b", "a/./b", "a/../b", "a/", "a\n" })
            Assert.Throws<ArgumentException>(() => provider.OpenReadAsync(new(key)));
        using var content = new MemoryStream([1]);
        await Assert.ThrowsAsync<NotSupportedException>(() => provider.WriteAsync(new("file", content, overwrite: false)));
        Assert.Throws<ArgumentException>(() => provider.DeleteDirectory("/"));
        Assert.AreEqual(0, created);
    }

    [TestMethod]
    public void LegacyPathHelpersAreRemoteAndNetworkFree()
    {
        Assert.AreEqual("ftp://ftp.example.test/a/b%20c", provider.GetFullPath("a\\b c/"));
        Assert.AreEqual("/a", provider.GetDirectoryName("/a/b"));
        Assert.AreEqual("/", provider.GetDirectoryName("file"));
        Assert.IsNull(provider.GetDirectoryName("/"));
        Assert.AreEqual("file", provider.GetFileName("/a/file"));
        Assert.AreEqual("", provider.GetFileName("/"));
        Assert.AreEqual(0, created);
    }

    [TestMethod]
    public async Task ObjectMetadataMapsOnlyKnownFactsAndRelativeKeys()
    {
        using var scoped = new FtpStorageProvider(Options("/root/"), NullLogger<FtpStorageProvider>.Instance, () => client.Object);
        var utc = new DateTime(2026, 9, 9, 0, 0, 0, DateTimeKind.Utc);
        listing = [Item("/root/file", modified: utc)];
        StorageObjectMetadata? metadata = await scoped.GetMetadataAsync(new("file"));
        Assert.AreEqual(new StorageObjectMetadata("file", 5, new DateTimeOffset(utc)), metadata);
        client.Verify(value => value.GetListing("/root/", FtpListOption.UseStat, default), Times.Once);
        listing = [Item("/file", size: -1)];
        metadata = await provider.GetMetadataAsync(new("file"));
        Assert.IsNull(metadata!.Length);
        Assert.IsNull(metadata.LastModified);
        Assert.IsNull(metadata.ContentType);
        Assert.IsNull(metadata.Version);
        listing = [Item("/file", size: 0, modified: DateTime.SpecifyKind(utc, DateTimeKind.Local))];
        Assert.IsNull((await provider.GetMetadataAsync(new("file")))!.Length);
        listing = [Item("/other"), Item("/directory", FtpObjectType.Directory)];
        Assert.IsNull(await provider.GetMetadataAsync(new("missing")));
        Assert.IsFalse(provider.FileExists("missing"));
        listing = [Item("/file")];
        Assert.IsTrue(provider.FileExists("file"));
        listing = [Item("/server/share/file")];
        Assert.IsTrue(provider.FileExists(new FileInfo(OperatingSystem.IsWindows() ? @"\\server\share\file" : "/server/share/file")));
    }

    private void Download(byte[] bytes, bool success = true)
    {
        listing = [Item("/file")];
        client.Setup(value => value.DownloadStream(It.IsAny<Stream>(), "/file", 0, null, It.IsAny<CancellationToken>(), 0))
            .Returns(async (Stream stream, string path, long restart, IProgress<FtpProgress>? progress, CancellationToken token, long stop) =>
            { await stream.WriteAsync(bytes, token); return success; });
    }

    [TestMethod]
    public async Task ReadsPreserveBytesAndReturnCallerOwnedStream()
    {
        Download(Encoding.UTF8.GetBytes("hello"));
        using Stream stream = await provider.OpenReadAsync(new("file"));
        Assert.AreEqual(0L, stream.Position);
        Assert.AreEqual(FtpDataType.Binary, config.DownloadDataType);
        client.Verify(value => value.Dispose(), Times.Once);
        Assert.IsTrue(stream.CanRead);
        Assert.AreEqual("hello", provider.ReadAllText("file"));
        Assert.AreEqual("hello", await provider.ReadAllTextAsync("file"));
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("hello"), provider.ReadAllBytes("file"));
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("hello"), await provider.ReadAllBytesAsync("file"));
        listing = [];
        await Assert.ThrowsAsync<FileNotFoundException>(() => provider.OpenReadAsync(new("missing")));
    }

    [TestMethod]
    public async Task FailedDownloadClosesItsBufferAndClient()
    {
        listing = [Item("/file")];
        Stream? buffer = null;
        client.Setup(value => value.DownloadStream(It.IsAny<Stream>(), "/file", 0, null, default, 0))
            .Callback((Stream value, string path, long restart, IProgress<FtpProgress>? progress, CancellationToken token, long stop) => buffer = value).ReturnsAsync(false);
        await Assert.ThrowsAsync<IOException>(() => provider.OpenReadAsync(new("file")));
        Assert.IsFalse(buffer!.CanRead);
        client.Verify(value => value.Dispose(), Times.Once);
    }

    private void Upload(FtpStatus status = FtpStatus.Success, Action<Stream>? inspect = null)
    {
        client.Setup(value => value.UploadStream(It.IsAny<Stream>(), It.IsAny<string>(), FtpRemoteExists.Overwrite, true, null, It.IsAny<CancellationToken>()))
            .Callback((Stream stream, string path, FtpRemoteExists mode, bool create, IProgress<FtpProgress>? progress, CancellationToken token) => inspect?.Invoke(stream)).ReturnsAsync(status);
    }

    [TestMethod]
    public async Task WritesBufferRemainingContentAndLeaveInputsOpen()
    {
        Stream? sdkBuffer = null;
        Upload(inspect: stream => { sdkBuffer = stream; Assert.AreEqual(0L, stream.Position); CollectionAssert.AreEqual(new byte[] { 2, 3 }, ((MemoryStream)stream).ToArray()); });
        using var input = new MemoryStream([1, 2, 3]);
        input.Position = 1;
        Assert.AreEqual(new StorageObjectMetadata("file"), await provider.WriteAsync(new("file", input)));
        Assert.AreEqual(FtpDataType.Binary, config.UploadDataType);
        Assert.AreEqual(3L, input.Position);
        Assert.IsTrue(input.CanRead);
        Assert.IsFalse(sdkBuffer!.CanRead);
        using var nonseekable = new NonseekableStream([2, 3]);
        await provider.WriteAsync(new("file", nonseekable));
        Assert.IsTrue(nonseekable.CanRead);
        Upload();
        using var empty = new MemoryStream();
        await provider.WriteAsync(new("empty", empty));
        provider.WriteAllText("file", "text");
        await provider.WriteAllTextAsync("file", "text");
        provider.WriteAllBytes("file", [1]);
        await provider.WriteAllBytesAsync("file", [1]);
    }

    [TestMethod]
    [DataRow(FtpStatus.Failed)]
    [DataRow(FtpStatus.Skipped)]
    public async Task NonSuccessfulUploadIsAnErrorWithoutRetry(FtpStatus status)
    {
        Upload(status);
        using var input = new MemoryStream([1]);
        await Assert.ThrowsAsync<IOException>(() => provider.WriteAsync(new("file", input)));
        Assert.IsTrue(input.CanRead);
        Assert.AreEqual(1, created);
        client.Verify(value => value.UploadStream(It.IsAny<Stream>(), "file", FtpRemoteExists.Overwrite, true, null, default), Times.Never);
        client.Verify(value => value.Dispose(), Times.Once);
    }

    [TestMethod]
    public async Task DeleteMissingIsIdempotentAndExistingDeletesOnce()
    {
        await provider.DeleteAsync(new("missing"));
        listing = [Item("/file")];
        client.Setup(value => value.DeleteFile("/file", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        await provider.DeleteAsync(new("file"));
        provider.DeleteFile("file");
        await provider.DeleteFileAsync("file");
        client.Verify(value => value.DeleteFile("/file", It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [TestMethod]
    public async Task LegacyDirectoriesUseSdkAndNonrecursiveUsesRmd()
    {
        client.Setup(value => value.DirectoryExists("/dir", default)).ReturnsAsync(true);
        Assert.IsTrue(provider.DirectoryExists("dir"));
        client.Setup(value => value.CreateDirectory("/dir", true, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        Assert.AreEqual("dir", provider.CreateDirectory("dir").Name);
        Assert.AreEqual("dir", (await provider.CreateDirectoryAsync("dir")).Name);
        client.Setup(value => value.DeleteDirectory("/dir", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        provider.DeleteDirectory("dir");
        await provider.DeleteDirectoryAsync("dir");
        client.Setup(value => value.Execute("RMD /dir", It.IsAny<CancellationToken>())).ReturnsAsync(new FtpReply { Code = "250" });
        await provider.DeleteDirectoryAsync("dir", false);
        client.Verify(value => value.DeleteDirectory("/dir", It.IsAny<CancellationToken>()), Times.Exactly(2));
        client.Setup(value => value.Execute("RMD /dir", It.IsAny<CancellationToken>())).ReturnsAsync(new FtpReply { Code = "550", Message = "not empty" });
        await Assert.ThrowsAsync<IOException>(() => provider.DeleteDirectoryAsync("dir", false));
    }

    [TestMethod]
    public async Task LegacyListsFilterWildcardsAndTypes()
    {
        listing = [Item("/a.txt"), Item("/B.TXT"), Item("/c.bin"), Item("/folder", FtpObjectType.Directory), Item("/link", FtpObjectType.Link)];
        CollectionAssert.AreEqual(new[] { "/a.txt", "/B.TXT" }, provider.GetFiles("/", "?.txt"));
        Assert.AreEqual(3, provider.GetFiles("/").Length);
        Assert.AreEqual(3, provider.GetFiles("/", " ").Length);
        CollectionAssert.AreEqual(new[] { "/folder" }, provider.GetDirectories("/", "fold*"));
        var actual = new List<string>();
        await foreach (string path in provider.EnumerateFilesAsync("/", "*.txt")) actual.Add(path);
        CollectionAssert.AreEqual(new[] { "/a.txt", "/B.TXT" }, actual);
        listing = [];
        await foreach (string path in provider.EnumerateFilesAsync("/")) Assert.Fail();
    }

    [TestMethod]
    public async Task ObjectListTraversesDirectoriesAndMatchesOrdinalLiteralPrefix()
    {
        client.Setup(value => value.GetListing("/", FtpListOption.UseStat, It.IsAny<CancellationToken>())).ReturnsAsync([Item("/a"), Item("/A"), Item("/folder", FtpObjectType.Directory), Item("/link", FtpObjectType.Link)]);
        client.Setup(value => value.GetListing("/folder", FtpListOption.UseStat, It.IsAny<CancellationToken>())).ReturnsAsync([Item("/folder/child")]);
        var all = new List<string>();
        await foreach (var item in provider.ListAsync(new())) all.Add(item.Path);
        CollectionAssert.AreEqual(new[] { "a", "A", "folder/child" }, all);
        all.Clear();
        await foreach (var item in provider.ListAsync(new("folder/"))) all.Add(item.Path);
        CollectionAssert.AreEqual(new[] { "folder/child" }, all);
        all.Clear();
        await foreach (var item in provider.ListAsync(new("a"))) all.Add(item.Path);
        CollectionAssert.AreEqual(new[] { "a" }, all);
    }

    [TestMethod]
    [DataRow("/elsewhere")]
    [DataRow("/dir/a/b")]
    [DataRow("/dir/")]
    [DataRow("/dir/.")]
    [DataRow("/dir/..")]
    [DataRow("/dir/a\\b")]
    [DataRow("/dir/a:b")]
    [DataRow("/dir/a\nDELE x")]
    public async Task ListingRejectsInvalidChildren(string path)
    {
        listing = [Item(path)];
        await Assert.ThrowsAsync<IOException>(() => provider.GetMetadataAsync(new("dir/file")));
    }

    [TestMethod]
    [DataRow("550")]
    [DataRow("230")]
    public async Task AmbiguousOrStaleListingRepliesAreNeverMissing(string code)
    {
        reply = new FtpReply { Code = code, Message = "failure or stale greeting" };
        await Assert.ThrowsAsync<IOException>(() => provider.GetMetadataAsync(new("file")));
    }

    [TestMethod]
    public async Task DirectoryStatusReplyIsAccepted()
    {
        reply = new FtpReply { Code = "212" };
        Assert.IsNull(await provider.GetMetadataAsync(new("file")));
    }

    [TestMethod]
    [DataRow("530")]
    [DataRow("532")]
    public async Task AuthenticationReplyIsNotReportedAsMissing(string code)
    {
        reply = new FtpReply { Code = code };
        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => provider.GetMetadataAsync(new("file")));
        Assert.IsInstanceOfType<FtpCommandException>(error.InnerException);
    }

    [TestMethod]
    public async Task ConnectionFailuresAreTranslatedAndClientDisposedWithoutRetry()
    {
        Exception[] errors = [new FtpAuthenticationException("530", "denied"), new AuthenticationException("TLS"), new FtpException("ftp"), new SocketException(), new TimeoutException("timeout"), new IOException("io")];
        foreach (Exception error in errors)
        {
            client.Setup(value => value.Connect(default)).ThrowsAsync(error);
            Exception actual;
            if (error is FtpAuthenticationException or AuthenticationException)
                actual = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => provider.GetMetadataAsync(new("file")));
            else actual = await Assert.ThrowsAsync<IOException>(() => provider.GetMetadataAsync(new("file")));
            if (ReferenceEquals(error, actual)) Assert.IsInstanceOfType<IOException>(actual);
            else Assert.AreSame(error, actual.InnerException);
        }
        Assert.AreEqual(errors.Length, created);
        client.Verify(value => value.Dispose(), Times.Exactly(errors.Length));
        client.Verify(value => value.GetListing(It.IsAny<string>(), FtpListOption.UseStat, It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task NullFactoryResultIsAProgrammingError()
    {
        using var invalid = new FtpStorageProvider(Options(), NullLogger<FtpStorageProvider>.Instance, () => null!);
        await Assert.ThrowsAsync<InvalidOperationException>(() => invalid.GetMetadataAsync(new("file")));
    }

    [TestMethod]
    public async Task PreCancellationAndDisposedProviderNeverConnect()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        CancellationToken token = source.Token;
        using var input = new MemoryStream([1]);
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.OpenReadAsync(new("file"), token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.WriteAsync(new("file", input), token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.WriteAllBytesAsync("file", [1], token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.DeleteAsync(new("file"), token));
        await Assert.ThrowsAsync<OperationCanceledException>(async () => { await foreach (var item in provider.ListAsync(new(), token)) { } });
        Assert.AreEqual(0L, input.Position);
        provider.Dispose();
        Assert.Throws<ObjectDisposedException>(() => provider.GetFullPath("file"));
        Assert.Throws<ObjectDisposedException>(() => provider.OpenReadAsync(new("file")));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => { await foreach (var item in provider.ListAsync(new())) { } });
        Assert.AreEqual(0, created);
    }

    [TestMethod]
    public async Task CancellationDuringConnectDisposesClient()
    {
        using var source = new CancellationTokenSource();
        client.Setup(value => value.Connect(source.Token)).Callback(source.Cancel).Returns(Task.CompletedTask);
        var error = await Assert.ThrowsAsync<OperationCanceledException>(() => provider.GetMetadataAsync(new("file"), source.Token));
        Assert.AreEqual(source.Token, error.CancellationToken);
        client.Verify(value => value.Dispose(), Times.Once);
    }

    [TestMethod]
    public async Task CancellationDuringDownloadClosesBufferAndClient()
    {
        using var source = new CancellationTokenSource();
        listing = [Item("/file")];
        Stream? buffer = null;
        client.Setup(value => value.DownloadStream(It.IsAny<Stream>(), "/file", 0, null, source.Token, 0))
            .Callback((Stream stream, string path, long restart, IProgress<FtpProgress>? progress, CancellationToken token, long stop) => { buffer = stream; source.Cancel(); }).ReturnsAsync(true);
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.OpenReadAsync(new("file"), source.Token));
        Assert.IsFalse(buffer!.CanRead);
        client.Verify(value => value.Dispose(), Times.Once);
    }

    [TestMethod]
    public async Task CancellationAfterSdkOperationIsObserved()
    {
        using var source = new CancellationTokenSource();
        Upload(inspect: stream => source.Cancel());
        using var input = new MemoryStream([1]);
        var error = await Assert.ThrowsAsync<OperationCanceledException>(() => provider.WriteAsync(new("file", input), source.Token));
        Assert.AreEqual(source.Token, error.CancellationToken);
        Assert.IsTrue(input.CanRead);
        client.Verify(value => value.Dispose(), Times.Once);
    }

    [TestMethod]
    public async Task CancellationBetweenEnumerationItemsStopsWithoutMoreIo()
    {
        listing = [Item("/a"), Item("/b")];
        using var source = new CancellationTokenSource();
        await using var iterator = provider.ListAsync(new(), source.Token).GetAsyncEnumerator();
        Assert.IsTrue(await iterator.MoveNextAsync());
        source.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await iterator.MoveNextAsync());
        using var legacySource = new CancellationTokenSource();
        await using var legacy = provider.EnumerateFilesAsync("/", cancellationToken: legacySource.Token).GetAsyncEnumerator();
        Assert.IsTrue(await legacy.MoveNextAsync());
        legacySource.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await legacy.MoveNextAsync());
        Assert.AreEqual(2, created);
    }

    [TestMethod]
    public async Task CancellationAfterFinalItemAndLiteralPartialPrefixAreSupported()
    {
        listing = [Item("/.file")];
        using var source = new CancellationTokenSource();
        await using var iterator = provider.ListAsync(new("."), source.Token).GetAsyncEnumerator();
        Assert.IsTrue(await iterator.MoveNextAsync());
        Assert.AreEqual(".file", iterator.Current.Path);
        source.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await iterator.MoveNextAsync());
        using var legacySource = new CancellationTokenSource();
        await using var legacy = provider.EnumerateFilesAsync("/", cancellationToken: legacySource.Token).GetAsyncEnumerator();
        Assert.IsTrue(await legacy.MoveNextAsync());
        legacySource.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await legacy.MoveNextAsync());
    }

    [TestMethod]
    public async Task InputFailureNeverConnectsAndDoesNotDisposeCallerStream()
    {
        using var input = new FailingInput();
        await Assert.ThrowsAsync<IOException>(() => provider.WriteAsync(new("file", input)));
        Assert.IsTrue(input.CanRead);
        Assert.AreEqual(0, created);
    }

    [TestMethod]
    public async Task SdkCancellationAndOperationExceptionsPreserveOwnership()
    {
        using var source = new CancellationTokenSource();
        var cancellation = new OperationCanceledException(source.Token);
        listing = [Item("/file")];
        client.Setup(value => value.DownloadStream(It.IsAny<Stream>(), "/file", 0, null, source.Token, 0)).ThrowsAsync(cancellation);
        Assert.AreSame(cancellation, await Assert.ThrowsAsync<OperationCanceledException>(() => provider.OpenReadAsync(new("file"), source.Token)));
        using var input = new MemoryStream([1]);
        var failure = new FtpException("upload failed");
        client.Setup(value => value.UploadStream(It.IsAny<Stream>(), "/file", FtpRemoteExists.Overwrite, true, null, default)).ThrowsAsync(failure);
        var error = await Assert.ThrowsAsync<IOException>(() => provider.WriteAsync(new("file", input)));
        Assert.AreSame(failure, error.InnerException);
        Assert.IsTrue(input.CanRead);
        client.Verify(value => value.Dispose(), Times.Exactly(2));
    }

    [TestMethod]
    public async Task CancellationDuringInputCopyDoesNotConnect()
    {
        using var source = new CancellationTokenSource();
        using var input = new CancelingInput(source);
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.WriteAsync(new("file", input), source.Token));
        Assert.IsTrue(input.CanRead);
        Assert.AreEqual(0, created);
    }

    private sealed class FailingInput : MemoryStream
    {
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            => Task.FromException(new IOException("source failed"));
    }

    private sealed class CancelingInput(CancellationTokenSource source) : MemoryStream
    {
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            source.Cancel();
            return Task.CompletedTask;
        }
    }

    private sealed class NonseekableStream(byte[] bytes) : MemoryStream(bytes)
    {
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    }
}
