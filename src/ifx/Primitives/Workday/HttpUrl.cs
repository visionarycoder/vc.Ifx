namespace Wa.Ofm.Accounting.Workday.OneWa.Ifx.Primitives;

/// <summary>An absolute HTTP(S) URL with a host and no embedded credentials.</summary>
public sealed record HttpUrl
{
    public string Value { get; }

    [JsonConstructor]
    public HttpUrl(string value)
    {
        if (!TryNormalize(value, out var normalized))
            throw new ArgumentException("An absolute HTTP(S) URL without credentials is required.", nameof(value));
        Value = normalized;
    }

    public static bool TryCreate(string? value, [NotNullWhen(true)] out HttpUrl? result)
    {
        result = TryNormalize(value, out var normalized) ? new(normalized) : null;
        return result is not null;
    }

    private static bool TryNormalize(string? value, [NotNullWhen(true)] out string? normalized)
    {
        normalized = null;
        if (string.IsNullOrWhiteSpace(value) || value.Any(char.IsWhiteSpace) ||
            !Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("http" or "https") ||
            string.IsNullOrEmpty(uri.Host) || !string.IsNullOrEmpty(uri.UserInfo))
            return false;
        normalized = uri.AbsoluteUri;
        return true;
    }

    public Uri ToUri() => new(Value, UriKind.Absolute);
    public override string ToString() => Value;
    public static explicit operator string(HttpUrl value) => value.Value;
    public static explicit operator HttpUrl(string value) => new(value);
}
