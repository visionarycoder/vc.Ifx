namespace VisionaryCoder.Framework.Data.Azure.Table;

/// <summary>
/// Configuration options for Azure Table Storage operations.
/// </summary>
public sealed class AzureTableStorageOptions
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
    /// Gets or sets the default table name for table operations.
    /// </summary>
    public required string TableName { get; init; }

    /// <summary>
    /// Gets or sets whether to use managed identity for authentication.
    /// When true, StorageAccountUri must be provided. When false, ConnectionString must be provided.
    /// </summary>
    public bool UseManagedIdentity { get; init; } = false;

    /// <summary>
    /// Gets or sets whether to create the table if it doesn't exist.
    /// </summary>
    public bool CreateTableIfNotExists { get; init; } = true;

    /// <summary>
    /// Gets or sets the timeout for table operations in milliseconds.
    /// </summary>
    public int TimeoutMilliseconds { get; init; } = 30000;

    /// <summary>
    /// Gets or sets the maximum number of entities to retrieve in a single query operation. Default is 1000.
    /// </summary>
    public int MaxEntitiesPerQuery { get; init; } = 1000;

    /// <summary>
    /// Gets or sets the maximum number of entities to include in a batch operation. Default is 100 (max).
    /// </summary>
    public int MaxEntitiesPerBatch { get; init; } = 100;

    /// <summary>
    /// Gets or sets whether to enable optimistic concurrency using ETags.
    /// </summary>
    public bool EnableOptimisticConcurrency { get; init; } = true;

    /// <summary>
    /// Gets or sets the retry policy maximum attempts for transient failures.
    /// </summary>
    public int MaxRetryAttempts { get; init; } = 3;

    /// <summary>
    /// Gets or sets the delay between retry attempts in milliseconds.
    /// </summary>
    public int RetryDelayMilliseconds { get; init; } = 1000;

    /// <summary>
    /// Validates the configuration and throws exceptions for invalid settings.
    /// </summary>
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(TableName);

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

        if (MaxEntitiesPerQuery <= 0 || MaxEntitiesPerQuery > 1000)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxEntitiesPerQuery), "Max entities per query must be between 1 and 1000");
        }

        if (MaxEntitiesPerBatch <= 0 || MaxEntitiesPerBatch > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxEntitiesPerBatch), "Max entities per batch must be between 1 and 100");
        }

        if (MaxRetryAttempts < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxRetryAttempts), "Max retry attempts must be greater than or equal to 0");
        }

        if (RetryDelayMilliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(RetryDelayMilliseconds), "Retry delay must be greater than or equal to 0");
        }

        // Validate table name according to Azure naming rules
        if (!IsValidTableName(TableName))
        {
            throw new ArgumentException("Table name must be 3-63 characters long, start with a letter, and contain only alphanumeric characters.", nameof(TableName));
        }
    }

    private static bool IsValidTableName(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName) ||
            tableName.Length < 3 ||
            tableName.Length > 63 ||
            !char.IsLetter(tableName[0]))
        {
            return false;
        }

        return tableName.All(c => char.IsLetterOrDigit(c));
    }
}
