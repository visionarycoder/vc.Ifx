// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Auditing;

/// <summary>
/// Represents an audit record for proxy operations.
/// </summary>
public class AuditRecord
{
    /// <summary>
    /// Gets or sets the operation identifier.
    /// </summary>
    public string OperationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the method name.
    /// </summary>
    public string MethodName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the operation name (alias for MethodName).
    /// </summary>
    public string OperationName
    {
        get => MethodName;
        set => MethodName = value;
    }

    /// <summary>
    /// Gets or sets the operation result.
    /// </summary>
    public object? Result { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the operation was recorded.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the request identifier.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the operation completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the exception type if an exception occurred.
    /// </summary>
    public string? ExceptionType { get; set; }

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the user agent.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets the IP address.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method.
    /// </summary>
    public string? Method { get; set; }

    /// <summary>
    /// Gets or sets the URL.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the operation started.
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Gets or sets the request headers.
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Gets or sets the request size in bytes.
    /// </summary>
    public long? RequestSize { get; set; }

    /// <summary>
    /// Gets or sets the response size in bytes.
    /// </summary>
    public long? ResponseSize { get; set; }

    /// <summary>
    /// Gets or sets the duration of the operation.
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// Gets or sets whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the correlation identifier.
    /// </summary>
    public string? CorrelationId { get; set; }
}
