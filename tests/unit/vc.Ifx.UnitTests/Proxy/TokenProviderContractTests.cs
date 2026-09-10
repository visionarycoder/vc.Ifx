using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Jwt;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Providers;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public sealed class TokenProviderContractTests
{
    private const string SigningKey = "a-development-test-key-at-least-32-bytes-long";

    [TestMethod]
    public async Task ValidationRequiresAuthenticSignatureInBothEntryPoints()
    {
        using var client = new HttpClient(new ReplyHandler());
        JwtOptions options = Options();
        var provider = Provider(client, options);
        string valid = Token(SigningKey, "issuer", "audience", DateTime.UtcNow.AddMinutes(5));
        provider.ValidateToken(valid).Should().BeTrue();
        (await provider.ValidateTokenAsync(valid)).Should().BeTrue();
        foreach (string invalid in new[]
        {
            "", "not-a-token",
            Token("another-untrusted-signing-key-at-least-32-bytes", "issuer", "audience", DateTime.UtcNow.AddMinutes(5)),
            Token(SigningKey, "other", "audience", DateTime.UtcNow.AddMinutes(5)),
            Token(SigningKey, "issuer", "other", DateTime.UtcNow.AddMinutes(5)),
            Token(SigningKey, "issuer", "audience", DateTime.UtcNow.AddMinutes(-5)),
            new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken("issuer", "audience", expires: DateTime.UtcNow.AddMinutes(5)))
        })
        {
            provider.ValidateToken(invalid).Should().BeFalse();
            (await provider.ValidateTokenAsync(invalid)).Should().BeFalse();
        }
        options.SigningKey = string.Empty;
        provider.ValidateToken(valid).Should().BeFalse();
        Func<Task> canceled = () => provider.ValidateTokenAsync(valid, new CancellationToken(true));
        await canceled.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public async Task OAuthScalarFieldsAndFormAreMappedWithoutMutatingClient()
    {
        string? form = null;
        var handler = new ReplyHandler(async (request, token) =>
        {
            request.RequestUri.Should().Be(new Uri("https://issuer.example/token"));
            request.Method.Should().Be(HttpMethod.Post);
            form = await request.Content!.ReadAsStringAsync(token);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"access_token\":\"token-value\",\"expires_in\":60,\"token_type\":\"Bearer\",\"refresh_token\":\"refresh-value\",\"scope\":\"read write\"}")
            };
        });
        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(7) };
        var provider = Provider(client, Options());
        var request = TokenRequest.CreateClientCredentials("client", "secret", ["read", "write"], "audience");
        TokenResult result = await provider.GetTokenAsync(request);
        result.IsSuccess.Should().BeTrue();
        result.AccessToken.Should().Be("token-value");
        result.ExpiresIn.Should().Be(60);
        result.RefreshToken.Should().Be("refresh-value");
        result.Scope.Should().Be("read write");
        form.Should().Contain("client_id=client").And.Contain("client_secret=secret").And.Contain("scope=read+write");
        client.Timeout.Should().Be(TimeSpan.FromSeconds(7));
        client.DefaultRequestHeaders.Should().BeEmpty();
        (await provider.GetTokenAsync()).Should().Be("token-value");
    }

    [TestMethod]
    public async Task OAuthErrorsMissingAccessTokensAndCancellationFailClosed()
    {
        foreach ((HttpStatusCode status, string body, string error) in new[]
        {
            (HttpStatusCode.BadRequest, "{\"error\":\"invalid_client\",\"error_description\":\"rejected\",\"error_uri\":\"https://issuer.example/errors\"}", "invalid_client"),
            (HttpStatusCode.OK, "{}", "unknown_error"),
            (HttpStatusCode.OK, "null", "unknown_error"),
            (HttpStatusCode.BadGateway, "not json", "unknown_error")
        })
        {
            using var client = new HttpClient(new ReplyHandler((request, token) => Task.FromResult(
                new HttpResponseMessage(status) { Content = new StringContent(body) })));
            var provider = Provider(client, Options());
            TokenResult result = await provider.GetTokenAsync(TokenRequest.CreateClientCredentials("client", "secret"));
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(error);
            Func<Task> get = () => provider.GetTokenAsync();
            await get.Should().ThrowAsync<InvalidOperationException>();
        }

        using var neverCalled = new HttpClient(new ReplyHandler());
        var instance = Provider(neverCalled, Options());
        Func<Task> canceledGet = () => instance.GetTokenAsync(TokenRequest.CreateClientCredentials("client", "secret"), new CancellationToken(true));
        await canceledGet.Should().ThrowAsync<OperationCanceledException>();
        Func<Task> canceledRefresh = () => instance.RefreshTokenAsync("refresh", new CancellationToken(true));
        await canceledRefresh.Should().ThrowAsync<OperationCanceledException>();
        (await instance.RefreshTokenAsync(" ")).Should().BeNull();
        (await instance.GetTokenAsync(new TokenRequest())).IsSuccess.Should().BeFalse();
    }

    [TestMethod]
    public async Task EndpointsEnforceHttpsUnlessExplicitlyDisabled()
    {
        using var client = new HttpClient(new ReplyHandler());
        JwtOptions options = Options();
        options.TokenEndpoint = "http://localhost/token";
        var provider = Provider(client, options);
        TokenRequest request = TokenRequest.CreateClientCredentials("client", "secret");
        (await provider.GetTokenAsync(request)).Error.Should().Be("request_error");
        options.RequireHttpsMetadata = false;
        (await provider.GetTokenAsync(request)).Error.Should().Be("unknown_error");
        options.TokenEndpoint = "file:///sensitive";
        (await provider.GetTokenAsync(request)).Error.Should().Be("request_error");
        options.TokenEndpoint = "not an endpoint";
        (await provider.GetTokenAsync(request)).Error.Should().Be("request_error");
    }

    [TestMethod]
    public void ExtractedClaimsAreInformationalAndPreserveRepeatedValues()
    {
        using var client = new HttpClient(new ReplyHandler());
        var provider = Provider(client, Options());
        provider.ExtractClaims("").Should().BeEmpty();
        provider.ExtractClaims("not-a-token").Should().BeEmpty();
        string token = Token(SigningKey, "issuer", "audience", DateTime.UtcNow.AddMinutes(5),
            [new Claim("role", "a"), new Claim("role", "b"), new Claim("role", "c")]);
        provider.ExtractClaims(token)["role"].Should().BeEquivalentTo(new[] { "a", "b", "c" });
    }

    [TestMethod]
    public async Task GrantSpecificFormsRefreshAndDefaultTokenTypeAreSupported()
    {
        var forms = new List<string>();
        using var client = new HttpClient(new ReplyHandler(async (request, token) =>
        {
            forms.Add(await request.Content!.ReadAsStringAsync(token));
            return new(HttpStatusCode.OK) { Content = new StringContent("{\"access_token\":\"access\",\"expires_in\":3600}") };
        }));
        var provider = Provider(client, Options());
        var password = await provider.GetTokenAsync(TokenRequest.CreatePasswordCredentials("client", "user", "pass"));
        password.IsSuccess.Should().BeTrue();
        password.TokenType.Should().Be("Bearer");
        forms[^1].Should().Contain("username=user").And.Contain("password=pass");
        (await provider.GetTokenAsync(new TokenRequest { GrantType = "authorization_code", ClientId = "client", AuthorizationCode = "code", RedirectUri = "https://callback.example/" })).IsSuccess.Should().BeTrue();
        forms[^1].Should().Contain("code=code").And.Contain("redirect_uri=");
        (await provider.RefreshTokenAsync("refresh-value"))!.IsSuccess.Should().BeTrue();
        forms[^1].Should().Contain("grant_type=refresh_token").And.Contain("refresh_token=refresh-value");
        (await provider.GetTokenAsync(new TokenRequest { GrantType = "custom", ClientId = "client", CustomParameters = new() { ["custom"] = "value" } })).IsSuccess.Should().BeTrue();
        forms[^1].Should().Contain("custom=value");
    }

    [TestMethod]
    public async Task AcquisitionCancellationTimeoutAndBadJsonHaveDistinctOutcomes()
    {
        using var client = new HttpClient(new ReplyHandler(async (request, token) =>
        {
            await Task.Delay(Timeout.Infinite, token);
            throw new AssertFailedException();
        }));
        var options = Options(); options.RequestTimeout = TimeSpan.FromMilliseconds(20);
        var provider = Provider(client, options);
        (await provider.GetTokenAsync(TokenRequest.CreateClientCredentials("client", "secret"))).Error.Should().Be("request_error");
        using var cancellation = new CancellationTokenSource(); cancellation.CancelAfter(5);
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.GetTokenAsync(TokenRequest.CreateClientCredentials("client", "secret"), cancellation.Token));
        using var refreshing = new CancellationTokenSource(); refreshing.CancelAfter(5);
        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.RefreshTokenAsync("refresh", refreshing.Token));
        using var badJson = new HttpClient(new ReplyHandler((request, token) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("broken-json") })));
        (await Provider(badJson, Options()).GetTokenAsync(TokenRequest.CreateClientCredentials("client", "secret"))).Error.Should().Be("request_error");
    }

    [TestMethod]
    public void ConstructorRejectsInvalidDependenciesAndTimeout()
    {
        using var client = new HttpClient(new ReplyHandler());
        Assert.ThrowsExactly<ArgumentNullException>(() => Provider(null!, Options()));
        Assert.ThrowsExactly<ArgumentNullException>(() => Provider(client, null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => new DefaultTokenProvider(client, Options(), null!));
        var options = Options(); options.RequestTimeout = TimeSpan.Zero;
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Provider(client, options));
    }

    private static JwtOptions Options() => new()
    {
        Authority = "https://issuer.example", Audience = "audience", Issuer = "issuer",
        SigningKey = SigningKey, ClientId = "client", ClientSecret = "secret", ClockSkew = TimeSpan.Zero
    };

    private static DefaultTokenProvider Provider(HttpClient client, JwtOptions options)
        => new(client, options, NullLogger<DefaultTokenProvider>.Instance);

    private static string Token(string key, string issuer, string audience, DateTime expiry, IEnumerable<Claim>? claims = null)
        => new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(issuer, audience, claims,
            expires: expiry, signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256)));

    private sealed class ReplyHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>? reply = null) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => reply?.Invoke(request, cancellationToken) ?? Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("{}") });
    }
}
