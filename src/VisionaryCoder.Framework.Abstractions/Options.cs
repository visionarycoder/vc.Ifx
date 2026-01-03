// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Abstractions;

/// <summary>
/// Configuration options for the VisionaryCoder Framework.
/// </summary>
public class Options
{
    /// <summary>
    /// Gets or sets whether correlation ID generation is enabled.
    /// </summary>
    public bool EnableCorrelationId { get; set; } = true;

    /// <summary>
    /// Gets or sets whether request ID generation is enabled.
    /// </summary>
    public bool EnableRequestId { get; set; } = true;

    /// <summary>
    /// Gets or sets whether structured logging is enabled.
    /// </summary>
    public bool EnableStructuredLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets the default HTTP timeout in seconds.
    /// </summary>
    public int DefaultHttpTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the default cache expiration in minutes.
    /// </summary>
    public int DefaultCacheExpirationMinutes { get; set; } = 15;
}
