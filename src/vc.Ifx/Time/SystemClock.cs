using vc.Ifx.Abstractions.Time;

namespace vc.Ifx.Time;

/// <summary>
/// Provides the current system UTC time.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
