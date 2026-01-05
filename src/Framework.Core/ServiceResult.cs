// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

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
public class ServiceResult(bool isSuccess = false, string? errorMessage = null, Exception? exception = null)
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

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static ServiceResult Success() => new(true, null, null);

    /// <summary>
    /// Creates a failure result with an error message.
    /// </summary>
    /// <param name="errorMessage">Human-readable error message describing the failure.</param>
    public static ServiceResult Failure(string errorMessage) => new(false, errorMessage, null);

    /// <summary>
    /// Creates a failure result from an exception. The exception's message is used
    /// as the <see cref="ErrorMessage"/>.
    /// </summary>
    /// <param name="exception">The exception that caused the failure.</param>
    public static ServiceResult Failure(Exception exception) => new(false, exception?.Message, exception);

    /// <summary>
    /// Creates a failure result with both a custom message and the originating exception.
    /// </summary>
    /// <param name="errorMessage">Human-readable error message describing the failure.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    public static ServiceResult Failure(string errorMessage, Exception exception) => new(false, errorMessage, exception);

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

