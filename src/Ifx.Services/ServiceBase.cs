// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

using Microsoft.Extensions.Logging;

#pragma warning disable CA1051
#pragma warning disable DerivedClasses
#pragma warning disable ClassWithData


namespace vc.Ifx.Services;

/// <summary>
/// Represents the base class for services, providing common functionality such as logging, instance identification, and creation timestamp.
/// </summary>
/// <typeparam name="T">The type of the service.</typeparam>
public abstract class ServiceBase<T>(ILogger<T> logger)
{
    /// <summary>
    /// The logger instance for logging information, warnings, and errors.
    /// </summary>
    protected internal readonly ILogger<T> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Gets the unique identifier for the service instance.
    /// </summary>
    public Guid InstanceId { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the timestamp when the service instance was created.
    /// </summary>
    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Logs an exception with a custom message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The custom message to log.</param>
    protected void LogException(Exception exception, string message)
    {
        if (exception == null) throw new ArgumentNullException(nameof(exception));
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message cannot be null or whitespace.", nameof(message));

        logger.LogError(exception, "{Message} | InstanceId: {InstanceId}", message, InstanceId);
    }

    /// <summary>
    /// Tracks the execution time of a given action and logs it.
    /// </summary>
    /// <param name="actionName">The name of the action being tracked.</param>
    /// <param name="action">The action to execute.</param>
    protected void TrackExecutionTime(string actionName, Action action)
    {
        if (string.IsNullOrWhiteSpace(actionName)) throw new ArgumentException("Action name cannot be null or whitespace.", nameof(actionName));
        if (action == null) throw new ArgumentNullException(nameof(action));

        var startTime = DateTime.UtcNow;
        try
        {
            action();
        }
        finally
        {
            var elapsedTime = DateTime.UtcNow - startTime;
            logger.LogInformation("Action '{ActionName}' executed in {ElapsedMilliseconds} ms | InstanceId: {InstanceId}",
                actionName, elapsedTime.TotalMilliseconds, InstanceId);
        }
    }

    /// <summary>
    /// Tracks the execution time of an asynchronous operation and logs it.
    /// </summary>
    /// <param name="actionName">The name of the action being tracked.</param>
    /// <param name="action">The asynchronous operation to execute.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task TrackExecutionTimeAsync(string actionName, Func<Task> action)
    {
        if (string.IsNullOrWhiteSpace(actionName)) throw new ArgumentException("Action name cannot be null or whitespace.", nameof(actionName));
        if (action == null) throw new ArgumentNullException(nameof(action));

        var startTime = DateTime.UtcNow;
        try
        {
            await action();
        }
        finally
        {
            var elapsedTime = DateTime.UtcNow - startTime;
            logger.LogInformation("Action '{ActionName}' executed in {ElapsedMilliseconds} ms | InstanceId: {InstanceId}",
                actionName, elapsedTime.TotalMilliseconds, InstanceId);
        }
    }

    /// <summary>
    /// Lifecycle hook for starting the service. Override this method to add custom startup logic.
    /// </summary>
    public virtual void OnStart()
    {
        logger.LogInformation("Service '{ServiceType}' started at {CreatedAt} | InstanceId: {InstanceId}",
            typeof(T).Name, CreatedAt, InstanceId);
    }

    /// <summary>
    /// Lifecycle hook for stopping the service. Override this method to add custom shutdown logic.
    /// </summary>
    public virtual void OnStop()
    {
        logger.LogInformation("Service '{ServiceType}' stopped at {StoppedAt} | InstanceId: {InstanceId}",
            typeof(T).Name, DateTime.UtcNow, InstanceId);
    }
}
