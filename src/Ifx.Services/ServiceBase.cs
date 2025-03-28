// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

#pragma warning disable CA1051
#pragma warning disable DerivedClasses
#pragma warning disable ClassWithData


namespace Ifx.Services;

/// <summary>
///     Represents the base class for services, providing common functionality such as logging, instance identification, and creation timestamp.
/// </summary>
/// <typeparam name="T">The type of the service.</typeparam>
public abstract class ServiceBase<T>(ILogger<T> logger)
{
    /// <summary>
    ///     The logger instance for logging information, warnings, and errors.
    /// </summary>
    protected internal readonly ILogger<T> logger = logger;

    /// <summary>
    ///     Gets the unique identifier for the service instance.
    /// </summary>
    public Guid InstanceId { get; } = Guid.NewGuid();

    /// <summary>
    ///     Gets the timestamp when the service instance was created.
    /// </summary>
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}