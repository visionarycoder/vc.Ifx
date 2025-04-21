using System.Diagnostics;
using System.Reflection;

namespace vc.Ifx.Helpers;

/// <summary>
/// Provides helper methods for reflection operations.
/// </summary>
public static class ReflectionHelper
{
    /// <summary>
    /// Gets the name of the calling class.
    /// </summary>
    /// <returns>The name of the calling class. Returns the method name if the class is not found.</returns>
    public static string NameOfCallingClass()
    {
        string fullName;
        Type? declaringType;
        var skipFrames = 2;
        do
        {
            var method = new StackFrame(skipFrames, false).GetMethod();
            declaringType = method?.DeclaringType;
            if (declaringType == null)
            {
                return method?.Name ?? "Unknown";
            }
            skipFrames++;
            fullName = declaringType.FullName!;
        }
        while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

        return fullName;
    }

    /// <summary>
    /// Reads the stack frame to get the root calling type.
    /// </summary>
    /// <returns>The type of the calling class, or <c>null</c> if not found.</returns>
    public static Type? TypeOfCallingClass()
    {
        return new StackFrame(2).GetMethod()?.ReflectedType;
    }

    /// <summary>
    /// Gets a custom attribute of the specified type from a member.
    /// </summary>
    /// <typeparam name="T">The type of the custom attribute.</typeparam>
    /// <param name="member">The member to retrieve the attribute from.</param>
    /// <returns>The custom attribute of the specified type, or <c>null</c> if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="member"/> is null.</exception>
    public static T? GetCustomAttribute<T>(this MemberInfo member) where T : Attribute
    {
        ArgumentNullException.ThrowIfNull(member);
        return member.GetCustomAttribute<T>();
    }

    /// <summary>
    /// Checks if a type implements a specific interface.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="interfaceType">The interface type to check for.</param>
    /// <returns><c>true</c> if the type implements the specified interface; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> or <paramref name="interfaceType"/> is null.</exception>
    public static bool ImplementsInterface(this Type type, Type interfaceType)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(interfaceType);

        return interfaceType.IsInterface && interfaceType.IsAssignableFrom(type);
    }

    /// <summary>
    /// Dynamically invokes a method on an object.
    /// </summary>
    /// <param name="obj">The object to invoke the method on.</param>
    /// <param name="methodName">The name of the method to invoke.</param>
    /// <param name="parameters">The parameters to pass to the method.</param>
    /// <returns>The result of the method invocation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> or <paramref name="methodName"/> is null.</exception>
    /// <exception cref="MissingMethodException">Thrown if the method is not found.</exception>
    public static object? InvokeMethod(this object obj, string methodName, params object[] parameters)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(methodName);

        var method = obj.GetType().GetMethod(methodName);
        if (method == null)
        {
            throw new MissingMethodException($"Method '{methodName}' not found on type '{obj.GetType().FullName}'.");
        }

        return method.Invoke(obj, parameters);
    }
}
