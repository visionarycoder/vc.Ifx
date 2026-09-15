// Copyright (c) 2025 Ifx. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Ifx.Providers;
/// <summary>
/// Provides information about the Ifx.
/// </summary>
public interface IFrameworkInfoProvider
{

    /// <summary>
    /// Gets the current framework version.
    /// </summary>
    string Version { get; }

    /// Gets the framework name.
    string Name { get; }

    /// Gets the framework description.
    string Description { get; }

    /// Gets when the framework was compiled.
    DateTimeOffset CompiledAt { get; }
}
