using Confluent.Kafka;

namespace vc.Ifx.Messaging.Kafka;

/// <summary>Defines Kafka broker, topic, consumer-group, and client settings.</summary>
public sealed class KafkaOptions
{
    /// <summary>Gets the default configuration section path.</summary>
    public const string DefaultSectionName = "Messaging:Kafka";

    /// <summary>Gets or sets the comma-separated Kafka bootstrap servers.</summary>
    public required string BootstrapServers { get; set; }

    /// <summary>Gets or sets the base consumer-group identifier.</summary>
    public required string GroupId { get; set; }

    /// <summary>Gets or sets the prefix used for generated message topics.</summary>
    public string TopicPrefix { get; set; } = "ifx";

    /// <summary>Gets or sets the consumer offset reset behavior.</summary>
    public AutoOffsetReset AutoOffsetReset { get; set; } = AutoOffsetReset.Earliest;

    /// <summary>Gets or sets the optional producer client configuration.</summary>
    public Action<ProducerConfig>? ConfigureProducer { get; set; }

    /// <summary>Gets or sets the optional consumer client configuration.</summary>
    public Action<ConsumerConfig>? ConfigureConsumer { get; set; }

    /// <summary>Gets or sets the optional administration client configuration.</summary>
    public Action<AdminClientConfig>? ConfigureAdminClient { get; set; }

    /// <summary>Validates Kafka connection and topic settings.</summary>
    public KafkaOptions Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(BootstrapServers);
        ArgumentException.ThrowIfNullOrWhiteSpace(GroupId);
        ArgumentException.ThrowIfNullOrWhiteSpace(TopicPrefix);

        if (BootstrapServers.Any(char.IsControl) || GroupId.Any(char.IsControl))
        {
            throw new ArgumentException("Kafka connection and group identifiers cannot contain control characters.");
        }

        if (TopicPrefix.Length > 216 ||
            TopicPrefix.StartsWith('.') || TopicPrefix.StartsWith('_') ||
            TopicPrefix.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '.' and not '_' and not '-'))
        {
            throw new ArgumentException("The topic prefix must be a Kafka-safe name of at most 216 characters.", nameof(TopicPrefix));
        }

        if (!Enum.IsDefined(AutoOffsetReset))
        {
            throw new ArgumentOutOfRangeException(nameof(AutoOffsetReset));
        }

        return this;
    }

    internal string GetTopicName<TMessage>() => GetTopicName(typeof(TMessage));

    internal string GetTopicName(Type messageType)
    {
        ArgumentNullException.ThrowIfNull(messageType);
        string identity = $"{messageType.Assembly.GetName().Name}:{messageType.FullName ?? messageType.Name}";
        string suffix = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(identity)))[..32]
            .ToLowerInvariant();
        return $"{TopicPrefix}-{suffix}";
    }

    internal ConsumerConfig CreateConsumerConfig(string subscriptionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionName);
        var config = new ConsumerConfig
        {
            BootstrapServers = BootstrapServers,
            GroupId = $"{GroupId}.{subscriptionName}",
            AutoOffsetReset = AutoOffsetReset,
            EnableAutoCommit = false
        };
        ConfigureConsumer?.Invoke(config);
        config.BootstrapServers = BootstrapServers;
        config.GroupId = $"{GroupId}.{subscriptionName}";
        config.EnableAutoCommit = false;
        return config;
    }

    internal ProducerConfig CreateProducerConfig()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = BootstrapServers,
            EnableIdempotence = true,
            Acks = Acks.All
        };
        ConfigureProducer?.Invoke(config);
        config.BootstrapServers = BootstrapServers;
        config.EnableIdempotence = true;
        config.Acks = Acks.All;
        return config;
    }

    internal AdminClientConfig CreateAdminClientConfig()
    {
        var config = new AdminClientConfig { BootstrapServers = BootstrapServers };
        ConfigureAdminClient?.Invoke(config);
        config.BootstrapServers = BootstrapServers;
        return config;
    }
}
