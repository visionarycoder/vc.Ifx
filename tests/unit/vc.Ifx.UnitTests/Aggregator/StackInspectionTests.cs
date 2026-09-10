using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Moq;
using VisionaryCoder.Framework.Extensions;

namespace VisionaryCoder.Framework.Tests.Aggregator;

[TestClass]
public sealed class StackInspectionTests
{
    [TestMethod]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void PhysicalWrapperBoundaryReportsTheActualCallerInOptimizedBuilds()
    {
        Assert.AreEqual(typeof(StackInspectionTests).FullName, Wrapper.Name());
        Assert.AreEqual(typeof(StackInspectionTests), Wrapper.Type());
    }

    [TestMethod]
    public void MissingFramesAndMethodsYieldUnknownRatherThanInventingACaller()
    {
        var stack = new Mock<StackTrace>();
        Assert.AreEqual("Unknown", Inspect<string>("NameOfCallingClass", stack.Object));
        Assert.IsNull(Inspect<Type?>("TypeOfCallingClass", stack.Object));
        var frame = new Mock<StackFrame>();
        stack.Setup(value => value.GetFrame(2)).Returns(frame.Object);
        Assert.AreEqual("Unknown", Inspect<string>("NameOfCallingClass", stack.Object));
        Assert.IsNull(Inspect<Type?>("TypeOfCallingClass", stack.Object));
        frame.Setup(value => value.GetMethod()).Returns(new DynamicMethod("GlobalMethod", typeof(void), Type.EmptyTypes));
        Assert.AreEqual("GlobalMethod", Inspect<string>("NameOfCallingClass", stack.Object));
        Assert.IsNull(Inspect<Type?>("TypeOfCallingClass", stack.Object));
    }

    [TestMethod]
    public void LegacyRuntimeFramesAreSkippedBeforeReportingTheUserType()
    {
        // The current runtime cannot load mscorlib execution frames; model only
        // this retained legacy metadata branch, separately from the real-stack test.
        var module = new Mock<Module>();
        module.SetupGet(value => value.Name).Returns("mscorlib.dll");
        var type = new Mock<Type>();
        type.SetupGet(value => value.Module).Returns(module.Object);
        type.SetupGet(value => value.FullName).Returns("LegacyRuntimeFrame");
        var method = new Mock<MethodInfo>();
        method.SetupGet(value => value.DeclaringType).Returns(type.Object);
        var stack = new Mock<StackTrace>();
        var runtimeFrame = new Mock<StackFrame>();
        runtimeFrame.Setup(value => value.GetMethod()).Returns(method.Object);
        var userFrame = new Mock<StackFrame>();
        userFrame.Setup(value => value.GetMethod()).Returns(typeof(StackInspectionTests).GetMethod(nameof(LegacyRuntimeFramesAreSkippedBeforeReportingTheUserType)));
        stack.Setup(value => value.GetFrame(2)).Returns(runtimeFrame.Object);
        stack.Setup(value => value.GetFrame(3)).Returns(userFrame.Object);
        Assert.AreEqual(typeof(StackInspectionTests).FullName, Inspect<string>("NameOfCallingClass", stack.Object));
        stack.Verify(value => value.GetFrame(3), Times.Once);
    }

    private static T Inspect<T>(string name, StackTrace stack) => (T)typeof(ReflectionExtensions)
        .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static, [typeof(StackTrace)])!.Invoke(null, [stack])!;

    private static class Wrapper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string Name() => ReflectionExtensions.NameOfCallingClass();
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Type? Type() => ReflectionExtensions.TypeOfCallingClass();
    }
}
