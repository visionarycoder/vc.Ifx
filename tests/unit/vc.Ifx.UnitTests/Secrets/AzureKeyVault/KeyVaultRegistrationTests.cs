using System.Net;
using System.Text;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VisionaryCoder.Framework.Secrets;
using VisionaryCoder.Framework.Secrets.Azure;
using VisionaryCoder.Framework.Secrets.Azure.KeyVault;
using VisionaryCoder.Framework.Secrets.Local;

namespace VisionaryCoder.Framework.Tests.Secrets.AzureKeyVault;

[TestClass]
public sealed class KeyVaultRegistrationTests
{
    [TestMethod]
    public void RegistrationRejectsNullArguments()
    {
        using var configuration = new ConfigurationManager();
        Action noServices = () => KeyVaultExtensions.AddAzureKeyVaultSecrets(null!, configuration);
        Action noConfiguration = () => new ServiceCollection().AddAzureKeyVaultSecrets(null!);
        Action noNullServices = () => KeyVaultExtensions.AddNullSecrets(null!);

        noServices.Should().Throw<ArgumentNullException>().WithParameterName("services");
        noConfiguration.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
        noNullServices.Should().Throw<ArgumentNullException>().WithParameterName("services");
    }

    [TestMethod]
    public async Task LocalModeIsExplicitAndUsesSuppliedConfigurationWithoutAzureServices()
    {
        using var configuration = new ConfigurationManager();
        configuration["KeyVault:UseLocalSecrets"] = "true";
        configuration["KeyVault:LocalSecretsPrefix"] = "Local:Secrets";
        configuration["Local:Secrets:Password"] = "first";
        var services = new ServiceCollection();

        services.AddAzureKeyVaultSecrets(configuration).Should().BeSameAs(services);
        using ServiceProvider scope = services.BuildServiceProvider();
        ISecretProvider provider = scope.GetRequiredService<ISecretProvider>();
        provider.Should().BeOfType<LocalSecretProvider>();
        (await provider.GetAsync("Password")).Should().Be("first");
        configuration["Local:Secrets:Password"] = "reloaded";
        (await provider.GetAsync("Password")).Should().Be("reloaded");
        scope.GetService<SecretClient>().Should().BeNull();
        scope.GetService<TokenCredential>().Should().BeNull();
        scope.GetRequiredService<IOptions<KeyVaultOptions>>().Value.UseLocalSecrets.Should().BeTrue();
    }

    [TestMethod]
    public void ExplicitLocalModeValidatesOnlyConsumedOptions()
    {
        using var configuration = new ConfigurationManager();
        var services = new ServiceCollection();
        services.AddAzureKeyVaultSecrets(configuration, options =>
        {
            options.UseLocalSecrets = true;
            options.VaultUri = new Uri("http://invalid-for-remote.example/");
            options.MaxRetries = -1;
        });
        using ServiceProvider scope = services.BuildServiceProvider();
        scope.GetRequiredService<ISecretProvider>().Should().BeOfType<LocalSecretProvider>();

        Action invalidPrefix = () => new ServiceCollection().AddAzureKeyVaultSecrets(configuration, options =>
        {
            options.UseLocalSecrets = true;
            options.LocalSecretsPrefix = "Invalid::Prefix";
        });
        invalidPrefix.Should().Throw<ArgumentException>().WithParameterName("LocalSecretsPrefix");
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("relative")]
    [DataRow("http://vault.example/")]
    [DataRow("https://user:password@vault.example/")]
    [DataRow("https://vault.example/?query=1")]
    [DataRow("https://vault.example/#fragment")]
    [DataRow("https://vault.example/secrets/")]
    public void RemoteModeRejectsMissingOrInvalidVaultUriWithoutRegisteringFallback(string? uri)
    {
        using var configuration = new ConfigurationManager();
        var services = new ServiceCollection();
        Action action = () => services.AddAzureKeyVaultSecrets(configuration,
            options => options.VaultUri = uri is null ? null : new Uri(uri, UriKind.RelativeOrAbsolute));

        action.Should().Throw<ArgumentException>().WithParameterName("options");
        services.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("negative-cache")]
    [DataRow("excess-cache")]
    [DataRow("negative-retries")]
    [DataRow("excess-retries")]
    [DataRow("negative-delay")]
    [DataRow("excess-delay")]
    public void RemoteModeRejectsInvalidCacheAndRetryOptions(string invalid)
    {
        using var configuration = new ConfigurationManager();
        Action action = () => new ServiceCollection().AddAzureKeyVaultSecrets(configuration, options =>
        {
            options.VaultUri = new Uri("https://vault.example/");
            switch (invalid)
            {
                case "negative-cache": options.CacheTtl = TimeSpan.FromTicks(-1); break;
                case "excess-cache": options.CacheTtl = TimeSpan.FromDays(1).Add(TimeSpan.FromTicks(1)); break;
                case "negative-retries": options.MaxRetries = -1; break;
                case "excess-retries": options.MaxRetries = 11; break;
                case "negative-delay": options.RetryDelay = TimeSpan.FromTicks(-1); break;
                case "excess-delay": options.RetryDelay = TimeSpan.FromSeconds(30).Add(TimeSpan.FromTicks(1)); break;
            }
        });

        action.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("options");
    }

