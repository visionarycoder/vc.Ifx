using System.Collections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Ifx.Extensions;
using Ifx.Pagination;
using Ifx.Secrets;
using Ifx.Errors;

namespace Ifx.Tests.Aggregator;

[TestClass]
public sealed class CompatibilityContractTests
{
    [TestMethod]
    public void ErrorsReadOnlyCollectionReflectsRecoverabilityOfContainedErrors()
    {
        var empty = new ErrorCollection([]);
        Assert.IsFalse(empty.HasErrors);
        Assert.AreEqual(IsRecoverableError.Yes, empty.IsRecoverableError);
        Assert.AreEqual(0, empty.Count);

        var recoverable = new ErrorCollection([new Error("retry", Code<Error>.Create("temporary"), "try later", IsRecoverableError.Yes)]);
        Assert.IsTrue(recoverable.HasErrors);
        Assert.AreEqual(IsRecoverableError.Yes, recoverable.IsRecoverableError);
        CollectionAssert.AreEqual(new[] { "retry" }, recoverable.Select(e => e.Name).ToArray());
        CollectionAssert.AreEqual(new[] { "retry" }, ((IEnumerable)recoverable).Cast<Error>().Select(e => e.Name).ToArray());

        var mixed = new ErrorCollection([
            new Error("retry", Code<Error>.Create("temporary"), "try later", IsRecoverableError.Yes),
            new Error("stop", Code<Error>.Create("permanent"), "stop", IsRecoverableError.No)
        ]);
        Assert.AreEqual(2, mixed.Count);
        Assert.AreEqual(IsRecoverableError.No, mixed.IsRecoverableError);

        Assert.ThrowsExactly<ArgumentNullException>(() => new ErrorCollection(null!));
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
