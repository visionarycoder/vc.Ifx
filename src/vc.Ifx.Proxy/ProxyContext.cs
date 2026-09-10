// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy;
/// <summary>
/// Represents a proxy context containing metadata about the proxy operation.
/// </summary>
public class ProxyContext
{
    /// <summary>
    /// Gets or sets the operation identifier.
    /// </summary>
    public string OperationId { get; set; } = Guid.NewGuid().ToString();
    /// Gets or sets the method name being proxied.
    public string? MethodName { get; set; }
    /// Gets or sets the service name.
    public string? ServiceName { get; set; }
    /// Gets or sets additional properties for the operation.
    public Dictionary<string, object?> Properties { get; set; } = new();
    /// Gets or sets the correlation identifier.
    public string? CorrelationId { get; set; }
    /// Gets or sets the start time of the operation.
    public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;
    /// Gets or sets the HTTP method.
    public string? Method { get; set; }
    /// Gets or sets the request URL.
    public string? Url { get; set; }
    /// Gets or sets the request headers.
    public Dictionary<string, string> Headers { get; set; } = new();
    /// Gets or sets the request body.
    public object? Body { get; set; }
    /// Gets or sets the request object.
    public object? Request { get; set; }
    /// Gets or sets additional items for the context.
    public Dictionary<string, object?> Items { get; set; } = new();
    /// Gets or sets metadata for the operation.
    public Dictionary<string, object?> Metadata { get; set; } = new();
    /// Gets or sets the operation name.
    public string? OperationName { get; set; }
    /// Gets or sets the result type.
    public Type? ResultType { get; set; }
    /// Gets or sets the request identifier.
    public string? RequestId { get; set; }
    /// Gets or sets the cancellation token.
    public CancellationToken CancellationToken { get; set; }
}