// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Abstractions.Primitives;

/// <summary>
/// Marker interface for strongly-typed entity identifiers.
/// </summary>
public interface IEntityId
{
    /// <summary>
    /// Gets the underlying value type.
    /// </summary>
    Type ValueType { get; }

    /// <summary>
    /// Gets the boxed value for infrastructure purposes.
    /// </summary>
    object BoxedValue { get; }
}
