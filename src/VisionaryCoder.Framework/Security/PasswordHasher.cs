// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Logging;

namespace VisionaryCoder.Framework.Security;

/// <summary>
/// Default implementation of password hashing using BCrypt.Net algorithm.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private readonly ILogger<PasswordHasher> logger;
    private readonly int defaultWorkFactor;

    /// <summary>
    /// Initializes a new instance with default work factor of 11.
    /// </summary>
    public PasswordHasher(ILogger<PasswordHasher> logger)
        : this(logger, 11)
    {
    }

    /// <summary>
    /// Initializes a new instance with a specified default work factor.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="defaultWorkFactor">Default work factor (4-31). Recommended: 11-13.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when workFactor is out of valid range.</exception>
    public PasswordHasher(ILogger<PasswordHasher> logger, int defaultWorkFactor)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (defaultWorkFactor < 4 || defaultWorkFactor > 31)
        {
            throw new ArgumentOutOfRangeException(
                nameof(defaultWorkFactor),
                defaultWorkFactor,
                "Work factor must be between 4 and 31. Recommended: 11-13.");
        }

        this.defaultWorkFactor = defaultWorkFactor;
        logger.LogDebug("PasswordHasher initialized with work factor {WorkFactor}", defaultWorkFactor);
    }

    /// <inheritdoc />
    public string HashPassword(string password)
    {
        return HashPassword(password, defaultWorkFactor);
    }

    /// <inheritdoc />
    public string HashPassword(string password, int workFactor)
    {
        ArgumentNullException.ThrowIfNull(password);

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be empty or whitespace.", nameof(password));
        }

        if (workFactor < 4 || workFactor > 31)
        {
            throw new ArgumentOutOfRangeException(
                nameof(workFactor),
                workFactor,
                "Work factor must be between 4 and 31.");
        }

        try
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor);
            logger.LogDebug("Password hashed successfully with work factor {WorkFactor}", workFactor);
            return hash;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to hash password");
            throw;
        }
    }

    /// <inheritdoc />
    public bool VerifyPassword(string password, string hashedPassword)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(hashedPassword);

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be empty or whitespace.", nameof(password));
        }

        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            throw new ArgumentException("Hashed password cannot be empty or whitespace.", nameof(hashedPassword));
        }

        try
        {
            bool isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            logger.LogDebug("Password verification result: {IsValid}", isValid);
            return isValid;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Password verification failed due to exception");
            return false;
        }
    }

    /// <inheritdoc />
    public bool NeedsRehash(string hashedPassword, int desiredWorkFactor)
    {
        ArgumentNullException.ThrowIfNull(hashedPassword);

        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            throw new ArgumentException("Hashed password cannot be empty or whitespace.", nameof(hashedPassword));
        }

        if (desiredWorkFactor < 4 || desiredWorkFactor > 31)
        {
            throw new ArgumentOutOfRangeException(
                nameof(desiredWorkFactor),
                desiredWorkFactor,
                "Work factor must be between 4 and 31.");
        }

        try
        {
            bool needsRehash = BCrypt.Net.BCrypt.PasswordNeedsRehash(hashedPassword, desiredWorkFactor);
            
            if (needsRehash)
            {
                logger.LogInformation(
                    "Password hash needs rehashing to work factor {DesiredWorkFactor}",
                    desiredWorkFactor);
            }

            return needsRehash;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to check if password needs rehash");
            // Return true on error to trigger rehash
            return true;
        }
    }
}
