using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Moq;
using Ifx.Extensions;

namespace Ifx.Tests.Extensions;

[TestClass]
public class ReflectionExtensionsTests
{
    #region NameOfCallingClass Tests

    [TestMethod]
    public void NameOfCallingClass_FromTestMethod_ShouldReturnTestClassName()
    {
        // Act
        string result = GetCallingClassName();

        // Assert
        result.Should().Contain("ReflectionExtensionsTests");
    }

    [TestMethod]
    public void NameOfCallingClass_FromNestedCall_ShouldReturnOriginalCaller()
    {
        // Act
        string result = GetCallingClassNameNested();

        // Assert
        result.Should().Contain("ReflectionExtensionsTests");
    }

    [TestMethod]
    public void NameOfCallingClass_FromStaticMethod_ShouldReturnCallingClass()
    {
        // Act
        string result = StaticHelper.GetCallingClass();

        // Assert
        result.Should().Contain("ReflectionExtensionsTests");
    }

    [TestMethod]
    public void NameOfCallingClass_OnNet10_ShouldRecognizeTheCoreLibraryModule()
    {
        // Assert
        typeof(object).Module.Name.Should().Be("System.Private.CoreLib.dll");
        typeof(object).Module.Name.Should().NotBe("mscorlib.dll");
    }

    [TestMethod]
    public void NameOfCallingClass_WithCoreLibraryFrame_ShouldSkipToTheUserFrame()
    {
        // Arrange
        StackTrace stack = CreateStackTrace(
            typeof(string).GetMethod(nameof(string.Clone), Type.EmptyTypes)!,
            typeof(ReflectionExtensionsTests).GetMethod(nameof(NameOfCallingClass_WithCoreLibraryFrame_ShouldSkipToTheUserFrame))!);

        // Act
        string result = InvokeNameOfCallingClass(stack);

        // Assert
        result.Should().Be(typeof(ReflectionExtensionsTests).FullName);
    }

    [TestMethod]
    public void NameOfCallingClass_WhenTheFrameIsMissing_ShouldReturnUnknown()
    {
        // Arrange
        var stack = new Mock<StackTrace>();
        stack.Setup(value => value.GetFrame(2)).Returns((StackFrame?)null);

        // Act
        string result = InvokeNameOfCallingClass(stack.Object);

        // Assert
        result.Should().Be("Unknown");
    }

    [TestMethod]
    public void NameOfCallingClass_WhenTheFrameHasNoDeclaringType_ShouldReturnTheMethodName()
    {
        // Arrange
        var stack = new Mock<StackTrace>();
        var frame = new Mock<StackFrame>();

        frame.Setup(value => value.GetMethod()).Returns(new DynamicMethod("GlobalMethod", typeof(void), Type.EmptyTypes));
        stack.Setup(value => value.GetFrame(2)).Returns(frame.Object);

        // Act
        string result = InvokeNameOfCallingClass(stack.Object);

        // Assert
        result.Should().Be("GlobalMethod");
    }

    [TestMethod]
    public void NameOfCallingClass_WhenTheDeclaringTypeHasNoFullName_ShouldReturnTheTypeName()
    {
        // Arrange
        var module = new Mock<Module>();
        var declaringType = new Mock<Type>();
        var method = new Mock<MethodInfo>();
        StackTrace stack = CreateStackTrace(method.Object);

        module.SetupGet(value => value.Name).Returns("VerificationModule.dll");
        declaringType.SetupGet(value => value.Module).Returns(module.Object);
        declaringType.SetupGet(value => value.FullName).Returns((string?)null);
        declaringType.SetupGet(value => value.Name).Returns("FallbackTypeName");
        method.SetupGet(value => value.DeclaringType).Returns(declaringType.Object);

        // Act
        string result = InvokeNameOfCallingClass(stack);

        // Assert
        result.Should().Be("FallbackTypeName");
    }

