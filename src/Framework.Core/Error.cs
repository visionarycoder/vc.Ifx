// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework;

/// <summary>
/// Represents an error with a code and message.
/// </summary>
/// <param name="Code">The error code.</param>
/// <param name="Message">The error message.</param>
public record Error(string Code, string Message)
{
    /// <summary>
    /// Gets a default error indicating no error occurred.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Gets a default error indicating a null value was encountered.
    /// </summary>
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided");

    /// <summary>
    /// Creates a new validation error.
    /// </summary>
    public static Error Validation(string code, string message) => new(code, message);

    /// <summary>
    /// Creates a new not found error.
    /// </summary>
    public static Error NotFound(string code, string message) => new(code, message);

    /// <summary>
    /// Creates a new conflict error.
    /// </summary>
    public static Error Conflict(string code, string message) => new(code, message);

    /// <summary>
    /// Creates a new failure error.
    /// </summary>
    public static Error Failure(string code, string message) => new(code, message);
}
