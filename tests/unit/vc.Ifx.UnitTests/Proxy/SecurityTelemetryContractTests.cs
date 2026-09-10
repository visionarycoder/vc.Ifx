using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Diagnostics;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Exceptions;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Providers;
using VisionaryCoder.Framework.Proxy.Interceptors.Security;
using VisionaryCoder.Framework.Proxy.Interceptors.Telemetry;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public sealed class SecurityTelemetryContractTests
{
    [TestMethod]
    [DoNotParallelize]
    public async Task TelemetryReportsResponseAndExceptionOutcomeWithAndWithoutListeners()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new TelemetryInterceptor(null!));
        foreach (bool listen in new[] { false, true })
        {
            using var source = new ActivitySource(Guid.NewGuid().ToString());
            using var listener = new ActivityListener
            {
                ShouldListenTo = candidate => listen && candidate.Name == source.Name,
                Sample = (ref ActivityCreationOptions<ActivityContext> creation) => ActivitySamplingResult.AllDataAndRecorded
            };
            List<Activity> completed = [];
            listener.ActivityStopped = activity => completed.Add(activity);
            ActivitySource.AddActivityListener(listener);
            var interceptor = new TelemetryInterceptor(NullLogger<TelemetryInterceptor>.Instance, source);
            Assert.AreEqual(-50, interceptor.Order);
            foreach (bool success in new[] { true, false })
            {
                var context = new ProxyContext { Request = success ? "request" : null, ResultType = success ? typeof(int) : null };
                context.Items["CorrelationId"] = success ? "corr" : null;
                var response = success ? ProxyResponse<int>.Success(1) : ProxyResponse<int>.Failure("failed");
                Assert.AreSame(response, await interceptor.InvokeAsync<int>(context, (current, token) => Task.FromResult(response)));
                if (listen)
                {
                    var activity = completed[^1];
                    Assert.AreEqual(success, activity.GetTagItem("proxy.success"));
                    Assert.AreEqual(success ? ActivityStatusCode.Unset : ActivityStatusCode.Error, activity.Status);
                    Assert.AreEqual(success ? "String" : "Unknown", activity.GetTagItem("proxy.request_type"));
                    Assert.IsNotNull(activity.GetTagItem("proxy.duration_ms"));
                }
            }
            var exception = new InvalidOperationException("test failure");
            Assert.AreSame(exception, await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => interceptor.InvokeAsync<int>(new(), (context, token) => throw exception)));
            if (listen)
            {
                Assert.AreEqual(false, completed[^1].GetTagItem("proxy.success"));
                Assert.AreEqual(ActivityStatusCode.Error, completed[^1].Status);
                Assert.AreEqual(exception.Message, completed[^1].GetTagItem("proxy.error"));
            }
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => interceptor.InvokeAsync<int>(null!, (context, token) => throw new AssertFailedException()));
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => interceptor.InvokeAsync<int>(new(), null!));
            await Assert.ThrowsAsync<OperationCanceledException>(() => interceptor.InvokeAsync<int>(new(), (context, token) => throw new AssertFailedException(), new(true)));
        }
        var defaultSource = new TelemetryInterceptor(NullLogger<TelemetryInterceptor>.Instance);
        await defaultSource.InvokeAsync<int>(new(), (context, token) => Task.FromResult(ProxyResponse<int>.Success(1)));
    }

    [TestMethod]
    public async Task SecurityRunsEnrichersBeforePoliciesAndFailsClosedWithoutSwallowingCancellation()
    {
        var logger = NullLogger<SecurityInterceptor>.Instance;
        Assert.ThrowsExactly<ArgumentNullException>(() => new SecurityInterceptor(null!, [], []));
        Assert.ThrowsExactly<ArgumentNullException>(() => new SecurityInterceptor(logger, null!, []));
        Assert.ThrowsExactly<ArgumentNullException>(() => new SecurityInterceptor(logger, [], null!));
        List<string> calls = [];
        var enricher = new Mock<IProxySecurityEnricher>();
        enricher.Setup(value => value.EnrichAsync(It.IsAny<ProxyContext>(), It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("enrich")).Returns(Task.CompletedTask);
        var policy = new Mock<IProxyAuthorizationPolicy>();
        policy.Setup(value => value.IsAuthorizedAsync(It.IsAny<ProxyContext>(), It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("authorize")).ReturnsAsync(true);
        var interceptor = new SecurityInterceptor(logger, [enricher.Object], [policy.Object]);
        Assert.AreEqual(-200, interceptor.Order);
        using var cancellation = new CancellationTokenSource();
        var response = ProxyResponse<int>.Success(7);
        Assert.AreSame(response, await interceptor.InvokeAsync<int>(new() { Request = "request" }, (context, token) =>
        {
            Assert.AreEqual(cancellation.Token, token); calls.Add("next"); return Task.FromResult(response);
        }, cancellation.Token));
        CollectionAssert.AreEqual(new[] { "enrich", "authorize", "next" }, calls);
        policy.Setup(value => value.IsAuthorizedAsync(It.IsAny<ProxyContext>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        Assert.IsFalse((await interceptor.InvokeAsync<int>(new(), (context, token) => throw new AssertFailedException())).IsSuccess);
        enricher.Setup(value => value.EnrichAsync(It.IsAny<ProxyContext>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException());
        Assert.IsFalse((await interceptor.InvokeAsync<int>(new(), (context, token) => throw new AssertFailedException())).IsSuccess);
        var empty = new SecurityInterceptor(logger, [], []);
        var business = new BusinessException("business");
        Assert.AreSame(business, await Assert.ThrowsExactlyAsync<BusinessException>(() => empty.InvokeAsync<int>(new(), (context, token) => throw business)));
        await Assert.ThrowsAsync<OperationCanceledException>(() => empty.InvokeAsync<int>(new(), (context, token) => throw new OperationCanceledException()));
        await Assert.ThrowsAsync<OperationCanceledException>(() => empty.InvokeAsync<int>(new(), (context, token) => throw new AssertFailedException(), new(true)));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => empty.InvokeAsync<int>(null!, (context, token) => throw new AssertFailedException()));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => empty.InvokeAsync<int>(new(), null!));
        enricher.Setup(value => value.EnrichAsync(It.IsAny<ProxyContext>(), It.IsAny<CancellationToken>())).Callback(cancellation.Cancel).Returns(Task.CompletedTask);
        await Assert.ThrowsAsync<OperationCanceledException>(() => interceptor.InvokeAsync<int>(new(), (context, token) => throw new AssertFailedException(), cancellation.Token));
        using var canceledPolicy = new CancellationTokenSource();
        policy.Setup(value => value.IsAuthorizedAsync(It.IsAny<ProxyContext>(), It.IsAny<CancellationToken>())).Callback(canceledPolicy.Cancel).ReturnsAsync(false);
        await Assert.ThrowsAsync<OperationCanceledException>(() => new SecurityInterceptor(logger, [], [policy.Object]).InvokeAsync<int>(new(), (context, token) => throw new AssertFailedException(), canceledPolicy.Token));
    }

    [TestMethod]
    public async Task IdentityEnrichersHandleMissingValuesAndLogicalHeaderReplacement()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new UserContextEnricher(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => new TenantContextEnricher(null!));
        var users = new Mock<IUserContextProvider>();
        var tenants = new Mock<ITenantContextProvider>();
        var userEnricher = new UserContextEnricher(users.Object);
        var tenantEnricher = new TenantContextEnricher(tenants.Object);
        Assert.AreEqual(100, userEnricher.Order);
        Assert.AreEqual(200, tenantEnricher.Order);
        var context = new ProxyContext();
        foreach (UserContext? user in new UserContext?[] { null, new(), new() { UserId = "id" }, new() { UserId = "id", UserName = "name", Email = "email" } })
        {
            users.Setup(value => value.GetCurrentUserAsync(It.IsAny<CancellationToken>())).ReturnsAsync(user);
            await userEnricher.EnrichAsync(context);
        }
        Assert.AreEqual("id", context.Metadata["UserId"]);
        Assert.AreEqual("name", context.Metadata["UserName"]);
        Assert.AreEqual("email", context.Metadata["UserEmail"]);
        context.Headers = new() { ["x-tenant-id"] = "old", ["X-TENANT-ID"] = "duplicate", ["Other"] = "retained" };
        foreach (TenantContext? tenant in new TenantContext?[] { null, new(), new() { TenantId = "tenant" }, new() { TenantId = "tenant", TenantName = "name" } })
        {
            tenants.Setup(value => value.GetTenantContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
            await tenantEnricher.EnrichAsync(context);
        }
        Assert.AreEqual("tenant", context.Metadata["TenantId"]);
        Assert.AreEqual("name", context.Metadata["TenantName"]);
        Assert.AreEqual(2, context.Headers.Count);
        Assert.AreEqual("tenant", context.Headers["X-Tenant-ID"]);
        context.Headers = null!;
        await tenantEnricher.EnrichAsync(context);
        Assert.AreEqual("tenant", context.Headers!["X-Tenant-ID"]);
    }
}
