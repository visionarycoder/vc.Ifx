// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Abstractions.Proxy;

/// <summary>
/// Represents a proxy context containing metadata about the proxy operation.
/// </summary>
public class ProxyContext
{
    /// <summary>
    /// Gets or sets the operation identifier.
    /// </summary>
    public string OperationId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the method name being proxied.
    /// </summary>
    public string? MethodName { get; set; }

    /// <summary>
    /// Gets or sets the service name.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Gets or sets additional properties for the operation.
    /// </summary>
    public Dictionary<string, object?> Properties { get; set; } = new();

    /// <summary>
    /// Gets or sets the correlation identifier.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the start time of the operation.
    /// </summary>
    public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the HTTP method.
    /// </summary>
    public string? Method { get; set; }

    /// <summary>
    /// Gets or sets the request URL.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the request headers.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>
    /// Gets or sets the request body.
    /// </summary>
    public object? Body { get; set; }

    /// <summary>
    /// Gets or sets the request object.
    /// </summary>
    public object? Request { get; set; }

    /// <summary>
    /// Gets or sets additional items for the context.
    /// </summary>
    public Dictionary<string, object?> Items { get; set; } = new();

    /// <summary>
    /// Gets or sets metadata for the operation.
    /// </summary>
    public Dictionary<string, object?> Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the operation name.
    /// </summary>
    public string? OperationName { get; set; }

    /// <summary>
    /// Gets or sets the result type.
    /// </summary>
    public Type? ResultType { get; set; }

    /// <summary>
    /// Gets or sets the request identifier.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Gets or sets the cancellation token.
    /// </summary>
    public CancellationToken CancellationToken { get; set; }
}
