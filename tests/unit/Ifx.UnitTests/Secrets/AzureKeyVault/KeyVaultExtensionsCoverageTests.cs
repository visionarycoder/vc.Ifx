using Ifx.Secrets.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Ifx.Tests.Secrets.AzureKeyVault;

[TestClass]
public sealed class KeyVaultExtensionsCoverageTests
{
    [TestMethod]
    public void LocalProviderFactoryRejectsMissingAssemblyAndWrongProviderShape()
    {
        using var configuration = new ConfigurationManager();
        var options = new KeyVaultOptions { UseLocalSecrets = true, LocalSecretsPrefix = "Secrets" };

        Action missing = () => CreateLocalProvider(null, configuration, options);
        Action wrongShape = () => CreateLocalProvider(typeof(NotASecretProvider), configuration, options);

        missing.Should().Throw<InvalidOperationException>().WithMessage("*Ifx.Secrets.Local assembly*");
        wrongShape.Should().Throw<InvalidOperationException>().WithMessage("*could not create an ISecretProvider instance*");
    }

    [TestMethod]
    public void LocalProviderFactoryRethrowsInnerConstructionFailure()
    {
        using var configuration = new ConfigurationManager();
        var options = new KeyVaultOptions { UseLocalSecrets = true, LocalSecretsPrefix = "Secrets" };

        Action action = () => CreateLocalProvider(typeof(ThrowingProvider), configuration, options);

        action.Should().Throw<InvalidOperationException>().WithMessage("boom");
    }

    private static object? CreateLocalProvider(Type? providerType, IConfiguration configuration, KeyVaultOptions options)
    {
        try
        {
            return typeof(KeyVaultExtensions).GetMethod("CreateLocalProvider", BindingFlags.NonPublic | BindingFlags.Static, [typeof(Type), typeof(IConfiguration), typeof(KeyVaultOptions)])!
                .Invoke(null, [providerType, configuration, options]);
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    private sealed class NotASecretProvider
    {
        public NotASecretProvider(IConfiguration configuration, KeyVaultOptions options)
        {
        }
    }

    private sealed class ThrowingProvider
    {
        public ThrowingProvider(IConfiguration configuration, KeyVaultOptions options) =>
            throw new InvalidOperationException("boom");
    }
}
