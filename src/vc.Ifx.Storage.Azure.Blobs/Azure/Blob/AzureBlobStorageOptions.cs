using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace VisionaryCoder.Framework.Storage.Azure.Blob;

/// <summary>Azure Blob endpoint, container and SDK transfer settings.</summary>
public sealed class AzureBlobStorageOptions
{
    /// <summary>SDK connection string, required unless credential-chain authentication is selected.</summary>
    public string? ConnectionString { get; init; }
    /// <summary>HTTPS account endpoint for credential-chain authentication.</summary>
    public string? StorageAccountUri { get; init; }
    /// <summary>Lowercase ASCII container name, 3-63 characters.</summary>
    public required string ContainerName { get; init; }
    /// <summary>Use DefaultAzureCredential, which includes managed identity but also development credentials.</summary>
    public bool UseManagedIdentity { get; init; }
    /// <summary>Access tier applied to block blob uploads.</summary>
    public AccessTier DefaultAccessTier { get; init; } = AccessTier.Hot;
    /// <summary>Create the container before writes, never during construction or reads.</summary>
    public bool CreateContainerIfNotExists { get; init; } = true;
    /// <summary>Public access used only when a container is created; defaults to private.</summary>
    public PublicAccessType ContainerPublicAccess { get; init; } = PublicAccessType.None;
    /// <summary>SDK network timeout per attempt, not a whole-operation deadline.</summary>
    public int TimeoutMilliseconds { get; init; } = 30000;
    /// <summary>Initial and maximum SDK transfer chunk size; also used by legacy byte reads.</summary>
    public int BufferSize { get; init; } = 4 * 1024 * 1024;
    /// <summary>Maximum SDK retries after the first attempt. Zero disables retries.</summary>
    public int MaxRetries { get; init; } = 3;
    /// <summary>Initial exponential retry delay in milliseconds.</summary>
    public int RetryDelayMilliseconds { get; init; } = 800;
    /// <summary>Maximum exponential retry delay in milliseconds.</summary>
    public int MaxRetryDelayMilliseconds { get; init; } = 8000;

    /// <summary>Validate behavior and the active authentication configuration without network I/O.</summary>
    public void Validate()
    {
        ValidateBehavior();
        if (UseManagedIdentity)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(StorageAccountUri);
            if (!Uri.TryCreate(StorageAccountUri, UriKind.Absolute, out Uri? endpoint) ||
                endpoint.Scheme != Uri.UriSchemeHttps || endpoint.UserInfo.Length != 0 ||
                endpoint.Query.Length != 0 || endpoint.Fragment.Length != 0 || endpoint.AbsolutePath != "/")
                throw new ArgumentException("StorageAccountUri must be an HTTPS account endpoint without credentials, query, fragment or container path.", nameof(StorageAccountUri));
        }
        else
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(ConnectionString);
            // Reuse the SDK parser; constructing a client does not contact Azure.
            new BlobServiceClient(ConnectionString);
        }
    }

    internal void ValidateBehavior()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ContainerName);
        if (ContainerName.Length is < 3 or > 63 || ContainerName.StartsWith('-') || ContainerName.EndsWith('-') ||
            ContainerName.Contains("--", StringComparison.Ordinal) || !ContainerName.All(character => character is >= 'a' and <= 'z' or >= '0' and <= '9' or '-'))
            throw new ArgumentException("ContainerName must be 3-63 lowercase ASCII letters, digits or single interior hyphens.", nameof(ContainerName));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(TimeoutMilliseconds);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(BufferSize);
        ArgumentOutOfRangeException.ThrowIfNegative(MaxRetries);
        ArgumentOutOfRangeException.ThrowIfNegative(RetryDelayMilliseconds);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxRetryDelayMilliseconds, RetryDelayMilliseconds);
        if (!Enum.IsDefined(ContainerPublicAccess)) throw new ArgumentOutOfRangeException(nameof(ContainerPublicAccess));
        if (DefaultAccessTier != AccessTier.Hot && DefaultAccessTier != AccessTier.Cool && DefaultAccessTier != AccessTier.Cold && DefaultAccessTier != AccessTier.Archive)
            throw new ArgumentException("Use a standard block blob access tier.", nameof(DefaultAccessTier));
    }
}
