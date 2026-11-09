using System.Text.Json;

namespace Wa.Ofm.Accounting.Workday.OneWa.Ifx.Primitives;

/// <summary>
/// A secret value whose text, equality display, and JSON forms are redacted so records, logs, and serializers never expose it.
/// A struct keeps <c>default</c> as the empty secret and avoids null handling.
/// </summary>
[JsonConverter(typeof(SecretJsonConverter))]
public readonly record struct Secret
{
    private const string Redacted = "***";

    private readonly string? value;

    /// <summary>Wraps secret text.</summary>
    public Secret(string? value) => this.value = value;

    /// <summary>True when no usable secret text is present.</summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(value);

    /// <summary>Returns the secret text; call only at the point of authentication.</summary>
    public string Reveal() => value ?? string.Empty;

    /// <inheritdoc />
    public override string ToString() => Redacted;

    private bool PrintMembers(System.Text.StringBuilder builder)
    {
        builder.Append(Redacted);
        return true;
    }
}

/// <summary>Reads secret text from JSON and always writes the redacted form so serialization cannot leak or persist secrets.</summary>
public sealed class SecretJsonConverter : JsonConverter<Secret>
{
    /// <inheritdoc />
    public override Secret Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString());

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Secret value, JsonSerializerOptions options) =>
        writer.WriteStringValue("***");
}
