// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VisionaryCoder.Framework.Security;

/// <summary>
/// Extension methods for configuring password hashing services.
/// </summary>
public static class PasswordHashingServiceCollectionExtensions
{
    /// <summary>
    /// Adds password hashing services with default configuration (work factor: 11).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPasswordHashing(this IServiceCollection services)
    {
        return AddPasswordHashing(services, 11);
    }

    /// <summary>
    /// Adds password hashing services with custom work factor.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="workFactor">
    /// The BCrypt work factor (cost). Range: 4-31.
    /// Higher = more secure but slower. Recommended: 11-13.
    /// </param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when workFactor is out of range.</exception>
    public static IServiceCollection AddPasswordHashing(this IServiceCollection services, int workFactor)
    {
        if (workFactor < 4 || workFactor > 31)
        {
            throw new ArgumentOutOfRangeException(
                nameof(workFactor),
                workFactor,
                "Work factor must be between 4 and 31. Recommended: 11-13.");
        }

        services.TryAddSingleton<IPasswordHasher>(sp =>
        {
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<PasswordHasher>>();
            return new PasswordHasher(logger, workFactor);
        });

        return services;
    }
}
