using System.ComponentModel;
using System.Globalization;

namespace Ifx.Querying;

internal static class PortableValueConverter
{
    internal static string? Format(object? value) => value switch
    {
        null => null,
        DateTime date => date.ToString("O", CultureInfo.InvariantCulture),
        DateTimeOffset date => date.ToString("O", CultureInfo.InvariantCulture),
        IFormattable formatted => formatted.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString()
    };

    internal static object? Parse(string? text, Type targetType)
    {
        if (text is null) return null;
        targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (targetType == typeof(string)) return text;
        if (targetType == typeof(DateTime)) return DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        if (targetType == typeof(DateTimeOffset)) return DateTimeOffset.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        if (targetType.IsEnum) return Enum.Parse(targetType, text);
        return TypeDescriptor.GetConverter(targetType).ConvertFromInvariantString(text);
    }
}
