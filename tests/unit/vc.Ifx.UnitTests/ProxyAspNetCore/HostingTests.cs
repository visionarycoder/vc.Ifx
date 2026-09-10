using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Providers;

namespace VisionaryCoder.Framework.Tests.ProxyAspNetCore;

[TestClass]
public class HostingTests
{
    private const string SigningKey = "hosting-test-signing-key-at-least-32-bytes";

    [TestMethod]
    public async Task SignedRequestsRemainIsolatedAcrossConcurrentAsyncFlows()
    {
        var arrived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int count = 0;
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        IServiceCollection services = builder.Services;
        services.AddUserContext().AddTenantContext();
        services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions, SignedTestAuthentication>("Test", options => { });
        await using WebApplication app = builder.Build();
        app.UseAuthentication();
        app.Run(async context =>
        {
            IUserContextProvider users = context.RequestServices.GetRequiredService<IUserContextProvider>();
            ITenantContextProvider tenants = context.RequestServices.GetRequiredService<ITenantContextProvider>();
            string before = (await users.GetCurrentUserAsync(context.RequestAborted))!.UserId;
            if (context.Request.Path == "/concurrent")
            {
                if (Interlocked.Increment(ref count) == 2) arrived.TrySetResult();
                await arrived.Task.WaitAsync(TimeSpan.FromSeconds(10), context.RequestAborted);
            }
            await Task.Yield();
            string after = (await users.GetCurrentUserAsync(context.RequestAborted))!.UserId;
            TenantContext tenant = (await tenants.GetTenantContextAsync(context.RequestAborted))!;
            if (!tenant.IsActive) context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync($"{before}|{after}|{tenant.TenantId}|{tenant.IsActive}", context.RequestAborted);
        });
        await app.StartAsync();
        using HttpClient client = app.GetTestClient();
        using HttpRequestMessage alice = Request("alice", "acme", "/concurrent");
        using HttpRequestMessage bob = Request("bob", "beta", "/concurrent");
        alice.Headers.Add("X-Tenant-ID", "evil");
        Task<HttpResponseMessage> first = client.SendAsync(alice);
        Task<HttpResponseMessage> second = client.SendAsync(bob);
        using HttpResponseMessage firstResponse = await first;
        using HttpResponseMessage secondResponse = await second;
        (await firstResponse.Content.ReadAsStringAsync()).Should().Be("alice|alice|acme|True");
        (await secondResponse.Content.ReadAsStringAsync()).Should().Be("bob|bob|beta|True");
        foreach (HttpRequestMessage request in new[]
        {
            new HttpRequestMessage(HttpMethod.Get, "/tenant/evil"),
            Request("mallory", "evil", "/", "different-key-at-least-32-bytes-long"),
            Request("alice", "acme", "/", expired: true)
        })
        {
            using (request)
            {
                request.Headers.Add("X-Tenant-ID", "evil");
                using HttpResponseMessage response = await client.SendAsync(request);
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
                (await response.Content.ReadAsStringAsync()).Should().Be("anonymous|anonymous|default|False");
            }
        }
    }

    private static HttpRequestMessage Request(string user, string tenant, string path, string key = SigningKey, bool expired = false)
    {
        var token = new JwtSecurityToken("issuer", "audience", [new Claim("sub", user), new Claim("tenant_id", tenant)],
            DateTime.UtcNow.AddHours(-2), expired ? DateTime.UtcNow.AddHours(-1) : DateTime.UtcNow.AddHours(1),
            new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
        return request;
    }

    private sealed class SignedTestAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!AuthenticationHeaderValue.TryParse(Request.Headers.Authorization, out AuthenticationHeaderValue? header))
                return Task.FromResult(AuthenticateResult.NoResult());
            try
            {
                var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
                ClaimsPrincipal principal = handler.ValidateToken(header.Parameter, new TokenValidationParameters
                {
                    ValidIssuer = "issuer", ValidAudience = "audience", ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256], ClockSkew = TimeSpan.Zero,
                    RequireSignedTokens = true, ValidateLifetime = true
                }, out SecurityToken validated);
                validated.Should().NotBeNull();
                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, "Test")));
            }
            catch (SecurityTokenException)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid credential."));
            }
        }
    }
}
