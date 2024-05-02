using System.Collections;
using System.Reflection;

namespace vc.Framework.Extension;

public static class EnumerableEx
{

	public static bool IsNonStringEnumerable(this PropertyInfo pi)
	{
		return pi.PropertyType.IsNonStringEnumerable();
	}

	public static bool IsNonStringEnumerable(this object instance)
	{
		return instance.GetType().IsNonStringEnumerable();
	}

	public static bool IsNonStringEnumerable(this Type type)
	{
		return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
	}

}