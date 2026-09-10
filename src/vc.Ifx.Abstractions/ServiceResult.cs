namespace VisionaryCoder.Framework;

/// <summary>
/// Result for operations that don't return a value.
/// </summary>
/// <remarks>
/// Use this type to represent success/failure of operations that do not produce
/// a value. Factory helpers are provided for creating success and failure results.
/// The <see cref="Match"/> method provides a convenient way to branch on the
/// result without throwing exceptions.
/// </remarks>
public sealed class ServiceResult : ServiceResultBase
{
    private ServiceResult(bool isSuccess, string? errorMessage, Exception? exception)
        : base(isSuccess, errorMessage, exception)
    {
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static ServiceResult Success() => new(true, null, null);

    /// <summary>
    /// Creates a failure result with an error message.
    /// </summary>
    /// <param name="errorMessage">Human-readable error message describing the failure.</param>
    public static ServiceResult Failure(string errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);
        return new(false, errorMessage, null);
    }

    /// <summary>
    /// Creates a failure result from an exception. The exception's message is used
    /// as the <see cref="ServiceResultBase.ErrorMessage"/>.
    /// </summary>
    /// <param name="exception">The exception that caused the failure.</param>
    public static ServiceResult Failure(Exception exception)
    {
        ValidateFailureException(exception);
        return new(false, exception.Message, exception);
    }

    /// <summary>
    /// Creates a failure result with both a custom message and the originating exception.
    /// </summary>
    /// <param name="errorMessage">Human-readable error message describing the failure.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    public static ServiceResult Failure(string errorMessage, Exception exception)
    {
        ValidateFailureException(exception);
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);
        return new(false, errorMessage, exception);
    }

    /// <summary>
    /// Pattern-match the result: executes <paramref name="onSuccess"/> when successful,
    /// otherwise executes <paramref name="onFailure"/> with the error message and optional exception.
    /// </summary>
    /// <param name="onSuccess">Action to execute when the result is successful.</param>
    /// <param name="onFailure">Action to execute when the result is a failure. Receives the error message and optional exception.</param>
    public void Match(Action onSuccess, Action<string, Exception?> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        if (IsSuccess)
            onSuccess();
        else
            onFailure(ErrorMessage ?? "Unknown error", Exception);
    }
}

/// <summary>
/// Result for operations that return a value.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
/// <remarks>
/// Encapsulates the success/failure state and, when successful, the resulting value.
/// Provides helpers for mapping and transforming values in a safe manner that preserves
/// failure metadata.
/// Success is determined by IsSuccess, including legitimate null values. Mapping delegates
/// receive those values; ordinary mapper exceptions become failures and cancellation propagates.
/// </remarks>
public sealed class ServiceResult<T> : ServiceResultBase
{
    private ServiceResult(bool isSuccess, T? value, string? errorMessage, Exception? exception)
        : base(isSuccess, errorMessage, exception)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the result value if the operation was successful; otherwise the default value for <typeparamref name="T"/>.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Creates a successful result containing the given <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The successful result value; null is legitimate when T permits it.</param>
    public static ServiceResult<T> Success(T value) => new(true, value, null, null);

    /// <summary>
    /// Creates a failure result with an error message.
    /// </summary>
    /// <param name="errorMessage">Human-readable error message describing the failure.</param>
    public static ServiceResult<T> Failure(string errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);
        return new(false, default, errorMessage, null);
    }

    /// <summary>
    /// Creates a failure result from an exception.
    /// </summary>
    /// <param name="exception">The exception that caused the failure.</param>
    public static ServiceResult<T> Failure(Exception exception)
    {
        ValidateFailureException(exception);
        return new(false, default, exception.Message, exception);
    }

    /// <summary>
    /// Creates a failure result with both a custom message and the originating exception.
    /// </summary>
    /// <param name="errorMessage">Human-readable error message describing the failure.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    public static ServiceResult<T> Failure(string errorMessage, Exception exception)
    {
        ValidateFailureException(exception);
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);
        return new(false, default, errorMessage, exception);
    }

    /// <summary>
    /// Pattern-match the result: executes <paramref name="onSuccess"/> when successful,
    /// otherwise executes <paramref name="onFailure"/> with the error message and optional exception.
    /// </summary>
    /// <param name="onSuccess">Required action receiving the successful value, including null when T permits it.</param>
    /// <param name="onFailure">Action to execute when the result is a failure. Receives the error message and optional exception.</param>
    public void Match(Action<T> onSuccess, Action<string, Exception?> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        if (IsSuccess)
            onSuccess(Value!);
        else
            onFailure(ErrorMessage ?? "Unknown error", Exception);
    }

    /// <summary>
    /// Transforms the successful result value using <paramref name="mapper"/> into a new <see cref="ServiceResult{TNew}"/>.
    /// If the current result is a failure, the failure is propagated.
    /// </summary>
    /// <typeparam name="TNew">The type of the mapped value.</typeparam>
    /// <param name="mapper">Required function to transform the value, including legitimate null success values.</param>
    /// <returns>A new <see cref="ServiceResult{TNew}"/> containing the mapped value or a propagated failure.</returns>
    public ServiceResult<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        if (!IsSuccess)
            return new ServiceResult<TNew>(false, default, ErrorMessage, Exception);

        try
        {
            return ServiceResult<TNew>.Success(mapper(Value!));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ServiceResult<TNew>.Failure(ex);
        }
    }

    /// <summary>
    /// Asynchronously transforms the successful result value using <paramref name="mapper"/> into a new <see cref="ServiceResult{TNew}"/>.
    /// If the current result is a failure, the failure is propagated.
    /// </summary>
    /// <typeparam name="TNew">The type of the mapped value.</typeparam>
    /// <param name="mapper">Required asynchronous function; capture and forward cancellation to the underlying operation.</param>
    /// <returns>A task that produces a <see cref="ServiceResult{TNew}"/> containing the mapped value or a propagated failure.</returns>
    public async Task<ServiceResult<TNew>> MapAsync<TNew>(Func<T, Task<TNew>> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        if (!IsSuccess)
            return new ServiceResult<TNew>(false, default, ErrorMessage, Exception);

        try
        {
            TNew result = await mapper(Value!).ConfigureAwait(false);
            return ServiceResult<TNew>.Success(result);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ServiceResult<TNew>.Failure(ex);
        }
    }
}

