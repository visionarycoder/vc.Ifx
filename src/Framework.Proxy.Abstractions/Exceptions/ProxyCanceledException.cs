// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Abstractions.Exceptions;

/// <summary>
/// Represents a proxy operation that was cancelled.
/// </summary>
[Serializable]
public class ProxyCanceledException : ProxyException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyCanceledException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ProxyCanceledException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyCanceledException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ProxyCanceledException(string message, Exception innerException) : base(message, innerException) { }
}
