// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Reflection;

namespace VisionaryCoder.Framework.Core.Models;

/// <summary>
/// Base class for creating type-safe enumerations with behavior.
/// Provides compile-time type safety and run-time flexibility.
/// </summary>
/// <remarks>
/// Use this pattern when you need:
/// - Enums with behavior/methods
/// - Enums with additional properties
/// - Compile-time type safety
/// - Database-friendly int IDs
/// 
/// Example:
/// <code>
/// public class OrderStatus : Enumeration
/// {
///     public static readonly OrderStatus Pending = new(1, nameof(Pending));
///     public static readonly OrderStatus Confirmed = new(2, nameof(Confirmed));
///     public static readonly OrderStatus Shipped = new(3, nameof(Shipped));
///     
///     protected OrderStatus(int id, string name) : base(id, name) { }
/// }
/// </code>
/// </remarks>
public abstract class Enumeration(int id, string name) : IComparable
{
    /// <summary>
    /// Gets the display name of this enumeration value.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the unique identifier for this enumeration value.
    /// </summary>
    public int Id { get; } = id;

    /// <summary>
    /// Returns the name of this enumeration value.
    /// </summary>
    public override string ToString() => Name;

    /// <summary>
    /// Gets all defined values for the specified enumeration type.
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <returns>All defined enumeration values.</returns>
    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
        return typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null))
            .Cast<T>();
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current enumeration value.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
        {
            return false;
        }

        bool typeMatches = GetType() == obj.GetType();
        bool valueMatches = Id.Equals(otherValue.Id);
        
        return typeMatches && valueMatches;
    }

    /// <summary>
    /// Returns the hash code for this enumeration value.
    /// </summary>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Calculates the absolute difference between two enumeration values.
    /// </summary>
    public static int AbsoluteDifference(Enumeration firstValue, Enumeration secondValue)
    {
        return Math.Abs(firstValue.Id - secondValue.Id);
    }

    /// <summary>
    /// Gets the enumeration value with the specified ID.
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <param name="value">The ID to search for.</param>
    /// <returns>The matching enumeration value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no matching value is found.</exception>
    public static T FromValue<T>(int value) where T : Enumeration
    {
        return Parse<T, int>(value, "value", item => item.Id == value);
    }

    /// <summary>
    /// Gets the enumeration value with the specified display name.
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <param name="displayName">The display name to search for.</param>
    /// <returns>The matching enumeration value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no matching value is found.</exception>
    public static T FromDisplayName<T>(string displayName) where T : Enumeration
    {
        return Parse<T, string>(displayName, "display name", item => item.Name == displayName);
    }

    /// <summary>
    /// Tries to get the enumeration value with the specified ID.
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <param name="value">The ID to search for.</param>
    /// <param name="result">The matching enumeration value if found.</param>
    /// <returns>True if a matching value was found; otherwise false.</returns>
    public static bool TryFromValue<T>(int value, out T? result) where T : Enumeration
    {
        result = GetAll<T>().FirstOrDefault(item => item.Id == value);
        return result is not null;
    }

    /// <summary>
    /// Tries to get the enumeration value with the specified display name.
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <param name="displayName">The display name to search for.</param>
    /// <param name="result">The matching enumeration value if found.</param>
    /// <returns>True if a matching value was found; otherwise false.</returns>
    public static bool TryFromDisplayName<T>(string displayName, out T? result) where T : Enumeration
    {
        result = GetAll<T>().FirstOrDefault(item => item.Name == displayName);
        return result is not null;
    }

    private static T Parse<T, K>(K value, string description, Func<T, bool> predicate) where T : Enumeration
    {
        T? matchingItem = GetAll<T>().FirstOrDefault(predicate);
        
        if (matchingItem == null)
        {
            throw new InvalidOperationException($"'{value}' is not a valid {description} in {typeof(T)}");
        }
        
        return matchingItem;
    }

    /// <summary>
    /// Compares this enumeration value to another.
    /// </summary>
    public int CompareTo(object? other)
    {
        if (other is null)
        {
            return 1;
        }

        if (other is not Enumeration otherEnumeration)
        {
            throw new ArgumentException($"Object must be of type {nameof(Enumeration)}");
        }

        return Id.CompareTo(otherEnumeration.Id);
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(Enumeration? left, Enumeration? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(Enumeration? left, Enumeration? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Less than operator.
    /// </summary>
    public static bool operator <(Enumeration left, Enumeration right)
    {
        return left.Id < right.Id;
    }

    /// <summary>
    /// Less than or equal operator.
    /// </summary>
    public static bool operator <=(Enumeration left, Enumeration right)
    {
        return left.Id <= right.Id;
    }

    /// <summary>
    /// Greater than operator.
    /// </summary>
    public static bool operator >(Enumeration left, Enumeration right)
    {
        return left.Id > right.Id;
    }

    /// <summary>
    /// Greater than or equal operator.
    /// </summary>
    public static bool operator >=(Enumeration left, Enumeration right)
    {
        return left.Id >= right.Id;
    }
}
