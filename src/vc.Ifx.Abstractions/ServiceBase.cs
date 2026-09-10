using Microsoft.Extensions.Logging;

namespace VisionaryCoder.Framework;

/// <summary>
/// Base class for all framework services, providing common functionality like logging and disposal.
/// </summary>
/// <typeparam name="T">The concrete service type for typed logging.</typeparam>
public abstract class ServiceBase<T>(ILogger<T> logger) : IDisposable where T : class
{

    private bool disposed = false;

    /// <summary>
    /// Gets the logger instance for this service.
    /// </summary>
    protected ILogger<T> Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));


    /// <summary>
    /// Finalizer for ServiceBase.
    /// </summary>
    ~ServiceBase()
    {
        Dispose(false);
    }

    /// <summary>
    /// Releases all resources used by the ServiceBase.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the ServiceBase and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {

        if (!disposed)
        {
            if (disposing)
            {
                // Dispose managed resources here
                // Derived classes can override this method to dispose their resources
            }
            disposed = true;
        }

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
