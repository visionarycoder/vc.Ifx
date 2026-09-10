namespace VisionaryCoder.Framework.Storage;

/// <summary>Isolates object storage mechanisms behind provider-neutral commands and facts.</summary>
/// <remarks>
/// Paths are opaque and provider-relative. Providers validate their own path restrictions.
/// All operations reject null requests, honor cancellation before I/O and during pending I/O,
/// and propagate cancellation as OperationCanceledException. Unsupported operations throw
/// NotSupportedException; authorization failures throw UnauthorizedAccessException; other
/// provider failures throw IOException with the original exception as InnerException.
/// Failures are never returned as missing objects. Listing is not a consistent snapshot.
/// </remarks>
public interface IObjectStorageProvider
{
    /// <summary>Gets supported operations. Capabilities do not imply authorization or availability.</summary>
    StorageCapabilities Capabilities { get; }

    /// <summary>Opens a readable stream, owned and disposed by the caller.</summary>
    /// <param name="request">The object to read.</param>
    /// <param name="cancellationToken">Cancellation for opening the stream. Subsequent reads take their own token.</param>
    /// <returns>The content stream, which need not support seeking.</returns>
    /// <exception cref="FileNotFoundException">The object does not exist.</exception>
    Task<Stream> OpenReadAsync(StorageObjectRequest request, CancellationToken cancellationToken = default);

    /// <summary>Writes remaining content from the current stream position without disposing the input.</summary>
    /// <param name="request">The path, content, and replacement policy.</param>
    /// <param name="cancellationToken">Cancellation for the write.</param>
    /// <returns>Metadata for the successfully written object.</returns>
    /// <exception cref="IOException">A create-only write conflicts, or a provider operation fails.</exception>
    /// <exception cref="NotSupportedException">Writing or the requested create-only policy is unsupported.</exception>
    /// <remarks>Failure or cancellation does not guarantee rollback of an in-progress write.</remarks>
    Task<StorageObjectMetadata> WriteAsync(StorageWriteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Gets metadata, returning null only when the object is absent.</summary>
    /// <param name="request">The object to inspect.</param>
    /// <param name="cancellationToken">Cancellation for the lookup.</param>
    /// <returns>Object metadata, or null for an absent object.</returns>
    Task<StorageObjectMetadata?> GetMetadataAsync(StorageObjectRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes an object idempotently.</summary>
    /// <param name="request">The object to delete.</param>
    /// <param name="cancellationToken">Cancellation for the delete.</param>
    /// <returns>A task that completes successfully when deletion succeeds or the object is already absent.</returns>
    Task DeleteAsync(StorageObjectRequest request, CancellationToken cancellationToken = default);

    /// <summary>Lists matching objects, yielding nothing when no objects match.</summary>
    /// <param name="request">The literal path prefix.</param>
    /// <param name="cancellationToken">Cancellation observed throughout enumeration.</param>
    /// <returns>An unordered, potentially paged sequence of metadata without directory entries.</returns>
    IAsyncEnumerable<StorageObjectMetadata> ListAsync(StorageListRequest request, CancellationToken cancellationToken = default);
}
