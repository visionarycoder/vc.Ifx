using System;

namespace vc.Ifx.Generators.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class GenerateInterceptorsAttribute : Attribute
{
    public GenerateInterceptorsAttribute(string activitySourceName, params Type[] serviceTypes)
    {
        ActivitySourceName = activitySourceName;
        ServiceTypes = serviceTypes;
    }

    public string ActivitySourceName { get; }

    public Type[] ServiceTypes { get; }

    public string InterceptorSuffix { get; set; } = "OpenTelemetryInterceptor";

    public string DecoratorSuffix
    {
        get => InterceptorSuffix;
        set => InterceptorSuffix = value;
    }
}
