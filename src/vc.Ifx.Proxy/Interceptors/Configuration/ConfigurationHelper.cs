using System.Text.Json;

namespace VisionaryCoder.Framework.Proxy.Interceptors.Configuration;

internal static class ConfigurationHelper
{

    public static T ConvertValue<T>(string stringValue, T defaultValue)
    {

        try
        {
            Type t = typeof(T);
            switch (t)
            {
                case not null when t == typeof(string): return (T)(object)stringValue;
                case not null when t == typeof(int): return (T)(object)int.Parse(stringValue);
                case not null when t == typeof(long): return (T)(object)long.Parse(stringValue);
                case not null when t == typeof(double): return (T)(object)double.Parse(stringValue);
                case not null when t == typeof(decimal): return (T)(object)decimal.Parse(stringValue);
                case not null when t == typeof(bool): return (T)(object)bool.Parse(stringValue);
                case not null when t == typeof(DateTime): return (T)(object)DateTime.Parse(stringValue);
                case not null when t == typeof(DateTimeOffset): return (T)(object)DateTimeOffset.Parse(stringValue);
                case not null when t == typeof(TimeSpan): return (T)(object)TimeSpan.Parse(stringValue);
                case not null when t == typeof(Guid): return (T)(object)Guid.Parse(stringValue);
                default: return JsonSerializer.Deserialize<T>(stringValue) ?? defaultValue;
            }

        }
        catch
        {
            return defaultValue;
        }
    }
}
