using System.ComponentModel;
using Ifx.Abstractions;
using Ifx.Pipeline;
using Ifx.Pipeline.Abstractions;
using Ifx.Proxy.Interceptor;
using Ifx.Proxy.Interceptor.Abstractions;
using Ifx.Proxy.Interceptor.Authentication.Jwt;
using Ifx.Proxy.Interceptor.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;

namespace Ifx.Tests.Proxy;

[TestClass]
public sealed class InterceptorSmallGapTests
{
    [TestMethod]
    public async Task ConcreteInterceptorsKeepOrdersAndLegacyOverloads()
    {
        var auth = new AuthorizationServiceStub();
        var metrics = new MetricsStub();
        var tracer = new TracerStub();

        new AuthInterceptor(auth).Order.Should().Be(-200);
        new LoggingInterceptor(NullLogger<LoggingInterceptor>.Instance).Order.Should().Be(100);
        new MetricsInterceptor(metrics).Order.Should().Be(50);
        new ResilienceInterceptor(ResiliencePipeline.Empty).Order.Should().Be(10);
        new TracingInterceptor(tracer).Order.Should().Be(0);

        (await new AuthInterceptor(auth).InvokeAsync<TestQuery, string?>(new(), _ => Task.FromResult<string?>("authorized"))).Should().Be("authorized");
        auth.Calls.Should().Be(1);
        (await new LoggingInterceptor(NullLogger<LoggingInterceptor>.Instance).InvokeAsync<TestQuery, string?>(new(), _ => Task.FromResult<string?>("logged"))).Should().Be("logged");
        (await new MetricsInterceptor(metrics).InvokeAsync<TestQuery, string?>(new(), _ => Task.FromResult<string?>("measured"))).Should().Be("measured");
        (await new ResilienceInterceptor(Policy.NoOpAsync()).InvokeAsync<TestQuery, string?>(new(), _ => Task.FromResult<string?>("legacy"))).Should().Be("legacy");
        (await new TracingInterceptor(tracer).InvokeAsync<TestQuery, string?>(new(), _ => Task.FromResult<string?>("traced"))).Should().Be("traced");
    }

    [TestMethod]
    public void TokenRequestValidationCoversGrantSpecificGuardBranches()
    {
        var password = new TokenRequest
        {
            GrantType = "password",
            ClientId = "client",
            Username = "user",
            Password = " ",
            Scopes = [],
            CustomParameters = new()
        };
        password.IsValid().Should().BeFalse();
        password.Username = " ";
        password.Password = "password";
        password.IsValid().Should().BeFalse();
        password.Username = "user";
        password.Password = "password";
        password.IsValid().Should().BeTrue();

        var authorizationCode = new TokenRequest
        {
            GrantType = "authorization_code",
            ClientId = "client",
            AuthorizationCode = "code",
            RedirectUri = " ",
            Scopes = [],
            CustomParameters = new()
        };
        authorizationCode.IsValid().Should().BeFalse();
        authorizationCode.AuthorizationCode = " ";
        authorizationCode.RedirectUri = "https://client.example/callback";
        authorizationCode.IsValid().Should().BeFalse();
        authorizationCode.AuthorizationCode = "code";
        authorizationCode.RedirectUri = "https://client.example/callback";
        authorizationCode.IsValid().Should().BeTrue();

        var refresh = new TokenRequest
        {
            GrantType = "refresh_token",
            ClientId = "client",
            Scopes = [],
            CustomParameters = new()
        };
        refresh.IsValid().Should().BeFalse();
        refresh.CustomParameters["refresh_token"] = " ";
        refresh.IsValid().Should().BeFalse();
        refresh.CustomParameters["refresh_token"] = "refresh";
        refresh.IsValid().Should().BeTrue();
    }

    [TestMethod]
    public void TokenResultFormattingAndExplicitThresholdsRemainDeterministic()
    {
        var success = TokenResult.Success("token", 3600);
        success.CorrelationId = "corr";

        success.IsCloseToExpiry().Should().BeFalse();
        success.ExpiryTime = DateTimeOffset.UtcNow.AddSeconds(1);
        success.IsCloseToExpiry().Should().BeTrue();
        success.IsCloseToExpiry(TimeSpan.FromMilliseconds(100)).Should().BeFalse();
        success.ToString().Should().Contain("Success").And.Contain("HasRefreshToken: False");
        success.ToJson().Should().Contain("\"CorrelationId\": \"corr\"");

        var failure = TokenResult.Failure("denied");
        failure.ToString().Should().Be("TokenResult: Failed - denied");
    }

    [TestMethod]
    public void ConfigurationHelperCoversConvertibleJsonAndFallbackBranches()
    {
        ConfigurationHelper.ConvertValue("42", 0).Should().Be(42);

        var converterFallback = new ThrowingConvertedValue();
        ConfigurationHelper.ConvertValue("boom", converterFallback).Should().BeSameAs(converterFallback);

        var nullReturningFallback = new NullReturningConvertedValue();
        ConfigurationHelper.ConvertValue("ignored", nullReturningFallback).Should().BeSameAs(nullReturningFallback);

        var jsonFallback = new JsonOnlyValue();
        ConfigurationHelper.ConvertValue("{\"Name\":\"configured\"}", jsonFallback).Name.Should().Be("configured");
        ConfigurationHelper.ConvertValue("null", jsonFallback).Should().BeSameAs(jsonFallback);
        ConfigurationHelper.ConvertValue("bad json", jsonFallback).Should().BeSameAs(jsonFallback);
    }

    private sealed class TestQuery : IRequest<string?>;

    private sealed class AuthorizationServiceStub : IAuthorizationService
    {
        public int Calls { get; private set; }

        public Task AuthorizeAsync(object request)
        {
            Calls++;
            return Task.CompletedTask;
        }
    }

    private sealed class MetricsStub : IMetrics
    {
        public void IncrementCounter(string metric, string label)
        {
        }

        public void ObserveHistogram(string metric, string label, long value)
        {
        }
    }

    private sealed class TracerStub : ITracer
    {
        public ISpan StartSpan(string name) => new SpanStub();
    }

    private sealed class SpanStub : ISpan
    {
        public void Dispose()
        {
        }

        public void End()
        {
        }

        public void SetTag(string key, string value)
        {
        }
    }

    [TypeConverter(typeof(ThrowingTypeConverter))]
    private sealed class ThrowingConvertedValue
    {
    }

    private sealed class ThrowingTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string);

        public override object ConvertFrom(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value) =>
            throw new FormatException("boom");
    }

    [TypeConverter(typeof(NullReturningTypeConverter))]
    private sealed class NullReturningConvertedValue
    {
    }

    private sealed class NullReturningTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string);

        public override object? ConvertFrom(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value) => null;
    }

    private sealed class JsonOnlyValue
    {
        public string Name { get; set; } = string.Empty;
    }
}
