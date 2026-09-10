namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Status;

/// <summary>
/// Defines a contract for publishing invocation status updates.
/// Implementations route status updates to monitoring, alerting, or event-driven systems.
/// </summary>
public interface IStatusSink
{
    /// <summary>
    /// Publishes an invocation status update.
    /// </summary>
    /// <param name="update">The status update to publish.</param>
    ValueTask PublishAsync(StatusUpdated update);
}
