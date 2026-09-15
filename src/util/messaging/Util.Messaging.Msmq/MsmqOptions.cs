namespace Util.Messaging.Msmq;

/// <summary>
/// Configures MSMQ-backed messaging behavior.
/// </summary>
public sealed class MsmqOptions
{
    /// <summary>
    /// Gets or sets the MSMQ queue path.
    /// Use <c>{subscriptionName}</c> to bind each consumer subscription to a distinct queue path.
    /// </summary>
    public string? QueuePath { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of total retry attempts for send and receive operations.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the initial retry delay.
    /// </summary>
    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromMilliseconds(250);

    /// <summary>
    /// Gets or sets the receive timeout used to poll for messages while honoring cancellation.
    /// </summary>
    public TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Validates the configured option values.
    /// </summary>
    public void Validate()
    {
        QueuePath.ValidateNotNullOrWhiteSpace(nameof(QueuePath));
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxRetryAttempts, 1);

        if (InitialRetryDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(InitialRetryDelay), "Initial retry delay cannot be negative.");
        }

        if (ReceiveTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(ReceiveTimeout), "Receive timeout must be greater than zero.");
        }
    }

    internal string ResolveQueuePath(string subscriptionName)
    {
        string queuePath = QueuePath.ValidateNotNullOrWhiteSpace(nameof(QueuePath));
        string validatedSubscriptionName = subscriptionName.ValidateNotNullOrWhiteSpace(nameof(subscriptionName));

        return queuePath.Contains("{subscriptionName}", StringComparison.Ordinal)
            ? queuePath.Replace("{subscriptionName}", validatedSubscriptionName, StringComparison.Ordinal)
            : queuePath;
    }
}
