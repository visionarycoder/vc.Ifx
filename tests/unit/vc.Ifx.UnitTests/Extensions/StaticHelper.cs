using VisionaryCoder.Framework.Extensions;
using System.Runtime.CompilerServices;

namespace VisionaryCoder.Framework.Tests.Extensions;

public static class StaticHelper
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string GetCallingClass()
    {
        return ReflectionExtensions.NameOfCallingClass();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Type? GetCallingType()
    {
        return ReflectionExtensions.TypeOfCallingClass();
    }
}
