namespace Ifx.Abstractions;

/// <summary>
/// Provides the current time for framework services that need deterministic tests.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current UTC time.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
