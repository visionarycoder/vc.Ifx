using System.Globalization;

namespace VisionaryCoder.Framework.Primitives;

/// <summary>
/// Represents a monetary amount with an ISO 4217-style currency code.
/// </summary>
public sealed class Money : IEquatable<Money>, IPrimitiveValue<decimal>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> class.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The three-letter currency code.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="currency"/> is not a three-letter code.</exception>
    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = NormalizeCurrency(currency);
    }

    /// <summary>
    /// Gets the monetary amount.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the normalized currency code.
    /// </summary>
    public string Currency { get; }

    /// <inheritdoc />
    public decimal Value => Amount;

    /// <inheritdoc />
    public Type ValueType => typeof(decimal);

    /// <inheritdoc />
    public object BoxedValue => Amount;

    /// <summary>
    /// Creates a monetary value.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The three-letter currency code.</param>
    /// <returns>The monetary value.</returns>
    public static Money Create(decimal amount, string currency) => new(amount, currency);

    /// <summary>
    /// Creates a zero-valued monetary amount for the supplied currency.
    /// </summary>
    /// <param name="currency">The three-letter currency code.</param>
    /// <returns>The zero-valued monetary amount.</returns>
    public static Money Zero(string currency) => new(0m, currency);

    /// <summary>
    /// Parses a monetary value from text formatted as <c>CUR amount</c>.
    /// </summary>
    /// <param name="text">The monetary value text.</param>
    /// <returns>The parsed monetary value.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="text"/> is not formatted as a money value.</exception>
    public static Money Parse(string? text)
    {
        if (TryParse(text, out Money? money) && money is not null)
        {
            return money;
        }

        throw new FormatException("Invalid money value.");
    }

    /// <summary>
    /// Attempts to parse a monetary value from text formatted as <c>CUR amount</c>.
    /// </summary>
    /// <param name="text">The monetary value text.</param>
    /// <param name="money">The parsed monetary value when parsing succeeds.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string? text, out Money? money)
    {
        money = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        string[] parts = text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || !TryNormalizeCurrency(parts[0], out string currency) || !decimal.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount))
        {
            return false;
        }

        money = new Money(amount, currency);
        return true;
    }

    /// <summary>
    /// Adds another monetary value with the same currency.
    /// </summary>
    /// <param name="other">The monetary value to add.</param>
    /// <returns>The sum.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="other"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when currencies differ.</exception>
    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtracts another monetary value with the same currency.
    /// </summary>
    /// <param name="other">The monetary value to subtract.</param>
    /// <returns>The difference.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="other"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when currencies differ.</exception>
    public Money Subtract(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    /// <summary>
    /// Converts the value object to its decimal amount.
    /// </summary>
    /// <param name="money">The monetary value.</param>
    public static explicit operator decimal(Money money) => money.Amount;

    /// <inheritdoc />
    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{Currency} {Amount}");

    /// <inheritdoc />
    public bool Equals(Money? other) => other is not null && Amount == other.Amount && StringComparer.Ordinal.Equals(Currency, other.Currency);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Money other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Amount, StringComparer.Ordinal.GetHashCode(Currency));

    /// <summary>
    /// Compares two money values for equality.
    /// </summary>
    /// <param name="left">The left money value.</param>
    /// <param name="right">The right money value.</param>
    /// <returns><see langword="true"/> when both values are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Money? left, Money? right) => left is null ? right is null : left.Equals(right);

    /// <summary>
    /// Compares two money values for inequality.
    /// </summary>
    /// <param name="left">The left money value.</param>
    /// <param name="right">The right money value.</param>
    /// <returns><see langword="true"/> when both values differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Money? left, Money? right) => !(left == right);

    private static string NormalizeCurrency(string currency)
    {
        if (TryNormalizeCurrency(currency, out string normalized))
        {
            return normalized;
        }

        throw new ArgumentException("Currency value must be a three-letter code.", nameof(currency));
    }

    private static bool TryNormalizeCurrency(string? currency, out string normalized)
    {
        normalized = string.Empty;
        string? trimmed = currency?.Trim();
        if (trimmed is null || trimmed.Length != 3 || !trimmed.All(static c => char.IsLetter(c)))
        {
            return false;
        }

        normalized = trimmed.ToUpperInvariant();
        return true;
    }

    private void EnsureSameCurrency(Money other)
    {
        if (!StringComparer.Ordinal.Equals(Currency, other.Currency))
        {
            throw new InvalidOperationException("Money values must use the same currency.");
        }
    }
}
