using System.Globalization;
using System.Text;

namespace vc.Ifx.Extensions;

/// <summary>
/// Provides extension methods for type conversion operations.
/// </summary>
public static class TypeExtension
{

    #region Non-nullable conversions 
    /// <summary>
    /// Converts the value to a boolean.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The boolean value, or false if conversion fails.</returns>
    public static bool AsBoolean<T>(this T value)
    {

        if (value == null)
        {
            return false;
        }
        return value switch
        {
            bool boolValue => boolValue,
            string stringValue => bool.TryParse(stringValue, out var result) && result,
            int intValue => intValue != 0,
            long longValue => longValue != 0,
            double doubleValue => Math.Abs(doubleValue) > double.Epsilon,
            decimal decimalValue => decimalValue != 0,
            _ => false
        };

    }

    /// <summary>
    /// Converts the value to an integer.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="defaultValue">The default value to return if conversion fails.</param>
    /// <returns>The integer value, or the default value if conversion fails.</returns>
    public static int AsInteger<T>(this T value, int defaultValue = 0)
    {
        if (value == null)
        {
            return defaultValue;
        }

        return value switch
        {
            int intValue => intValue,
            bool boolValue => boolValue ? 1 : 0,
            string stringValue => int.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : defaultValue,
            double doubleValue => (int)doubleValue,
            decimal decimalValue => (int)decimalValue,
            long longValue => longValue > int.MaxValue || longValue < int.MinValue ? defaultValue : (int)longValue,
            float floatValue => (int)floatValue,
            byte byteValue => byteValue,
            short shortValue => shortValue,
            uint uintValue => uintValue > int.MaxValue ? defaultValue : (int)uintValue,
            _ => defaultValue
        };
    }

    /// <summary>
    /// Converts the value to a long.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="defaultValue">The default value to return if conversion fails.</param>
    /// <returns>The long value, or the default value if conversion fails.</returns>
    public static long AsLong<T>(this T value, long defaultValue = 0)
    {
        if (value == null)
        {
            return defaultValue;
        }

        return value switch
        {
            long longValue => longValue,
            int intValue => intValue,
            bool boolValue => boolValue ? 1 : 0,
            string stringValue => long.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : defaultValue,
            double doubleValue => (long)doubleValue,
            decimal decimalValue => (long)decimalValue,
            float floatValue => (long)floatValue,
            byte byteValue => byteValue,
            short shortValue => shortValue,
            uint uintValue => uintValue,
            ulong ulongValue => ulongValue > long.MaxValue ? defaultValue : (long)ulongValue,
            _ => defaultValue
        };
    }

    /// <summary>
    /// Converts the value to a double.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="defaultValue">The default value to return if conversion fails.</param>
    /// <returns>The double value, or the default value if conversion fails.</returns>
    public static double AsDouble<T>(this T value, double defaultValue = 0.0)
    {
        if (value == null)
        {
            return defaultValue;
        }

        return value switch
        {
            double doubleValue => doubleValue,
            int intValue => intValue,
            long longValue => longValue,
            bool boolValue => boolValue ? 1.0 : 0.0,
            string stringValue => double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : defaultValue,
            decimal decimalValue => (double)decimalValue,
            float floatValue => floatValue,
            byte byteValue => byteValue,
            short shortValue => shortValue,
            uint uintValue => uintValue,
            ulong ulongValue => ulongValue,
            _ => defaultValue
        };
    }

    /// <summary>
    /// Converts the value to a decimal.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="defaultValue">The default value to return if conversion fails.</param>
    /// <returns>The decimal value, or the default value if conversion fails.</returns>
    public static decimal AsDecimal<T>(this T value, decimal defaultValue = 0m)
    {
        if (value == null)
        {
            return defaultValue;
        }

        return value switch
        {
            decimal decimalValue => decimalValue,
            int intValue => intValue,
            long longValue => longValue,
            bool boolValue => boolValue ? 1m : 0m,
            string stringValue => decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : defaultValue,
            double doubleValue => (decimal)doubleValue,
            float floatValue => (decimal)floatValue,
            byte byteValue => byteValue,
            short shortValue => shortValue,
            uint uintValue => uintValue,
            ulong ulongValue => ulongValue,
            _ => defaultValue
        };
    }

