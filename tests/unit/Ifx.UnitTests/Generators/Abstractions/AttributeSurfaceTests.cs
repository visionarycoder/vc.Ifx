using System.Reflection;
using Ifx.Generators.Abstractions.Attributes;

namespace Ifx.Tests.Generators.Abstractions;

[TestClass]
public sealed class AttributeSurfaceTests
{
    [TestMethod]
    [DataRow(typeof(GenerateEndpointAttribute), AttributeTargets.Method, false)]
    [DataRow(typeof(GenerateEnumerationAttribute), AttributeTargets.Enum, false)]
    [DataRow(typeof(GenerateInterceptorsAttribute), AttributeTargets.Class, false)]
    [DataRow(typeof(GenerateMapperAttribute), AttributeTargets.Class, true)]
    [DataRow(typeof(MapFromAttribute), AttributeTargets.Property, false)]
    [DataRow(typeof(MapIgnoreAttribute), AttributeTargets.Property, false)]
    public void AttributeUsageRemainsSealedAndNonInherited(Type type, AttributeTargets targets, bool allowMultiple)
    {
        var usage = type.GetCustomAttribute<AttributeUsageAttribute>()!;
        type.IsPublic.Should().BeTrue();
        type.IsSealed.Should().BeTrue();
        type.BaseType.Should().Be(typeof(Attribute));
        usage.ValidOn.Should().Be(targets);
        usage.AllowMultiple.Should().Be(allowMultiple);
        usage.Inherited.Should().BeFalse();
    }

    [TestMethod]
    public void MarkerAssemblyHasNoRuntimeProviderOrToolingDependency()
    {
        var references = typeof(GenerateEndpointAttribute).Assembly.GetReferencedAssemblies();
        references.Should().OnlyContain(reference => reference.Name == "netstandard" || reference.Name!.StartsWith("System.", StringComparison.Ordinal));
        typeof(GenerateEndpointAttribute).Assembly.GetExportedTypes().Should().BeEquivalentTo(
            [typeof(GenerateEndpointAttribute), typeof(GenerateEnumerationAttribute), typeof(GenerateInterceptorsAttribute),
                typeof(GenerateMapperAttribute), typeof(MapFromAttribute), typeof(MapIgnoreAttribute)
            ]);
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
