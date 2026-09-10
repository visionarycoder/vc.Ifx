using System.Collections.Concurrent;
using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using VisionaryCoder.Framework.Secrets.Azure.KeyVault;

namespace VisionaryCoder.Framework.Tests.Secrets.AzureKeyVault;

internal sealed class KeyVaultTestContext : IDisposable
{
    internal ManualTimeProvider Clock { get; } = new();
    internal MemoryCache Cache { get; } = new(new MemoryCacheOptions());
    internal FakeSecretClient Client { get; } = new();
    internal KeyVaultOptions Options { get; } = new();

    internal KeyVaultSecretProvider Create(ILogger<KeyVaultSecretProvider>? logger = null) =>
        new(Client, Microsoft.Extensions.Options.Options.Create(Options), Cache,
            logger ?? NullLogger<KeyVaultSecretProvider>.Instance, Clock);

    internal static Response<KeyVaultSecret> Response(string? value = "secret") =>
        Azure.Response.FromValue(SecretModelFactory.KeyVaultSecret(
            SecretModelFactory.SecretProperties(name: "name"), value), Mock.Of<Azure.Response>());

    public void Dispose() => Cache.Dispose();
}

internal sealed class ManualTimeProvider : TimeProvider
{
    internal DateTimeOffset Now { get; set; } = new(2026, 9, 9, 0, 0, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => Now;
}

internal sealed class FakeSecretClient : SecretClient
{
    internal ConcurrentQueue<(string Name, string? Version, CancellationToken Token)> Calls { get; } = new();
    internal Func<string, string?, CancellationToken, Task<Response<KeyVaultSecret>>> Retrieve { get; set; } =
        (name, version, token) => Task.FromResult(KeyVaultTestContext.Response());

    public override Task<Response<KeyVaultSecret>> GetSecretAsync(string name, string? version = null,
        CancellationToken cancellationToken = default)
    {
        Calls.Enqueue((name, version, cancellationToken));
        return Retrieve(name, version, cancellationToken);
    }
}

internal sealed class RecordingSecretLogger : ILogger<KeyVaultSecretProvider>
{
    internal List<string> Messages { get; } = [];
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter) => Messages.Add(formatter(state, exception));
}
