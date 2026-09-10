using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Interceptors;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Jwt;
using VisionaryCoder.Framework.Secrets;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public sealed class OutboundAuthenticationContractTests
{
    [TestMethod]
    public async Task InvalidConfigurationAndCallerCancellationFailBeforeTransport()
    {
        var provider = new Mock<ITokenProvider>();
        var logger = NullLogger<JwtAuthenticationInterceptor>.Instance;
        Assert.ThrowsExactly<ArgumentNullException>(() => new JwtAuthenticationInterceptor(null!, logger, OAuthOptions()));
        Assert.ThrowsExactly<ArgumentNullException>(() => new JwtAuthenticationInterceptor(provider.Object, null!, OAuthOptions()));
        Assert.ThrowsExactly<ArgumentNullException>(() => new JwtAuthenticationInterceptor(provider.Object, logger, null!));
        Assert.ThrowsExactly<ArgumentException>(() => new JwtAuthenticationInterceptor(provider.Object, logger, new JwtOptions()));
        var incomplete = OAuthOptions();
        incomplete.ClientSecret = "";
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => new OAuthInterceptor(provider.Object, incomplete).InvokeAsync<int>(new(), Next));
        using var cancellation = new CancellationTokenSource();
        provider.Setup(x => x.GetTokenAsync(It.IsAny<TokenRequest>(), It.IsAny<CancellationToken>()))
            .Returns(() => { cancellation.Cancel(); throw new OperationCanceledException(cancellation.Token); });
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => new OAuthInterceptor(provider.Object, OAuthOptions()).InvokeAsync<int>(new(), Next, cancellation.Token));
        var interceptor = new OAuthInterceptor(provider.Object, OAuthOptions());
        Assert.IsFalse(interceptor.NeedsRefresh(TokenResult.Success("access", 600, refreshToken: "")));
        var secret = Secret("invalid");
        var storedLogger = NullLogger<KeyVaultJwtInterceptor>.Instance;
        var options = new KeyVaultJwtOptions { SecretName = "token" };
        Assert.ThrowsExactly<ArgumentNullException>(() => new KeyVaultJwtInterceptor(null!, storedLogger, options));
        Assert.ThrowsExactly<ArgumentNullException>(() => new KeyVaultJwtInterceptor(secret.Object, null!, options));
        Assert.ThrowsExactly<ArgumentNullException>(() => new KeyVaultJwtInterceptor(secret.Object, storedLogger, null!));
        foreach (var invalid in new[] { new KeyVaultJwtOptions(), new KeyVaultJwtOptions { SecretName = "token", HeaderName = "" }, new KeyVaultJwtOptions { SecretName = "token", RequestTimeout = TimeSpan.Zero } })
            Assert.ThrowsExactly<ArgumentException>(() => new KeyVaultJwtInterceptor(secret.Object, storedLogger, invalid));
        options.AutoRefresh = true;
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => new StoredInterceptor(secret.Object, options).InvokeAsync<int>(new(), Next));
        options.ValidateToken = false;
        options.IncludeMetadata = true;
        var context = new ProxyContext();
        await new StoredInterceptor(secret.Object, options).InvokeAsync<int>(context, Next);
        Assert.AreEqual("token", context.Headers["X-Token-Secret"]);
        Assert.IsFalse(context.Headers.ContainsKey("X-Correlation-ID"));
    }

    [TestMethod]
    public async Task RejectedSecretTokensAreNeverAttachedAndMissingPolicyIsNotSwallowed()
    {
        foreach (string? token in new[] { null, "", "not-a-token", StoredToken(expired: true), StoredToken(future: true) })
        {
            var secret = Secret(token);
            var options = new KeyVaultJwtOptions { SecretName = "token", FailOnError = false };
            var interceptor = new StoredInterceptor(secret.Object, options);
            var context = new ProxyContext();
            Func<Task> call = () => interceptor.InvokeAsync<int>(context, Next);
            await call.Should().ThrowAsync<InvalidOperationException>();
            context.Headers.Should().BeEmpty();
            options.FailOnMissingToken = false;
            (await interceptor.InvokeAsync<int>(context, Next)).Data.Should().Be(1);
            context.Headers.Should().BeEmpty();
        }
    }

    [TestMethod]
    public async Task StoredTokensUseMetadataAndRefreshOnlyAfterInspectionFailure()
    {
        string token = StoredToken();
        var options = new KeyVaultJwtOptions { SecretName = "token", IncludeMetadata = true, CorrelationId = "correlation" };
        var secret = Secret(token);
        var interceptor = new StoredInterceptor(secret.Object, options);
        var context = new ProxyContext();
        await interceptor.InvokeAsync<int>(context, Next);
        context.Headers["Authorization"].Should().Be("Bearer " + token);
        context.Headers["X-Token-Secret"].Should().Be("token");
        context.Headers["X-Correlation-ID"].Should().Be("correlation");
        interceptor.Format("Bearer " + token).Should().Be("Bearer " + token);
        interceptor.Inspect("Bearer " + token).Should().BeTrue();
        options.HeaderName = "X-Api-Key";
        interceptor.Format("opaque").Should().Be("opaque");
        options.ValidateToken = false;
        secret.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("opaque");
        await interceptor.InvokeAsync<int>(new ProxyContext(), Next);
        options.ValidateToken = true;
        options.AutoRefresh = true;
        options.RefreshSecretName = "refresh";
        var refreshed = new RefreshingStoredInterceptor(secret.Object, options, token);
        await refreshed.InvokeAsync<int>(new ProxyContext(), Next);
        var invalidRefresh = new RefreshingStoredInterceptor(secret.Object, options, "still-invalid");
        Func<Task> invalid = () => invalidRefresh.InvokeAsync<int>(new ProxyContext(), Next);
        await invalid.Should().ThrowAsync<InvalidOperationException>();
        Func<Task> unimplementedRefresh = () => interceptor.InvokeAsync<int>(new ProxyContext(), Next);
        await unimplementedRefresh.Should().ThrowAsync<InvalidOperationException>();
        options.CorrelationId = null;
        options.IncludeMetadata = false;
        options.ValidateToken = false;
        await interceptor.InvokeAsync<int>(new ProxyContext(), Next);
    }

    [TestMethod]
    public async Task SecretRetrievalFailureSwitchesAndCancellationAreIndependent()
    {
        foreach (Exception error in new Exception[] { new InvalidOperationException(), new TimeoutException(), new OperationCanceledException() })
        {
            var secret = Secret(null);
            secret.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(error);
            var options = new KeyVaultJwtOptions { SecretName = "token" };
            var interceptor = new StoredInterceptor(secret.Object, options);
            Func<Task> fail = () => interceptor.InvokeAsync<int>(new ProxyContext(), Next);
            await fail.Should().ThrowAsync<Exception>();
            options.FailOnError = false;
            options.FailOnTimeout = false;
            (await interceptor.InvokeAsync<int>(new ProxyContext(), Next)).Data.Should().Be(1);
        }
        using var cancellation = new CancellationTokenSource();
        var canceledSecret = Secret(null);
        canceledSecret.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(() => { cancellation.Cancel(); throw new OperationCanceledException(cancellation.Token); });
        var canceled = new StoredInterceptor(canceledSecret.Object, new KeyVaultJwtOptions { SecretName = "token" });
        Func<Task> pending = () => canceled.InvokeAsync<int>(new ProxyContext(), Next, cancellation.Token);
        await pending.Should().ThrowAsync<OperationCanceledException>();
        Func<Task> preCanceled = () => canceled.InvokeAsync<int>(new ProxyContext(), Next, new CancellationToken(true));
        await preCanceled.Should().ThrowAsync<OperationCanceledException>();
        Func<Task> refreshCanceled = () => canceled.Refresh(new CancellationToken(true));
        await refreshCanceled.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public async Task OAuthFailuresAreNotSwallowedByTheGenericErrorSwitch()
    {
        var provider = new Mock<ITokenProvider>();
        provider.Setup(x => x.GetTokenAsync(It.IsAny<TokenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TokenResult.Failure("rejected"));
        JwtOptions options = OAuthOptions();
        options.CustomProperties["FailOnError"] = false;
        var interceptor = new OAuthInterceptor(provider.Object, options);
        Func<Task> fail = () => interceptor.InvokeAsync<int>(new ProxyContext(), Next);
        await fail.Should().ThrowAsync<InvalidOperationException>();
        options.CustomProperties["FailOnTokenError"] = "false";
        await fail.Should().ThrowAsync<InvalidOperationException>();
        options.CustomProperties["FailOnTokenError"] = false;
        (await interceptor.InvokeAsync<int>(new ProxyContext(), Next)).Data.Should().Be(1);
        options.CustomProperties["FailOnTokenError"] = true;
        foreach (TokenResult invalid in new[] { TokenResult.Success("", 600), TokenResult.Success("expired", -1) })
        {
            provider.Setup(x => x.GetTokenAsync(It.IsAny<TokenRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(invalid);
            await fail.Should().ThrowAsync<InvalidOperationException>();
        }
    }

    [TestMethod]
    public async Task OAuthAddsFreshCredentialsAndRefreshesWhenSupported()
    {
        var provider = new Mock<ITokenProvider>();
        TokenResult result = TokenResult.Success("access", 600, scope: "read");
        result.CorrelationId = "correlation";
        provider.Setup(x => x.GetTokenAsync(It.IsAny<TokenRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(result);
        JwtOptions options = OAuthOptions();
        var interceptor = new OAuthInterceptor(provider.Object, options);
        var context = new ProxyContext();
        await interceptor.InvokeAsync<int>(context, Next);
        context.Headers["Authorization"].Should().Be("Bearer access");
        context.Headers["X-Correlation-ID"].Should().Be("correlation");
        context.Headers["X-Token-Scope"].Should().Be("read");
        result.RefreshToken = "refresh";
        result.ExpiryTime = DateTimeOffset.UtcNow.AddSeconds(10);
        provider.Setup(x => x.RefreshTokenAsync("refresh", It.IsAny<CancellationToken>())).ReturnsAsync(TokenResult.Success("refreshed", 600));
        context = new ProxyContext();
        await interceptor.InvokeAsync<int>(context, Next);
        context.Headers["Authorization"].Should().Be("Bearer refreshed");
        provider.Setup(x => x.RefreshTokenAsync("refresh", It.IsAny<CancellationToken>())).ReturnsAsync((TokenResult?)null);
        await interceptor.InvokeAsync<int>(new ProxyContext(), Next);
        options.RefreshIfExpired = false;
        interceptor.NeedsRefresh(result).Should().BeFalse();
        options.RefreshIfExpired = true;
        result.ExpiryTime = DateTimeOffset.UtcNow.AddMinutes(-1);
        interceptor.NeedsRefresh(result).Should().BeTrue();
        result.ExpiryTime = DateTimeOffset.UtcNow.AddHours(1);
        interceptor.NeedsRefresh(result).Should().BeFalse();
        interceptor.Inspect(null).Should().BeFalse();
        provider.Setup(x => x.ValidateToken("valid")).Returns(true);
        interceptor.Inspect("valid").Should().BeTrue();
        provider.Setup(x => x.ValidateToken("invalid")).Throws<InvalidOperationException>();
        interceptor.Inspect("invalid").Should().BeFalse();
    }

    [TestMethod]
    public async Task OAuthTimeoutsAndErrorsRequireExplicitFallback()
    {
        foreach (Exception error in new Exception[] { new InvalidOperationException(), new TimeoutException(), new OperationCanceledException() })
        {
            var provider = new Mock<ITokenProvider>();
            provider.Setup(x => x.GetTokenAsync(It.IsAny<TokenRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(error);
            JwtOptions options = OAuthOptions();
            var interceptor = new OAuthInterceptor(provider.Object, options);
            Func<Task> fail = () => interceptor.InvokeAsync<int>(new ProxyContext(), Next);
            await fail.Should().ThrowAsync<Exception>();
            options.CustomProperties["FailOnError"] = false;
            options.CustomProperties["FailOnTimeout"] = false;
            (await interceptor.InvokeAsync<int>(new ProxyContext(), Next)).Data.Should().Be(1);
        }
    }

    private static Task<ProxyResponse<int>> Next(ProxyContext context, CancellationToken token) => Task.FromResult(ProxyResponse<int>.Success(1));
    private static Mock<ISecretProvider> Secret(string? token)
    {
        var secret = new Mock<ISecretProvider>();
        secret.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(token);
        return secret;
    }
    private static string StoredToken(bool expired = false, bool future = false) => new JwtSecurityTokenHandler().WriteToken(
        new JwtSecurityToken(notBefore: DateTime.UtcNow.AddMinutes(future ? 1 : -10), expires: DateTime.UtcNow.AddMinutes(expired ? -1 : 5)));
    private static JwtOptions OAuthOptions() => new()
    {
        Authority = "https://issuer.example", Audience = "audience", ClientId = "client", ClientSecret = "secret"
    };
    private class StoredInterceptor(ISecretProvider provider, KeyVaultJwtOptions options)
        : KeyVaultJwtInterceptor(provider, NullLogger<KeyVaultJwtInterceptor>.Instance, options)
    {
        public bool Inspect(string token) => IsTokenValid(token);
        public string Format(string token) => FormatTokenForHeader(token);
        public Task<string?> Refresh(CancellationToken token) => TryRefreshTokenAsync(token);
    }
    private sealed class RefreshingStoredInterceptor(ISecretProvider provider, KeyVaultJwtOptions options, string refreshed)
        : StoredInterceptor(provider, options)
    {
        protected override Task<string?> TryRefreshTokenAsync(CancellationToken cancellationToken) => Task.FromResult<string?>(refreshed);
    }
    private sealed class OAuthInterceptor(ITokenProvider provider, JwtOptions options)
        : JwtAuthenticationInterceptor(provider, NullLogger<JwtAuthenticationInterceptor>.Instance, options)
    {
        public bool Inspect(string? token) => IsTokenValid(token);
        public bool NeedsRefresh(TokenResult result) => ShouldRefreshToken(result);
    }
}
