namespace VisionaryCoder.Framework.Messaging.Azure.Queue;

/// <summary>
/// Configuration options for Azure Queue Storage operations.
/// </summary>
public sealed class AzureQueueStorageOptions
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
    /// Gets or sets the default queue name for queue operations.
    /// </summary>
    public required string QueueName { get; init; }

    /// <summary>
    /// Gets or sets whether to use managed identity for authentication.
    /// When true, StorageAccountUri must be provided. When false, ConnectionString must be provided.
    /// </summary>
    public bool UseManagedIdentity { get; init; } = false;

    /// <summary>
    /// Gets or sets whether to create the queue if it doesn't exist.
    /// </summary>
    public bool CreateQueueIfNotExists { get; init; } = true;

    /// <summary>
    /// Gets or sets the timeout for queue operations in milliseconds.
    /// </summary>
    public int TimeoutMilliseconds { get; init; } = 30000;

    /// <summary>
    /// Gets or sets the message time-to-live in seconds. Default is 7 days (604800 seconds).
    /// Set to -1 for maximum allowed time-to-live.
    /// </summary>
    public int MessageTimeToLiveSeconds { get; init; } = 604800; // 7 days

    /// <summary>
    /// Gets or sets the visibility timeout for messages in seconds. Default is 30 seconds.
    /// </summary>
    public int VisibilityTimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Gets or sets the maximum number of messages to retrieve in a single operation. Default is 32 (max).
    /// </summary>
    public int MaxMessagesToRetrieve { get; init; } = 32;

    /// <summary>
    /// Gets or sets whether to Base64 encode message content.
    /// </summary>
    public bool EncodeMessages { get; init; } = true;

    /// <summary>
    /// Validates the configuration and throws exceptions for invalid settings.
    /// </summary>
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(QueueName);

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

        if (MessageTimeToLiveSeconds < -1 || MessageTimeToLiveSeconds == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MessageTimeToLiveSeconds), "Message time-to-live must be greater than 0 or -1 for maximum");
        }

        if (VisibilityTimeoutSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(VisibilityTimeoutSeconds), "Visibility timeout must be greater than 0");
        }

        if (MaxMessagesToRetrieve <= 0 || MaxMessagesToRetrieve > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxMessagesToRetrieve), "Max messages to retrieve must be between 1 and 32");
        }

        // Validate queue name according to Azure naming rules
        if (!IsValidQueueName(QueueName))
        {
            throw new ArgumentException("Queue name must be 3-63 characters long, contain only lowercase letters, numbers, and hyphens, and cannot start or end with a hyphen.", nameof(QueueName));
        }
    }

    private static bool IsValidQueueName(string queueName)
    {
        if (string.IsNullOrWhiteSpace(queueName) ||
            queueName.Length < 3 ||
            queueName.Length > 63 ||
            queueName.StartsWith('-') ||
            queueName.EndsWith('-') ||
            queueName.Contains("--"))
        {
            return false;
        }

        return queueName.All(c => char.IsLower(c) || char.IsDigit(c) || c == '-');
    }
}
