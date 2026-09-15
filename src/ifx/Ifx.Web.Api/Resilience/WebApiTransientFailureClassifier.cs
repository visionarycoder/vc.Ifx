using Ifx.Web.Api.Responses;
using Polly.Timeout;

namespace Ifx.Web.Api.Resilience;

/// <summary>Conservative default exception classification for explicitly replay-safe operations.</summary>
public sealed class WebApiTransientFailureClassifier : IWebApiTransientFailureClassifier
{
    /// <inheritdoc />
    public bool IsTransient(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception switch
        {
            HttpRequestException { StatusCode: null } => true,
            HttpRequestException http => HttpResponseCatalog.TryGet((int)http.StatusCode!.Value, out var entry)
                && entry.Retryability == HttpRetryability.PotentiallyTransient,
            TimeoutRejectedException or TimeoutException => true,
            _ => false
        };
    }
}
