using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Jwt;
using VisionaryCoder.Framework.Proxy.Interceptors.Security.Web;

namespace VisionaryCoder.Framework.Tests.ProxyAspNetCore;

[TestClass]
public class BearerTests
{
    private static readonly ProxyDelegate<string> Success = (context, token) => Task.FromResult(ProxyResponse<string>.Success("ok"));
    private static JwtBearerEnricher Enricher(string? token) => new(NullLogger<JwtBearerEnricher>.Instance, () => Task.FromResult(token));
    private static JwtBearerInterceptor Interceptor(string? token) => new(NullLogger<JwtBearerInterceptor>.Instance, cancellation => Task.FromResult(token));

    [TestMethod]
    public async Task BearerAdaptersReplaceAllStaleCredentialAliasesOnly()
    {
        foreach (string token in new[] { "opaque", "a.b_c~d+e/f-g==" })
        {
            var context = new ProxyContext
            {
                Headers = new() { ["authorization"] = "old", ["Authorization"] = "old", ["Keep"] = "value" },
                Metadata = new() { ["AUTHORIZATION"] = "stale", ["Keep"] = "value" }
            };
            await Enricher(token).EnrichAsync(context);
            context.Headers.Should().HaveCount(2).And.Contain("Authorization", "Bearer " + token);
            context.Metadata.Should().HaveCount(1).And.ContainKey("Keep");
            (await Interceptor(token).InvokeAsync(context, Success)).IsSuccess.Should().BeTrue();
            context.Headers["Authorization"].Should().Be("Bearer " + token);
        }
    }

