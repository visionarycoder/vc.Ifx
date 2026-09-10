using VisionaryCoder.Framework.Extensions;

namespace VisionaryCoder.Framework.Tests.Extensions;

public static class StaticHelper
{
    public static string GetCallingClass()
    {
        return ReflectionExtensions.NameOfCallingClass();
    }

    public static Type? GetCallingType()
    {
        return ReflectionExtensions.TypeOfCallingClass();
    }
}
