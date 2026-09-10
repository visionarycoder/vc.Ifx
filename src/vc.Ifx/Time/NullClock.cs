using vc.Ifx.Abstractions.Time;

namespace vc.Ifx.Time;

/// <summary>
/// Provides a deterministic empty clock value.
/// </summary>
public sealed class NullClock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.MinValue;
}
