namespace Ifx.Component;

/// <summary>
/// Result for operations that don't return a value.
/// </summary>
/// <remarks>
/// Use this type to represent success/failure of operations that do not produce
/// a value. Factory helpers are provided for creating success and failure results.
/// The <see cref="Match"/> method provides a convenient way to branch on the
/// result without throwing exceptions.
/// </remarks>
public sealed class ComponentResult : ComponentResultBase
{
    private ComponentResult(bool isSuccess, string? errorMessage, Exception? exception)
        : base(isSuccess, errorMessage, exception)
    {
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static ComponentResult Success() => new(true, null, null);

    /// <summary>
    /// Creates a failure result with an error message.
    /// </summary>
    /// <param name="errorMessage">Human-readable error message describing the failure.</param>
    public static ComponentResult Failure(string errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);
        return new(false, errorMessage, null);
    }

    /// <summary>
    /// Creates a failure result from an exception. The exception's message is used
    /// as the <see cref="ServiceResultBase.ErrorMessage"/>.
    /// </summary>
    /// <param name="exception">The exception that caused the failure.</param>
    public static ComponentResult Failure(Exception exception)
    {
        ValidateFailureException(exception);
        return new(false, exception.Message, exception);
    }

    /// <summary>
    /// Creates a failure result with both a custom message and the originating exception.
    /// </summary>
    /// <param name="errorMessage">Human-readable error message describing the failure.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    public static ComponentResult Failure(string errorMessage, Exception exception)
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


