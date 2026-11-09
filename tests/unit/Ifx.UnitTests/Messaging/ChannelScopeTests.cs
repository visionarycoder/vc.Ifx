using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using vc.Ifx.Messaging;
using vc.Ifx.Messaging.Channels;

namespace Ifx.Tests.Messaging;

[TestClass]
public sealed class ChannelScopeTests
{
    [TestMethod]
    public async Task Dispatch_TwoMessages_CreatesAndDisposesIndependentScopes()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Observations>();
        services.AddScoped<Dependency>();
        services.AddChannelMessaging().AddChannelMessageHandler<Handler, Message>();
        await using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
        IHostedService registration = provider.GetRequiredService<IHostedService>();
        await registration.StartAsync(CancellationToken.None);
        try
        {
            var bus = provider.GetRequiredService<ChannelMessageBus>();
            var observations = provider.GetRequiredService<Observations>();
            await bus.PublishAsync(new Message());
            await bus.PublishAsync(new Message());
            await observations.Disposed.Task.WaitAsync(TimeSpan.FromSeconds(10));
            observations.Ids.Distinct().Should().HaveCount(2);
            observations.DisposeCount.Should().Be(2);
        }
        finally
        {
            await registration.StopAsync(CancellationToken.None);
        }
    }

    private sealed record Message : MessageBase;

    private sealed class Observations
    {
        public ConcurrentQueue<Guid> Ids { get; } = new();
        public TaskCompletionSource Disposed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int DisposeCount;
    }

    private sealed class Dependency(Observations observations) : IAsyncDisposable
    {
        public Guid Id { get; } = Guid.NewGuid();

        public ValueTask DisposeAsync()
        {
            if (Interlocked.Increment(ref observations.DisposeCount) == 2) observations.Disposed.TrySetResult();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class Handler(Dependency dependency, Observations observations) : IMessageHandler<Message>
    {
        public Task HandleAsync(Message message, CancellationToken cancellationToken = default)
        {
            observations.Ids.Enqueue(dependency.Id);
            return Task.CompletedTask;
        }
    }
}
