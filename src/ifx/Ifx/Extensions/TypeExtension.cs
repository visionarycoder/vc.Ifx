using System.Globalization;
using System.Text;

namespace Ifx.Extensions;
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
        return (value) switch
        {
            bool boolValue => boolValue,
            string stringValue => bool.TryParse(stringValue, out bool result) && result,
            int intValue => intValue != 0,
            long longValue => longValue != 0,
            double doubleValue => Math.Abs(doubleValue) > double.Epsilon,
            decimal decimalValue => decimalValue != 0,
            float floatValue => Math.Abs(floatValue) > float.Epsilon,
            byte byteValue => byteValue != 0,
            short shortValue => shortValue != 0,
            uint uintValue => uintValue != 0,
            ulong ulongValue => ulongValue != 0,
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
            return defaultValue;
        return value switch
        {
            int intValue => intValue,
            bool boolValue => boolValue ? 1 : 0,
            string stringValue => int.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out int result) ? result : defaultValue,
            double doubleValue => doubleValue.AsIntegerOrNull() ?? defaultValue,
            decimal decimalValue => decimalValue.AsIntegerOrNull() ?? defaultValue,
            long longValue => longValue > int.MaxValue || longValue < int.MinValue ? defaultValue : (int)longValue,
            float floatValue => floatValue.AsIntegerOrNull() ?? defaultValue,
            byte byteValue => byteValue,
            short shortValue => shortValue,
            uint uintValue => uintValue > int.MaxValue ? defaultValue : (int)uintValue,
            ulong ulongValue => ulongValue > int.MaxValue ? defaultValue : (int)ulongValue,
            _ => defaultValue
        };
    }
    /// <summary>
    /// Converts the value to a long.
    /// </summary>
    /// <returns>The long value, or the default value if conversion fails.</returns>
    public static long AsLong<T>(this T value, long defaultValue = 0)
    {
        if (value == null)
            return defaultValue;
        return value switch
        {
            long longValue => longValue,
            int intValue => intValue,
            bool boolValue => boolValue ? 1L : 0L,
            string stringValue => long.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out long result) ? result : defaultValue,
            double doubleValue => doubleValue.AsLongOrNull() ?? defaultValue,
            decimal decimalValue => decimalValue.AsLongOrNull() ?? defaultValue,
            float floatValue => floatValue.AsLongOrNull() ?? defaultValue,
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
    /// <returns>The double value, or the default value if conversion fails.</returns>
    public static double AsDouble<T>(this T value, double defaultValue = 0.0)
    {
        if (value == null)
            return defaultValue;
        return value switch
        {
            double doubleValue => doubleValue,
            int intValue => intValue,
            long longValue => longValue,
            bool boolValue => boolValue ? 1.0 : 0.0,
            string stringValue => double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double result) ? result : defaultValue,
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
    /// <returns>The decimal value, or the default value if conversion fails.</returns>
    public static decimal AsDecimal<T>(this T value, decimal defaultValue = 0m)
    {
        if (value == null)
            return defaultValue;
        try
        {
            return value switch
            {
                decimal decimalValue => decimalValue,
                bool boolValue => boolValue ? 1m : 0m,
                string stringValue => decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result) ? result : defaultValue,
                int intValue => intValue,
                long longValue => longValue,
                double doubleValue => (decimal)doubleValue,
                float floatValue => (decimal)floatValue,
                byte byteValue => byteValue,
                short shortValue => shortValue,
                uint uintValue => uintValue,
                ulong ulongValue => ulongValue,
                _ => defaultValue
            };
        }
        catch (OverflowException)
        {
            return defaultValue;
        }
    }
    /// <summary>
    /// Converts the value to a float.
    /// </summary>
    /// <returns>The float value, or the default value if conversion fails.</returns>
    public static float AsFloat<T>(this T value, float defaultValue = 0.0f)
    {
        if (value == null)
            return defaultValue;
        return value switch
        {
            float floatValue => floatValue,
            bool boolValue => boolValue ? 1.0f : 0.0f,
            string stringValue => float.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float result) ? result : defaultValue,
            int intValue => intValue,
            long longValue => longValue,
            double doubleValue => (float)doubleValue,
            decimal decimalValue => (float)decimalValue,
            byte byteValue => byteValue,
            short shortValue => shortValue,
            uint uintValue => uintValue,
            ulong ulongValue => ulongValue,
            _ => defaultValue
        };
    }
    /// <summary>
    /// Converts the value to a string.
    /// </summary>
    /// <returns>The string value, or the default value if conversion fails.</returns>
    public static string AsString<T>(this T value, string defaultValue = "")
    {
        return value?.ToString() ?? defaultValue;
    }
    /// <summary>
    /// Converts the value to a DateTime.
    /// </summary>
    /// <returns>The DateTime value, or the default value if conversion fails.</returns>
    public static DateTime AsDateTime<T>(this T value, DateTime defaultValue = default)
    {
        if (value == null)
            return defaultValue;
        return value switch
        {
            DateTime dateTimeValue => dateTimeValue,
            DateTimeOffset dateTimeOffsetValue => dateTimeOffsetValue.DateTime,
            string stringValue => DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result) ? result : defaultValue,
            long longValue => longValue >= -62135596800000L && longValue <= 253402300799999L
                ? DateTimeOffset.FromUnixTimeMilliseconds(longValue).DateTime : defaultValue,
            int intValue => DateTimeOffset.FromUnixTimeSeconds(intValue).DateTime,
            _ => defaultValue
        };
    }
    /// <summary>
    /// Converts the value to a DateTimeOffset.
    /// </summary>
    /// <returns>The DateTimeOffset value, or the default value if conversion fails.</returns>
    public static DateTimeOffset AsDateTimeOffset<T>(this T value, DateTimeOffset defaultValue = default)
    {
        if (value == null)
            return defaultValue;
        return value switch
        {
            DateTimeOffset dateTimeOffsetValue => dateTimeOffsetValue,
            DateTime dateTimeValue => new DateTimeOffset(dateTimeValue),
            string stringValue => DateTimeOffset.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset result) ? result : defaultValue,
            long longValue => longValue >= -62135596800000L && longValue <= 253402300799999L
                ? DateTimeOffset.FromUnixTimeMilliseconds(longValue) : defaultValue,
            int intValue => DateTimeOffset.FromUnixTimeSeconds(intValue),
            _ => defaultValue
        };
    }
    /// <summary>
    /// Converts the value to a Guid.
    /// </summary>
    /// <returns>The Guid value, or the default value if conversion fails.</returns>
    public static Guid AsGuid<T>(this T value, Guid defaultValue = default)
    {
        if (value == null)
            return defaultValue;
        return value switch
        {
            Guid guidValue => guidValue,
            string stringValue => Guid.TryParse(stringValue, out Guid result) ? result : defaultValue,
            byte[] byteArray => byteArray.Length == 16 ? new Guid(byteArray) : defaultValue,
            _ => defaultValue
        };
    }
    /// <summary>
    /// Converts the value to a byte.
    /// </summary>
    /// <returns>The byte value, or the default value if conversion fails.</returns>
    public static byte AsByte<T>(this T value, byte defaultValue = 0)
    {
        if (value == null)
            return defaultValue;
        return value switch
        {
            byte byteValue => byteValue,
            int intValue => intValue >= byte.MinValue && intValue <= byte.MaxValue ? (byte)intValue : defaultValue,
            long longValue => longValue >= byte.MinValue && longValue <= byte.MaxValue ? (byte)longValue : defaultValue,
            bool boolValue => boolValue ? (byte)1 : (byte)0,
            string stringValue => byte.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out byte result) ? result : defaultValue,
            double doubleValue => doubleValue >= byte.MinValue && doubleValue <= byte.MaxValue ? (byte)doubleValue : defaultValue,
            decimal decimalValue => decimalValue >= byte.MinValue && decimalValue <= byte.MaxValue ? (byte)decimalValue : defaultValue,
            float floatValue => floatValue >= byte.MinValue && floatValue <= byte.MaxValue ? (byte)floatValue : defaultValue,
            short shortValue => shortValue >= byte.MinValue ? (byte)shortValue : defaultValue,
            uint uintValue => uintValue <= byte.MaxValue ? (byte)uintValue : defaultValue,
            _ => defaultValue
        };
    }
    /// <summary>
    /// Converts the value to a short.
    /// </summary>
    /// <returns>The short value, or the default value if conversion fails.</returns>
    public static short AsShort<T>(this T value, short defaultValue = 0)
    {
        if (value == null)
            return defaultValue;
        return value switch
        {
            short shortValue => shortValue,
            int intValue => intValue >= short.MinValue && intValue <= short.MaxValue ? (short)intValue : defaultValue,
            long longValue => longValue >= short.MinValue && longValue <= short.MaxValue ? (short)longValue : defaultValue,
            bool boolValue => boolValue ? (short)1 : (short)0,
            string stringValue => short.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out short result) ? result : defaultValue,
            double doubleValue => doubleValue >= short.MinValue && doubleValue <= short.MaxValue ? (short)doubleValue : defaultValue,
            decimal decimalValue => decimalValue >= short.MinValue && decimalValue <= short.MaxValue ? (short)decimalValue : defaultValue,
            float floatValue => floatValue >= short.MinValue && floatValue <= short.MaxValue ? (short)floatValue : defaultValue,
            byte byteValue => byteValue,
            uint uintValue => uintValue <= short.MaxValue ? (short)uintValue : defaultValue,
            _ => defaultValue
        };
    }
    /// <summary>
    /// Converts the value to a char.
    /// </summary>
    /// <returns>The char value, or the default value if conversion fails.</returns>
    public static char AsChar<T>(this T value, char defaultValue = default)
    {
        if (value == null)
            return defaultValue;
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
    /// <returns>The byte array, or null if conversion fails.</returns>
    public static byte[]? AsByteArray<T>(this T value)
    {
        if (value == null)
            return null;
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
            return defaultValue;
        return (value) switch
        {
            TEnum enumValue => enumValue,
            string stringValue => Enum.TryParse(stringValue, true, out TEnum result) ? result : defaultValue,
            int intValue => DefinedEnumOrNull<TEnum>(intValue) ?? defaultValue,
            byte byteValue => DefinedEnumOrNull<TEnum>(byteValue) ?? defaultValue,
            short shortValue => DefinedEnumOrNull<TEnum>(shortValue) ?? defaultValue,
            _ => defaultValue
        };
    }
    /// <summary>
    /// Converts the value to a TimeSpan.
    /// </summary>
    /// <returns>The TimeSpan value, or the default value if conversion fails.</returns>
    public static TimeSpan AsTimeSpan<T>(this T value, TimeSpan defaultValue = default)
    {
        if (value == null)
            return defaultValue;
        return (value) switch
        {
            TimeSpan timeSpanValue => timeSpanValue,
            string stringValue => TimeSpan.TryParse(stringValue, CultureInfo.InvariantCulture, out TimeSpan result) ? result : defaultValue,
            long longValue => TimeSpan.FromTicks(longValue),
            int intValue => TimeSpan.FromMilliseconds(intValue),
            double doubleValue => MillisecondsOrNull(doubleValue) ?? defaultValue,
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
            return null;
        return (value) switch
        {
            TElement[] arrayValue => arrayValue,
            IEnumerable<TElement> enumerableValue => enumerableValue.ToArray(),
            _ => null
        };
    }
    #endregion Non-nullable conversions

    #region Nullable conversions 
    /// <summary>
    /// Converts the value to a nullable boolean, similar to the 'as' operator.
    /// </summary>
    /// <returns>The boolean value, or null if conversion fails.</returns>
    public static bool? AsBooleanOrNull<T>(this T? value)
    {
        if (value == null)
            return null;
        return (value) switch
        {
            bool boolValue => boolValue,
            string stringValue => bool.TryParse(stringValue, out bool result) ? result : null,
            int intValue => intValue != 0,
            long longValue => longValue != 0,
            double doubleValue => Math.Abs(doubleValue) > double.Epsilon,
            decimal decimalValue => decimalValue != 0,
            float floatValue => Math.Abs(floatValue) > float.Epsilon,
            byte byteValue => byteValue != 0,
            short shortValue => shortValue != 0,
            uint uintValue => uintValue != 0,
            ulong ulongValue => ulongValue != 0,
            _ => null
        };
    }
    /// <summary>
    /// Converts the value to a nullable integer, similar to the 'as' operator.
    /// </summary>
    /// <returns>The integer value, or null if conversion fails.</returns>
    public static int? AsIntegerOrNull<T>(this T? value)
    {
        if (value == null)
            return null;
        return (value) switch
        {
            int intValue => intValue,
            bool boolValue => boolValue ? 1 : 0,
            string stringValue => int.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out int result) ? result : null,
            double doubleValue => doubleValue >= int.MinValue && doubleValue <= int.MaxValue ? (int)doubleValue : null,
            decimal decimalValue => decimalValue >= int.MinValue && decimalValue <= int.MaxValue ? (int)decimalValue : null,
            long longValue => longValue >= int.MinValue && longValue <= int.MaxValue ? (int)longValue : null,
            float floatValue => floatValue >= int.MinValue && (double)floatValue <= int.MaxValue ? (int)floatValue : null,
            byte byteValue => byteValue,
            short shortValue => shortValue,
            uint uintValue => uintValue <= int.MaxValue ? (int)uintValue : null,
            ulong ulongValue => ulongValue <= int.MaxValue ? (int)ulongValue : null,
            _ => null
        };
    }
    /// <summary>
    /// Converts the value to a nullable long, similar to the 'as' operator.
    /// </summary>
    /// <returns>The long value, or null if conversion fails.</returns>
    public static long? AsLongOrNull<T>(this T? value)
    {
        if (value == null)
            return null;
        return (value) switch
        {
            long longValue => longValue,
            int intValue => intValue,
            bool boolValue => boolValue ? 1L : 0L,
            string stringValue => long.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out long result) ? result : null,
            double doubleValue => doubleValue >= long.MinValue && doubleValue < 9223372036854775808D ? (long)doubleValue : null,
            decimal decimalValue => decimalValue >= long.MinValue && decimalValue <= long.MaxValue ? (long)decimalValue : null,
            float floatValue => floatValue >= long.MinValue && floatValue < 9223372036854775808D ? (long)floatValue : null,
            byte byteValue => byteValue,
            short shortValue => shortValue,
            uint uintValue => uintValue,
            ulong ulongValue => ulongValue <= long.MaxValue ? (long)ulongValue : null,
            _ => null
        };
    }
    /// <summary>
    /// Converts the value to a nullable double, similar to the 'as' operator.
    /// </summary>
    /// <returns>The double value, or null if conversion fails.</returns>
    public static double? AsDoubleOrNull<T>(this T? value)
    {
        if (value == null)
            return null;
        return (value) switch
        {
            double doubleValue => doubleValue,
            int intValue => intValue,
            long longValue => longValue,
            bool boolValue => boolValue ? 1.0 : 0.0,
            string stringValue => double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double result) ? result : null,
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
    /// <returns>The decimal value, or null if conversion fails.</returns>
    public static decimal? AsDecimalOrNull<T>(this T? value)
    {
        if (value == null)
            return null;
        try
        {
            return (value) switch
            {
                decimal decimalValue => decimalValue,
                bool boolValue => boolValue ? 1m : 0m,
                string stringValue => decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result) ? result : null,
                int intValue => intValue,
                long longValue => longValue,
                double doubleValue => (decimal)doubleValue,
                float floatValue => (decimal)floatValue,
                byte byteValue => byteValue,
                short shortValue => shortValue,
                uint uintValue => uintValue,
                ulong ulongValue => ulongValue,
                _ => null
            };
        }
        catch (OverflowException)
        {
            return null;
        }
    }
    /// <summary>
    /// Converts the value to a nullable float, similar to the 'as' operator.
    /// </summary>
    /// <returns>The float value, or null if conversion fails.</returns>
    public static float? AsFloatOrNull<T>(this T? value)
    {
        if (value == null)
            return null;
        return (value) switch
        {
            float floatValue => floatValue,
            bool boolValue => boolValue ? 1.0f : 0.0f,
            string stringValue => float.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float result) ? result : null,
            int intValue => intValue,
            long longValue => longValue,
            double doubleValue => doubleValue >= float.MinValue && doubleValue <= float.MaxValue ? (float)doubleValue : null,
            decimal decimalValue => (float)decimalValue,
            byte byteValue => byteValue,
            short shortValue => shortValue,
            uint uintValue => uintValue,
            ulong ulongValue => ulongValue,
            _ => null
        };
    }
    /// <summary>
    /// Converts the value to a string, similar to the 'as' operator.
    /// </summary>
    /// <returns>The string value, or null if conversion fails.</returns>
    public static string? AsStringOrNull<T>(this T? value)
    {
        if (value == null)
            return null;
        return value is string stringValue ? stringValue : value.ToString();
    }
    /// <summary>
    /// Converts the value to a nullable DateTime, similar to the 'as' operator.
    /// </summary>
    /// <returns>The DateTime value, or null if conversion fails.</returns>
    public static DateTime? AsDateTimeOrNull<T>(this T? value)
    {
        if (value == null)
            return null;
        return (value) switch
        {
            DateTime dateTimeValue => dateTimeValue,
            DateTimeOffset dateTimeOffsetValue => dateTimeOffsetValue.DateTime,
            string stringValue => DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result) ? result : null,
            long longValue => longValue >= -62135596800000L && longValue <= 253402300799999L
                ? DateTimeOffset.FromUnixTimeMilliseconds(longValue).DateTime : null,
            int intValue => DateTimeOffset.FromUnixTimeSeconds(intValue).DateTime,
            _ => null
        };
    }
    /// <summary>
    /// Converts the value to a nullable DateTimeOffset, similar to the 'as' operator.
    /// </summary>
    /// <returns>The DateTimeOffset value, or null if conversion fails.</returns>
    public static DateTimeOffset? AsDateTimeOffsetOrNull<T>(this T? value)
    {
        if (value == null)
            return null;
        return (value) switch
        {
            DateTimeOffset dateTimeOffsetValue => dateTimeOffsetValue,
            DateTime dateTimeValue => new DateTimeOffset(dateTimeValue),
            string stringValue => DateTimeOffset.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset result) ? result : null,
            long longValue => longValue >= -62135596800000L && longValue <= 253402300799999L
                ? DateTimeOffset.FromUnixTimeMilliseconds(longValue) : null,
            int intValue => DateTimeOffset.FromUnixTimeSeconds(intValue),
            _ => null
        };
    }
    /// <summary>
    /// Converts the value to a nullable Guid, similar to the 'as' operator.
    /// </summary>
    /// <returns>The Guid value, or null if conversion fails.</returns>
    public static Guid? AsGuidOrNull<T>(this T? value)
    {
        if (value == null)
            return null;
        return (value) switch
        {
            Guid guidValue => guidValue,
            string stringValue => Guid.TryParse(stringValue, out Guid result) ? result : null,
            byte[] byteArray => byteArray.Length == 16 ? new Guid(byteArray) : null,
            _ => null
        };
    }
    /// <summary>
    /// Converts the value to a nullable byte, similar to the 'as' operator.
    /// </summary>
    /// <returns>The byte value, or null if conversion fails.</returns>
    public static byte? AsByteOrNull<T>(this T? value)
    {
        if (value == null)
            return null;
        return (value) switch
        {
            byte byteValue => byteValue,
            int intValue => intValue >= byte.MinValue && intValue <= byte.MaxValue ? (byte)intValue : null,
            long longValue => longValue >= byte.MinValue && longValue <= byte.MaxValue ? (byte)longValue : null,
            bool boolValue => boolValue ? (byte)1 : (byte)0,
            string stringValue => byte.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out byte result) ? result : null,
            double doubleValue => doubleValue >= byte.MinValue && doubleValue <= byte.MaxValue ? (byte)doubleValue : null,
            decimal decimalValue => decimalValue >= byte.MinValue && decimalValue <= byte.MaxValue ? (byte)decimalValue : null,
            float floatValue => floatValue >= byte.MinValue && floatValue <= byte.MaxValue ? (byte)floatValue : null,
            short shortValue => shortValue >= byte.MinValue ? (byte)shortValue : null,
            uint uintValue => uintValue <= byte.MaxValue ? (byte)uintValue : null,
            _ => null
        };
    }
    /// <summary>
    /// Converts the value to a nullable short, similar to the 'as' operator.
    /// </summary>
    /// <returns>The short value, or null if conversion fails.</returns>
    public static short? AsShortOrNull<T>(this T? value)
    {
        if (value == null)
            return null;
        return (value) switch
        {
            short shortValue => shortValue,
            int intValue => intValue >= short.MinValue && intValue <= short.MaxValue ? (short)intValue : null,
            long longValue => longValue >= short.MinValue && longValue <= short.MaxValue ? (short)longValue : null,
            bool boolValue => boolValue ? (short)1 : (short)0,
            string stringValue => short.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out short result) ? result : null,
            double doubleValue => doubleValue >= short.MinValue && doubleValue <= short.MaxValue ? (short)doubleValue : null,
            decimal decimalValue => decimalValue >= short.MinValue && decimalValue <= short.MaxValue ? (short)decimalValue : null,
            float floatValue => floatValue >= short.MinValue && floatValue <= short.MaxValue ? (short)floatValue : null,
            byte byteValue => byteValue,
            uint uintValue => uintValue <= short.MaxValue ? (short)uintValue : null,
            _ => null
        };
    }
    /// <summary>
    /// Converts the value to a nullable char, similar to the 'as' operator.
    /// </summary>
    /// <returns>The char value, or null if conversion fails.</returns>
    public static char? AsCharOrNull<T>(this T? value)
    {
        if (value == null)
            return null;
        return (value) switch
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
    /// <returns>The TimeSpan value, or null if conversion fails.</returns>
    public static TimeSpan? AsTimeSpanOrNull<T>(this T? value)
    {
        if (value == null)
            return null;
        return (value) switch
        {
            TimeSpan timeSpanValue => timeSpanValue,
            string stringValue => TimeSpan.TryParse(stringValue, CultureInfo.InvariantCulture, out TimeSpan result) ? result : null,
            long longValue => TimeSpan.FromTicks(longValue),
            int intValue => TimeSpan.FromMilliseconds(intValue),
            double doubleValue => MillisecondsOrNull(doubleValue),
            _ => null
        };
    }
    /// <summary>
    /// Converts the value to an enum of type TEnum, similar to the 'as' operator.
    /// </summary>
    /// <returns>The enum value, or null if conversion fails.</returns>
    public static TEnum? AsEnumOrNull<T, TEnum>(this T? value) where TEnum : struct, Enum
    {
        if (value == null)
            return null;
        return (value) switch
        {
            TEnum enumValue => enumValue,
            string stringValue => Enum.TryParse(stringValue, true, out TEnum result) ? result : null,
            int intValue => DefinedEnumOrNull<TEnum>(intValue),
            byte byteValue => DefinedEnumOrNull<TEnum>(byteValue),
            short shortValue => DefinedEnumOrNull<TEnum>(shortValue),
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
        try
        {
            if (value is TResult result)
            {
                return result;
            }
            // The class constraint permits reference identity and string conversion only.
            if (typeof(TResult) == typeof(string)) return value.AsStringOrNull() as TResult;
        }
        catch
        {
            // Retain the legacy nonthrowing contract for user-defined ToString methods.
        }
        return null;
    }

    /// <summary>
    /// Attempts to convert the value to the specified value type, similar to the 'as' operator.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TResult">The value type to convert to.</typeparam>
    /// <param name="value">The value to convert.</param>
    public static TResult? AsValueTypeOrNull<T, TResult>(this T? value) where TResult : struct
    {
        Type targetType = typeof(TResult);
        if (targetType == typeof(bool)) return (TResult?)(object?)value.AsBooleanOrNull();
        if (targetType == typeof(int)) return (TResult?)(object?)value.AsIntegerOrNull();
        if (targetType == typeof(long)) return (TResult?)(object?)value.AsLongOrNull();
        if (targetType == typeof(double)) return (TResult?)(object?)value.AsDoubleOrNull();
        if (targetType == typeof(decimal)) return (TResult?)(object?)value.AsDecimalOrNull();
        if (targetType == typeof(float)) return (TResult?)(object?)value.AsFloatOrNull();
        if (targetType == typeof(DateTime)) return (TResult?)(object?)value.AsDateTimeOrNull();
        if (targetType == typeof(DateTimeOffset)) return (TResult?)(object?)value.AsDateTimeOffsetOrNull();
        if (targetType == typeof(Guid)) return (TResult?)(object?)value.AsGuidOrNull();
        if (targetType == typeof(byte)) return (TResult?)(object?)value.AsByteOrNull();
        if (targetType == typeof(short)) return (TResult?)(object?)value.AsShortOrNull();
        if (targetType == typeof(char)) return (TResult?)(object?)value.AsCharOrNull();
        if (targetType == typeof(TimeSpan)) return (TResult?)(object?)value.AsTimeSpanOrNull();
        // Try direct conversion if it's a value type
        if (value is TResult resultValue)
            return resultValue;
        return null;
    }

    #endregion

    private static TEnum? DefinedEnumOrNull<TEnum>(int value) where TEnum : struct, Enum
    {
        // Parsing checks the enum's actual underlying range before IsDefined, avoiding
        // both mismatched boxed primitive types and truncation in Enum.ToObject.
        return Enum.TryParse(value.ToString(CultureInfo.InvariantCulture), out TEnum result)
            && Enum.IsDefined(result) ? result : null;
    }

    private static TimeSpan? MillisecondsOrNull(double value)
    {
        if (double.IsNaN(value)) return null;
        try
        {
            return TimeSpan.FromMilliseconds(value);
        }
        catch (OverflowException)
        {
            return null;
        }
    }

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
