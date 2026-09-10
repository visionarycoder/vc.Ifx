using System.Diagnostics;
using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Core;
using Wa.Wsdot.Fin.Idl.Ifx.Services.Messaging;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Correlation;

/// <summary>
/// Establishes a correlation identifier for tracing invocations.
/// Attempts to extract correlation ID from IServiceMessage arguments, falls back to Activity.Current, or generates a new one.
/// </summary>
public sealed class CorrelationInterceptor : IProxyInterceptor
{
    /// <summary>
    /// Executes the provider to establish a correlation identifier in the invocation context.
    /// </summary>
    public async ValueTask<object?> InvokeAsync(MethodContext context, HandlerDelegate next)
    {
        // Attempt to extract correlation ID from service message arguments
        var correlationId = FindServiceMessage(context.Arguments)?.CorrelationId.ToString()
            // Fall back to the current Activity trace ID for distributed tracing
            ?? Activity.Current?.TraceId.ToString()
            // Generate a new correlation ID if none is available
            ?? Guid.NewGuid().ToString();

        // Store the correlation ID in the invocation context for use by other interceptors
        context.Items[InterceptorConstants.CorrelationId] = correlationId;

        return await next().ConfigureAwait(false);
    }

    /// <summary>
    /// Searches the invocation arguments for an IServiceMessage instance.
    /// Also inspects properties of argument objects to find nested service messages.
    /// </summary>
    private static IServiceMessage? FindServiceMessage(IEnumerable<MethodArgument> arguments)
    {
        foreach (var argument in arguments)
        {
            if (argument.Value is IServiceMessage serviceMessage)
                return serviceMessage;

            if (argument.Value is null)
                continue;

            // Search nested properties for a service message
            foreach (var property in argument.Value.GetType().GetProperties())
            {
                if (property.GetIndexParameters().Length > 0)
                    continue;

                if (property.GetValue(argument.Value) is IServiceMessage nestedMessage)
                    return nestedMessage;
            }
        }

        return null;
    }
}
