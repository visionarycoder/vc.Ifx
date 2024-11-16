namespace vc.Ifx.Web.Security.Saml;

public class SamlOptions : AuthProviderOptions
{
    public string IdentityProviderUrl { get; set; } = string.Empty;
    public string AssertionConsumerServiceUrl { get; set; } = string.Empty;
}