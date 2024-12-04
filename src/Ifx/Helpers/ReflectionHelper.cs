using System.Diagnostics;
#pragma warning disable ClassMethodMissingInterface
#pragma warning disable DerivedClasses

namespace vc.Ifx.Helpers;

public static class ReflectionHelper
{

    /// <summary>
    /// Get the name of the calling class.
    /// </summary>
    /// <returns>Name of the calling class.  Returns if not found.</returns>
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
    /// Read the stack frame to get the root calling type.
    /// </summary>
    /// <returns>Nullable calling type.</returns>
    public static Type? TypeOfCallingClass()
    {

        return new StackFrame(2).GetMethod()?.ReflectedType;

    }

}