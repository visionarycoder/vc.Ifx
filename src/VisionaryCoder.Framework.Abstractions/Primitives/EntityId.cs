// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;

namespace VisionaryCoder.Framework.Abstractions.Primitives;

/// <summary>
/// Strongly-typed entity identifier.
/// </summary>
/// <typeparam name="TEntity">The entity type this identifier belongs to.</typeparam>
/// <typeparam name="TKey">The underlying key type (e.g., Guid, int, string).</typeparam>
public readonly record struct EntityId<TEntity, TKey>(TKey Value) : IEntityId
    where TEntity : class
    where TKey : notnull
{
    /// <summary>
    /// Creates a new entity identifier with validation.
    /// </summary>
    /// <param name="value">The underlying key value.</param>
    /// <returns>A validated EntityId.</returns>
    /// <exception cref="ArgumentException">Thrown when the value is invalid.</exception>
    public static EntityId<TEntity, TKey> Create(TKey value)
    {
        if (EqualityComparer<TKey>.Default.Equals(value, default!))
            throw new ArgumentException("ID cannot be the default value.", nameof(value));

        if (value is string s && string.IsNullOrWhiteSpace(s))
            throw new ArgumentException("ID cannot be empty/whitespace.", nameof(value));

        return new(value);
    }

    /// <summary>
    /// Returns the string representation of the identifier.
    /// </summary>
    public override string ToString() => Value?.ToString() ?? string.Empty;

    // IEntityId implementation for infrastructure
    Type IEntityId.ValueType => typeof(TKey);
    object IEntityId.BoxedValue => Value;

    // Implicit conversion from TKey to EntityId
    public static implicit operator EntityId<TEntity, TKey>(TKey value) => Create(value);

    // Explicit conversion from EntityId to TKey
    public static explicit operator TKey(EntityId<TEntity, TKey> id) => id.Value;

    /// <summary>
    /// Parses a string into an EntityId.
    /// </summary>
    /// <param name="text">The string to parse.</param>
    /// <returns>A parsed EntityId.</returns>
    /// <exception cref="FormatException">Thrown when the text cannot be parsed.</exception>
    public static EntityId<TEntity, TKey> Parse(string text) => TryParse(text, out EntityId<TEntity, TKey> id)
        ? id
        : throw new FormatException($"Invalid {typeof(TKey).Name}.");

    /// <summary>
    /// Tries to parse a string into an EntityId.
    /// </summary>
    /// <param name="text">The string to parse.</param>
    /// <param name="id">The parsed EntityId if successful.</param>
    /// <returns>True if parsing succeeded; otherwise false.</returns>
    public static bool TryParse(string text, out EntityId<TEntity, TKey> id)
    {
        id = default;

        if (typeof(TKey) == typeof(Guid) && Guid.TryParse(text, out Guid g))
        {
            id = new((TKey)(object)g);
            return true;
        }

        if (typeof(TKey) == typeof(string))
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                id = new((TKey)(object)text);
                return true;
            }
            return false;
        }

        if (typeof(TKey) == typeof(int) && int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i))
        {
            id = new((TKey)(object)i);
            return true;
        }

        if (typeof(TKey) == typeof(long) && long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l))
        {
            id = new((TKey)(object)l);
            return true;
        }

        if (typeof(TKey) == typeof(short) && short.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out short s))
        {
            id = new((TKey)(object)s);
            return true;
        }

        return false;
    }
}
