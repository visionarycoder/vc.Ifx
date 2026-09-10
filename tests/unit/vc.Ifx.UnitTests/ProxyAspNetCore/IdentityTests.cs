using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Security.Claims;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Providers;

namespace VisionaryCoder.Framework.Tests.ProxyAspNetCore;

[TestClass]
public class IdentityTests
{
    internal static DefaultHttpContext Context(params Claim[] claims) => new()
    {
        User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
    };

    internal static DefaultHttpContext Authenticated(string id = "alice", string tenant = "acme") =>
        Context(new Claim("sub", id), new Claim("tenant_id", tenant));

    private static UserProbe User(HttpContext? context) => new(new HttpContextAccessor { HttpContext = context });
    private static TenantProbe Tenant(HttpContext? context) => new(new HttpContextAccessor { HttpContext = context });

    [TestMethod]
    public async Task AnonymousAndAbsentRequestsNeverValidateOrSelectTenants()
    {
        foreach (HttpContext? context in new HttpContext?[] { null, new DefaultHttpContext(), Context() })
        {
            UserProbe user = User(context);
            UserContext anonymous = (await user.GetCurrentUserAsync())!;
            anonymous.UserId.Should().Be("anonymous");
            (await user.GetUserAsync("anonymous")).Should().BeNull();
            (await user.ValidateUserContextAsync(anonymous)).Should().BeFalse();
            (await user.ValidateUserContextAsync(null!)).Should().BeFalse();
            user.HasPermission("read").Should().BeFalse();
            user.HasPermission("").Should().BeFalse();
            user.IsInRole("Admin").Should().BeFalse();
            TenantProbe tenant = Tenant(context);
            tenant.GetTenantId().Should().BeNull();
            (await tenant.GetTenantIdAsync()).Should().BeNull();
            TenantContext fallback = (await tenant.GetTenantContextAsync())!;
            fallback.TenantId.Should().Be("default");
            fallback.TenantName.Should().Be("Default");
            fallback.IsActive.Should().BeFalse();
            (await tenant.GetTenantContextAsync("default")).Should().BeNull();
            (await tenant.ValidateTenantContextAsync(fallback)).Should().BeFalse();
            (await tenant.ValidateTenantContextAsync(new TenantContext { IsActive = true })).Should().BeFalse();
            (await tenant.ValidateTenantContextAsync(null!)).Should().BeFalse();
            tenant.SwitchTenant("acme").Should().BeFalse();
        }
    }

