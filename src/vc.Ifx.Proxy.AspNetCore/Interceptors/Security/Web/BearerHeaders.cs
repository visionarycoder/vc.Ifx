using System.Text.RegularExpressions;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Security.Web;

internal static class BearerHeaders
{
    internal static void Validate(string? token)
    {
        if (token is null || !Regex.IsMatch(token, @"\A[A-Za-z0-9._~+/-]+=*\z", RegexOptions.CultureInvariant))
            throw new UnauthorizedAccessException("A valid bearer credential is required.");
    }

    internal static void Set(ProxyContext context, string name, string? token)
    {
        ArgumentNullException.ThrowIfNull(context.Headers);
        ArgumentNullException.ThrowIfNull(context.Metadata);
        Validate(token);
        foreach (string key in context.Headers.Keys.Where(key => key.Equals(name, StringComparison.OrdinalIgnoreCase)).ToArray())
            context.Headers.Remove(key);
        foreach (string key in context.Metadata.Keys.Where(key => key.Equals(name, StringComparison.OrdinalIgnoreCase)).ToArray())
            context.Metadata.Remove(key);
        context.Headers[name] = $"Bearer {token}";
    }
}
