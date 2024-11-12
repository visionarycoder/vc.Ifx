namespace vc.Ifx.Web.Security;

public abstract class AuthProviderService(AuthProviderOptions options)
{

    protected AuthProviderOptions Options { get; } = options;

    public abstract Task<string> AuthenticateAsync(string code);

}