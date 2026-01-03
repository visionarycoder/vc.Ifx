// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Abstractions.Proxy.Exceptions;

/// <summary>
/// Represents a transport exception that can be retried.
/// </summary>
[Serializable]
public class RetryableTransportException : ProxyException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetryableTransportException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RetryableTransportException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryableTransportException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public RetryableTransportException(string message, Exception innerException) : base(message, innerException) { }
}
