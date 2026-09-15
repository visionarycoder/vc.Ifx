namespace Ifx.Abstractions;

/// <summary>
/// Represents an active tracing span created by <see cref="ITracer.StartSpan"/>.
/// </summary>
public interface ISpan : IDisposable
{
    /// <summary>Sets a tag on the span.</summary>
    void SetTag(string key, string value);

    /// <summary>Ends the span.</summary>
    void End();
}
