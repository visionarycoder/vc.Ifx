namespace vc.Ifx.Web.Security;

public abstract class AuthProviderOptions
{

    public string Instance { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string CallbackPath { get; set; } = string.Empty;

}