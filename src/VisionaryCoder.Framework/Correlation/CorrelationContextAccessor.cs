// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Correlation;

/// <summary>
/// Accessor for correlation context using AsyncLocal storage.
/// </summary>
public interface ICorrelationContextAccessor
{
    /// <summary>
    /// Gets or sets the current correlation context.
    /// </summary>
    ICorrelationContext? CorrelationContext { get; set; }
}

/// <summary>
/// Default implementation using AsyncLocal storage.
/// </summary>
public sealed class CorrelationContextAccessor : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<ICorrelationContext?> CorrelationContextCurrent = new();

    public ICorrelationContext? CorrelationContext
    {
        get => CorrelationContextCurrent.Value;
        set => CorrelationContextCurrent.Value = value;
    }
}
