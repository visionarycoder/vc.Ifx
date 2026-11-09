using Ifx.Primitives;

namespace Ifx.Errors;

/// <summary>Indicates whether an error permits recovery.</summary>
public sealed class IsRecoverableError : Enumeration
{
    public static readonly IsRecoverableError No = new(0, nameof(No));
    public static readonly IsRecoverableError Yes = new(1, nameof(Yes));

    private IsRecoverableError(int value, string name) : base(value, name) { }

    public static IReadOnlyCollection<IsRecoverableError> All { get; } = new[] { No, Yes };

    public static IsRecoverableError FromValue(int value) => All.FirstOrDefault(item => item.Id == value) ?? No;

    public static IsRecoverableError FromName(string? name) => All.FirstOrDefault(item => item.Name == name) ?? No;

    public static bool operator ==(IsRecoverableError? left, IsRecoverableError? right) => left?.Id == right?.Id;

    public static bool operator !=(IsRecoverableError? left, IsRecoverableError? right) => left?.Id != right?.Id;

    public override bool Equals(object? obj) => obj is IsRecoverableError other && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();
}
