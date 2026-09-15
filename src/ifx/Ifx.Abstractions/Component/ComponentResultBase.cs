using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;

namespace Ifx.Component;

/// <summary>
/// Base result class for all operation outcomes.
/// </summary>
public abstract class ComponentResultBase(bool isSuccess, string? errorMessage, Exception? exception)
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; } = isSuccess;

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; } = errorMessage;

    /// <summary>
    /// Gets the exception if the operation failed with an exception.
    /// </summary>
    public Exception? Exception { get; } = exception;

    internal static void ValidateFailureException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        if (exception is OperationCanceledException)
            ExceptionDispatchInfo.Throw(exception);
    }
}

