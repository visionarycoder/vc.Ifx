// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Identity;

/// <summary>
/// Service for secure password hashing and verification using BCrypt algorithm.
/// </summary>
/// <remarks>
/// BCrypt is a password hashing function designed to be slow and computationally expensive,
/// making it resistant to brute-force attacks. It automatically handles salt generation
/// and includes the salt in the hash output.
/// 
/// Security features:
/// - Automatic salt generation per password
/// - Configurable work factor (cost)
/// - Resistant to rainbow table attacks
/// - Time-constant verification to prevent timing attacks
/// </remarks>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plaintext password using BCrypt with the default work factor.
    /// </summary>
    /// <param name="password">The plaintext password to hash.</param>
    /// <returns>The hashed password including salt and work factor.</returns>
    /// <exception cref="ArgumentNullException">Thrown when password is null.</exception>
    /// <exception cref="ArgumentException">Thrown when password is empty or whitespace.</exception>
    string HashPassword(string password);

    /// <summary>
    /// Hashes a plaintext password using BCrypt with a custom work factor.
    /// </summary>
    /// <param name="password">The plaintext password to hash.</param>
    /// <param name="workFactor">
    /// The work factor (cost) determining how many iterations to perform.
    /// Range: 4-31. Higher values are more secure but slower.
    /// Default: 11 (recommended for most applications).
    /// </param>
    /// <returns>The hashed password including salt and work factor.</returns>
    /// <exception cref="ArgumentNullException">Thrown when password is null.</exception>
    /// <exception cref="ArgumentException">Thrown when password is empty/whitespace or workFactor is out of range.</exception>
    string HashPassword(string password, int workFactor);

    /// <summary>
    /// Verifies a plaintext password against a BCrypt hash.
    /// </summary>
    /// <param name="password">The plaintext password to verify.</param>
    /// <param name="hashedPassword">The BCrypt hash to verify against.</param>
    /// <returns>True if the password matches the hash; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when password or hashedPassword is null.</exception>
    /// <exception cref="ArgumentException">Thrown when password or hashedPassword is empty/whitespace.</exception>
    bool VerifyPassword(string password, string hashedPassword);

    /// <summary>
    /// Determines if a password hash needs to be rehashed due to a different work factor.
    /// </summary>
    /// <param name="hashedPassword">The BCrypt hash to check.</param>
    /// <param name="desiredWorkFactor">The desired work factor.</param>
    /// <returns>True if the hash should be regenerated with the new work factor; otherwise, false.</returns>
    /// <remarks>
    /// Use this method to implement password upgrade strategies when increasing security over time.
    /// Typically called after successful password verification.
    /// </remarks>
    bool NeedsRehash(string hashedPassword, int desiredWorkFactor);
}
