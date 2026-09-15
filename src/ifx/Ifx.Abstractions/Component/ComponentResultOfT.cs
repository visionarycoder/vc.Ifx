using System;
using System.Collections.Generic;
using System.Text;

namespace Ifx.Component;

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
public sealed class ComponentResult<T> : ComponentResultBase
{
    private ComponentResult(bool isSuccess, T? value, string? errorMessage, Exception? exception)
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
    public static ComponentResult<T> Success(T value) => new(true, value, null, null);

    /// <summary>
    /// Creates a failure result with an error message.
    /// </summary>
    /// <param name="errorMessage">Human-readable error message describing the failure.</param>
    public static ComponentResult<T> Failure(string errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);
        return new(false, default, errorMessage, null);
    }

    /// <summary>
    /// Creates a failure result from an exception.
    /// </summary>
    /// <param name="exception">The exception that caused the failure.</param>
    public static ComponentResult<T> Failure(Exception exception)
    {
        ValidateFailureException(exception);
        return new(false, default, exception.Message, exception);
    }

    /// <summary>
    /// Creates a failure result with both a custom message and the originating exception.
    /// </summary>
    /// <param name="errorMessage">Human-readable error message describing the failure.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    public static ComponentResult<T> Failure(string errorMessage, Exception exception)
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
    /// Transforms the successful result value using <paramref name="mapper"/> into a new <see cref="ComponentResult{TNew}"/>.
    /// If the current result is a failure, the failure is propagated.
    /// </summary>
    /// <typeparam name="TNew">The type of the mapped value.</typeparam>
    /// <param name="mapper">Required function to transform the value, including legitimate null success values.</param>
    /// <returns>A new <see cref="ComponentResult{TNew}"/> containing the mapped value or a propagated failure.</returns>
    public ComponentResult<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        if (!IsSuccess)
            return new ComponentResult<TNew>(false, default, ErrorMessage, Exception);

        try
        {
            return ComponentResult<TNew>.Success(mapper(Value!));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ComponentResult<TNew>.Failure(ex);
        }
    }

    /// <summary>
    /// Asynchronously transforms the successful result value using <paramref name="mapper"/> into a new <see cref="ComponentResult{TNew}"/>.
    /// If the current result is a failure, the failure is propagated.
    /// </summary>
    /// <typeparam name="TNew">The type of the mapped value.</typeparam>
    /// <param name="mapper">Required asynchronous function; capture and forward cancellation to the underlying operation.</param>
    /// <returns>A task that produces a <see cref="ComponentResult{TNew}"/> containing the mapped value or a propagated failure.</returns>
    public async Task<ComponentResult<TNew>> MapAsync<TNew>(Func<T, Task<TNew>> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        if (!IsSuccess)
            return new ComponentResult<TNew>(false, default, ErrorMessage, Exception);

        try
        {
            TNew result = await mapper(Value!).ConfigureAwait(false);
            return ComponentResult<TNew>.Success(result);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ComponentResult<TNew>.Failure(ex);
        }
    }
}
