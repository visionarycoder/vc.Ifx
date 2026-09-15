using RabbitMQ.Client;

namespace Util.Messaging.RabbitMQ;

/// <summary>
/// Defines RabbitMQ transport settings for the reusable message-bus provider.
/// </summary>
public sealed class RabbitMqOptions
{
    /// <summary>
    /// Gets the default configuration section path.
    /// </summary>
    public const string DefaultSectionName = "Messaging:RabbitMq";

    /// <summary>
    /// Gets or sets the broker connection URI.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the broker host name when a connection string is not used.
    /// </summary>
    public string? HostName { get; set; }

    /// <summary>
    /// Gets or sets the broker port when a connection string is not used.
    /// </summary>
    public int Port { get; set; } = AmqpTcpEndpoint.UseDefaultPort;

    /// <summary>
    /// Gets or sets the RabbitMQ virtual host.
    /// </summary>
    public string? VirtualHost { get; set; } = "/";

    /// <summary>
    /// Gets or sets the broker user name when a connection string is not used.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Gets or sets the broker password when a connection string is not used.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the primary exchange name.
    /// </summary>
    public string? ExchangeName { get; set; }

    /// <summary>
    /// Gets or sets the exchange type.
    /// </summary>
    public string? ExchangeType { get; set; } = "direct";

    /// <summary>
    /// Gets or sets the base queue name.
    /// </summary>
    public string? QueueName { get; set; }

    /// <summary>
    /// Gets or sets the dead-letter exchange name.
    /// </summary>
    public string? DeadLetterExchangeName { get; set; }

    /// <summary>
    /// Gets or sets the dead-letter queue base name.
    /// </summary>
    public string? DeadLetterQueueName { get; set; }

    /// <summary>
    /// Gets or sets the broker prefetch count for each consumer.
    /// </summary>
    public ushort PrefetchCount { get; set; } = 20;

    /// <summary>
    /// Gets or sets the maximum number of publish or consume attempts.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the initial retry delay used by the shared retry helper.
    /// </summary>
    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromMilliseconds(250);

    /// <summary>
    /// Gets or sets the RabbitMQ client-provided connection name.
    /// </summary>
    public string? ClientProvidedName { get; set; } = "Util.Messaging.RabbitMQ";

    /// <summary>
    /// Gets or sets the asynchronous consumer dispatch concurrency.
    /// </summary>
    public ushort ConsumerDispatchConcurrency { get; set; } = 1;

    /// <summary>
    /// Validates the configured settings.
    /// </summary>
    public RabbitMqOptions Validate()
    {
        ExchangeName.ValidateNotNullOrWhiteSpace(nameof(ExchangeName));
        ExchangeType.ValidateNotNullOrWhiteSpace(nameof(ExchangeType));
        QueueName.ValidateNotNullOrWhiteSpace(nameof(QueueName));
        ClientProvidedName.ValidateNotNullOrWhiteSpace(nameof(ClientProvidedName));

        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            HostName.ValidateNotNullOrWhiteSpace(nameof(HostName));
            VirtualHost.ValidateNotNullOrWhiteSpace(nameof(VirtualHost));
            UserName.ValidateNotNullOrWhiteSpace(nameof(UserName));
            Password.ValidateNotNullOrWhiteSpace(nameof(Password));

            if (Port != AmqpTcpEndpoint.UseDefaultPort)
            {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(Port);
            }
        }
        else
        {
            ConnectionString.ValidateNotNullOrWhiteSpace(nameof(ConnectionString));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(PrefetchCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxRetryAttempts, 1);

        if (InitialRetryDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(InitialRetryDelay), "Initial retry delay cannot be negative.");
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ConsumerDispatchConcurrency);

        return this;
    }
}
