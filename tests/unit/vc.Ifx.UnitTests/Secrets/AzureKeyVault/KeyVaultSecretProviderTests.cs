using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using VisionaryCoder.Framework.Secrets.Azure.KeyVault;

namespace VisionaryCoder.Framework.Tests.Secrets.AzureKeyVault;

[TestClass]
public sealed class KeyVaultSecretProviderTests
{
    [TestMethod]
    [DataRow("client")]
    [DataRow("options")]
    [DataRow("cache")]
    [DataRow("logger")]
    [DataRow("timeProvider")]
    public void ConstructorRejectsNullDependencies(string argument)
    {
        using var context = new KeyVaultTestContext();
        Action action = () => new KeyVaultSecretProvider(
            argument == "client" ? null! : context.Client,
            argument == "options" ? null! : Microsoft.Extensions.Options.Options.Create(context.Options),
            argument == "cache" ? null! : context.Cache,
            argument == "logger" ? null! : NullLogger<KeyVaultSecretProvider>.Instance,
            argument == "timeProvider" ? null! : context.Clock);

        action.Should().Throw<ArgumentNullException>().WithParameterName(argument);
    }

    [TestMethod]
    public void ConstructorRejectsNullOptionsValue()
    {
        using var context = new KeyVaultTestContext();
        var options = new Mock<IOptions<KeyVaultOptions>>();
        options.SetupGet(value => value.Value).Returns((KeyVaultOptions)null!);
        Action action = () => new KeyVaultSecretProvider(context.Client, options.Object,
            context.Cache, NullLogger<KeyVaultSecretProvider>.Instance);

        action.Should().Throw<ArgumentException>().WithParameterName("options");
    }

    [TestMethod]
    public async Task LegacyConstructorAndDefaultLiteralCallRemainUsable()
    {
        using var context = new KeyVaultTestContext();
        var provider = new KeyVaultSecretProvider(context.Client, Microsoft.Extensions.Options.Options.Create(context.Options),
            context.Cache, NullLogger<KeyVaultSecretProvider>.Instance);

        (await provider.GetAsync("secret", default)).Should().Be("secret");
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("a:b")]
    [DataRow("a/b")]
    [DataRow("a_b")]
    [DataRow("na\u00efve")]
    public async Task InvalidNamesFailBeforeSdkAccess(string? name)
    {
        using var context = new KeyVaultTestContext();
        Func<Task> action = () => context.Create().GetAsync(name!);
        await action.Should().ThrowAsync<ArgumentException>().WithParameterName("name");
        context.Client.Calls.Should().BeEmpty();
    }

    [TestMethod]
    public async Task NameLengthBoundaryIsValidated()
    {
        using var context = new KeyVaultTestContext();
        KeyVaultSecretProvider provider = context.Create();
        (await provider.GetAsync(new string('a', 127))).Should().Be("secret");
        (await provider.GetAsync("A-b-123")).Should().Be("secret");
        Func<Task> action = () => provider.GetAsync(new string('a', 128));
        await action.Should().ThrowAsync<ArgumentException>().WithParameterName("name");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("abc")]
    [DataRow("0123456789012345678901234567890z")]
    [DataRow("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz")]
    public async Task InvalidVersionsFailBeforeSdkAccess(string version)
    {
        using var context = new KeyVaultTestContext();
        Func<Task> action = () => context.Create().GetAsync("name", version, default);
        await action.Should().ThrowAsync<ArgumentException>().WithParameterName("version");
        context.Client.Calls.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("value")]
    public async Task ValidValuesArePreservedAndCached(string value)
    {
        using var context = new KeyVaultTestContext();
        context.Client.Retrieve = (name, version, token) => Task.FromResult(KeyVaultTestContext.Response(value));
        KeyVaultSecretProvider provider = context.Create();

        (await provider.GetAsync("name")).Should().Be(value);
        (await provider.GetAsync("name")).Should().Be(value);
        context.Client.Calls.Should().ContainSingle();
    }

    [TestMethod]
    public async Task CacheSeparatesNamesVersionsAndProviderInstances()
    {
        using var context = new KeyVaultTestContext();
        const string firstVersion = "0123456789abcdef0123456789ABCDEF";
        const string secondVersion = "abcdef0123456789abcdef0123456789";
        context.Client.Retrieve = (name, version, token) => Task.FromResult(KeyVaultTestContext.Response(version ?? name));
        KeyVaultSecretProvider first = context.Create();
        KeyVaultSecretProvider second = context.Create();

        (await first.GetAsync("name")).Should().Be("name");
        (await first.GetAsync("name", firstVersion, default)).Should().Be(firstVersion);
        (await first.GetAsync("name", secondVersion, default)).Should().Be(secondVersion);
        (await first.GetAsync("other")).Should().Be("other");
        (await first.GetAsync("name", firstVersion, default)).Should().Be(firstVersion);
        (await second.GetAsync("name")).Should().Be("name");
        context.Client.Calls.Should().HaveCount(5);
        context.Client.Calls.Select(call => call.Version).Should().Contain(firstVersion);
    }

