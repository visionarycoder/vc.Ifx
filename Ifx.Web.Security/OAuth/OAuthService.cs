using Microsoft.Extensions.Options;

using System.Text.Json;

namespace vc.Ifx.Web.Security.OAuth;

public class OAuthService(IOptions<OAuthOptions> options) : AuthProviderService(options.Value)
{

    private readonly HttpClient httpClient = new();

    public override async Task<string> AuthenticateAsync(string code)
    {

        var oAuthOptions = (OAuthOptions)Options;
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, oAuthOptions.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", oAuthOptions.CallbackPath),
                new KeyValuePair<string, string>("client_id", oAuthOptions.ClientId),
                new KeyValuePair<string, string>("client_secret", oAuthOptions.ClientSecret)
            })
        };

        var response = await httpClient.SendAsync(tokenRequest);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content) ?? new TokenResponse();

        return tokenResponse.AccessToken;
    }

    private class TokenResponse
    {
        public string AccessToken { get; init; } = string.Empty;
        public string IdToken { get; init; } = string.Empty;
    }

}