    [TestMethod]
    public async Task AuthenticatedSnapshotUsesClaimsAndSafeMetadataOnly()
    {
        DefaultHttpContext context = Context(new Claim(ClaimTypes.NameIdentifier, "alice"), new Claim("sub", "alice"),
            new Claim(ClaimTypes.Name, "Alice"), new Claim("email", "a@example.test"),
            new Claim(ClaimTypes.Role, "Admin"), new Claim("role", "Admin"), new Claim("roles", "Editor"),
            new Claim("permission", "read"), new Claim("permissions", "write"), new Claim("given_name", "A"),
            new Claim("family_name", "B"), new Claim("tid", "acme"), new Claim("cid", "trusted"));
        context.User.AddIdentity(new ClaimsIdentity([new Claim("role", "Root"), new Claim("permission", "delete")]));
        context.Request.Headers["X-Correlation-ID"] = "untrusted";
        context.Request.Headers["User-Agent"] = "Agent";
        context.Request.Headers["X-User-Timezone"] = "UTC";
        context.Request.Headers["X-User-Locale"] = "en";
        context.Request.Headers["X-Forwarded-For"] = "1.2.3.4";
        context.Connection.RemoteIpAddress = IPAddress.Loopback;
        UserProbe provider = User(context);
        UserContext user = (await provider.GetCurrentUserAsync())!;
        user.UserId.Should().Be("alice");
        user.UserName.Should().Be("Alice");
        user.Email.Should().Be("a@example.test");
        user.Roles.Should().BeEquivalentTo("Admin", "Editor");
        user.Permissions.Should().BeEquivalentTo("read", "write");
        user.Claims["CorrelationId"].Should().Be("trusted");
        user.Claims["FirstName"].Should().Be("A");
        user.Claims["LastName"].Should().Be("B");
        user.Claims["ClientIP"].Should().Be("127.0.0.1");
        user.Claims["Timezone"].Should().Be("UTC");
        user.Claims["Locale"].Should().Be("en");
        user.AuthenticatedAt.Should().BeBefore(DateTimeOffset.UtcNow.AddSeconds(1));
        provider.IsInRole("Admin").Should().BeTrue();
        provider.IsInRole("admin").Should().BeFalse();
        provider.IsInRole("").Should().BeFalse();
        provider.IsInRole(null!).Should().BeFalse();
        provider.HasPermission("read").Should().BeTrue();
        provider.HasPermission("READ").Should().BeFalse();
        provider.HasPermission("delete").Should().BeFalse();
        (await provider.GetUserAsync("alice")).Should().NotBeNull();
        (await provider.GetUserAsync("bob")).Should().BeNull();
        (await provider.ValidateUserContextAsync(user)).Should().BeTrue();
        user.Claims["TenantId"] = "evil";
        (await provider.ValidateUserContextAsync(user)).Should().BeFalse();
        user.Claims["TenantId"] = "acme";
        user.Permissions.Add("delete");
        (await provider.ValidateUserContextAsync(user)).Should().BeFalse();
        user.Permissions.Remove("delete");
        user.Roles.Add("Root");
        (await provider.ValidateUserContextAsync(user)).Should().BeFalse();
        user.Roles.Remove("Root");
        user.UserId = "bob";
        (await provider.ValidateUserContextAsync(user)).Should().BeFalse();
        user.Claims["authenticated"] = false;
        (await provider.ValidateUserContextAsync(user)).Should().BeFalse();
        user.Claims["authenticated"] = "true";
        (await provider.ValidateUserContextAsync(user)).Should().BeFalse();
        user.Claims.Remove("authenticated");
        (await provider.ValidateUserContextAsync(user)).Should().BeFalse();
    }