    /// <summary>
    /// Converts the value to a float.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="defaultValue">The default value to return if conversion fails.</param>
    /// <returns>The float value, or the default value if conversion fails.</returns>
    public static float AsFloat<T>(this T value, float defaultValue = 0.0f)
    {
        if (value == null)
        {
            return defaultValue;
        }

        return value switch
        {
            float floatValue => floatValue,
            int intValue => intValue,
            long longValue => longValue,
            bool boolValue => boolValue ? 1.0f : 0.0f,
            string stringValue => float.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : defaultValue,
            double doubleValue => (float)doubleValue,
            decimal decimalValue => (float)decimalValue,
            byte byteValue => byteValue,
            short shortValue => shortValue,
            _ => defaultValue
        };
    }

    /// <summary>
    /// Converts the value to a string.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="defaultValue">The default value to return if conversion fails.</param>
    /// <returns>The string value, or the default value if conversion fails.</returns>
    public static string AsString<T>(this T value, string defaultValue = "")
    {
        if (value == null)
        {
            return defaultValue;
        }

        return value.ToString() ?? defaultValue;
    }

    /// <summary>
    /// Converts the value to a DateTime.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="defaultValue">The default value to return if conversion fails.</param>
    /// <returns>The DateTime value, or the default value if conversion fails.</returns>
    public static DateTime AsDateTime<T>(this T value, DateTime defaultValue = default)
    {
        if (value == null)
        {
            return defaultValue;
        }

        return value switch
        {
            DateTime dateTimeValue => dateTimeValue,
            string stringValue => DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result) ? result : defaultValue,
            long longValue => DateTimeOffset.FromUnixTimeMilliseconds(longValue).DateTime,
            int intValue => DateTimeOffset.FromUnixTimeSeconds(intValue).DateTime,
            _ => defaultValue
        };
    }

    /// <summary>
    /// Converts the value to a DateTimeOffset.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="defaultValue">The default value to return if conversion fails.</param>
    /// <returns>The DateTimeOffset value, or the default value if conversion fails.</returns>
    public static DateTimeOffset AsDateTimeOffset<T>(this T value, DateTimeOffset defaultValue = default)
    {
        if (value == null)
        {
            return defaultValue;
        }

        return value switch
        {
            DateTimeOffset dateTimeOffsetValue => dateTimeOffsetValue,
            DateTime dateTimeValue => new DateTimeOffset(dateTimeValue),
            string stringValue => DateTimeOffset.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result) ? result : defaultValue,
            long longValue => DateTimeOffset.FromUnixTimeMilliseconds(longValue),
            int intValue => DateTimeOffset.FromUnixTimeSeconds(intValue),
            _ => defaultValue
        };
    }

    /// <summary>
    /// Converts the value to a Guid.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="defaultValue">The default value to return if conversion fails.</param>
    /// <returns>The Guid value, or the default value if conversion fails.</returns>
    public static Guid AsGuid<T>(this T value, Guid defaultValue = default)
    {
        if (value == null)
        {
            return defaultValue;
        }

        return value switch
        {
            Guid guidValue => guidValue,
            string stringValue => Guid.TryParse(stringValue, out var result) ? result : defaultValue,
            byte[] byteArray => byteArray.Length == 16 ? new Guid(byteArray) : defaultValue,
            _ => defaultValue
        };
    }

    /// <summary>
    /// Converts the value to a byte.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="defaultValue">The default value to return if conversion fails.</param>
    /// <returns>The byte value, or the default value if conversion fails.</returns>
    public static byte AsByte<T>(this T value, byte defaultValue = 0)
    {
        if (value == null)
        {
            return defaultValue;
        }

        return value switch
        {
            byte byteValue => byteValue,
            int intValue => intValue >= byte.MinValue && intValue <= byte.MaxValue ? (byte)intValue : defaultValue,
            bool boolValue => boolValue ? (byte)1 : (byte)0,
            string stringValue => byte.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : defaultValue,
            double doubleValue => doubleValue >= byte.MinValue && doubleValue <= byte.MaxValue ? (byte)doubleValue : defaultValue,
            decimal decimalValue => decimalValue >= byte.MinValue && decimalValue <= byte.MaxValue ? (byte)decimalValue : defaultValue,
            _ => defaultValue
        };
    }

    /// <summary>
    /// Converts the value to a short.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="defaultValue">The default value to return if conversion fails.</param>
    /// <returns>The short value, or the default value if conversion fails.</returns>
    public static short AsShort<T>(this T value, short defaultValue = 0)
    {
        if (value == null)
        {
            return defaultValue;
        }

        return value switch
        {
            short shortValue => shortValue,
            int intValue => intValue >= short.MinValue && intValue <= short.MaxValue ? (short)intValue : defaultValue,
            bool boolValue => boolValue ? (short)1 : (short)0,
            string stringValue => short.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : defaultValue,
            double doubleValue => doubleValue >= short.MinValue && doubleValue <= short.MaxValue ? (short)doubleValue : defaultValue,
            decimal decimalValue => decimalValue >= short.MinValue && decimalValue <= short.MaxValue ? (short)decimalValue : defaultValue,
            byte byteValue => byteValue,
            _ => defaultValue
        };
    }

    /// <summary>
    /// Converts the value to a char.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="defaultValue">The default value to return if conversion fails.</param>
    /// <returns>The char value, or the default value if conversion fails.</returns>
    public static char AsChar<T>(this T value, char defaultValue = default)
    {
        if (value == null)
        {
            return defaultValue;
        }

        return value switch
        {
            char charValue => charValue,
            string stringValue => stringValue.Length > 0 ? stringValue[0] : defaultValue,
            int intValue => intValue >= char.MinValue && intValue <= char.MaxValue ? (char)intValue : defaultValue,
            byte byteValue => (char)byteValue,
            _ => defaultValue
        };
    }

    /// <summary>
    /// Converts the value to a byte array.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The byte array, or null if conversion fails.</returns>
    public static byte[]? AsByteArray<T>(this T value)
    {
        if (value == null)
        {
            return null;
        }

        return value switch
        {
            byte[] byteArrayValue => byteArrayValue,
            string stringValue => Encoding.UTF8.GetBytes(stringValue),
            Guid guidValue => guidValue.ToByteArray(),
            _ => null
        };
    }

    /// <summary>
    /// Converts the value to an enum of type TEnum.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TEnum">The enum type to convert to.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="defaultValue">The default value to return if conversion fails.</param>
    /// <returns>The enum value, or the default value if conversion fails.</returns>
    public static TEnum AsEnum<T, TEnum>(this T value, TEnum defaultValue = default) where TEnum : struct, Enum
    {
        if (value == null)
        {
            return defaultValue;
        }

        return value switch
        {
            TEnum enumValue => enumValue,
            string stringValue => Enum.TryParse<TEnum>(stringValue, true, out var result) ? result : defaultValue,
            int intValue => Enum.IsDefined(typeof(TEnum), intValue) ? (TEnum)Enum.ToObject(typeof(TEnum), intValue) : defaultValue,
            byte byteValue => Enum.IsDefined(typeof(TEnum), byteValue) ? (TEnum)Enum.ToObject(typeof(TEnum), byteValue) : defaultValue,
            short shortValue => Enum.IsDefined(typeof(TEnum), shortValue) ? (TEnum)Enum.ToObject(typeof(TEnum), shortValue) : defaultValue,
            _ => defaultValue
        };
    }

    /// <summary>
    /// Converts the value to a TimeSpan.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <param name="defaultValue">The default value to return if conversion fails.</param>
    /// <returns>The TimeSpan value, or the default value if conversion fails.</returns>
    public static TimeSpan AsTimeSpan<T>(this T value, TimeSpan defaultValue = default)
    {
        if (value == null)
        {
            return defaultValue;
        }

        return value switch
        {
            TimeSpan timeSpanValue => timeSpanValue,
            string stringValue => TimeSpan.TryParse(stringValue, CultureInfo.InvariantCulture, out var result) ? result : defaultValue,
            long longValue => TimeSpan.FromTicks(longValue),
            int intValue => TimeSpan.FromMilliseconds(intValue),
            double doubleValue => TimeSpan.FromMilliseconds(doubleValue),
            _ => defaultValue
        };
    }

    /// <summary>
    /// Converts the value to an array of T where T is the type of the array elements.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TElement">The type of the array elements.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The array, or null if conversion fails.</returns>
    public static TElement[]? AsList<T, TElement>(this T value)
    {
        if (value == null)
        {
            return null;
        }

        return value switch
        {
            TElement[] arrayValue => arrayValue,
            IEnumerable<TElement> enumerableValue => enumerableValue.ToArray(),
            string stringValue when typeof(TElement) == typeof(char) => stringValue.Cast<TElement>().ToArray(),
            _ => null
        };
    }
    #endregion Non-nullable conversions

    #region Nullable conversions 
    /// <summary>
    /// Converts the value to a nullable boolean, similar to the 'as' operator.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The boolean value, or null if conversion fails.</returns>
    public static bool? AsBooleanOrNull<T>(this T? value)
    {
        if (value == null)
        {
            return null;
        }

        return value switch
        {
            bool boolValue => boolValue,
            string stringValue => bool.TryParse(stringValue, out var result) ? result : null,
            int intValue => intValue != 0,
            long longValue => longValue != 0,
            double doubleValue => Math.Abs(doubleValue) > double.Epsilon,
            decimal decimalValue => decimalValue != 0,
            _ => null
        };
    }

    /// <summary>
    /// Converts the value to a nullable integer, similar to the 'as' operator.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The integer value, or null if conversion fails.</returns>
    public static int? AsIntegerOrNull<T>(this T? value)
    {
        if (value == null)
        {
            return null;
        }

        return value switch
        {
            int intValue => intValue,
            bool boolValue => boolValue ? 1 : 0,
            string stringValue => int.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null,
            double doubleValue => doubleValue >= int.MinValue && doubleValue <= int.MaxValue ? (int)doubleValue : null,
            decimal decimalValue => decimalValue >= int.MinValue && decimalValue <= int.MaxValue ? (int)decimalValue : null,
            long longValue => longValue >= int.MinValue && longValue <= int.MaxValue ? (int)longValue : null,
            float floatValue => floatValue >= int.MinValue && floatValue <= int.MaxValue ? (int)floatValue : null,
            byte byteValue => byteValue,
            short shortValue => shortValue,
            uint uintValue => uintValue <= int.MaxValue ? (int)uintValue : null,
            _ => null
        };
    }

    /// <summary>
    /// Converts the value to a nullable long, similar to the 'as' operator.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The long value, or null if conversion fails.</returns>
    public static long? AsLongOrNull<T>(this T? value)
    {
        if (value == null)
        {
            return null;
        }

        return value switch
        {
            long longValue => longValue,
            int intValue => intValue,
            bool boolValue => boolValue ? 1L : 0L,
            string stringValue => long.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null,
            double doubleValue => doubleValue >= long.MinValue && doubleValue <= long.MaxValue ? (long)doubleValue : null,
            decimal decimalValue => decimalValue >= long.MinValue && decimalValue <= long.MaxValue ? (long)decimalValue : null,
            float floatValue => floatValue >= long.MinValue && floatValue <= long.MaxValue ? (long)floatValue : null,
            byte byteValue => byteValue,
            short shortValue => shortValue,
            uint uintValue => uintValue,
            ulong ulongValue => ulongValue <= (ulong)long.MaxValue ? (long)ulongValue : null,
            _ => null
        };
    }

    /// <summary>
    /// Converts the value to a nullable double, similar to the 'as' operator.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The double value, or null if conversion fails.</returns>
    public static double? AsDoubleOrNull<T>(this T? value)
    {
        if (value == null)
        {
            return null;
        }

        return value switch
        {
            double doubleValue => doubleValue,
            int intValue => intValue,
            long longValue => longValue,
            bool boolValue => boolValue ? 1.0 : 0.0,
            string stringValue => double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null,
            decimal decimalValue => (double)decimalValue,
            float floatValue => floatValue,
            byte byteValue => byteValue,
            short shortValue => shortValue,
            uint uintValue => uintValue,
            ulong ulongValue => ulongValue,
            _ => null
        };
    }

    /// <summary>
    /// Converts the value to a nullable decimal, similar to the 'as' operator.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The decimal value, or null if conversion fails.</returns>
    public static decimal? AsDecimalOrNull<T>(this T? value)
    {
        if (value == null)
        {
            return null;
        }

        return value switch
        {
            decimal decimalValue => decimalValue,
            int intValue => intValue,
            long longValue => longValue,
            bool boolValue => boolValue ? 1m : 0m,
            string stringValue => decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null,
            double doubleValue => (decimal)doubleValue,
            float floatValue => (decimal)floatValue,
            byte byteValue => byteValue,
            short shortValue => shortValue,
            uint uintValue => uintValue,
            ulong ulongValue => ulongValue,
            _ => null
        };
    }

    /// <summary>
    /// Converts the value to a nullable float, similar to the 'as' operator.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The float value, or null if conversion fails.</returns>
    public static float? AsFloatOrNull<T>(this T? value)
    {
        if (value == null)
        {
            return null;
        }

        return value switch
        {
            float floatValue => floatValue,
            int intValue => intValue,
            long longValue => longValue,
            bool boolValue => boolValue ? 1.0f : 0.0f,
            string stringValue => float.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null,
            double doubleValue => doubleValue >= float.MinValue && doubleValue <= float.MaxValue ? (float)doubleValue : null,
            decimal decimalValue => decimal.ToSingle(decimalValue),
            byte byteValue => byteValue,
            short shortValue => shortValue,
            _ => null
        };
    }

    /// <summary>
    /// Converts the value to a string, similar to the 'as' operator.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The string value, or null if conversion fails.</returns>
    public static string? AsStringOrNull<T>(this T? value)
    {
        if (value == null)
        {
            return null;
        }

        return value is string stringValue ? stringValue : value.ToString();
    }

    /// <summary>
    /// Converts the value to a nullable DateTime, similar to the 'as' operator.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The DateTime value, or null if conversion fails.</returns>
    public static DateTime? AsDateTimeOrNull<T>(this T? value)
    {
        if (value == null)
        {
            return null;
        }

        return value switch
        {
            DateTime dateTimeValue => dateTimeValue,
            DateTimeOffset dateTimeOffsetValue => dateTimeOffsetValue.DateTime,
            string stringValue => DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result) ? result : null,
            long longValue => DateTimeOffset.FromUnixTimeMilliseconds(longValue).DateTime,
            int intValue => DateTimeOffset.FromUnixTimeSeconds(intValue).DateTime,
            _ => null
        };
    }

    /// <summary>
    /// Converts the value to a nullable DateTimeOffset, similar to the 'as' operator.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The DateTimeOffset value, or null if conversion fails.</returns>
    public static DateTimeOffset? AsDateTimeOffsetOrNull<T>(this T? value)
    {
        if (value == null)
        {
            return null;
        }

        return value switch
        {
            DateTimeOffset dateTimeOffsetValue => dateTimeOffsetValue,
            DateTime dateTimeValue => new DateTimeOffset(dateTimeValue),
            string stringValue => DateTimeOffset.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result) ? result : null,
            long longValue => DateTimeOffset.FromUnixTimeMilliseconds(longValue),
            int intValue => DateTimeOffset.FromUnixTimeSeconds(intValue),
            _ => null
        };
    }

    /// <summary>
    /// Converts the value to a nullable Guid, similar to the 'as' operator.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The Guid value, or null if conversion fails.</returns>
    public static Guid? AsGuidOrNull<T>(this T? value)
    {
        if (value == null)
        {
            return null;
        }

        return value switch
        {
            Guid guidValue => guidValue,
            string stringValue => Guid.TryParse(stringValue, out var result) ? result : null,
            byte[] byteArray => byteArray.Length == 16 ? new Guid(byteArray) : null,
            _ => null
        };
    }

    /// <summary>
    /// Converts the value to a nullable byte, similar to the 'as' operator.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The byte value, or null if conversion fails.</returns>
    public static byte? AsByteOrNull<T>(this T? value)
    {
        if (value == null)
        {
            return null;
        }

        return value switch
        {
            byte byteValue => byteValue,
            int intValue => intValue >= byte.MinValue && intValue <= byte.MaxValue ? (byte)intValue : null,
            bool boolValue => boolValue ? (byte)1 : (byte)0,
            string stringValue => byte.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null,
            double doubleValue => doubleValue >= byte.MinValue && doubleValue <= byte.MaxValue ? (byte)doubleValue : null,
            decimal decimalValue => decimalValue >= byte.MinValue && decimalValue <= byte.MaxValue ? (byte)decimalValue : null,
            _ => null
        };
    }

    /// <summary>
    /// Converts the value to a nullable short, similar to the 'as' operator.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The short value, or null if conversion fails.</returns>
    public static short? AsShortOrNull<T>(this T? value)
    {
        if (value == null)
        {
            return null;
        }

        return value switch
        {
            short shortValue => shortValue,
            int intValue => intValue >= short.MinValue && intValue <= short.MaxValue ? (short)intValue : null,
            bool boolValue => boolValue ? (short)1 : (short)0,
            string stringValue => short.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null,
            double doubleValue => doubleValue >= short.MinValue && doubleValue <= short.MaxValue ? (short)doubleValue : null,
            decimal decimalValue => decimalValue >= short.MinValue && decimalValue <= short.MaxValue ? (short)decimalValue : null,
            byte byteValue => byteValue,
            _ => null
        };
    }

    /// <summary>
    /// Converts the value to a nullable char, similar to the 'as' operator.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The char value, or null if conversion fails.</returns>
    public static char? AsCharOrNull<T>(this T? value)
    {
        if (value == null)
        {
            return null;
        }

        return value switch
        {
            char charValue => charValue,
            string stringValue => stringValue.Length > 0 ? stringValue[0] : null,
            int intValue => intValue >= char.MinValue && intValue <= char.MaxValue ? (char)intValue : null,
            byte byteValue => (char)byteValue,
            _ => null
        };
    }

    /// <summary>
    /// Converts the value to a nullable TimeSpan, similar to the 'as' operator.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The TimeSpan value, or null if conversion fails.</returns>
    public static TimeSpan? AsTimeSpanOrNull<T>(this T? value)
    {
        if (value == null)
        {
            return null;
        }

        return value switch
        {
            TimeSpan timeSpanValue => timeSpanValue,
            string stringValue => TimeSpan.TryParse(stringValue, CultureInfo.InvariantCulture, out var result) ? result : null,
            long longValue => TimeSpan.FromTicks(longValue),
            int intValue => TimeSpan.FromMilliseconds(intValue),
            double doubleValue => TimeSpan.FromMilliseconds(doubleValue),
            _ => null
        };
    }

    /// <summary>
    /// Converts the value to an enum of type TEnum, similar to the 'as' operator.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TEnum">The enum type to convert to.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The enum value, or null if conversion fails.</returns>
    public static TEnum? AsEnumOrNull<T, TEnum>(this T? value) where TEnum : struct, Enum
    {
        if (value == null)
        {
            return null;
        }

        return value switch
        {
            TEnum enumValue => enumValue,
            string stringValue => Enum.TryParse<TEnum>(stringValue, true, out var result) ? result : null,
            int intValue => Enum.IsDefined(typeof(TEnum), intValue) ? (TEnum)Enum.ToObject(typeof(TEnum), intValue) : null,
            byte byteValue => Enum.IsDefined(typeof(TEnum), byteValue) ? (TEnum)Enum.ToObject(typeof(TEnum), byteValue) : null,
            short shortValue => Enum.IsDefined(typeof(TEnum), shortValue) ? (TEnum)Enum.ToObject(typeof(TEnum), shortValue) : null,
            _ => null
        };
    }

    /// <summary>
    /// Attempts to convert the value to the specified type, similar to the 'as' operator.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TResult">The type to convert to.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value, or null if conversion fails.</returns>
    public static TResult? AsTypeOrNull<T, TResult>(this T? value) where TResult : class
    {
        if (value == null)
        {
            return null;
        }

        try
        {
            if (value is TResult result)
            {
                return result;
            }

            // Try standard conversions for reference types
            var targetType = typeof(TResult);
            if (targetType == typeof(string)) return AsStringOrNull(value) as TResult;

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to convert the value to the specified value type, similar to the 'as' operator.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TResult">The value type to convert to.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value, or null if conversion fails.</returns>
    public static TResult? AsValueTypeOrNull<T, TResult>(this T? value) where TResult : struct
    {
        if (value == null)
        {
            return null;
        }

        try
        {
            // Try standard conversions for value types
            var targetType = typeof(TResult);
            if (targetType == typeof(bool)) return (TResult?)(object?)AsBooleanOrNull(value);
            if (targetType == typeof(int)) return (TResult?)(object?)AsIntegerOrNull(value);
            if (targetType == typeof(long)) return (TResult?)(object?)AsLongOrNull(value);
            if (targetType == typeof(double)) return (TResult?)(object?)AsDoubleOrNull(value);
            if (targetType == typeof(decimal)) return (TResult?)(object?)AsDecimalOrNull(value);
            if (targetType == typeof(float)) return (TResult?)(object?)AsFloatOrNull(value);
            if (targetType == typeof(DateTime)) return (TResult?)(object?)AsDateTimeOrNull(value);
            if (targetType == typeof(Guid)) return (TResult?)(object?)AsGuidOrNull(value);
            if (targetType == typeof(byte)) return (TResult?)(object?)AsByteOrNull(value);
            if (targetType == typeof(short)) return (TResult?)(object?)AsShortOrNull(value);
            if (targetType == typeof(char)) return (TResult?)(object?)AsCharOrNull(value);
            if (targetType == typeof(TimeSpan)) return (TResult?)(object?)AsTimeSpanOrNull(value);

            // Try direct conversion if it's a value type
            if (value is TResult resultValue)
            {
                return resultValue;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    #endregion

    /// <summary>
    /// Gets a value indicating whether the object is of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <param name="obj">The object to check.</param>
    /// <returns>True if the object is of the specified type; otherwise, false.</returns>
    public static bool IsOfType<T>(this object obj)
    {
        return obj is T;
    }

    /// <summary>
    /// Gets the underlying type for a nullable type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>The underlying type if the type is nullable; otherwise, the original type.</returns>
    public static Type GetUnderlyingType(this Type type)
    {
        return Nullable.GetUnderlyingType(type) ?? type;
    }

}