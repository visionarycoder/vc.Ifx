namespace VisionaryCoder.Framework.Storage.Abstractions;

/// <summary>
/// Custom exception for storage-specific errors.
/// </summary>
public class StorageException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StorageException"/> class.
    /// </summary>
    public StorageException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public StorageException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public StorageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Gets or sets the path associated with the storage operation that failed.
    /// </summary>
    public string? Path { get; init; }

    /// <summary>
    /// Gets or sets the storage provider type that generated the exception.
    /// </summary>
    public string? ProviderType { get; init; }
}
