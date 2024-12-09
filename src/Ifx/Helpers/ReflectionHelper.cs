using System.Diagnostics;
#pragma warning disable ClassMethodMissingInterface
#pragma warning disable DerivedClasses

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
                return method!.Name;
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
}