    // Helper methods for testing NameOfCallingClass
    [MethodImpl(MethodImplOptions.NoInlining)]
    private string GetCallingClassName()
    {
        return ReflectionExtensions.NameOfCallingClass();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private string GetCallingClassNameNested()
    {
        return GetCallingClassName();
    }

    private static StackTrace CreateStackTrace(MethodBase runtimeMethod, MethodBase userMethod)
    {
        var stack = new Mock<StackTrace>();
        var runtimeFrame = new Mock<StackFrame>();
        var userFrame = new Mock<StackFrame>();

        runtimeFrame.Setup(value => value.GetMethod()).Returns(runtimeMethod);
        userFrame.Setup(value => value.GetMethod()).Returns(userMethod);
        stack.Setup(value => value.GetFrame(2)).Returns(runtimeFrame.Object);
        stack.Setup(value => value.GetFrame(3)).Returns(userFrame.Object);

        return stack.Object;
    }

    private static StackTrace CreateStackTrace(MethodBase method)
    {
        var stack = new Mock<StackTrace>();
        var frame = new Mock<StackFrame>();

        frame.Setup(value => value.GetMethod()).Returns(method);
        stack.Setup(value => value.GetFrame(2)).Returns(frame.Object);

        return stack.Object;
    }

    private static string InvokeNameOfCallingClass(StackTrace stack)
    {
        return (string)typeof(ReflectionExtensions)
            .GetMethod("NameOfCallingClass", BindingFlags.NonPublic | BindingFlags.Static, [typeof(StackTrace)])!
            .Invoke(null, [stack])!;
    }

    #endregion

    #region TypeOfCallingClass Tests

    [TestMethod]
    public void TypeOfCallingClass_FromTestMethod_ShouldReturnTestClassType()
    {
        // Act
        Type? result = GetCallingClassType();

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Contain("ReflectionExtensionsTests");
    }

    [TestMethod]
    public void TypeOfCallingClass_FromNestedCall_ShouldReturnOriginalCallerType()
    {
        // Act
        Type? result = GetCallingClassTypeNested();

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Contain("ReflectionExtensionsTests");
    }

    [TestMethod]
    public void TypeOfCallingClass_FromStaticMethod_ShouldReturnCallingClassType()
    {
        // Act
        Type? result = StaticHelper.GetCallingType();

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Contain("ReflectionExtensionsTests");
    }

    [TestMethod]
    public void TypeOfCallingClass_WhenTheFrameIsMissing_ShouldReturnNull()
    {
        // Arrange
        var stack = new Mock<StackTrace>();
        stack.Setup(value => value.GetFrame(2)).Returns((StackFrame?)null);

        // Act
        Type? result = InvokeTypeOfCallingClass(stack.Object);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void TypeOfCallingClass_WhenTheFrameHasNoMethod_ShouldReturnNull()
    {
        // Arrange
        var stack = new Mock<StackTrace>();
        var frame = new Mock<StackFrame>();

        frame.Setup(value => value.GetMethod()).Returns((MethodBase?)null);
        stack.Setup(value => value.GetFrame(2)).Returns(frame.Object);

        // Act
        Type? result = InvokeTypeOfCallingClass(stack.Object);

        // Assert
        result.Should().BeNull();
    }

    // Helper methods for testing TypeOfCallingClass
    [MethodImpl(MethodImplOptions.NoInlining)]
    private Type? GetCallingClassType()
    {
        return ReflectionExtensions.TypeOfCallingClass();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private Type? GetCallingClassTypeNested()
    {
        return GetCallingClassType();
    }

    private static Type? InvokeTypeOfCallingClass(StackTrace stack)
    {
        return (Type?)typeof(ReflectionExtensions)
            .GetMethod("TypeOfCallingClass", BindingFlags.NonPublic | BindingFlags.Static, [typeof(StackTrace)])!
            .Invoke(null, [stack]);
    }

    #endregion

    #region ImplementsInterface Tests

    [TestMethod]
    public void ImplementsInterface_WithTypeImplementingInterface_ShouldReturnTrue()
    {
        // Arrange
        Type type = typeof(List<string>);
        Type interfaceType = typeof(IList);

        // Act
        bool result = type.ImplementsInterface(interfaceType);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void ImplementsInterface_WithTypeNotImplementingInterface_ShouldReturnFalse()
    {
        // Arrange
        Type type = typeof(string);
        Type interfaceType = typeof(IList);

        // Act
        bool result = type.ImplementsInterface(interfaceType);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void ImplementsInterface_WithGenericInterface_ShouldReturnTrue()
    {
        // Arrange
        Type type = typeof(List<int>);
        Type interfaceType = typeof(IEnumerable<int>);

        // Act
        bool result = type.ImplementsInterface(interfaceType);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void ImplementsInterface_WithNonInterfaceType_ShouldReturnFalse()
    {
        // Arrange
        Type type = typeof(List<string>);
        Type interfaceType = typeof(string); // Not an interface

        // Act
        bool result = type.ImplementsInterface(interfaceType);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void ImplementsInterface_WithSameType_ShouldReturnTrueForInterface()
    {
        // Arrange
        Type interfaceType = typeof(IDisposable);

        // Act
        bool result = interfaceType.ImplementsInterface(interfaceType);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void ImplementsInterface_WithSameType_ShouldReturnFalseForClass()
    {
        // Arrange
        Type classType = typeof(string);

        // Act
        bool result = classType.ImplementsInterface(classType);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void ImplementsInterface_WithNullType_ShouldThrowArgumentNullException()
    {
        // Arrange
        Type? type = null;
        Type interfaceType = typeof(IDisposable);

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => type!.ImplementsInterface(interfaceType));
        exception.ParamName.Should().Be("type");
    }

    [TestMethod]
    public void ImplementsInterface_WithNullInterfaceType_ShouldThrowArgumentNullException()
    {
        // Arrange
        Type type = typeof(string);
        Type? interfaceType = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => type.ImplementsInterface(interfaceType!));
        exception.ParamName.Should().Be("interfaceType");
    }

    [TestMethod]
    public void ImplementsInterface_WithComplexInheritance_ShouldReturnTrue()
    {
        // Arrange
        Type type = typeof(Dictionary<string, int>);
        Type interfaceType = typeof(IEnumerable);

        // Act
        bool result = type.ImplementsInterface(interfaceType);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region InvokeMethod Tests

    [TestMethod]
    public void InvokeMethod_WithValidMethodAndNoParameters_ShouldThrowAmbiguousMatchException()
    {
        // Arrange
        string obj = "Hello World";
        string methodName = "GetHashCode"; // This method has overloads causing ambiguous match

        // Act & Assert - GetHashCode has overloads causing AmbiguousMatchException
        Func<object?> act = () => obj.InvokeMethod(methodName);
        act.Should().Throw<AmbiguousMatchException>();
    }

    [TestMethod]
    public void InvokeMethod_WithValidMethodAndParameters_ShouldThrowAmbiguousMatchException()
    {
        // Arrange
        string obj = "Hello World";
        string methodName = "IndexOf";
        object[] parameters = ["World"];

        // Act & Assert - IndexOf has overloads causing AmbiguousMatchException
        Func<object?> act = () => obj.InvokeMethod(methodName, parameters);
        act.Should().Throw<AmbiguousMatchException>();
    }

    [TestMethod]
    public void InvokeMethod_WithMultipleParameters_ShouldThrowAmbiguousMatchException()
    {
        // Arrange
        string obj = "Hello World";
        string methodName = "Replace";
        object[] parameters = ["World", "Universe"];

        // Act & Assert - Replace has overloads causing AmbiguousMatchException
        Func<object?> act = () => obj.InvokeMethod(methodName, parameters);
        act.Should().Throw<AmbiguousMatchException>();
    }

    [TestMethod]
    public void InvokeMethod_WithVoidMethod_ShouldReturnNull()
    {
        // Arrange
        var list = new List<string>();
        string methodName = "Add";
        object[] parameters = ["test"];

        // Act
        object? result = list.InvokeMethod(methodName, parameters);

        // Assert
        result.Should().BeNull();
        list.Should().Contain("test");
    }

    [TestMethod]
    public void InvokeMethod_WithStaticLikeInstance_ShouldWork()
    {
        // Arrange
        var obj = new TestClass();
        string methodName = "GetValue";

        // Act
        object? result = obj.InvokeMethod(methodName);

        // Assert
        result.Should().Be("TestValue");
    }

    [TestMethod]
    public void InvokeMethod_WithMethodThatThrows_ShouldPropagateException()
    {
        // Arrange
        var obj = new TestClass();
        string methodName = "ThrowException";

        // Act & Assert
        TargetInvocationException? exception = Assert.ThrowsExactly<TargetInvocationException>(() => obj.InvokeMethod(methodName));
        exception.InnerException.Should().BeOfType<InvalidOperationException>();
    }

    [TestMethod]
    public void InvokeMethod_WithNonExistentMethod_ShouldThrowMissingMethodException()
    {
        // Arrange
        string obj = "test";
        string methodName = "NonExistentMethod";

        // Act & Assert
        MissingMethodException? exception = Assert.ThrowsExactly<MissingMethodException>(() => obj.InvokeMethod(methodName));
        exception.Message.Should().Contain("NonExistentMethod");
    }

    [TestMethod]
    public void InvokeMethod_WithNullObject_ShouldThrowArgumentNullException()
    {
        // Arrange
        object? obj = null;
        string methodName = "ToString";

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => obj!.InvokeMethod(methodName));
        exception.ParamName.Should().Be("obj");
    }

    [TestMethod]
    public void InvokeMethod_WithNullMethodName_ShouldThrowArgumentNullException()
    {
        // Arrange
        string obj = "test";
        string? methodName = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => obj.InvokeMethod(methodName!));
        exception.ParamName.Should().Be("methodName");
    }

    [TestMethod]
    public void InvokeMethod_WithWrongParameterTypes_ShouldThrowAmbiguousMatchException()
    {
        // Arrange
        string obj = "Hello World";
        string methodName = "IndexOf";
        object[] parameters = [123, "extra param"]; // Wrong parameter types/count

        // Act & Assert
        // Note: The implementation has a flaw - it doesn't handle overloaded methods properly
        Assert.ThrowsExactly<AmbiguousMatchException>(() => obj.InvokeMethod(methodName, parameters));
    }

    [TestMethod]
    public void InvokeMethod_WithOverloadedMethod_ShouldThrowAmbiguousMatchException()
    {
        // Arrange
        var obj = new TestClass();
        string methodName = "OverloadedMethod";

        // Act & Assert
        // Note: The current implementation doesn't handle method overloads properly
        Assert.ThrowsExactly<AmbiguousMatchException>(() => obj.InvokeMethod(methodName));
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void ReflectionExtensions_ComplexScenario_ShouldWorkTogether()
    {
        // Arrange
        var testObj = new TestClass();

        // Act & Assert
        // Test type checking
        bool implementsDisposable = testObj.GetType().ImplementsInterface(typeof(IDisposable));
        implementsDisposable.Should().BeTrue();

        // Test method invocation
        object? result = testObj.InvokeMethod("GetValue");
        result.Should().Be("TestValue");

        // Test calling class detection
        string callingClass = GetCallingClassName();
        callingClass.Should().Contain("ReflectionExtensionsTests");

        // Test calling type detection
        Type? callingType = GetCallingClassType();
        callingType.Should().NotBeNull();
        callingType!.Name.Should().Contain("ReflectionExtensionsTests");
    }

    [TestMethod]
    public void ReflectionExtensions_RealWorldScenario_ShouldHandleComplexTypes()
    {
        // Arrange
        var dictionary = new Dictionary<string, object>
        {
            ["test"] = "value"
        };

        // Act & Assert
        // Test interface implementation checking
        dictionary.GetType().ImplementsInterface(typeof(IDictionary<string, object>)).Should().BeTrue();
        dictionary.GetType().ImplementsInterface(typeof(IEnumerable)).Should().BeTrue();
        dictionary.GetType().ImplementsInterface(typeof(IDisposable)).Should().BeFalse();

        // Test method invocation
        object? containsResult = dictionary.InvokeMethod("ContainsKey", "test");
        containsResult.Should().Be(true);

        object? countResult = dictionary.InvokeMethod("get_Count");
        countResult.Should().Be(1);
    }

    #endregion
}

public class TestClass : IDisposable
{
    public string GetValue() => "TestValue";
    public void ThrowException() => throw new InvalidOperationException("Expected fixture failure.");
    public void OverloadedMethod() { }
    public void OverloadedMethod(int value) { }
    public void Dispose() { }
}
