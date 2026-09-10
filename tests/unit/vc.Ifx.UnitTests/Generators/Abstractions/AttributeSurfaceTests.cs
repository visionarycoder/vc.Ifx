using System.Reflection;
using vc.Ifx.Generators.Abstractions.Attributes;

namespace VisionaryCoder.Framework.Tests.Generators.Abstractions;

[TestClass]
public sealed class AttributeSurfaceTests
{
    [TestMethod]
    [DataRow(typeof(GenerateEndpointAttribute), AttributeTargets.Method)]
    [DataRow(typeof(GenerateEnumerationAttribute), AttributeTargets.Enum)]
    [DataRow(typeof(GenerateInterceptorsAttribute), AttributeTargets.Class)]
    public void AttributeUsageRemainsSealedSingleAndNonInherited(Type type, AttributeTargets targets)
    {
        var usage = type.GetCustomAttribute<AttributeUsageAttribute>()!;
        type.IsPublic.Should().BeTrue();
        type.IsSealed.Should().BeTrue();
        type.BaseType.Should().Be(typeof(Attribute));
        usage.ValidOn.Should().Be(targets);
        usage.AllowMultiple.Should().BeFalse();
        usage.Inherited.Should().BeFalse();
    }

    [TestMethod]
    public void MarkerAssemblyHasNoRuntimeProviderOrToolingDependency()
    {
        var references = typeof(GenerateEndpointAttribute).Assembly.GetReferencedAssemblies();
        references.Should().OnlyContain(reference => reference.Name == "netstandard" || reference.Name!.StartsWith("System.", StringComparison.Ordinal));
        typeof(GenerateEndpointAttribute).Assembly.GetExportedTypes().Should().BeEquivalentTo(
            new[] { typeof(GenerateEndpointAttribute), typeof(GenerateEnumerationAttribute), typeof(GenerateInterceptorsAttribute) });
    }

    [TestMethod]
    public void ConstructorParameterNamesAndReadOnlyMembersRemainCompatible()
    {
        typeof(GenerateEnumerationAttribute).GetConstructor([typeof(string)])!.GetParameters()[0].Name.Should().Be("name");
        typeof(GenerateEnumerationAttribute).GetConstructor([typeof(string), typeof(string)])!.GetParameters().Select(parameter => parameter.Name)
            .Should().Equal("name", "defaultName");
        var interceptor = typeof(GenerateInterceptorsAttribute).GetConstructor([typeof(string), typeof(Type[])])!;
        interceptor.GetParameters().Select(parameter => parameter.Name).Should().Equal("activitySourceName", "serviceTypes");
        interceptor.GetParameters()[1].IsDefined(typeof(ParamArrayAttribute)).Should().BeTrue();
        typeof(GenerateEndpointAttribute).GetConstructor([typeof(string), typeof(string)])!.GetParameters().Select(parameter => parameter.Name)
            .Should().Equal("route", "httpMethod");

        typeof(GenerateEnumerationAttribute).GetProperty(nameof(GenerateEnumerationAttribute.ClassName))!.CanWrite.Should().BeFalse();
        typeof(GenerateEnumerationAttribute).GetProperty(nameof(GenerateEnumerationAttribute.Name))!.CanWrite.Should().BeFalse();
        typeof(GenerateInterceptorsAttribute).GetProperty(nameof(GenerateInterceptorsAttribute.ActivitySourceName))!.CanWrite.Should().BeFalse();
        typeof(GenerateInterceptorsAttribute).GetProperty(nameof(GenerateInterceptorsAttribute.ServiceTypes))!.CanWrite.Should().BeFalse();
        typeof(GenerateEndpointAttribute).GetProperty(nameof(GenerateEndpointAttribute.Route))!.CanWrite.Should().BeFalse();
        typeof(GenerateEndpointAttribute).GetProperty(nameof(GenerateEndpointAttribute.HttpMethod))!.CanWrite.Should().BeFalse();
    }
}
