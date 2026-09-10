using System.Diagnostics;
using System.Reflection;

namespace VisionaryCoder.Framework.Extensions;
/// <summary>
/// Provides helper methods for reflection operations.
/// </summary>
public static class ReflectionExtensions
{
    /// <summary>
    /// Gets the name of the calling class.
    /// </summary>
    /// <returns>The name of the calling class. Returns the method name if the class is not found.</returns>
    public static string NameOfCallingClass()
    {
        string fullName;
        Type? declaringType;
        int skipFrames = 2;
        do
        {
            MethodBase? method = new StackFrame(skipFrames, false).GetMethod();
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
    /// Reads the stack frame to get the root calling type.
    /// <returns>The type of the calling class, or <c>null</c> if not found.</returns>
    public static Type? TypeOfCallingClass()
    {
        return new StackFrame(2).GetMethod()?.ReflectedType;
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
        MethodInfo? method = obj.GetType().GetMethod(methodName);
        if (method is null)
        {
            throw new MissingMethodException(methodName);
        }
        return method.Invoke(obj, parameters);
    }
}
