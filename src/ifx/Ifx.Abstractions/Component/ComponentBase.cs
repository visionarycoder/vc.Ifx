using Microsoft.Extensions.Logging;

namespace Ifx.Component;

/// <summary>
/// Base class for all framework services, providing common functionality like logging and disposal.
/// </summary>
/// <typeparam name="T">The concrete component type for typed logging.</typeparam>
public abstract class ComponentBase<T>(ILogger<T> logger) : IDisposable where T : class
{

    private bool disposed;

    /// <summary>
    /// Gets the logger instance for this component.
    /// </summary>
    protected ILogger<T> Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));


    /// <summary>
    /// Releases all resources used by the ComponentBase.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Marks the service disposed. Derived classes release their own resources and call this base hook.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        disposed = true;
    }

    /// <summary>
    /// Throws an ObjectDisposedException if the service has been disposed.
    /// </summary>
    protected void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }

}
