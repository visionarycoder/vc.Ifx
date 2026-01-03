namespace VisionaryCoder.Framework.Tests.Extensions;

public class TestClass : IDisposable
{

    public string GetValue() => "TestValue";

    public void ThrowException() => throw new InvalidOperationException("Test exception");

    public string OverloadedMethod() => "No parameters";

    public string OverloadedMethod(string parameter) => $"String parameter: {parameter}";

    public string OverloadedMethod(int parameter) => $"Int parameter: {parameter}";

    public void Dispose() { }

}
