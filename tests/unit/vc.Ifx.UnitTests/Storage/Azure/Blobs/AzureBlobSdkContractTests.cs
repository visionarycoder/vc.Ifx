using Azure.Core.Pipeline;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using System.Net;
using VisionaryCoder.Framework.Storage;
using VisionaryCoder.Framework.Storage.Azure.Blob;

namespace VisionaryCoder.Framework.Tests.Storage.Azure.Blobs;

[TestClass]
public sealed class AzureBlobSdkContractTests
{
    [TestMethod]
    public async Task RealSdkUsesConditionalHeaderWithOneConcurrentCreateWinner()
    {
        int exists = 0;
        int requests = 0;
        using var handler = new CallbackHandler(async (request, token) =>
        {
            Interlocked.Increment(ref requests);
            Assert.AreEqual("*", request.Headers.IfNoneMatch.Single().Tag);
            Assert.AreEqual(HttpMethod.Put, request.Method);
            await request.Content!.ReadAsByteArrayAsync(token);
            return Interlocked.CompareExchange(ref exists, 1, 0) == 0 ? Success() : Conflict();
        });
        using var http = new HttpClient(handler);
        using AzureBlobStorageProvider provider = Create(http);
        Task<bool>[] writes = Enumerable.Range(0, 8).Select(async number =>
        {
            using var stream = new MemoryStream([(byte)number]);
            try { await provider.WriteAsync(new("same-key", stream, overwrite: false)); return true; }
            catch (IOException exception) { Assert.IsInstanceOfType<global::Azure.RequestFailedException>(exception.InnerException); return false; }
            finally { Assert.IsTrue(stream.CanRead); }
        }).ToArray();
        bool[] results = await Task.WhenAll(writes);
        Assert.AreEqual(1, results.Count(value => value));
        Assert.AreEqual(8, requests);
    }

    [TestMethod]
    public async Task RealSdkReadsRemainingSeekableAndNonseekableStreamsWithoutDisposingThem()
    {
        var payloads = new ConcurrentQueue<byte[]>();
        using var handler = new CallbackHandler(async (request, token) =>
        {
            byte[] bytes = await request.Content!.ReadAsByteArrayAsync(token);
            if (!request.RequestUri!.Query.Contains("comp=blocklist", StringComparison.Ordinal)) payloads.Enqueue(bytes);
            Assert.IsFalse(request.Headers.IfNoneMatch.Any());
            return Success();
        });
        using var http = new HttpClient(handler);
        using AzureBlobStorageProvider provider = Create(http);
        using var seekable = new MemoryStream([1, 2, 3]);
        seekable.Position = 1;
        var metadata = await provider.WriteAsync(new("seekable", seekable));
        Assert.AreEqual("\"wire-etag\"", metadata.Version);
        Assert.AreEqual(new DateTimeOffset(2026, 9, 9, 0, 0, 0, TimeSpan.Zero), metadata.LastModified);
        Assert.IsNull(metadata.ContentType);
        Assert.IsNull(metadata.Length);
        Assert.IsTrue(seekable.CanRead);
        using var nonseekable = new NonseekableStream([4, 5]);
        await provider.WriteAsync(new("nonseekable", nonseekable));
        Assert.IsTrue(nonseekable.CanRead);
        CollectionAssert.AreEqual(new byte[] { 2, 3, 4, 5 }, payloads.SelectMany(value => value).ToArray());
    }

    [TestMethod]
    public async Task RealSdkFailureAndPendingCancellationKeepInputOpen()
    {
        using var source = new CancellationTokenSource();
        using var handler = new CallbackHandler(async (request, token) =>
        {
            source.Cancel();
            await Task.Delay(Timeout.Infinite, token);
            throw new InvalidOperationException("Cancellation was not propagated.");
        });
        using var http = new HttpClient(handler);
        using AzureBlobStorageProvider provider = Create(http);
        using var stream = new MemoryStream([1]);
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.WriteAsync(new("file", stream), source.Token));
        Assert.IsTrue(stream.CanRead);
    }

    private static AzureBlobStorageProvider Create(HttpClient http)
    {
        var clientOptions = new BlobClientOptions { Transport = new HttpClientTransport(http) };
        clientOptions.Retry.MaxRetries = 0;
        var container = new BlobContainerClient(new Uri("https://unit-test.invalid/container"), clientOptions);
        return new AzureBlobStorageProvider(new AzureBlobStorageOptions { ContainerName = "container", CreateContainerIfNotExists = false }, NullLogger<AzureBlobStorageProvider>.Instance, container);
    }

    private static HttpResponseMessage Success()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Created) { Content = new ByteArrayContent([]) };
        response.Headers.TryAddWithoutValidation("ETag", "\"wire-etag\"");
        response.Content.Headers.LastModified = new DateTimeOffset(2026, 9, 9, 0, 0, 0, TimeSpan.Zero);
        response.Headers.TryAddWithoutValidation("x-ms-request-id", "test-request");
        return response;
    }
    private static HttpResponseMessage Conflict()
    {
        var response = new HttpResponseMessage(HttpStatusCode.PreconditionFailed)
        {
            Content = new StringContent("<Error><Code>ConditionNotMet</Code><Message>Existing blob</Message></Error>", System.Text.Encoding.UTF8, "application/xml")
        };
        response.Headers.TryAddWithoutValidation("x-ms-error-code", "ConditionNotMet");
        return response;
    }
    private sealed class CallbackHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> callback) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => callback(request, cancellationToken);
    }
    private sealed class NonseekableStream(byte[] content) : MemoryStream(content)
    {
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    }
}
