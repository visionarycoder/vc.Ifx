namespace VisionaryCoder.Framework.Proxy;

/// <summary>Updates logical headers without changing the caller's public dictionary comparer.</summary>
internal static class ProxyHeaders
{
    public static void Set(ProxyContext context, string name, string value)
    {
        context.Headers ??= [];
        foreach (string existing in context.Headers.Keys.Where(key => key.Equals(name, StringComparison.OrdinalIgnoreCase)).ToArray())
            context.Headers.Remove(existing);
        context.Headers[name] = value;
    }
}
