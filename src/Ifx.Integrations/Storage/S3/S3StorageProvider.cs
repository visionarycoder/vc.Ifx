using System.Runtime.CompilerServices;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Ifx.Storage;

namespace Ifx.Storage.S3;

/// <summary>Implements provider-neutral object storage with Amazon S3.</summary>
public sealed class S3StorageProvider(IAmazonS3 client, S3StorageOptions options) : IObjectStorageProvider
{
    private readonly string keyPrefix = Validate(client, options).NormalizedPrefix;

    /// <inheritdoc />
    public StorageCapabilities Capabilities =>
        StorageCapabilities.Read | StorageCapabilities.Write | StorageCapabilities.Delete |
        StorageCapabilities.Metadata | StorageCapabilities.List;

    /// <inheritdoc />
    public async Task<Stream> OpenReadAsync(StorageObjectRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        string key = GetKey(request.Path);
        cancellationToken.ThrowIfCancellationRequested();

        GetObjectResponse response = await ExecuteAsync(
            () => client.GetObjectAsync(new GetObjectRequest { BucketName = options.BucketName, Key = key }, cancellationToken),
            cancellationToken).ConfigureAwait(false);
        if (cancellationToken.IsCancellationRequested)
        {
            response.Dispose();
            cancellationToken.ThrowIfCancellationRequested();
        }

        return new S3ResponseStream(response);
    }

    /// <inheritdoc />
    public async Task<StorageObjectMetadata> WriteAsync(StorageWriteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        string key = GetKey(request.Path);
        cancellationToken.ThrowIfCancellationRequested();
        if (!request.Overwrite)
        {
            throw new NotSupportedException("The S3 provider does not expose atomic create-only writes.");
        }

        PutObjectResponse response = await ExecuteAsync(
            () => client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = options.BucketName,
                Key = key,
                InputStream = request.Content
            }, cancellationToken),
            cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return new StorageObjectMetadata(request.Path, version: response.ETag);
    }

    /// <inheritdoc />
    public async Task<StorageObjectMetadata?> GetMetadataAsync(
        StorageObjectRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        string key = GetKey(request.Path);
        try
        {
            GetObjectMetadataResponse response = await ExecuteAsync(
                () => client.GetObjectMetadataAsync(
                    new GetObjectMetadataRequest { BucketName = options.BucketName, Key = key },
                    cancellationToken),
                cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return new StorageObjectMetadata(
                request.Path,
                response.Headers.ContentLength,
                response.LastModified is DateTime lastModified
                    ? new DateTimeOffset(lastModified.ToUniversalTime())
                    : null,
                response.Headers.ContentType,
                response.ETag);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(StorageObjectRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        string key = GetKey(request.Path);
        _ = await ExecuteAsync(
            () => client.DeleteObjectAsync(new DeleteObjectRequest { BucketName = options.BucketName, Key = key }, cancellationToken),
            cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StorageObjectMetadata> ListAsync(
        StorageListRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidatePath(request.Prefix, allowEmpty: true);
        string prefix = keyPrefix + request.Prefix;
        string? continuationToken = null;

        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            ListObjectsV2Response response = await ExecuteAsync(
                () => client.ListObjectsV2Async(new ListObjectsV2Request
                {
                    BucketName = options.BucketName,
                    Prefix = prefix,
                    ContinuationToken = continuationToken
                }, cancellationToken),
                cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            foreach (S3Object item in response.S3Objects)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!item.Key.StartsWith(keyPrefix, StringComparison.Ordinal) ||
                    !item.Key.StartsWith(prefix, StringComparison.Ordinal))
                {
                    throw new IOException("S3 returned an object outside the configured key prefix.");
                }

                yield return new StorageObjectMetadata(
                    item.Key[keyPrefix.Length..],
                    item.Size,
                    item.LastModified is DateTime lastModified
                        ? new DateTimeOffset(lastModified.ToUniversalTime())
                        : null,
                    version: item.ETag);
            }

            if (response.IsTruncated != true)
            {
                yield break;
            }

            continuationToken = response.NextContinuationToken;
            if (string.IsNullOrWhiteSpace(continuationToken))
            {
                throw new IOException("S3 returned a truncated listing without a continuation token.");
            }
        }
        while (true);
    }

    private static S3StorageOptions Validate(IAmazonS3 client, S3StorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        return options;
    }

    private string GetKey(string path)
    {
        ValidatePath(path, allowEmpty: false);
        return keyPrefix + path;
    }

    private static void ValidatePath(string path, bool allowEmpty)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (!allowEmpty)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
        }

        if (path.Length > 1024 || path.StartsWith('/') || path.Contains('\\') ||
            path.Any(char.IsControl) ||
            path.Split('/', StringSplitOptions.RemoveEmptyEntries).Any(segment => segment is "." or ".."))
        {
            throw new ArgumentException("S3 object paths must be relative keys without traversal segments or controls.", nameof(path));
        }
    }

    private static async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            T response = await action().ConfigureAwait(false);
            return response;
        }
        catch (AmazonS3Exception exception) when (exception.ErrorCode == "NoSuchBucket")
        {
            throw new IOException("The S3 bucket does not exist.", exception);
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound ||
                                                  exception.ErrorCode is "NoSuchKey" or "NotFound")
        {
            throw new FileNotFoundException("The S3 object or bucket does not exist.", exception);
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode is System.Net.HttpStatusCode.Forbidden or System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("S3 access was denied.", exception);
        }
        catch (AmazonS3Exception exception)
        {
            throw new IOException("S3 operation failed.", exception);
        }
        catch (AmazonServiceException exception)
        {
            throw new IOException("S3 service operation failed.", exception);
        }
    }

    private sealed class S3ResponseStream(GetObjectResponse response) : Stream
    {
        public override bool CanRead => response.ResponseStream.CanRead;
        public override bool CanSeek => response.ResponseStream.CanSeek;
        public override bool CanWrite => false;
        public override long Length => response.ResponseStream.Length;
        public override long Position { get => response.ResponseStream.Position; set => response.ResponseStream.Position = value; }
        public override void Flush() => response.ResponseStream.Flush();
        public override int Read(byte[] buffer, int offset, int count) => response.ResponseStream.Read(buffer, offset, count);
        public override int Read(Span<byte> buffer) => response.ResponseStream.Read(buffer);
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            response.ResponseStream.ReadAsync(buffer, cancellationToken);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            response.ResponseStream.ReadAsync(buffer, offset, count, cancellationToken);
        public override long Seek(long offset, SeekOrigin origin) => response.ResponseStream.Seek(offset, origin);
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                response.ResponseStream.Dispose();
                response.Dispose();
            }

            base.Dispose(disposing);
        }
        public override async ValueTask DisposeAsync()
        {
            await response.ResponseStream.DisposeAsync().ConfigureAwait(false);
            response.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
