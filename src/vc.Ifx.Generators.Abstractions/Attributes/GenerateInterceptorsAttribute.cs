using System;

namespace vc.Ifx.Generators.Abstractions.Attributes;

/// <summary>Declares a class for service-interceptor generation.</summary>
/// <remarks>Values and the caller-owned service array are retained without validation or copying.</remarks>
/// <example><code>[GenerateInterceptors("Orders", typeof(IDisposable))] class OrderService { }</code></example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class GenerateInterceptorsAttribute : Attribute
{
    /// <summary>Initializes an activity source name and the caller-owned service-type array.</summary>
    /// <param name="activitySourceName">The activity source name, stored verbatim.</param>
    /// <param name="serviceTypes">Service types to intercept; omitted params produce an empty array.</param>
    public GenerateInterceptorsAttribute(string activitySourceName, params Type[] serviceTypes)
    {
        ActivitySourceName = activitySourceName;
        ServiceTypes = serviceTypes;
    }

    /// <summary>Gets the declared activity source name.</summary>
    public string ActivitySourceName { get; }

    /// <summary>Gets the caller-owned service-type array.</summary>
    public Type[] ServiceTypes { get; }

    /// <summary>Gets or sets the generated suffix; defaults to OpenTelemetryInterceptor.</summary>
    public string InterceptorSuffix { get; set; } = "OpenTelemetryInterceptor";

    /// <summary>Gets or sets the compatibility alias for <see cref="InterceptorSuffix"/>.</summary>
    public string DecoratorSuffix
    {
        get => InterceptorSuffix;
        set => InterceptorSuffix = value;
    }
}
