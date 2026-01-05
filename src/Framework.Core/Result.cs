// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework;

/// <summary>
/// Represents the result of an operation that does not return a value.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error if the operation failed.
    /// </summary>
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException("Cannot create a successful result with an error");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException("Cannot create a failed result without an error");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a failed result with the specified error code and message.
    /// </summary>
    public static Result Failure(string code, string message) => new(false, new Error(code, message));

    /// <summary>
    /// Implicitly converts a Result to a boolean indicating success.
    /// </summary>
    public static implicit operator bool(Result result) => result.IsSuccess;
}


