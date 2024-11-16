using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace vc.Ifx.Web.Security.Oidc;

public class OpenIdConnectService(IOptions<OpenIdConnectOptions> options) : AuthProviderService(options.Value)
{

    private readonly OpenIdConnectOptions options = options.Value;

    private const string OPENID = "openid";
    private const string PROFILE = "profile";
    private const string EMAIL = "email";

    public ICollection<string> Scopes { get; } = new List<string> { OPENID, PROFILE, EMAIL };

    public override async Task<string> AuthenticateAsync(string code)
    {
        var clientApp = ConfidentialClientApplicationBuilder.Create(options.ClientId)
            .WithClientSecret(options.ClientSecret)
            .WithAuthority(new Uri(options.Authority))
            .Build();

        var result = await clientApp.AcquireTokenByAuthorizationCode(Scopes, code).ExecuteAsync();
        return result.IdToken;
    }

}