    [TestMethod]
    public async Task MissingOrMalformedCredentialsNeverDispatch()
    {
        foreach (string? token in new[] { null, "", " ", "bad token", "bad\r\nInjected: true", "a=b", "=", "é", "a\n" })
        {
            await Assert.ThrowsExactlyAsync<UnauthorizedAccessException>(() => Enricher(token).EnrichAsync(new ProxyContext()));
            await Assert.ThrowsExactlyAsync<UnauthorizedAccessException>(() => Interceptor(token).InvokeAsync(new ProxyContext(), Success));
        }
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => Enricher("ok").EnrichAsync(null!));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => Enricher("ok").EnrichAsync(new ProxyContext { Headers = null! }));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => Enricher("ok").EnrichAsync(new ProxyContext { Metadata = null! }));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => Interceptor("ok").InvokeAsync(null!, Success));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => Interceptor("ok").InvokeAsync<string>(new ProxyContext(), null!));
    }

    [TestMethod]
    public async Task BearerCancellationAndDownstreamExceptionsRemainUnwrapped()
    {
        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => Enricher("ok").EnrichAsync(new ProxyContext(), canceled.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => Interceptor("ok").InvokeAsync(new ProxyContext(), Success, canceled.Token));
        using var during = new CancellationTokenSource();
        var interceptor = new JwtBearerInterceptor(NullLogger<JwtBearerInterceptor>.Instance, token =>
        {
            token.Should().Be(during.Token);
            during.Cancel();
            return Task.FromResult<string?>("ok");
        });
        await Assert.ThrowsAsync<OperationCanceledException>(() => interceptor.InvokeAsync(new ProxyContext(), Success, during.Token));
        using var enrichment = new CancellationTokenSource();
        var enricher = new JwtBearerEnricher(NullLogger<JwtBearerEnricher>.Instance, () =>
        {
            enrichment.Cancel();
            return Task.FromResult<string?>("ok");
        });
        await Assert.ThrowsAsync<OperationCanceledException>(() => enricher.EnrichAsync(new ProxyContext(), enrichment.Token));
        var failure = new InvalidOperationException("original");
        (await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => Interceptor("ok").InvokeAsync<string>(new ProxyContext(),
            (context, token) => throw failure))).Should().BeSameAs(failure);
        interceptor = new JwtBearerInterceptor(NullLogger<JwtBearerInterceptor>.Instance, token => throw failure);
        (await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => interceptor.InvokeAsync(new ProxyContext(), Success))).Should().BeSameAs(failure);
        enricher = new JwtBearerEnricher(NullLogger<JwtBearerEnricher>.Instance, () => throw new OperationCanceledException());
        await Assert.ThrowsAsync<OperationCanceledException>(() => enricher.EnrichAsync(new ProxyContext()));
    }

    [TestMethod]
    public async Task WebJwtSnapshotsOptionsAndValidatesBeforeDispatch()
    {
        using var cancellation = new CancellationTokenSource();
        var provider = new Mock<ITokenProvider>(MockBehavior.Strict);
        var options = new WebJwtOptions
        {
            Authority = "https://issuer.example", Audience = "api", Issuer = "issuer", SigningKey = "legacy",
            RequireHttpsMetadata = false, Scopes = ["read"], RefreshIfExpired = false, HeaderName = "X-Api-Token"
        };
        options.Authority.Should().Be("https://issuer.example");
        options.Issuer.Should().Be("issuer");
        options.SigningKey.Should().Be("legacy");
        options.RequireHttpsMetadata.Should().BeFalse();
        provider.Setup(value => value.GetTokenAsync(It.IsAny<TokenRequest>(), cancellation.Token))
            .Callback<TokenRequest, CancellationToken>((request, token) =>
            {
                request.Audience.Should().Be("api");
                request.Scopes.Should().Equal("read");
                request.RefreshIfExpired.Should().BeFalse();
                request.Scopes[0] = "mutated by provider";
            }).ReturnsAsync(TokenResult.Success("valid", 3600));
        provider.Setup(value => value.ValidateTokenAsync("valid", cancellation.Token)).ReturnsAsync(true);
        var interceptor = new WebJwtInterceptor(provider.Object, NullLogger<WebJwtInterceptor>.Instance, options);
        options.Audience = "changed";
        options.Scopes[0] = "write";
        options.HeaderName = "Host";
        var context = new ProxyContext();
        for (int call = 0; call < 2; call++)
            (await interceptor.InvokeAsync(context, Success, cancellation.Token)).IsSuccess.Should().BeTrue();
        context.Headers["X-Api-Token"].Should().Be("Bearer valid");
        context.Headers.Should().NotContainKey("X-Correlation-ID");
        provider.VerifyAll();
    }

    [TestMethod]
    public async Task WebJwtRejectsEveryAcquisitionAndValidationFailure()
    {
        foreach (TokenResult? result in new TokenResult?[] { null, TokenResult.Failure("secret detail"),
            TokenResult.Success("expired", -10), new() { AccessToken = "ok", ExpiryTime = DateTimeOffset.MaxValue, TokenType = "Basic" },
            TokenResult.Success("bad token", 3600), TokenResult.Success("invalid", 3600) })
        {
            var provider = new Mock<ITokenProvider>();
            provider.Setup(value => value.GetTokenAsync(It.IsAny<TokenRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(result!);
            provider.Setup(value => value.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var interceptor = new WebJwtInterceptor(provider.Object, NullLogger<WebJwtInterceptor>.Instance, new() { Audience = "api" });
            await Assert.ThrowsExactlyAsync<UnauthorizedAccessException>(() => interceptor.InvokeAsync(new ProxyContext(), Success));
        }
    }

    [TestMethod]
    public async Task WebJwtCancellationAtEveryAwaitAndInputGuards()
    {
        for (int stage = 0; stage < 3; stage++)
        {
            using var cancellation = new CancellationTokenSource();
            var provider = new Mock<ITokenProvider>();
            if (stage == 0)
                cancellation.Cancel();
            provider.Setup(value => value.GetTokenAsync(It.IsAny<TokenRequest>(), cancellation.Token)).Returns(() =>
            {
                if (stage == 1) cancellation.Cancel();
                return Task.FromResult(TokenResult.Success("ok", 3600));
            });
            provider.Setup(value => value.ValidateTokenAsync("ok", cancellation.Token)).Returns(() =>
            {
                cancellation.Cancel();
                return Task.FromResult(true);
            });
            var interceptor = new WebJwtInterceptor(provider.Object, NullLogger<WebJwtInterceptor>.Instance, new() { Audience = "api" });
            await Assert.ThrowsAsync<OperationCanceledException>(() => interceptor.InvokeAsync(new ProxyContext(), Success, cancellation.Token));
        }
        var valid = new WebJwtInterceptor(Mock.Of<ITokenProvider>(), NullLogger<WebJwtInterceptor>.Instance, new() { Audience = "api" });
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => valid.InvokeAsync(null!, Success));
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => valid.InvokeAsync<string>(new ProxyContext(), null!));
    }

    [TestMethod]
    public void ConstructorsAndHeaderConfigurationAreValidated()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new JwtBearerEnricher(null!, () => Task.FromResult<string?>("ok")));
        Assert.ThrowsExactly<ArgumentNullException>(() => new JwtBearerEnricher(NullLogger<JwtBearerEnricher>.Instance, null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => new JwtBearerInterceptor(null!, token => Task.FromResult<string?>("ok")));
        Assert.ThrowsExactly<ArgumentNullException>(() => new JwtBearerInterceptor(NullLogger<JwtBearerInterceptor>.Instance, null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => new WebJwtInterceptor(null!, NullLogger<WebJwtInterceptor>.Instance, new()));
        Assert.ThrowsExactly<ArgumentNullException>(() => new WebJwtInterceptor(Mock.Of<ITokenProvider>(), null!, new()));
        Assert.ThrowsExactly<ArgumentNullException>(() => new WebJwtInterceptor(Mock.Of<ITokenProvider>(), NullLogger<WebJwtInterceptor>.Instance, null!));
        Assert.ThrowsExactly<ArgumentException>(() => new WebJwtInterceptor(Mock.Of<ITokenProvider>(), NullLogger<WebJwtInterceptor>.Instance, new()));
        Assert.ThrowsExactly<ArgumentNullException>(() => new WebJwtInterceptor(Mock.Of<ITokenProvider>(), NullLogger<WebJwtInterceptor>.Instance, new() { Audience = "api", Scopes = null! }));
        foreach (string? header in new[] { null, "", "Host", "Cookie", "Content-Length", "Authorization\r\nX: a", "X-A\n" })
            Assert.ThrowsExactly<ArgumentException>(() => new WebJwtInterceptor(Mock.Of<ITokenProvider>(), NullLogger<WebJwtInterceptor>.Instance,
                new() { Audience = "api", HeaderName = header! }));
    }
}
