namespace Wa.Ofm.Accounting.Workday.OneWa.Ifx.Primitives;

/// <summary>A three-letter uppercase currency code. Validates syntax, not registry membership.</summary>
public sealed record Currency
{
    public string Code { get; }

    [JsonConstructor]
    public Currency(string code)
    {
        if (code is not { Length: 3 } || !code.All(char.IsAsciiLetter))
            throw new ArgumentException("Currency must contain exactly three ASCII letters.", nameof(code));
        Code = code.ToUpperInvariant();
    }

    public static Currency Usd { get; } = new("USD");
    public static Currency Eur { get; } = new("EUR");
    public static Currency Gbp { get; } = new("GBP");
    public static Currency Cad { get; } = new("CAD");
    public static Currency Jpy { get; } = new("JPY");

    public override string ToString() => Code;
    public static explicit operator string(Currency value) => value.Code;
    public static explicit operator Currency(string value) => new(value);
}
