// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Pipeline.Abstractions;

/// <summary>
/// Represents a span in distributed tracing, allowing tracking of operations across service boundaries.
/// </summary>
public interface ISpan : IDisposable
{
    /// <summary>
    /// Sets a tag (key-value pair) on the span for contextual information.
    /// </summary>
    /// <param name="key">The tag key.</param>
    /// <param name="value">The tag value.</param>
    void SetTag(string key, string value);

    /// <summary>
    /// Ends the span, marking the operation as complete.
    /// </summary>
    void End();
}
