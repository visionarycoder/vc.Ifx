namespace vc.Ifx.Web.Security.Oidc;

public class OpenIdConnectOptions : AuthProviderOptions
{
    public string Authority { get; set; } = string.Empty;
    public string MetadataAddress { get; set; } = string.Empty;
}
