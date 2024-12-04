using System.Collections;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace vc.Ifx.Extensions;

public static class EnumerableExtension
{

    public static bool IsNonStringEnumerable(this PropertyInfo pi)
    {
        Contract.Assert(pi != null, nameof(pi) + " != null");
        return pi.PropertyType.IsNonStringEnumerable();
    }

    public static bool IsNonStringEnumerable(this object instance)
    {
        Contract.Assert(instance != null, nameof(instance) + " != null");
        return instance.GetType().IsNonStringEnumerable();
    }

    public static bool IsNonStringEnumerable(this Type type)
    {
        return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
    }

}