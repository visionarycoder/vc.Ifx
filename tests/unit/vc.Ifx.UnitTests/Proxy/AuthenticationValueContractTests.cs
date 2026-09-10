using System.Globalization;
using System.Text.Json;
using VisionaryCoder.Framework.Proxy.Interceptors.Authentication.Jwt;
using VisionaryCoder.Framework.Proxy.Interceptors.Configuration;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public sealed class AuthenticationValueContractTests
{
    [TestMethod]
    public void TokenResultExpiryAndSafeDiagnosticsDoNotExposeCredentials()
    {
        var result = TokenResult.Success("sensitive-access", 600, "sensitive-refresh", "read");
        result.IsExpired.Should().BeFalse();
        result.IsCloseToExpiry().Should().BeFalse();
        result.ToString().Should().NotContain("sensitive-access").And.NotContain("sensitive-refresh");
        string json = result.ToJson();
        json.Should().NotContain("sensitive-access").And.NotContain("sensitive-refresh");
        using JsonDocument document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("HasAccessToken").GetBoolean().Should().BeTrue();
        document.RootElement.GetProperty("HasRefreshToken").GetBoolean().Should().BeTrue();
        result.ExpiresIn = 0;
        result.UpdateExpiryTime();
        result.IsExpired.Should().BeTrue();
        result.TimeUntilExpiry.Should().Be(TimeSpan.Zero);
        result.IsCloseToExpiry().Should().BeTrue();
        result.AccessToken = string.Empty;
        result.RefreshToken = null;
        result.ToJson().Should().Contain("\"HasAccessToken\": false").And.Contain("\"HasRefreshToken\": false");
        var failure = TokenResult.Failure("rejected", "safe description", "https://issuer.example/error");
        failure.ToString().Should().Contain("Failed");
        failure.ErrorDescription.Should().Be("safe description");
        failure.ErrorUri.Should().Be("https://issuer.example/error");
        failure.ToJson().Should().Contain("rejected");
    }

    [TestMethod]
    public void TokenRequestsValidateGrantSpecificRequirements()
    {
        var request = TokenRequest.CreateClientCredentials("client", "secret", ["read", "write"], "audience");
        request.IsValid().Should().BeTrue();
        request.GetScopeString().Should().Be("read write");
        request.RefreshIfExpired = false;
        request.RefreshIfExpired.Should().BeFalse();
        request.GrantType = "";
        request.IsValid().Should().BeFalse();
        request.GrantType = "client_credentials";
        request.ClientId = "";
        request.IsValid().Should().BeFalse();
        request.ClientId = "client";
        request.Scopes = null!;
        request.IsValid().Should().BeFalse();
        request.Scopes = [];
        request.CustomParameters = null!;
        request.IsValid().Should().BeFalse();
        request.CustomParameters = new();
        request.GrantType = "refresh_token";
        request.IsValid().Should().BeFalse();
        request.CustomParameters["refresh_token"] = " ";
        request.IsValid().Should().BeFalse();
        request.CustomParameters["refresh_token"] = "refresh";
        request.IsValid().Should().BeTrue();
        request.GrantType = "custom-grant";
        request.IsValid().Should().BeTrue();
        request.GrantType = "authorization_code";
        request.IsValid().Should().BeFalse();
        request.AuthorizationCode = "code";
        request.IsValid().Should().BeFalse();
        request.RedirectUri = "https://client.example/callback";
        request.IsValid().Should().BeTrue();
        var password = TokenRequest.CreatePasswordCredentials("client", "user", "password", ["read"]);
        password.IsValid().Should().BeTrue();
        password.Password = "";
        password.IsValid().Should().BeFalse();
        password.Username = "";
        password.IsValid().Should().BeFalse();
        TokenRequest.CreatePasswordCredentials("client", "user", "password").GetScopeString().Should().BeNull();
    }

    [TestMethod]
    public void ConfigurationScalarConversionUsesInvariantCultureAndJsonFallback()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            ConfigurationHelper.ConvertValue("1.25", 0m).Should().Be(1.25m);
            ConfigurationHelper.ConvertValue("42", 0).Should().Be(42);
            ConfigurationHelper.ConvertValue("text", "fallback").Should().Be("text");
            ConfigurationHelper.ConvertValue("not a number", 7).Should().Be(7);
            ConfigurationHelper.ConvertValue("{\"Name\":\"configured\"}", new Setting()).Name.Should().Be("configured");
            var fallback = new Setting();
            ConfigurationHelper.ConvertValue("bad json", fallback).Should().BeSameAs(fallback);
            ConfigurationHelper.ConvertValue("null", fallback).Should().BeSameAs(fallback);
            ConfigurationHelper.ConvertValue<int?>("", 7).Should().Be(7);
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    public sealed class Setting
    {
        public string Name { get; set; } = string.Empty;
    }
}
