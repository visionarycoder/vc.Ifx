using Microsoft.Extensions.Logging;

using VisionaryCoder.Framework.Pipeline.Abstractions;

namespace VisionaryCoder.Framework.Pipeline;

/// <summary>
/// Pipeline behavior that logs command and query execution.
/// Logs before execution, after successful execution, and on exceptions.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response being returned.</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            TResponse response = await next().ConfigureAwait(false);
            logger.LogInformation("Successfully handled {RequestName}", requestName);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling {RequestName}: {ErrorMessage}", requestName, ex.Message);
            throw;
        }
    }
}
