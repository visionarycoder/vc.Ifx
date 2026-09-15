using Ifx.Abstractions;

namespace Ifx.Time;

public sealed class NullClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.MinValue;
}

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
