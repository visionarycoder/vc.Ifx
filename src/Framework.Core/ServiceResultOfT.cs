namespace VisionaryCoder.Framework;

/// <summary>
/// Result for operations that return a value.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
/// <remarks>
/// Encapsulates the success/failure state and, when successful, the resulting value.
/// Provides helpers for mapping and transforming values in a safe manner that preserves
/// failure metadata.
/// </remarks>
public class ServiceResult<T>(bool isSuccess, T? value, string? errorMessage, Exception? exception) : ServiceResult(isSuccess, errorMessage, exception)
{

    /// <summary>
    /// Gets the result value if the operation was successful; otherwise the default value for <typeparamref name="T"/>.
    /// </summary>
    public T? Value { get; } = value;

    /// <summary>
    /// Creates a successful result containing the given <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The successful result value.</param>
    public static ServiceResult<T> Success(T value) => new(true, value, null, null);

    /// <summary>
    /// Creates a failure result with an error message.
    /// </summary>
    /// <param name="errorMessage">Human-readable error message describing the failure.</param>
    public static ServiceResult<T> Failure(string errorMessage) => new(false, default, errorMessage, null);

    /// <summary>
    /// Creates a failure result from an exception.
    /// </summary>
    /// <param name="exception">The exception that caused the failure.</param>
    public static ServiceResult<T> Failure(Exception exception) => new(false, default, exception?.Message, exception);

    /// <summary>
    /// Creates a failure result with both a custom message and the originating exception.
    /// </summary>
    /// <param name="errorMessage">Human-readable error message describing the failure.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    public static ServiceResult<T> Failure(string errorMessage, Exception exception) => new(false, default, errorMessage, exception);

    /// <summary>
    /// Pattern-match the result: executes <paramref name="onSuccess"/> when successful,
    /// otherwise executes <paramref name="onFailure"/> with the error message and optional exception.
    /// </summary>
    /// <param name="onSuccess">Action to execute when the result is successful. Receives the successful value.</param>
    /// <param name="onFailure">Action to execute when the result is a failure. Receives the error message and optional exception.</param>
    public void Match(Action<T> onSuccess, Action<string, Exception?> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        if (IsSuccess && Value is not null)
            onSuccess(Value);
        else
            onFailure(ErrorMessage ?? "Unknown error", Exception);
    }

    /// <summary>
    /// Transforms the successful result value using <paramref name="mapper"/> into a new <see cref="ServiceResult{TNew}"/>.
    /// If the current result is a failure, the failure is propagated.
    /// </summary>
    /// <typeparam name="TNew">The type of the mapped value.</typeparam>
    /// <param name="mapper">Function to transform the value.</param>
    /// <returns>A new <see cref="ServiceResult{TNew}"/> containing the mapped value or a propagated failure.</returns>
    public ServiceResult<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        if (!IsSuccess || Value is null)
        {
            return Exception is not null
                ? ServiceResult<TNew>.Failure(ErrorMessage ?? "Value is null", Exception)
                : ServiceResult<TNew>.Failure(ErrorMessage ?? "Value is null");
        }

        try
        {
            return ServiceResult<TNew>.Success(mapper(Value));
        }
        catch (Exception ex)
        {
            return ServiceResult<TNew>.Failure(ex);
        }
    }

    /// <summary>
    /// Asynchronously transforms the successful result value using <paramref name="mapper"/> into a new <see cref="ServiceResult{TNew}"/>.
    /// If the current result is a failure, the failure is propagated.
    /// </summary>
    /// <typeparam name="TNew">The type of the mapped value.</typeparam>
    /// <param name="mapper">Asynchronous function to transform the value.</param>
    /// <returns>A task that produces a <see cref="ServiceResult{TNew}"/> containing the mapped value or a propagated failure.</returns>
    public async Task<ServiceResult<TNew>> MapAsync<TNew>(Func<T, Task<TNew>> mapper)
    {
        if (!IsSuccess || Value is null)
        {
            return Exception is not null
                ? ServiceResult<TNew>.Failure(ErrorMessage ?? "Value is null", Exception)
                : ServiceResult<TNew>.Failure(ErrorMessage ?? "Value is null");
        }

        try
        {
            TNew result = await mapper(Value).ConfigureAwait(false);
            return ServiceResult<TNew>.Success(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<TNew>.Failure(ex);
        }
    }
}
