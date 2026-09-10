using Azure.Core;
using Azure.Storage.Queues;
using System.Text.RegularExpressions;

namespace VisionaryCoder.Framework.Messaging.Azure.Queue;

/// <summary>Configuration for the queue transport; business retry and poison policies belong to the caller.</summary>
public sealed class AzureQueueStorageOptions
{
    public string? ConnectionString { get; init; }
    public string? StorageAccountUri { get; init; }
    public required string QueueName { get; init; }
    public bool UseManagedIdentity { get; init; }
    public bool CreateQueueIfNotExists { get; init; } = true;
    /// <summary>Per-network-operation timeout, not a total workflow deadline.</summary>
    public int TimeoutMilliseconds { get; init; } = 30000;
    /// <summary>Positive lifetime in seconds, or -1 for no expiration.</summary>
    public int MessageTimeToLiveSeconds { get; init; } = 604800;
    /// <summary>Receive invisibility, between one second and seven days.</summary>
    public int VisibilityTimeoutSeconds { get; init; } = 30;
    public int MaxMessagesToRetrieve { get; init; } = 32;
    public bool EncodeMessages { get; init; } = true;
    /// <summary>Additional SDK retry attempts, from zero to ten.</summary>
    public int MaxRetryAttempts { get; init; } = 3;
    public int RetryDelayMilliseconds { get; init; } = 1000;

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(QueueName);
        if (!Regex.IsMatch(QueueName, "\\A[a-z0-9](?:[a-z0-9]|-(?!-)){1,61}[a-z0-9]\\z", RegexOptions.CultureInvariant))
            throw new ArgumentException("Queue name must be 3-63 ASCII lowercase letters, digits or single internal hyphens.", nameof(QueueName));

        if (UseManagedIdentity)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(StorageAccountUri);
            if (!Uri.TryCreate(StorageAccountUri, UriKind.Absolute, out var uri) ||
                uri.Scheme != Uri.UriSchemeHttps || uri.UserInfo.Length != 0 ||
                uri.Query.Length != 0 || uri.Fragment.Length != 0)
                throw new ArgumentException("StorageAccountUri must be an absolute HTTPS URI without credentials, query or fragment.", nameof(StorageAccountUri));
        }
        else
            ArgumentException.ThrowIfNullOrWhiteSpace(ConnectionString);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(TimeoutMilliseconds);
        if (MessageTimeToLiveSeconds < -1 || MessageTimeToLiveSeconds == 0)
            throw new ArgumentOutOfRangeException(nameof(MessageTimeToLiveSeconds));
        if (VisibilityTimeoutSeconds is < 1 or > 604800)
            throw new ArgumentOutOfRangeException(nameof(VisibilityTimeoutSeconds));
        if (MaxMessagesToRetrieve is < 1 or > 32)
            throw new ArgumentOutOfRangeException(nameof(MaxMessagesToRetrieve));
        if (MaxRetryAttempts is < 0 or > 10)
            throw new ArgumentOutOfRangeException(nameof(MaxRetryAttempts));
        ArgumentOutOfRangeException.ThrowIfNegative(RetryDelayMilliseconds);
    }

    /// <summary>Creates SDK-owned encoding, bounded exponential retry and network timeout settings.</summary>
    public QueueClientOptions CreateClientOptions()
    {
        Validate();
        var clientOptions = new QueueClientOptions
        {
            MessageEncoding = EncodeMessages ? QueueMessageEncoding.Base64 : QueueMessageEncoding.None
        };
        clientOptions.Retry.Mode = RetryMode.Exponential;
        clientOptions.Retry.MaxRetries = MaxRetryAttempts;
        clientOptions.Retry.Delay = TimeSpan.FromMilliseconds(RetryDelayMilliseconds);
        clientOptions.Retry.NetworkTimeout = TimeSpan.FromMilliseconds(TimeoutMilliseconds);
        return clientOptions;
    }
}