    [TestMethod]
    public void DirectRemoteProviderRejectsLocalModeAndValidatesOptionalUri()
    {
        using var context = new KeyVaultTestContext();
        context.Options.UseLocalSecrets = true;
        Action localMode = () => context.Create();
        localMode.Should().Throw<ArgumentException>();

        context.Options.UseLocalSecrets = false;
        context.Options.VaultUri = new Uri("http://vault.example/");
        Action invalidUri = () => context.Create();
        invalidUri.Should().Throw<ArgumentException>();

        context.Options.VaultUri = new Uri("https://vault.example/");
        context.Create().Should().NotBeNull();
    }

    [TestMethod]
    public void DefaultRemoteRegistrationCreatesSdkClientAndNoninteractiveCredentialWithoutNetwork()
    {
        using var configuration = new ConfigurationManager();
        configuration["KeyVault:VaultUri"] = "https://vault.example/";
        var services = new ServiceCollection();
        services.AddAzureKeyVaultSecrets(configuration);
        using ServiceProvider scope = services.BuildServiceProvider();

        scope.GetRequiredService<TokenCredential>().Should().BeOfType<DefaultAzureCredential>();
        scope.GetRequiredService<SecretClient>().VaultUri.Should().Be(new Uri("https://vault.example/"));
        scope.GetRequiredService<TimeProvider>().Should().BeSameAs(TimeProvider.System);
        scope.GetRequiredService<ISecretProvider>().Should().BeOfType<KeyVaultSecretProvider>();
        scope.GetRequiredService<ISecretProvider>().Should().BeSameAs(scope.GetRequiredService<ISecretProvider>());
        SecretClientOptions options = scope.GetRequiredService<SecretClientOptions>();
        options.Retry.MaxRetries.Should().Be(3);
        options.Retry.Delay.Should().Be(TimeSpan.FromSeconds(1));
        options.Retry.Mode.Should().Be(RetryMode.Exponential);
        options.Retry.MaxDelay.Should().Be(TimeSpan.FromSeconds(30));
        options.Retry.NetworkTimeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [TestMethod]
    public async Task InjectionAndOptionsOverrideArePreservedWithoutMutableRegistrationLeaks()
    {
        using var configuration = new ConfigurationManager();
        configuration["KeyVault:VaultUri"] = "https://vault.example/";
        configuration["KeyVault:MaxRetries"] = "1";
        var client = new FakeSecretClient();
        var clock = new ManualTimeProvider();
        var logger = new RecordingSecretLogger();
        var services = new ServiceCollection();
        services.AddSingleton<SecretClient>(client);
        services.AddSingleton<TimeProvider>(clock);
        services.AddSingleton<ILogger<KeyVaultSecretProvider>>(logger);
        KeyVaultOptions? captured = null;
        services.AddAzureKeyVaultSecrets(configuration, options =>
        {
            options.MaxRetries = 10;
            options.RetryDelay = TimeSpan.FromSeconds(30);
            options.CacheTtl = TimeSpan.FromDays(1);
            captured = options;
        });
        captured!.UseLocalSecrets = true;
        using ServiceProvider scope = services.BuildServiceProvider();
        scope.GetRequiredService<IOptions<KeyVaultOptions>>().Value.CacheTtl = TimeSpan.Zero;

        scope.GetRequiredService<SecretClient>().Should().BeSameAs(client);
        scope.GetRequiredService<TimeProvider>().Should().BeSameAs(clock);
        ISecretProvider provider = scope.GetRequiredService<ISecretProvider>();
        (await provider.GetAsync("name")).Should().Be("secret");
        (await provider.GetAsync("name")).Should().Be("secret");
        client.Calls.Should().ContainSingle();
        scope.GetRequiredService<SecretClientOptions>().Retry.MaxRetries.Should().Be(10);
    }

    [TestMethod]
    [DataRow(503, 2, 3, true)]
    [DataRow(503, 1, 2, false)]
    [DataRow(404, 2, 1, false)]
    [DataRow(403, 2, 1, false)]
    [DataRow(429, 2, 3, true)]
    public async Task SdkAloneOwnsBoundedRetriesWithInMemoryHttpTransport(int status, int retries, int expectedCalls, bool success)
    {
        using var configuration = new ConfigurationManager();
        using var handler = new ScriptedHandler(status);
        using var httpClient = new HttpClient(handler);
        var credential = new TestCredential();
        var services = new ServiceCollection();
        services.AddSingleton<TokenCredential>(credential);
        services.AddAzureKeyVaultSecrets(configuration, options =>
        {
            options.VaultUri = new Uri("https://vault.example/");
            options.MaxRetries = retries;
            options.RetryDelay = TimeSpan.Zero;
            options.CacheTtl = TimeSpan.Zero;
        });
        using ServiceProvider scope = services.BuildServiceProvider();
        scope.GetRequiredService<SecretClientOptions>().Transport = new HttpClientTransport(httpClient);
        var provider = (KeyVaultSecretProvider)scope.GetRequiredService<ISecretProvider>();

        if (success)
        {
            (await provider.GetAsync("name")).Should().Be("transport-secret");
        }
        else if (status == 404)
        {
            (await provider.GetAsync("name")).Should().BeNull();
        }
        else
        {
            Func<Task> action = () => provider.GetAsync("name");
            (await action.Should().ThrowAsync<RequestFailedException>()).Which.Status.Should().Be(status);
        }

        handler.Calls.Should().Be(expectedCalls);
        scope.GetRequiredService<TokenCredential>().Should().BeSameAs(credential);
    }

    [TestMethod]
    public async Task NullRegistrationReturnsSingleton()
    {
        var services = new ServiceCollection();
        services.AddNullSecrets().Should().BeSameAs(services);
        using ServiceProvider scope = services.BuildServiceProvider();
        ISecretProvider provider = scope.GetRequiredService<ISecretProvider>();
        provider.Should().BeSameAs(NullSecretProvider.Instance);
        (await provider.GetAsync("name")).Should().BeNull();
    }

    [TestMethod]
    public void LegacySecretOptionsRecordRetainsDefaultsAndValueSemantics()
    {
        var defaults = new SecretOptions();
        defaults.KeyVaultUri.Should().BeNull();
        defaults.CacheTtl.Should().Be(TimeSpan.FromMinutes(5));
        defaults.UseLocalSecrets.Should().BeFalse();
        var changed = defaults with
        {
            KeyVaultUri = new Uri("https://vault.example/"), CacheTtl = TimeSpan.Zero, UseLocalSecrets = true
        };
        changed.KeyVaultUri.Should().Be(new Uri("https://vault.example/"));
        changed.CacheTtl.Should().Be(TimeSpan.Zero);
        changed.UseLocalSecrets.Should().BeTrue();
        changed.Should().NotBe(defaults);
        (changed with { }).Should().Be(changed);
        (changed with { }).GetHashCode().Should().Be(changed.GetHashCode());
    }

    private sealed class TestCredential : TokenCredential
    {
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken) =>
            new("test-token", DateTimeOffset.MaxValue);
        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken) =>
            ValueTask.FromResult(GetToken(requestContext, cancellationToken));
    }

    private sealed class ScriptedHandler(int failureStatus) : HttpMessageHandler
    {
        internal int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            bool successful = Calls > 2;
            var response = new HttpResponseMessage(successful ? HttpStatusCode.OK : (HttpStatusCode)failureStatus)
            {
                Content = new StringContent(successful
                    ? "{\"value\":\"transport-secret\",\"id\":\"https://vault.example/secrets/name/0123456789abcdef0123456789abcdef\",\"attributes\":{\"enabled\":true}}"
                    : "{\"error\":{\"code\":\"Failure\",\"message\":\"test\"}}", Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
