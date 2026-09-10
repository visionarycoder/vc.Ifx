using System.Collections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VisionaryCoder.Framework.Extensions;
using VisionaryCoder.Framework.Pagination;
using VisionaryCoder.Framework.Secrets;
using Wa.Wsdot.Fin.Idl.Ifx.Errors;
using Wa.Wsdot.Fin.Idl.Ifx.Generics;

namespace VisionaryCoder.Framework.Tests.Aggregator;

[TestClass]
public sealed class CompatibilityContractTests
{
    [TestMethod]
    public void LegacyCollectionAddsTheProvidedItemsAndEnumeratesBothInterfaces()
    {
        var values = new GenericReadOnlyCollection<int>();
        values.Add(1);
        values.AddRange(new[] { 2, 3 });
        Assert.AreEqual(3, values.Count);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, values.ToArray());
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, ((IEnumerable)values).Cast<int>().ToArray());
        Assert.ThrowsExactly<ArgumentNullException>(() => values.AddRange(null!));
        var errors = new ErrorsReadOnlyCollection();
        Assert.IsFalse(errors.HasErrors);
        Assert.AreEqual(IsRecoverableError.Yes, errors.IsRecoverableError);
        errors.Add(new Error("retry", "temporary", "try later", IsRecoverableError.Yes));
        Assert.IsTrue(errors.HasErrors);
        Assert.AreEqual(IsRecoverableError.Yes, errors.IsRecoverableError);
        errors.Add(new Error("stop", "permanent", "stop", IsRecoverableError.No));
        Assert.AreEqual(IsRecoverableError.No, errors.IsRecoverableError);
    }

    [TestMethod]
    public async Task PagingRejectsOverflowAndCancellationBeforeExecutingTheStore()
    {
        IQueryable<int> source = new[] { 1 }.AsQueryable();
        await Assert.ThrowsExactlyAsync<OverflowException>(() => source.ToPageAsync(new PageRequest(int.MaxValue, 1000)));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => PageExtensions.ToPageAsync<int>(null!, new PageRequest()));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ToPageAsync(null!));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ToPageWithTokenAsync(new PageRequest(), null!));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => source.ToPageAsync(new PageRequest(), cancellation.Token));
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => source.ToPageWithTokenAsync(new PageRequest(),
            (query, token, size, ct) => throw new AssertFailedException(), cancellation.Token));
    }

    [TestMethod]
    public void ConnectionRegistrationPreservesNamedIsolationAndFailsForMissingSecrets()
    {
        IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:one"] = "first",
            ["ConnectionStrings:two"] = "second"
        }).Build();
        var services = new ServiceCollection();
        Assert.AreSame(services, services.AddConnectionString(config, "one"));
        Assert.AreSame(services, services.AddNamedConnectionString(config, "two", "named"));
        using (ServiceProvider provider = services.BuildServiceProvider())
        {
            Assert.AreEqual("first", provider.GetRequiredService<string>());
            Assert.AreEqual("second", provider.GetRequiredKeyedService<string>("named"));
        }
        Assert.ThrowsExactly<InvalidOperationException>(() => services.AddConnectionString(config, "missing"));
        Assert.ThrowsExactly<InvalidOperationException>(() => services.AddNamedConnectionString(config, "missing", "named"));
        var secret = new Mock<ISecretProvider>();
        secret.Setup(value => value.GetAsync("secret", It.IsAny<CancellationToken>())).ReturnsAsync("resolved");
        var secretServices = new ServiceCollection();
        secretServices.AddSingleton(secret.Object);
        secretServices.AddConnectionStringFromSecret("secret");
        using ServiceProvider secretProvider = secretServices.BuildServiceProvider();
        Assert.AreEqual("resolved", secretProvider.GetRequiredService<string>());
        var emptyServices = new ServiceCollection();
        emptyServices.AddSingleton(secret.Object);
        emptyServices.AddConnectionStringFromSecret("missing");
        using ServiceProvider emptyProvider = emptyServices.BuildServiceProvider();
        Assert.ThrowsExactly<InvalidOperationException>(() => emptyProvider.GetRequiredService<string>());
    }
}
