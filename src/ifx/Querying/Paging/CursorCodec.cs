using System.Text.Json;

namespace Ifx.Querying.Paging;

internal static class CursorCodec
{
    internal static string Encode(IReadOnlyList<string?> values) =>
        Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(values));

    internal static IReadOnlyList<string?> Decode(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        try
        {
            return JsonSerializer.Deserialize<string?[]>(Convert.FromBase64String(token))
                ?? throw new FormatException("Cursor payload must be an array of values.");
        }
        catch (JsonException exception)
        {
            throw new FormatException("Cursor payload is invalid.", exception);
        }
    }
}
