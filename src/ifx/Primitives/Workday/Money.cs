namespace Wa.Ofm.Accounting.Workday.OneWa.Ifx.Primitives;

/// <summary>A signed decimal amount and currency. No implicit rounding or currency conversion.</summary>
public sealed record Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    [JsonConstructor]
    public Money(decimal amount, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);
        Amount = amount;
        Currency = currency;
    }

    public static Money Zero(Currency currency) => new(0m, currency);

    public Money Round(int decimalPlaces, MidpointRounding rounding) =>
        new(decimal.Round(Amount, decimalPlaces, rounding), Currency);

    public Money Apply(Percentage percentage)
    {
        ArgumentNullException.ThrowIfNull(percentage);
        return new(percentage.ApplyTo(Amount), Currency);
    }

    public static Money operator +(Money left, Money right)
    {
        RequireSameCurrency(left, right);
        return new(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        RequireSameCurrency(left, right);
        return new(left.Amount - right.Amount, left.Currency);
    }

    public static Money operator *(Money money, decimal multiplier)
    {
        ArgumentNullException.ThrowIfNull(money);
        return new(money.Amount * multiplier, money.Currency);
    }

    public static Money operator *(decimal multiplier, Money money) => money * multiplier;

    public static Money operator /(Money money, decimal divisor)
    {
        ArgumentNullException.ThrowIfNull(money);
        return new(money.Amount / divisor, money.Currency);
    }

    private static void RequireSameCurrency(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Arithmetic requires matching currencies.");
    }

    public override string ToString() => $"{Amount.ToString(CultureInfo.InvariantCulture)} {Currency.Code}";
}
