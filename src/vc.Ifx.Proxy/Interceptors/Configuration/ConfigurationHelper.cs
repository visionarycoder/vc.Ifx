using System.ComponentModel;
using System.Text.Json;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Configuration;

internal static class ConfigurationHelper
{
    public static T ConvertValue<T>(string stringValue, T defaultValue)
    {
        try
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter.CanConvertFrom(typeof(string)))
                return converter.ConvertFromInvariantString(stringValue) is T value ? value : defaultValue;
            return JsonSerializer.Deserialize<T>(stringValue) ?? defaultValue;
        }
        catch (Exception exception) when (exception is FormatException or NotSupportedException or JsonException or ArgumentException)
        {
            return defaultValue;
        }
    }
}