    [TestMethod]
    public async Task ClaimAmbiguityFailsClosedAndNamesHaveSafeFallbacks()
    {
        DefaultHttpContext context = Authenticated();
        context.User.AddIdentity(new ClaimsIdentity([new Claim("sub", "other")], "Second"));
        (await User(context).GetCurrentUserAsync())!.UserId.Should().Be("anonymous");
        Tenant(context).GetTenantId().Should().BeNull();
        context = Context(new Claim("sub", "alice"), new Claim("user_id", "bob"));
        await Assert.ThrowsExactlyAsync<UnauthorizedAccessException>(() => User(context).GetCurrentUserAsync());
        context = Context(new Claim("sub", "alice"), new Claim("tenant_id", "acme"), new Claim("tid", "evil"));
        await Assert.ThrowsExactlyAsync<UnauthorizedAccessException>(() => User(context).GetCurrentUserAsync());
        await Assert.ThrowsExactlyAsync<UnauthorizedAccessException>(() => Tenant(context).GetTenantIdAsync());
        context = Context(new Claim("sub", "alice"), new Claim("name", "Fallback"));
        (await User(context).GetCurrentUserAsync())!.UserName.Should().Be("Fallback");
        context = Context(new Claim("sub", "alice"), new Claim("preferred_username", "Preferred"));
        (await User(context).GetCurrentUserAsync())!.UserName.Should().Be("Preferred");
        context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "alice"), new Claim("custom-role", "Custom")],
            "Test", "custom-name", "custom-role"));
        User(context).IsInRole("Custom").Should().BeTrue();
        context = Authenticated();
        (await User(context).GetCurrentUserAsync())!.UserName.Should().BeEmpty();
    }

    [TestMethod]
    public async Task OptionalHeadersRequireOneBoundedControlFreeValue()
    {
        foreach (StringValues value in new StringValues[] { StringValues.Empty, new[] { "one", "two" }, "", " ", "bad\r\nvalue", new string('a', 513), "good" })
        {
            DefaultHttpContext context = Authenticated();
            context.Request.Headers["X-Correlation-ID"] = value;
            UserContext user = (await User(context).GetCurrentUserAsync())!;
            user.Claims.ContainsKey("CorrelationId").Should().Be(value == "good");
            user.Claims["ClientIP"].Should().Be("Unknown");
        }
    }

    [TestMethod]
    public async Task TenantClaimsAreAuthorityNotRequestHintsOrItems()
    {
        DefaultHttpContext context = Authenticated();
        context.Request.Headers["X-Tenant-ID"] = "evil";
        context.Request.Headers["X-Correlation-ID"] = "trace";
        context.Request.Path = "/tenant/evil";
        context.Request.Host = new HostString("evil.example.test");
        context.Items["CurrentTenantId"] = "evil";
        TenantProbe provider = Tenant(context);
        provider.GetTenantId().Should().Be("acme");
        (await provider.GetTenantIdAsync()).Should().Be("acme");
        TenantContext tenant = (await provider.GetTenantContextAsync("acme"))!;
        tenant.IsActive.Should().BeTrue();
        tenant.TenantName.Should().Be("acme");
        tenant.Settings["CorrelationId"].Should().Be("trace");
        tenant.Settings["RequestPath"].Should().Be("/tenant/evil");
        (await provider.GetTenantContextAsync("evil")).Should().BeNull();
        (await provider.ValidateTenantContextAsync(tenant)).Should().BeTrue();
        tenant.TenantId = "evil";
        (await provider.ValidateTenantContextAsync(tenant)).Should().BeFalse();
        provider.SwitchTenant("evil").Should().BeFalse();
        provider.SwitchTenant("bad/id").Should().BeFalse();
        provider.SwitchTenant("acme").Should().BeTrue();
        context.Response.Headers["X-Current-Tenant"].ToString().Should().Be("acme");
        provider.Header(context).Should().BeNull();
        provider.Path(context).Should().BeNull();
        provider.Host(context).Should().BeNull();
        context.Request.Headers["X-Tenant-ID"] = "acme";
        context.Request.Path = "/tenant/acme/x";
        context.Request.Host = new HostString("acme.example.test");
        provider.Header(context)!.Settings["Source"].Should().Be("Header");
        provider.Path(context)!.Settings["Source"].Should().Be("Path");
        provider.Host(context)!.Settings["Source"].Should().Be("Subdomain");
        context.Request.Path = "/t/acme";
        provider.Path(context).Should().NotBeNull();
        foreach (string path in new[] { "", "/", "/acme", "/api/acme", "/nested/tenant/acme" })
        {
            context.Request.Path = path;
            provider.Path(context).Should().BeNull();
        }
        context.Request.Host = new HostString("localhost");
        provider.Host(context).Should().BeNull();
        context.Request.Path = default;
        provider.Path(context).Should().BeNull();
        (await provider.GetCurrentTenantAsync()).Settings["RequestPath"].Should().Be("");
        context.User = new ClaimsPrincipal();
        provider.Header(context).Should().BeNull();
        context = Context(new Claim("tenantid", "acme"), new Claim("tenant_name", "Acme Inc"));
        (await Tenant(context).GetCurrentTenantAsync()).TenantName.Should().Be("Acme Inc");
    }

    [TestMethod]
    public void TenantSyntaxAndStartedResponseAreValidated()
    {
        TenantProbe provider = Tenant(null);
        foreach (string? id in new[] { null, "", " ", "a/b", "a\n", "é", new string('a', 129) })
        {
            provider.IsTenantValid(id!).Should().BeFalse();
            provider.Potential(id!).Should().BeFalse();
        }
        foreach (string id in new[] { "a", "A-1_b", Guid.NewGuid().ToString(), new string('a', 128) })
            provider.Potential(id).Should().BeTrue();
        DefaultHttpContext context = Authenticated();
        context.Features.Set<IHttpResponseFeature>(new StartedResponse());
        Tenant(context).SwitchTenant("acme").Should().BeFalse();
    }

    [TestMethod]
    public async Task CancellationAndEnrichmentFailuresPropagateWithoutAmbientReuse()
    {
        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        DefaultHttpContext context = Authenticated();
        UserProbe user = User(context);
        TenantProbe tenant = Tenant(context);
        await Assert.ThrowsAsync<OperationCanceledException>(() => user.GetUserAsync("alice", canceled.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => user.ValidateUserContextAsync(null!, canceled.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => tenant.GetTenantIdAsync(canceled.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => tenant.GetTenantContextAsync("acme", canceled.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => tenant.ValidateTenantContextAsync(null!, canceled.Token));
        context.RequestAborted = canceled.Token;
        await Assert.ThrowsAsync<OperationCanceledException>(() => user.GetCurrentUserAsync());
        await Assert.ThrowsAsync<OperationCanceledException>(() => tenant.GetCurrentTenantAsync());
        context.RequestAborted = default;
        using var midFlight = new CancellationTokenSource();
        user.Enrichment = (value, token) => { midFlight.Cancel(); return Task.CompletedTask; };
        await Assert.ThrowsAsync<OperationCanceledException>(() => user.GetCurrentUserAsync(midFlight.Token));
        using var tenantFlight = new CancellationTokenSource();
        tenant.Enrichment = (value, token) => { tenantFlight.Cancel(); return Task.CompletedTask; };
        await Assert.ThrowsAsync<OperationCanceledException>(() => tenant.GetCurrentTenantAsync(tenantFlight.Token));
        user.Enrichment = (value, token) => throw new InvalidOperationException("failure");
        tenant.Enrichment = (value, token) => throw new OperationCanceledException();
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => user.GetCurrentUserAsync());
        await Assert.ThrowsAsync<OperationCanceledException>(() => tenant.GetCurrentTenantAsync());
        var accessor = new HttpContextAccessor { HttpContext = context };
        user = new UserProbe(accessor);
        user.Enrichment = async (value, token) =>
        {
            accessor.HttpContext = Authenticated("bob", "other");
            await Task.Yield();
            value.UserId.Should().Be("alice");
        };
        (await user.GetCurrentUserAsync())!.UserId.Should().Be("alice");
        accessor.HttpContext = null;
    }

    [TestMethod]
    public void ConstructorsRejectNullDependencies()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new DefaultUserContextProvider(null!, NullLogger<DefaultUserContextProvider>.Instance));
        Assert.ThrowsExactly<ArgumentNullException>(() => new DefaultUserContextProvider(new HttpContextAccessor(), null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => new DefaultTenantContextProvider(null!, NullLogger<DefaultTenantContextProvider>.Instance));
        Assert.ThrowsExactly<ArgumentNullException>(() => new DefaultTenantContextProvider(new HttpContextAccessor(), null!));
    }

    private sealed class UserProbe(IHttpContextAccessor accessor) : DefaultUserContextProvider(accessor, NullLogger<DefaultUserContextProvider>.Instance)
    {
        internal Func<UserContext, CancellationToken, Task>? Enrichment { get; set; }
        protected override Task EnrichUserContextAsync(UserContext userContext, CancellationToken cancellationToken) =>
            Enrichment is null ? base.EnrichUserContextAsync(userContext, cancellationToken) : Enrichment(userContext, cancellationToken);
    }

    private sealed class TenantProbe(IHttpContextAccessor accessor) : DefaultTenantContextProvider(accessor, NullLogger<DefaultTenantContextProvider>.Instance)
    {
        internal Func<TenantContext, CancellationToken, Task>? Enrichment { get; set; }
        internal TenantContext? Header(HttpContext context) => ExtractTenantFromHeaders(context);
        internal TenantContext? Path(HttpContext context) => ExtractTenantFromPath(context);
        internal TenantContext? Host(HttpContext context) => ExtractTenantFromSubdomain(context);
        internal bool Potential(string value) => IsPotentialTenantId(value);
        protected override Task EnrichTenantContextAsync(TenantContext tenantContext, CancellationToken cancellationToken) =>
            Enrichment is null ? base.EnrichTenantContextAsync(tenantContext, cancellationToken) : Enrichment(tenantContext, cancellationToken);
    }

    private sealed class StartedResponse : HttpResponseFeature
    {
        public override bool HasStarted => true;
    }
}
