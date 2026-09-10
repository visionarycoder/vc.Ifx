using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Text;
using VisionaryCoder.Framework.Storage;
using VisionaryCoder.Framework.Storage.Azure.Blob;

namespace VisionaryCoder.Framework.Tests.Storage.Azure.Blobs;

[TestClass]
public sealed class AzureBlobStorageProviderTests
{
    private Mock<BlobContainerClient> container = null!;
    private Mock<BlobClient> blob = null!;
    private AzureBlobStorageProvider provider = null!;
    private static readonly DateTimeOffset modified = new(2026, 9, 9, 0, 0, 0, TimeSpan.Zero);
    private static readonly ETag tag = new("\"opaque-tag\"");

    [TestInitialize]
    public void Initialize()
    {
        container = new Mock<BlobContainerClient>(MockBehavior.Strict);
        blob = new Mock<BlobClient>(MockBehavior.Strict);
        container.SetupGet(value => value.Name).Returns("container");
        container.Setup(value => value.GetBlobClient(It.IsAny<string>())).Returns(blob.Object);
        container.Setup(value => value.CreateIfNotExistsAsync(PublicAccessType.None, null, null, It.IsAny<CancellationToken>())).ReturnsAsync(Response.FromValue(BlobsModelFactory.BlobContainerInfo(tag, modified), Mock.Of<Response>()));
        blob.Setup(value => value.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(UploadResponse());
        blob.Setup(value => value.DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, It.IsAny<CancellationToken>())).ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));
        provider = CreateProvider();
    }

    private AzureBlobStorageProvider CreateProvider(bool create = true) => new(new AzureBlobStorageOptions { ContainerName = "container", CreateContainerIfNotExists = create }, NullLogger<AzureBlobStorageProvider>.Instance, container.Object);
    private static Response<BlobContentInfo> UploadResponse() => Response.FromValue(BlobsModelFactory.BlobContentInfo(tag, modified, null, null, null, null, 0), Mock.Of<Response>());
    private static RequestFailedException Failure(int status, string? code) => new(status, "service failure", code, null);
    private static AzureBlobStorageOptions ConnectionOptions() => new() { ContainerName = "container", ConnectionString = "UseDevelopmentStorage=true" };
    private static BlobItem Item(string name, bool known = true) => BlobsModelFactory.BlobItem(name: name,
        properties: BlobsModelFactory.BlobItemProperties(false, contentType: known ? "application/json" : null, contentLength: known ? 0 : null, lastModified: known ? modified : null, eTag: known ? tag : null));
    private void Listings(params BlobItem[] items)
    {
        AsyncPageable<BlobItem> pages = AsyncPageable<BlobItem>.FromPages([Page<BlobItem>.FromValues(items, null, Mock.Of<Response>())]);
        container.Setup(value => value.GetBlobsAsync(BlobTraits.None, BlobStates.None, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(pages);
    }
    private void Properties() => blob.Setup(value => value.GetPropertiesAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(Response.FromValue(
        BlobsModelFactory.BlobProperties(contentLength: 0, contentType: "application/json", lastModified: modified, eTag: tag), Mock.Of<Response>()));
    private void Download(Func<Stream> content) => blob.Setup(value => value.DownloadStreamingAsync(null, It.IsAny<CancellationToken>())).Returns(() => Task.FromResult(Response.FromValue(
        BlobsModelFactory.BlobDownloadStreamingResult(content(), BlobsModelFactory.BlobDownloadDetails()), Mock.Of<Response>())));

    [TestMethod]
    public void ConstructorsAndDefaultSdkOptionsAreNetworkFree()
    {
        using var actual = new AzureBlobStorageProvider(ConnectionOptions(), NullLogger<AzureBlobStorageProvider>.Instance);
        Assert.AreEqual("http://127.0.0.1:10000/devstoreaccount1/container/file", actual.GetFullPath("file"));
        using var identity = new AzureBlobStorageProvider(new AzureBlobStorageOptions { ContainerName = "container", UseManagedIdentity = true, StorageAccountUri = "https://account.blob.core.windows.net" }, NullLogger<AzureBlobStorageProvider>.Instance);
        Assert.AreEqual("https://account.blob.core.windows.net/container/file", identity.GetFullPath("file"));
        Assert.IsInstanceOfType<IObjectStorageProvider>(actual);
        Assert.IsInstanceOfType<IStorageProvider>(actual);
        Assert.AreEqual(StorageCapabilities.Read | StorageCapabilities.Write | StorageCapabilities.Delete | StorageCapabilities.Metadata | StorageCapabilities.List | StorageCapabilities.CreateOnly, actual.Capabilities);
        BlobClientOptions options = AzureBlobStorageProvider.CreateClientOptions(ConnectionOptions());
        Assert.AreEqual(RetryMode.Exponential, options.Retry.Mode);
        Assert.AreEqual(3, options.Retry.MaxRetries);
        Assert.AreEqual(TimeSpan.FromMilliseconds(800), options.Retry.Delay);
        Assert.AreEqual(TimeSpan.FromSeconds(8), options.Retry.MaxDelay);
        Assert.AreEqual(TimeSpan.FromSeconds(30), options.Retry.NetworkTimeout);
        var customized = new AzureBlobStorageOptions { ContainerName = "container", MaxRetries = 0, TimeoutMilliseconds = 123, BufferSize = 1, RetryDelayMilliseconds = 0, MaxRetryDelayMilliseconds = 0 };
        customized.ValidateBehavior();
        options = AzureBlobStorageProvider.CreateClientOptions(customized);
        Assert.AreEqual(0, options.Retry.MaxRetries);
        Assert.AreEqual(TimeSpan.FromMilliseconds(123), options.Retry.NetworkTimeout);
    }

    [TestMethod]
    public void ConstructorGuardsAndInjectedClientDi()
    {
        Assert.Throws<ArgumentNullException>(() => new AzureBlobStorageProvider(null!, NullLogger<AzureBlobStorageProvider>.Instance));
        Assert.Throws<ArgumentNullException>(() => new AzureBlobStorageProvider(null!, NullLogger<AzureBlobStorageProvider>.Instance, container.Object));
        Assert.Throws<ArgumentNullException>(() => new AzureBlobStorageProvider(ConnectionOptions(), null!, container.Object));
        Assert.Throws<ArgumentNullException>(() => new AzureBlobStorageProvider(ConnectionOptions(), NullLogger<AzureBlobStorageProvider>.Instance, null!));
        Assert.Throws<ArgumentException>(() => new AzureBlobStorageProvider(new AzureBlobStorageOptions { ContainerName = "other" }, NullLogger<AzureBlobStorageProvider>.Instance, container.Object));
        var services = new ServiceCollection();
        services.AddSingleton(new AzureBlobStorageOptions { ContainerName = "container" });
        services.AddSingleton(container.Object);
        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<AzureBlobStorageProvider>>(NullLogger<AzureBlobStorageProvider>.Instance);
        services.AddTransient<IStorageProvider, AzureBlobStorageProvider>();
        using var scope = services.BuildServiceProvider();
        Assert.IsInstanceOfType<AzureBlobStorageProvider>(scope.GetRequiredService<IStorageProvider>());
        container.Verify(value => value.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(), null, null, It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public void OptionsRejectInvalidNamesCredentialsAndLimits()
    {
        foreach (string name in new[] { "", " ", "ab", new string('a', 64), "-abc", "abc-", "a--b", "ABC", "caf\u00e9", "abc_", "ab\u0661" })
            Assert.Throws<ArgumentException>(() => new AzureBlobStorageOptions { ContainerName = name }.ValidateBehavior());
        foreach (string uri in new[] { "", "relative", "http://account/", "https://user:pass@account/", "https://account/?sas=secret", "https://account/#part", "https://account/container" })
            Assert.Throws<ArgumentException>(() => new AzureBlobStorageOptions { ContainerName = "container", UseManagedIdentity = true, StorageAccountUri = uri }.Validate());
        Assert.Throws<ArgumentException>(() => new AzureBlobStorageOptions { ContainerName = "container" }.Validate());
        Assert.Throws<FormatException>(() => new AzureBlobStorageOptions { ContainerName = "container", ConnectionString = "not-a-connection-string" }.Validate());
        Assert.Throws<ArgumentOutOfRangeException>(() => new AzureBlobStorageOptions { ContainerName = "container", TimeoutMilliseconds = 0 }.ValidateBehavior());
        Assert.Throws<ArgumentOutOfRangeException>(() => new AzureBlobStorageOptions { ContainerName = "container", BufferSize = 0 }.ValidateBehavior());
        Assert.Throws<ArgumentOutOfRangeException>(() => new AzureBlobStorageOptions { ContainerName = "container", MaxRetries = -1 }.ValidateBehavior());
        Assert.Throws<ArgumentOutOfRangeException>(() => new AzureBlobStorageOptions { ContainerName = "container", RetryDelayMilliseconds = -1 }.ValidateBehavior());
        Assert.Throws<ArgumentOutOfRangeException>(() => new AzureBlobStorageOptions { ContainerName = "container", RetryDelayMilliseconds = 5, MaxRetryDelayMilliseconds = 4 }.ValidateBehavior());
        Assert.Throws<ArgumentOutOfRangeException>(() => new AzureBlobStorageOptions { ContainerName = "container", ContainerPublicAccess = (PublicAccessType)999 }.ValidateBehavior());
        Assert.Throws<ArgumentException>(() => new AzureBlobStorageOptions { ContainerName = "container", DefaultAccessTier = new AccessTier("imaginary") }.ValidateBehavior());
        foreach (AccessTier tier in new[] { AccessTier.Hot, AccessTier.Cool, AccessTier.Cold, AccessTier.Archive })
            new AzureBlobStorageOptions { ContainerName = "ab-09", DefaultAccessTier = tier }.ValidateBehavior();
    }

    [TestMethod]
    public async Task NullAndInvalidRequestsDoNotAccessSdk()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.OpenReadAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.WriteAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.GetMetadataAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.DeleteAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => { await foreach (var item in provider.ListAsync(null!)) { } });
        Assert.Throws<ArgumentNullException>(() => provider.FileExists((FileInfo)null!));
        Assert.Throws<ArgumentNullException>(() => provider.WriteAllText("file", null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.WriteAllBytesAsync("file", null!));
        foreach (string path in new[] { "/file", "a\\b", "a\r\n", "a/../b", "a/./b", new string('a', 1025) })
            await Assert.ThrowsAsync<ArgumentException>(() => provider.OpenReadAsync(new(path)));
        Assert.Throws<ArgumentException>(() => provider.GetFullPath("/"));
        Assert.Throws<ArgumentException>(() => provider.GetFullPath(" "));
        Assert.Throws<ArgumentException>(() => provider.GetFiles("dir", ""));
        await Assert.ThrowsAsync<ArgumentException>(async () => { await foreach (var item in provider.EnumerateFilesAsync("dir", "")) { } });
        container.Verify(value => value.GetBlobClient(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public void LegacyPathsNormalizeAndRemoveSasFromOutput()
    {
        blob.SetupGet(value => value.Uri).Returns(new Uri("https://account/container/a/b%20c?sig=secret"));
        Assert.AreEqual("https://account/container/a/b%20c", provider.GetFullPath("/a\\//b c"));
        container.Verify(value => value.GetBlobClient("a/b c"), Times.Once);
        Assert.AreEqual("a", provider.GetDirectoryName("a/b"));
        Assert.IsNull(provider.GetDirectoryName("file"));
        Assert.AreEqual("b", provider.GetFileName("a/b"));
    }

    [TestMethod]
    public async Task MetadataPreservesStoredFactsIncludingZeroLengthAndQuotedEtag()
    {
        Properties();
        Assert.AreEqual(new StorageObjectMetadata("folder//File", 0, modified, "application/json", tag.ToString()), await provider.GetMetadataAsync(new("folder//File")));
        container.Verify(value => value.GetBlobClient("folder//File"), Times.Once);
        Assert.IsTrue(provider.FileExists("file"));
        Assert.IsTrue(provider.FileExists(new FileInfo("file")));
        container.Verify(value => value.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(), null, null, It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    [DataRow("BlobNotFound")]
    [DataRow("ContainerNotFound")]
    public async Task MissingBlobOrContainerHasOperationSpecificResults(string code)
    {
        var error = Failure(404, code);
        blob.Setup(value => value.GetPropertiesAsync(null, It.IsAny<CancellationToken>())).ThrowsAsync(error);
        blob.Setup(value => value.DownloadStreamingAsync(null, It.IsAny<CancellationToken>())).ThrowsAsync(error);
        blob.Setup(value => value.DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, It.IsAny<CancellationToken>())).ThrowsAsync(error);
        Assert.IsNull(await provider.GetMetadataAsync(new("file")));
        Assert.IsFalse(provider.FileExists("file"));
        Assert.AreSame(error, (await Assert.ThrowsAsync<FileNotFoundException>(() => provider.OpenReadAsync(new("file")))).InnerException);
        await provider.DeleteAsync(new("file"));
    }

    [TestMethod]
    [DataRow(404, "UnknownResource")]
    [DataRow(500, "BlobNotFound")]
    [DataRow(409, "BlobAlreadyExists")]
    [DataRow(412, "ConditionNotMet")]
    public async Task OtherServiceFailuresAreNotAbsenceAndAreNotRetried(int status, string code)
    {
        var failure = Failure(status, code);
        blob.Setup(value => value.GetPropertiesAsync(null, default)).ThrowsAsync(failure);
        var result = await Assert.ThrowsAsync<IOException>(() => provider.GetMetadataAsync(new("file")));
        Assert.AreSame(failure, result.InnerException);
        blob.Verify(value => value.GetPropertiesAsync(null, default), Times.Once);
    }

    [TestMethod]
    [DataRow(401)]
    [DataRow(403)]
    public async Task AccessFailuresAreUnauthorized(int status)
    {
        var failure = Failure(status, "AuthorizationFailure");
        blob.Setup(value => value.GetPropertiesAsync(null, default)).ThrowsAsync(failure);
        Assert.AreSame(failure, (await Assert.ThrowsAsync<UnauthorizedAccessException>(() => provider.GetMetadataAsync(new("file")))).InnerException);
    }

    [TestMethod]
    public async Task CredentialAndAggregatedTransferErrorsPreserveCauses()
    {
        var credential = new AuthenticationFailedException("credential unavailable");
        blob.Setup(value => value.GetPropertiesAsync(null, default)).ThrowsAsync(credential);
        Assert.AreSame(credential, (await Assert.ThrowsAsync<UnauthorizedAccessException>(() => provider.GetMetadataAsync(new("file")))).InnerException);
        var aggregate = new AggregateException(Failure(500, "failed"));
        blob.Setup(value => value.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), default)).ThrowsAsync(aggregate);
        using var input = new MemoryStream([1]);
        Assert.AreSame(aggregate, (await Assert.ThrowsAsync<IOException>(() => provider.WriteAsync(new("file", input)))).InnerException);
        Assert.IsTrue(input.CanRead);
    }

    [TestMethod]
    public async Task StreamingReadTransfersOwnershipWithoutBuffering()
    {
        using var content = new MemoryStream([1, 2]);
        Download(() => content);
        Assert.AreSame(content, await provider.OpenReadAsync(new("file")));
        provider.Dispose();
        Assert.IsTrue(content.CanRead);
        container.Verify(value => value.GetBlobClient("file"), Times.Once);
    }

    [TestMethod]
    public async Task LegacyReadsBufferUtf8AndCloseDownloadStream()
    {
        Stream? latest = null;
        Download(() => latest = new MemoryStream(Encoding.UTF8.GetBytes("hello")));
        Assert.AreEqual("hello", provider.ReadAllText("file"));
        Assert.IsFalse(latest!.CanRead);
        Assert.AreEqual("hello", await provider.ReadAllTextAsync("file"));
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("hello"), provider.ReadAllBytes("file"));
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("hello"), await provider.ReadAllBytesAsync("file"));
    }

    [TestMethod]
    public async Task UploadReceivesOriginalCurrentPositionAndTrueConditionalCreate()
    {
        using var input = new MemoryStream([1, 2, 3]);
        input.Position = 1;
        BlobUploadOptions? captured = null;
        blob.Setup(value => value.UploadAsync(input, It.IsAny<BlobUploadOptions>(), default)).Callback((Stream stream, BlobUploadOptions upload, CancellationToken token) =>
        { Assert.AreSame(input, stream); Assert.AreEqual(1L, stream.Position); captured = upload; }).ReturnsAsync(UploadResponse());
        Assert.AreEqual(new StorageObjectMetadata("file", lastModified: modified, version: tag.ToString()), await provider.WriteAsync(new("file", input, false)));
        Assert.AreEqual(ETag.All, captured!.Conditions.IfNoneMatch);
        Assert.AreEqual(AccessTier.Hot, captured.AccessTier);
        Assert.AreEqual(4 * 1024 * 1024, captured.TransferOptions.InitialTransferSize);
        Assert.AreEqual(4 * 1024 * 1024, captured.TransferOptions.MaximumTransferSize);
        Assert.AreEqual(1, captured.TransferOptions.MaximumConcurrency);
        Assert.IsTrue(input.CanRead);
        await provider.WriteAsync(new("file", input));
        Assert.IsNull(captured.Conditions);
        blob.Verify(value => value.ExistsAsync(It.IsAny<CancellationToken>()), Times.Never);
        container.Verify(value => value.CreateIfNotExistsAsync(PublicAccessType.None, null, null, default), Times.Exactly(2));
    }

    [TestMethod]
    public async Task LegacyWritesLeaveSdkInputDisposalToProviderAndOptionalCreateCanBeDisabled()
    {
        using var withoutCreate = CreateProvider(false);
        Stream? captured = null;
        blob.Setup(value => value.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), default)).Callback((Stream stream, BlobUploadOptions upload, CancellationToken token) => captured = stream).ReturnsAsync(UploadResponse());
        withoutCreate.WriteAllText("file", "text");
        Assert.IsFalse(captured!.CanRead);
        await withoutCreate.WriteAllTextAsync("file", "text");
        withoutCreate.WriteAllBytes("file", [1]);
        await withoutCreate.WriteAllBytesAsync("file", []);
        container.Verify(value => value.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(), null, null, It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task UploadFailureAndCancellationNeverDisposeInput()
    {
        using var input = new MemoryStream([1]);
        blob.Setup(value => value.UploadAsync(input, It.IsAny<BlobUploadOptions>(), default)).ThrowsAsync(Failure(412, "ConditionNotMet"));
        await Assert.ThrowsAsync<IOException>(() => provider.WriteAsync(new("file", input, false)));
        Assert.IsTrue(input.CanRead);
        var canceled = new OperationCanceledException();
        blob.Setup(value => value.UploadAsync(input, It.IsAny<BlobUploadOptions>(), default)).ThrowsAsync(canceled);
        Assert.AreSame(canceled, await Assert.ThrowsAsync<OperationCanceledException>(() => provider.WriteAsync(new("file", input))));
        Assert.IsTrue(input.CanRead);
    }

    [TestMethod]
    public async Task DeleteIsIdempotentWithoutSilentlyRemovingSnapshots()
    {
        provider.DeleteFile("file");
        await provider.DeleteFileAsync("file");
        blob.Setup(value => value.DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, default)).ReturnsAsync(Response.FromValue(false, Mock.Of<Response>()));
        await provider.DeleteAsync(new("missing"));
        blob.Verify(value => value.DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, default), Times.Exactly(3));
    }

    [TestMethod]
    public async Task PagedListIncludesRealMarkerBlobsAndPreservesNullableFacts()
    {
        var pages = AsyncPageable<BlobItem>.FromPages([
            Page<BlobItem>.FromValues([Item("a/file")], "next", Mock.Of<Response>()),
            Page<BlobItem>.FromValues([Item("a/.directory", false), Item("A/other")], null, Mock.Of<Response>())]);
        container.Setup(value => value.GetBlobsAsync(BlobTraits.None, BlobStates.None, "a/", It.IsAny<CancellationToken>())).Returns(pages);
        var results = new List<StorageObjectMetadata>();
        await foreach (var item in provider.ListAsync(new("a/"))) results.Add(item);
        Assert.AreEqual(2, results.Count);
        Assert.AreEqual(new StorageObjectMetadata("a/file", 0, modified, "application/json", tag.ToString()), results[0]);
        Assert.AreEqual(new StorageObjectMetadata("a/.directory"), results[1]);
    }

    [TestMethod]
    public async Task EmptyAndMissingContainerListingsAreEmptyButFailuresPropagate()
    {
        Listings();
        await foreach (var item in provider.ListAsync(new())) Assert.Fail();
        container.Setup(value => value.GetBlobsAsync(BlobTraits.None, BlobStates.None, "", It.IsAny<CancellationToken>())).Returns(new ThrowingPageable(Failure(404, "ContainerNotFound")));
        await foreach (var item in provider.ListAsync(new())) Assert.Fail();
        container.Setup(value => value.GetBlobsAsync(BlobTraits.None, BlobStates.None, "", It.IsAny<CancellationToken>())).Returns(new ThrowingPageable(Failure(403, "AuthorizationFailure")));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => { await foreach (var item in provider.ListAsync(new())) { } });
    }

    [TestMethod]
    public async Task LegacyVirtualDirectoriesRetainMarkerBehavior()
    {
        Listings();
        Assert.IsFalse(provider.DirectoryExists("dir"));
        Listings(Item("dir/.directory"));
        Assert.IsTrue(provider.DirectoryExists("dir"));
        Assert.AreEqual("dir", provider.CreateDirectory("dir").Name);
        Assert.AreEqual("dir", (await provider.CreateDirectoryAsync("dir")).Name);
        container.Verify(value => value.GetBlobClient("dir/.directory"), Times.Exactly(2));
        await provider.DeleteDirectoryAsync("dir", false);
        provider.DeleteDirectory("dir");
        Listings(Item("dir/single-file"));
        await Assert.ThrowsAsync<IOException>(() => provider.DeleteDirectoryAsync("dir", false));
        Listings(Item("dir/sub/.directory"));
        await Assert.ThrowsAsync<IOException>(() => provider.DeleteDirectoryAsync("dir", false));
        Listings();
        await provider.DeleteDirectoryAsync("dir", false);
        Listings(Item("dir/file"), Item("dir/sub/file"));
        await provider.DeleteDirectoryAsync("dir", true);
    }

    [TestMethod]
    public async Task LegacyFilesAndImmediateDirectoriesHandleLiteralRegexCharacters()
    {
        Listings(Item("dir/a.txt"), Item("dir/b.bin"), Item("dir/file[1].txt"), Item("dir/.directory"), Item("dir/sub/.directory"), Item("dir/sub/nested.txt"), Item("dir/sub/other"), Item("dir/skip/file"));
        CollectionAssert.AreEqual(new[] { "dir/file[1].txt" }, provider.GetFiles("dir", "file[1].txt"));
        Assert.AreEqual(6, provider.GetFiles("dir").Length);
        CollectionAssert.AreEqual(new[] { "dir/sub" }, provider.GetDirectories("dir", "su?"));
        Assert.AreEqual(2, provider.GetDirectories("dir").Length);
        var actual = new List<string>();
        await foreach (string name in provider.EnumerateFilesAsync("dir", "*.txt")) actual.Add(name);
        CollectionAssert.AreEqual(new[] { "dir/a.txt", "dir/file[1].txt", "dir/sub/nested.txt" }, actual);
    }

    [TestMethod]
    public async Task PreCancellationAndDisposalPreventIo()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        using var input = new MemoryStream([1]);
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.OpenReadAsync(new("file"), source.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.WriteAsync(new("file", input), source.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.GetMetadataAsync(new("file"), source.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.DeleteAsync(new("file"), source.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(async () => { await foreach (var item in provider.ListAsync(new(), source.Token)) { } });
        Assert.AreEqual(0L, input.Position);
        provider.Dispose();
        Assert.Throws<ObjectDisposedException>(() => provider.GetFullPath("file"));
        await Assert.ThrowsAsync<ObjectDisposedException>(() => provider.GetMetadataAsync(new("file")));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => { await foreach (var item in provider.ListAsync(new())) { } });
        container.Verify(value => value.GetBlobClient(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task CancellationAfterDownloadClosesUnclaimedStream()
    {
        using var source = new CancellationTokenSource();
        var content = new MemoryStream([1]);
        Download(() => { source.Cancel(); return content; });
        var error = await Assert.ThrowsAsync<OperationCanceledException>(() => provider.OpenReadAsync(new("file"), source.Token));
        Assert.AreEqual(source.Token, error.CancellationToken);
        Assert.IsFalse(content.CanRead);
    }

    [TestMethod]
    public async Task CancellationAfterContainerCreationDoesNotUpload()
    {
        using var source = new CancellationTokenSource();
        container.Setup(value => value.CreateIfNotExistsAsync(PublicAccessType.None, null, null, source.Token)).Callback(source.Cancel).ReturnsAsync(Response.FromValue(BlobsModelFactory.BlobContainerInfo(tag, modified), Mock.Of<Response>()));
        using var input = new MemoryStream([1]);
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.WriteAsync(new("file", input), source.Token));
        blob.Verify(value => value.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.AreEqual(0L, input.Position);
    }

    [TestMethod]
    public async Task CancellationBetweenAndAfterFinalListingItemsDisposesEnumerator()
    {
        using var source = new CancellationTokenSource();
        var pageable = new TrackingPageable([Item("a"), Item("b")]);
        container.Setup(value => value.GetBlobsAsync(BlobTraits.None, BlobStates.None, "", source.Token)).Returns(pageable);
        await using (var iterator = provider.ListAsync(new(), source.Token).GetAsyncEnumerator())
        {
            Assert.IsTrue(await iterator.MoveNextAsync());
            source.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await iterator.MoveNextAsync());
        }
        Assert.IsTrue(pageable.Disposed);
        using var finalSource = new CancellationTokenSource();
        Listings(Item("dir/file"));
        await using var legacy = provider.EnumerateFilesAsync("dir", cancellationToken: finalSource.Token).GetAsyncEnumerator();
        Assert.IsTrue(await legacy.MoveNextAsync());
        finalSource.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await legacy.MoveNextAsync());
    }

    private sealed class ThrowingPageable(Exception failure) : AsyncPageable<BlobItem>
    {
        public override IAsyncEnumerable<Page<BlobItem>> AsPages(string? continuationToken = null, int? pageSizeHint = null) => throw new NotSupportedException();
        public override IAsyncEnumerator<BlobItem> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new FailingEnumerator(failure);
        private sealed class FailingEnumerator(Exception failure) : IAsyncEnumerator<BlobItem>
        {
            public BlobItem Current => throw new InvalidOperationException();
            public ValueTask<bool> MoveNextAsync() => ValueTask.FromException<bool>(failure);
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }
    }
    private sealed class TrackingPageable(BlobItem[] items) : AsyncPageable<BlobItem>
    {
        public bool Disposed { get; private set; }
        public override IAsyncEnumerable<Page<BlobItem>> AsPages(string? continuationToken = null, int? pageSizeHint = null) => throw new NotSupportedException();
        public override IAsyncEnumerator<BlobItem> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new TrackingEnumerator(this, items);
        private sealed class TrackingEnumerator(TrackingPageable owner, BlobItem[] items) : IAsyncEnumerator<BlobItem>
        {
            private int index = -1;
            public BlobItem Current => items[index];
            public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(++index < items.Length);
            public ValueTask DisposeAsync() { owner.Disposed = true; return ValueTask.CompletedTask; }
        }
    }
}
