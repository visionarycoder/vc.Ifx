using System.Numerics;

namespace VisionaryCoder.Framework.Extensions;
/// <summary>
/// Provides extension methods for divide-by-zero validation and safe division operations.
/// </summary>
public static class MathExtensions
{
    /// <summary>
    /// Throws a <see cref="DivideByZeroException"/> if the specified value equals zero.
    /// </summary>
    /// <typeparam name="T">The numeric type of the value.</typeparam>
    /// <param name="value">The divisor to check.</param>
    /// <param name="paramName">The name of the parameter (optional).</param>
    /// <exception cref="DivideByZeroException">Thrown when the value is zero.</exception>
    public static void ThrowIfZero<T>(T value, string? paramName = null) where T : INumberBase<T>
    {
        if (T.IsZero(value))
        {
            throw new DivideByZeroException(paramName != null
                ? $"Division by zero would occur with parameter '{paramName}'."
                : "Division by zero would occur.");
        }
    }

    /// <summary>
    /// Determines whether the specified value is zero.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns><c>true</c> if the value is zero; otherwise, <c>false</c>.</returns>
    public static bool IsZero<T>(this T value) where T : INumberBase<T>
    {
        return T.IsZero(value);
    }

    /// <summary>
    /// Safely divides two numbers, returning a default value if the divisor is zero.
    /// </summary>
    /// <typeparam name="T">The numeric type of the values.</typeparam>
    /// <param name="numerator">The numerator.</param>
    /// <param name="denominator">The denominator.</param>
    /// <param name="defaultValue">The default value to return if the denominator is zero.</param>
    /// <returns>The result of the division, or the default value if the denominator is zero.</returns>
    public static T SafeDivide<T>(T numerator, T denominator, T defaultValue) where T : INumberBase<T>, IDivisionOperators<T, T, T>
    {
        return T.IsZero(denominator) ? defaultValue : numerator / denominator;
    }

    /// <summary>
    /// Safely divides two numbers, returning zero if the divisor is zero.
    /// </summary>
    /// <typeparam name="T">The numeric type of the values.</typeparam>
    /// <param name="numerator">The numerator.</param>
    /// <param name="denominator">The denominator.</param>
    /// <returns>The result of the division, or zero if the denominator is zero.</returns>
    public static T SafeDivide<T>(T numerator, T denominator) where T : INumberBase<T>, IDivisionOperators<T, T, T>
    {
        return SafeDivide(numerator, denominator, T.Zero);
    }

    /// <summary>
    /// Attempts to divide two numbers and outputs the result.
    /// </summary>
    /// <typeparam name="T">The numeric type of the values.</typeparam>
    /// <param name="numerator">The numerator.</param>
    /// <param name="denominator">The denominator.</param>
    /// <param name="result">When this method returns, contains the result of the division if successful, or default value if unsuccessful.</param>
    /// <returns><c>true</c> if the division was successful; otherwise, <c>false</c>.</returns>
    public static bool TryDivide<T>(T numerator, T denominator, out T result) where T : INumberBase<T>, IDivisionOperators<T, T, T>
    {
        if (T.IsZero(denominator))
        {
            result = default!;
            return false;
        }
        result = numerator / denominator;
        return true;
    }

    /// <summary>
    /// Returns a default value if the input is zero.
    /// </summary>
    /// <typeparam name="T">The numeric type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="defaultValue">The default value to return if the input is zero.</param>
    /// <returns>The original value if not zero, otherwise the default value.</returns>
    public static T DefaultIfZero<T>(this T value, T defaultValue) where T : INumberBase<T>
    {
        return T.IsZero(value) ? defaultValue : value;
    }
}
