namespace vc.Ifx.Web.Security.OAuth;

public class OAuthOptions : AuthProviderOptions
{
    public string AuthorizationEndpoint { get; set; } = string.Empty;
    public string TokenEndpoint { get; set; } = string.Empty;
}