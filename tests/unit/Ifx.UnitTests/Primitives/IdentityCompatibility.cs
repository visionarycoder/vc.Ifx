using Ifx.Abstractions;

namespace Ifx.Primitives;

public readonly struct Identity<TOwner> : IEquatable<Identity<TOwner>>, IPrimitiveValue<Guid>
    where TOwner : class
{
    public Identity(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Identity value cannot be empty.", nameof(value));

        Value = value;
    }

    public Guid Value { get; }
    public Type ValueType => typeof(Guid);
    public object BoxedValue => Value;

    public static Identity<TOwner> Create(Guid value) => new(value);
    public static Identity<TOwner> New() => new(Guid.NewGuid());

    public static Identity<TOwner> Parse(string? text)
    {
        if (TryParse(text, out var identity))
            return identity;

        throw new FormatException("Invalid identity value.");
    }

    public static bool TryParse(string? text, out Identity<TOwner> identity)
    {
        identity = default;
        if (!Guid.TryParse(text, out Guid value) || value == Guid.Empty)
            return false;

        identity = new Identity<TOwner>(value);
        return true;
    }

    public static explicit operator Guid(Identity<TOwner> identity) => identity.Value;
    public override string ToString() => Value.ToString("D", System.Globalization.CultureInfo.InvariantCulture);
    public bool Equals(Identity<TOwner> other) => Value.Equals(other.Value);
    public override bool Equals(object? obj) => obj is Identity<TOwner> other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(Identity<TOwner> left, Identity<TOwner> right) => left.Equals(right);
    public static bool operator !=(Identity<TOwner> left, Identity<TOwner> right) => !left.Equals(right);
}
