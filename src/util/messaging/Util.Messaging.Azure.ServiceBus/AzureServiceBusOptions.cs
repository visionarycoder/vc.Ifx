using Azure.Messaging.ServiceBus;

namespace Util.Messaging.Azure.ServiceBus;

/// <summary>
/// Defines configuration for the Azure Service Bus messaging provider.
/// </summary>
public sealed class AzureServiceBusOptions
{
    /// <summary>
    /// Gets or sets the Service Bus connection string. Configure this value or <see cref="FullyQualifiedNamespace"/>.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the fully qualified namespace for Azure AD-authenticated clients.
    /// Configure this value or <see cref="ConnectionString"/>.
    /// </summary>
    public string? FullyQualifiedNamespace { get; set; }

    /// <summary>
    /// Gets or sets the queue name used for publish and receive operations.
    /// Configure this value or <see cref="TopicName"/>.
    /// </summary>
    public string? QueueName { get; set; }

    /// <summary>
    /// Gets or sets the topic name used for publish operations and subscription-based receives.
    /// Configure this value or <see cref="QueueName"/>.
    /// </summary>
    public string? TopicName { get; set; }

    /// <summary>
    /// Gets or sets the total retry attempts for publish and settlement operations.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the initial retry delay used for exponential backoff.
    /// </summary>
    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// Gets or sets the maximum concurrent message callbacks for the processor.
    /// </summary>
    public int MaxConcurrentCalls { get; set; } = 1;

    /// <summary>
    /// Gets or sets the prefetch count for the processor.
    /// </summary>
    public int PrefetchCount { get; set; }

    /// <summary>
    /// Gets or sets the maximum auto-lock renewal duration for the processor.
    /// </summary>
    public TimeSpan MaxAutoLockRenewalDuration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the delivery-count threshold that triggers dead-lettering after handler failures.
    /// </summary>
    public int MaxDeliveryCountBeforeDeadLetter { get; set; } = 5;

    /// <summary>
    /// Validates the configured options and returns the same instance.
    /// </summary>
    /// <returns>The validated options instance.</returns>
    public AzureServiceBusOptions Validate()
    {
        bool hasConnectionString = !string.IsNullOrWhiteSpace(ConnectionString);
        bool hasNamespace = !string.IsNullOrWhiteSpace(FullyQualifiedNamespace);
        bool hasQueue = !string.IsNullOrWhiteSpace(QueueName);
        bool hasTopic = !string.IsNullOrWhiteSpace(TopicName);

        if (hasConnectionString == hasNamespace)
        {
            throw new ArgumentException(
                "Exactly one of ConnectionString or FullyQualifiedNamespace must be configured.",
                nameof(ConnectionString));
        }

        if (hasQueue == hasTopic)
        {
            throw new ArgumentException(
                "Exactly one of QueueName or TopicName must be configured.",
                nameof(QueueName));
        }

        if (hasConnectionString)
        {
            ConnectionString = ConnectionString.ValidateNotNullOrWhiteSpace(nameof(ConnectionString));
        }
        else
        {
            FullyQualifiedNamespace = FullyQualifiedNamespace.ValidateNotNullOrWhiteSpace(nameof(FullyQualifiedNamespace));
        }

        if (hasQueue)
        {
            QueueName = QueueName.ValidateNotNullOrWhiteSpace(nameof(QueueName));
        }
        else
        {
            TopicName = TopicName.ValidateNotNullOrWhiteSpace(nameof(TopicName));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(MaxRetryAttempts, 1);

        if (InitialRetryDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(InitialRetryDelay), "Initial retry delay cannot be negative.");
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(MaxConcurrentCalls, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(PrefetchCount, 0);

        if (MaxAutoLockRenewalDuration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxAutoLockRenewalDuration), "Max auto-lock renewal duration cannot be negative.");
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(MaxDeliveryCountBeforeDeadLetter, 1);

        return this;
    }

    internal string GetEntityPath()
    {
        AzureServiceBusOptions validatedOptions = Validate();
        return validatedOptions.QueueName ?? validatedOptions.TopicName!;
    }

    internal ServiceBusProcessorOptions CreateProcessorOptions()
    {
        Validate();

        return new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = MaxConcurrentCalls,
            PrefetchCount = PrefetchCount,
            MaxAutoLockRenewalDuration = MaxAutoLockRenewalDuration
        };
    }

    internal bool UsesQueue()
    {
        Validate();
        return !string.IsNullOrWhiteSpace(QueueName);
    }
}
