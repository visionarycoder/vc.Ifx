using Polly;
using Polly.Retry;

namespace Util.Messaging;

/// <summary>
/// Executes transient operations with bounded exponential backoff.
/// </summary>
public static class RetryPolicy
{
    /// <summary>
    /// Executes an operation with retries and exponential backoff.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="maxAttempts">The maximum number of total attempts, including the first execution.</param>
    /// <param name="initialDelay">The initial delay before the first retry.</param>
    /// <param name="cancellationToken">The cancellation token for the retry pipeline.</param>
    /// <returns>A task that completes when the operation succeeds or the final failure is rethrown.</returns>
    public static Task ExecuteAsync(
        Func<Task> operation,
        int maxAttempts,
        TimeSpan initialDelay,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        return ExecuteAsync(
            _ => operation(),
            maxAttempts,
            initialDelay,
            cancellationToken);
    }

    /// <summary>
    /// Executes an operation with retries and exponential backoff.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="maxAttempts">The maximum number of total attempts, including the first execution.</param>
    /// <param name="initialDelay">The initial delay before the first retry.</param>
    /// <param name="cancellationToken">The cancellation token for the retry pipeline.</param>
    /// <returns>A task that completes when the operation succeeds or the final failure is rethrown.</returns>
    public static async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        int maxAttempts,
        TimeSpan initialDelay,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);

        if (initialDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(initialDelay), "Initial delay cannot be negative.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        ResiliencePipeline pipeline = maxAttempts == 1
            ? ResiliencePipeline.Empty
            : new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = maxAttempts - 1,
                    Delay = initialDelay,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = false,
                    ShouldHandle = static arguments => ValueTask.FromResult(
                        !arguments.Context.CancellationToken.IsCancellationRequested
                        && arguments.Outcome.Exception is not null
                        && arguments.Outcome.Exception is not OperationCanceledException)
                })
                .Build();

        await pipeline.ExecuteAsync(
            async token => await operation(token).ConfigureAwait(false),
            cancellationToken).ConfigureAwait(false);
    }
}
