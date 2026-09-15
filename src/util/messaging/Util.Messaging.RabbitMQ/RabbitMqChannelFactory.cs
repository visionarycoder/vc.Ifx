using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Util.Messaging.RabbitMQ;

public sealed class RabbitMqChannelFactory : IRabbitMqChannelFactory, IDisposable, IAsyncDisposable
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly Func<CancellationToken, Task<IConnection>> createConnectionAsync;
    private readonly RabbitMqOptions options;
    private IConnection? connection;

    public RabbitMqChannelFactory(IOptions<RabbitMqOptions> options)
        : this(GetOptions(options))
    {
    }

    internal RabbitMqChannelFactory(
        RabbitMqOptions options,
        Func<CancellationToken, Task<IConnection>>? createConnectionAsync = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        this.options = options.Validate();
        this.createConnectionAsync = createConnectionAsync ?? CreateConnectionAsync;
    }

    async Task<IRabbitMqChannel> IRabbitMqChannelFactory.CreateChannelAsync(CancellationToken cancellationToken)
    {
        IConnection activeConnection = await GetConnectionAsync(cancellationToken).ConfigureAwait(false);
        IChannel channel = await activeConnection.CreateChannelAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return new RabbitMqChannel(channel);
    }

    public void Dispose()
    {
        connection?.Dispose();
        gate.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (connection is not null)
        {
            await connection.DisposeAsync().ConfigureAwait(false);
        }

        gate.Dispose();
    }

    private async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (connection is { IsOpen: true } openConnection)
        {
            return openConnection;
        }

        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (connection is { IsOpen: true } existingConnection)
            {
                return existingConnection;
            }

            connection?.Dispose();

            connection = await createConnectionAsync(cancellationToken).ConfigureAwait(false);

            return connection;
        }
        finally
        {
            gate.Release();
        }
    }

    private ConnectionFactory CreateFactory()
    {
        ConnectionFactory factory = new()
        {
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            ConsumerDispatchConcurrency = options.ConsumerDispatchConcurrency,
            ClientProvidedName = options.ClientProvidedName,
            VirtualHost = options.VirtualHost!
        };

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            factory.HostName = options.HostName!;
            factory.Port = options.Port;
            factory.UserName = options.UserName!;
            factory.Password = options.Password!;
        }
        else
        {
            factory.Uri = new Uri(options.ConnectionString!, UriKind.Absolute);
        }

        return factory;
    }

    private Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken)
    {
        ConnectionFactory factory = CreateFactory();
        return factory.CreateConnectionAsync(
            [factory.Endpoint],
            factory.ClientProvidedName,
            cancellationToken);
    }

    private static RabbitMqOptions GetOptions(IOptions<RabbitMqOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.Value.Validate();
    }
}