    [TestMethod]
    public async Task ClockExpiresCacheAtBoundaryAndOptionsAreSnapshotted()
    {
        using var context = new KeyVaultTestContext();
        context.Options.CacheTtl = TimeSpan.FromMinutes(2);
        KeyVaultSecretProvider provider = context.Create();
        await provider.GetAsync("name");
        context.Options.CacheTtl = TimeSpan.FromDays(1);
        context.Clock.Now += TimeSpan.FromMinutes(2);
        await provider.GetAsync("name");
        context.Client.Calls.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task ZeroTtlDisablesCompletedValueCaching()
    {
        using var context = new KeyVaultTestContext();
        context.Options.CacheTtl = TimeSpan.Zero;
        KeyVaultSecretProvider provider = context.Create();
        await provider.GetAsync("name");
        await provider.GetAsync("name");
        context.Client.Calls.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task SecretExpiryCapsCacheTtl()
    {
        using var context = new KeyVaultTestContext();
        Response<KeyVaultSecret> response = KeyVaultTestContext.Response();
        response.Value.Properties.Enabled = true;
        response.Value.Properties.NotBefore = context.Clock.Now;
        response.Value.Properties.ExpiresOn = context.Clock.Now.AddMinutes(1);
        context.Client.Retrieve = (name, version, token) => Task.FromResult(response);
        KeyVaultSecretProvider provider = context.Create();
        await provider.GetAsync("name");
        context.Clock.Now += TimeSpan.FromMinutes(1);

        Func<Task> action = () => provider.GetAsync("name");
        await action.Should().ThrowAsync<InvalidOperationException>();
        context.Client.Calls.Should().HaveCount(2);
    }

    [TestMethod]
    [DataRow("disabled")]
    [DataRow("future")]
    [DataRow("expired")]
    [DataRow("expires-now")]
    public async Task InvalidSecretStateFailsAndIsNotCached(string state)
    {
        using var context = new KeyVaultTestContext();
        Response<KeyVaultSecret> response = KeyVaultTestContext.Response();
        response.Value.Properties.Enabled = state != "disabled";
        response.Value.Properties.NotBefore = state == "future" ? context.Clock.Now.AddSeconds(1) : context.Clock.Now.AddSeconds(-1);
        response.Value.Properties.ExpiresOn = state switch
        {
            "expired" => context.Clock.Now.AddSeconds(-1),
            "expires-now" => context.Clock.Now,
            _ => context.Clock.Now.AddHours(1)
        };
        context.Client.Retrieve = (name, version, token) => Task.FromResult(response);
        KeyVaultSecretProvider provider = context.Create();
        Func<Task> action = () => provider.GetAsync("name");

        await action.Should().ThrowAsync<InvalidOperationException>();
        await action.Should().ThrowAsync<InvalidOperationException>();
        context.Client.Calls.Should().HaveCount(2);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task MalformedResponsesAreFailuresNotMisses(bool missingObject)
    {
        using var context = new KeyVaultTestContext();
        Response<KeyVaultSecret> response = missingObject
            ? Response.FromValue<KeyVaultSecret>(null!, Mock.Of<Response>())
            : KeyVaultTestContext.Response(null);
        context.Client.Retrieve = (name, version, token) => Task.FromResult(response);

        Func<Task> action = () => context.Create().GetAsync("name");
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [TestMethod]
    public async Task Only404ReturnsMissingAndMissesAreNotCachedOrLoggedWithPayloads()
    {
        using var context = new KeyVaultTestContext();
        var logger = new RecordingSecretLogger();
        context.Client.Retrieve = (name, version, token) => throw new RequestFailedException(404, "sensitive-payload");
        KeyVaultSecretProvider provider = context.Create(logger);

        (await provider.GetAsync("sensitive-name")).Should().BeNull();
        (await provider.GetAsync("sensitive-name")).Should().BeNull();
        context.Client.Calls.Should().HaveCount(2);
        logger.Messages.Should().HaveCount(2).And.OnlyContain(message =>
            !message.Contains("sensitive", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow(400)]
    [DataRow(401)]
    [DataRow(403)]
    [DataRow(429)]
    [DataRow(500)]
    [DataRow(503)]
    public async Task Non404FailuresPropagateUnchangedWithoutProviderRetries(int status)
    {
        using var context = new KeyVaultTestContext();
        var failure = new RequestFailedException(status, "failure");
        context.Client.Retrieve = (name, version, token) => Task.FromException<Response<KeyVaultSecret>>(failure);
        KeyVaultSecretProvider provider = context.Create();
        Func<Task> action = () => provider.GetAsync("name");

        var exception = await action.Should().ThrowAsync<RequestFailedException>();
        exception.Which.Should().BeSameAs(failure);
        context.Client.Calls.Should().ContainSingle();
        await action.Should().ThrowAsync<RequestFailedException>();
        context.Client.Calls.Should().HaveCount(2);
    }
}
