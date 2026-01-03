using Azure.Storage.Blobs.Models;

namespace VisionaryCoder.Framework.Storage.Azure.Blob;

/// <summary>
/// Configuration options for Azure Blob Storage operations.
/// </summary>
public sealed class AzureBlobStorageOptions
{
    /// <summary>
    /// Gets or sets the Azure Storage account connection string.
    /// </summary>
    public string? ConnectionString { get; init; }

    /// <summary>
    /// Gets or sets the Azure Storage account URI (when using managed identity).
    /// </summary>
    public string? StorageAccountUri { get; init; }

    /// <summary>
    /// Gets or sets the default container name for blob operations.
    /// </summary>
    public required string ContainerName { get; init; }

    /// <summary>
    /// Gets or sets whether to use managed identity for authentication.
    /// When true, StorageAccountUri must be provided. When false, ConnectionString must be provided.
    /// </summary>
    public bool UseManagedIdentity { get; init; } = false;

    /// <summary>
    /// Gets or sets the default blob access tier.
    /// </summary>
    public AccessTier DefaultAccessTier { get; init; } = AccessTier.Hot;

    /// <summary>
    /// Gets or sets whether to create the container if it doesn't exist.
    /// </summary>
    public bool CreateContainerIfNotExists { get; init; } = true;

    /// <summary>
    /// Gets or sets the public access level for the container when creating it.
    /// </summary>
    public PublicAccessType ContainerPublicAccess { get; init; } = PublicAccessType.None;

    /// <summary>
    /// Gets or sets the timeout for blob operations in milliseconds.
    /// </summary>
    public int TimeoutMilliseconds { get; init; } = 30000;

    /// <summary>
    /// Gets or sets the buffer size for blob transfers.
    /// </summary>
    public int BufferSize { get; init; } = 4 * 1024 * 1024; // 4MB default

    /// <summary>
    /// Validates the configuration and throws exceptions for invalid settings.
    /// </summary>
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ContainerName);

        if (UseManagedIdentity)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(StorageAccountUri);
            if (!Uri.TryCreate(StorageAccountUri, UriKind.Absolute, out _))
            {
                throw new ArgumentException("StorageAccountUri must be a valid absolute URI.", nameof(StorageAccountUri));
            }
        }
        else
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(ConnectionString);
        }

        if (TimeoutMilliseconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(TimeoutMilliseconds), "Timeout must be greater than 0");
        }

        if (BufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(BufferSize), "Buffer size must be greater than 0");
        }

        // Validate container name according to Azure naming rules
        if (!IsValidContainerName(ContainerName))
        {
            throw new ArgumentException("Container name must be 3-63 characters long, contain only lowercase letters, numbers, and hyphens, and cannot start or end with a hyphen.", nameof(ContainerName));
        }
    }

    private static bool IsValidContainerName(string containerName)
    {
        if (string.IsNullOrWhiteSpace(containerName) ||
            containerName.Length < 3 ||
            containerName.Length > 63 ||
            containerName.StartsWith('-') ||
            containerName.EndsWith('-') ||
            containerName.Contains("--"))
        {
            return false;
        }

        return containerName.All(c => char.IsLower(c) || char.IsDigit(c) || c == '-');
    }
}
