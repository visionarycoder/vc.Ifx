using Ifx;
using Ifx.Component;
using Ifx.Pipeline;
using Ifx.Proxy.Exceptions;
using Ifx.Querying;
using Ifx.Querying.Serialization;
using Ifx.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Threading.RateLimiting;

using System.ComponentModel;
using System.Text.Json;

namespace Ifx.Proxy.Interceptor.Configuration;

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
