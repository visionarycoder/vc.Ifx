using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Core;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Telemetry;

/// <summary>
/// Logs method invocation timing and exceptions.
/// Measures the execution time of each method call and logs success or failure with duration.
/// </summary>
public sealed class TelemetryInterceptor(ILogger<TelemetryInterceptor> logger) : IProxyInterceptor
{
    /// <summary>
    /// Executes the collector to measure and log method invocation telemetry.
    /// </summary>
    public async ValueTask<object?> InvokeAsync(MethodContext context, HandlerDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await next().ConfigureAwait(false);

            // Log successful completion with elapsed time
            logger.LogDebug(
                "{Contract}.{Method} completed in {ElapsedMilliseconds} ms",
                context.ContractType.Name,
                context.MethodInfo.Name,
                stopwatch.Elapsed.TotalMilliseconds);

            return result;
        }
        catch (Exception exception)
        {
            // Log failure with exception and elapsed time
            logger.LogError(
                exception,
                "{Contract}.{Method} failed after {ElapsedMilliseconds} ms",
                context.ContractType.Name,
                context.MethodInfo.Name,
                stopwatch.Elapsed.TotalMilliseconds);

            throw;
        }
    }
}
