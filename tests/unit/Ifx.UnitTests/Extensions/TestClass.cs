using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Moq;
using Ifx.Extensions;

namespace Ifx.Tests.Extensions;

public class TestClass : IDisposable
{
    public string GetValue() => "TestValue";
    public void ThrowException() => throw new InvalidOperationException("Expected fixture failure.");
    public void OverloadedMethod() { }
    public void OverloadedMethod(int value) { }
    public void Dispose() { }
}
