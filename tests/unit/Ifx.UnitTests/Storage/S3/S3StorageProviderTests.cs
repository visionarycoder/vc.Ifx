using Amazon.S3;
using Amazon.S3.Model;
using Ifx.Storage;
using Ifx.Storage.S3;
using Moq;
using System.Net;

namespace Ifx.Tests.Storage.S3;

[TestClass]
public sealed class S3StorageProviderTests
{
    private Mock<IAmazonS3> client = null!;
    private S3StorageProvider provider = null!;

    [TestInitialize]
    public void Initialize()
    {
        client = new Mock<IAmazonS3>(MockBehavior.Strict);
        provider = new S3StorageProvider(client.Object, Options());
    }

    [TestMethod]
    public async Task WriteAsync_UsesBucketPrefixAndReturnsProviderRelativeMetadata()
    {
        client.Setup(value => value.PutObjectAsync(
                It.Is<PutObjectRequest>(request =>
                    request.BucketName == "orders" && request.Key == "tenant-a/invoice.json" && request.InputStream != null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse { ETag = "etag-1" });
        using var content = new MemoryStream([1, 2, 3]);

        StorageObjectMetadata result = await provider.WriteAsync(new("invoice.json", content));

        Assert.AreEqual("invoice.json", result.Path);
        Assert.AreEqual("etag-1", result.Version);
        Assert.IsTrue(content.CanRead);
    }

    [TestMethod]
    public async Task WriteAsync_RejectsCreateOnlyPolicyBeforeCallingS3()
    {
        using var content = new MemoryStream([1]);

        await Assert.ThrowsExactlyAsync<NotSupportedException>(() =>
            provider.WriteAsync(new("invoice.json", content, overwrite: false)));

        client.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task GetMetadataAsync_MapsAbsentObjectToNull()
    {
        client.Setup(value => value.GetObjectMetadataAsync(
                It.Is<GetObjectMetadataRequest>(request => request.BucketName == "orders" && request.Key == "tenant-a/missing"),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("missing") { StatusCode = HttpStatusCode.NotFound });

        Assert.IsNull(await provider.GetMetadataAsync(new("missing")));
    }

    [TestMethod]
    public async Task GetMetadataAsync_PropagatesMissingBucketFailure()
    {
        client.Setup(value => value.GetObjectMetadataAsync(
                It.IsAny<GetObjectMetadataRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("missing bucket")
            {
                ErrorCode = "NoSuchBucket",
                StatusCode = HttpStatusCode.NotFound
            });

        await Assert.ThrowsExactlyAsync<IOException>(() => provider.GetMetadataAsync(new("missing")));
    }

    [TestMethod]
    public async Task OpenReadAsync_ReturnsCallerOwnedStreamAndDisposesSdkResponse()
    {
        var content = new MemoryStream([4, 5]);
        var response = new GetObjectResponse { ResponseStream = content };
        client.Setup(value => value.GetObjectAsync(
                It.Is<GetObjectRequest>(request => request.BucketName == "orders" && request.Key == "tenant-a/file"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        Stream result = await provider.OpenReadAsync(new("file"));
        Assert.AreEqual(4, result.ReadByte());
        await result.DisposeAsync();
        Assert.IsFalse(content.CanRead);
    }

    [TestMethod]
    public async Task ListAsync_PaginatesAndRemovesConfiguredPrefix()
    {
        client.SetupSequence(value => value.ListObjectsV2Async(
                It.IsAny<ListObjectsV2Request>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListObjectsV2Response
            {
                IsTruncated = true,
                NextContinuationToken = "next",
                S3Objects = [new S3Object { Key = "tenant-a/one", Size = 1 }]
            })
            .ReturnsAsync(new ListObjectsV2Response
            {
                IsTruncated = false,
                S3Objects = [new S3Object { Key = "tenant-a/two", Size = 2 }]
            });

        var results = new List<StorageObjectMetadata>();
        await foreach (StorageObjectMetadata item in provider.ListAsync(new()))
        {
            results.Add(item);
        }

        CollectionAssert.AreEqual(new[] { "one", "two" }, results.Select(item => item.Path).ToArray());
        client.Verify(value => value.ListObjectsV2Async(
            It.Is<ListObjectsV2Request>(request => request.ContinuationToken == "next"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task InvalidPathsAndCanceledCallsDoNotReachS3()
    {
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => provider.DeleteAsync(new("../escape")));
        using var canceled = new CancellationTokenSource();
        await canceled.CancelAsync();
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() =>
            provider.DeleteAsync(new("file"), canceled.Token));
        client.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void OptionsAndCapabilitiesAreExplicit()
    {
        Assert.AreEqual(
            StorageCapabilities.Read | StorageCapabilities.Write | StorageCapabilities.Delete |
            StorageCapabilities.Metadata | StorageCapabilities.List,
            provider.Capabilities);
        Assert.ThrowsExactly<ArgumentException>(() => new S3StorageOptions { BucketName = "bucket", KeyPrefix = "../private" }.Validate());
        Assert.ThrowsExactly<ArgumentException>(() => new S3StorageOptions { BucketName = "bucket", KeyPrefix = "/root" }.Validate());
    }

    private static S3StorageOptions Options() => new() { BucketName = "orders", KeyPrefix = "tenant-a" };
}
