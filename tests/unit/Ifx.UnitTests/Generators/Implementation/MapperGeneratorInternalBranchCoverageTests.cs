using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Ifx.Generators;
using Moq;

namespace Ifx.Tests.Generators.Implementation;

[TestClass]
public sealed class MapperGeneratorInternalBranchCoverageTests
{
    [TestMethod]
    public void PropertyHelpersFilterPublicInstanceMembersAcrossClassAndInterfaceHierarchies()
    {
        var compilation = GeneratorHarness.Compilation("""
            namespace Example
            {
                public interface IContract
                {
                    int InterfaceValue { get; set; }
                }

                public class Base
                {
                    public int BaseValue { get; set; }
                }

                public class Sample : Base, IContract
                {
                    public static int StaticValue { get; set; }
                    public int ReadWrite { get; set; }
                    public int ReadOnly { get; }
                    public int WriteOnly { set { } }
                    internal int Hidden { get; set; }
                    int IContract.InterfaceValue { get; set; }
                }
            }
            """);
        var readableProperties = GetStaticMethod("ReadableProperties");
        var writableProperties = GetStaticMethod("WritableProperties");
        var allProperties = GetStaticMethod("AllProperties");
        var sampleType = compilation.GetTypeByMetadataName("Example.Sample")!;
        var interfaceType = compilation.GetTypeByMetadataName("Example.IContract")!;

        var readable = Invoke<Dictionary<string, IPropertySymbol>>(readableProperties, sampleType);
        readable.Keys.Should().BeEquivalentTo(["BaseValue", "ReadOnly", "ReadWrite"]);

        var writable = Invoke<IEnumerable<IPropertySymbol>>(writableProperties, sampleType).Select(property => property.Name).ToArray();
        writable.Should().BeEquivalentTo(["BaseValue", "ReadWrite", "WriteOnly"]);

        var classProperties = Invoke<IEnumerable<IPropertySymbol>>(allProperties, sampleType).Select(property => property.Name).ToArray();
        classProperties.Should().Contain(["BaseValue", "Hidden", "ReadOnly", "ReadWrite", "StaticValue", "WriteOnly"]);
        classProperties.Should().Contain(name => name.EndsWith("InterfaceValue", StringComparison.Ordinal));

        var interfaceProperties = Invoke<IEnumerable<IPropertySymbol>>(allProperties, interfaceType).Select(property => property.Name).ToArray();
        interfaceProperties.Should().Equal("InterfaceValue");
    }

    [TestMethod]
    public void AttributeHelperMatchesExactAttributeNames()
    {
        var compilation = GeneratorHarness.Compilation("""
            using Ifx.Generators.Abstractions.Attributes;
            using System;

            namespace Example
            {
                public class Sample
                {
                    [MapIgnore]
                    public int Ignored { get; set; }

                    public int Included { get; set; }

                    [Obsolete]
                    public int Decorated { get; set; }
                }
            }
            """);
        var hasAttribute = GetStaticMethod("HasAttribute");
        var sampleType = compilation.GetTypeByMetadataName("Example.Sample")!;
        var ignoredProperty = sampleType.GetMembers("Ignored").OfType<IPropertySymbol>().Single();
        var includedProperty = sampleType.GetMembers("Included").OfType<IPropertySymbol>().Single();
        var decoratedProperty = sampleType.GetMembers("Decorated").OfType<IPropertySymbol>().Single();
        var detachedSymbol = new Mock<ISymbol>();
        detachedSymbol.Setup(symbol => symbol.GetAttributes()).Returns([new Mock<AttributeData>().Object]);

        Invoke<bool>(hasAttribute, ignoredProperty, "Ifx.Generators.Abstractions.Attributes.MapIgnoreAttribute").Should().BeTrue();
        Invoke<bool>(hasAttribute, includedProperty, "Ifx.Generators.Abstractions.Attributes.MapIgnoreAttribute").Should().BeFalse();
        Invoke<bool>(hasAttribute, decoratedProperty, "Ifx.Generators.Abstractions.Attributes.MapIgnoreAttribute").Should().BeFalse();
        Invoke<bool>(hasAttribute, detachedSymbol.Object, "Ifx.Generators.Abstractions.Attributes.MapIgnoreAttribute").Should().BeFalse();
    }

    [TestMethod]
    public void MapFromHelperReturnsRedirectOnlyForMatchingAttribute()
    {
        var compilation = GeneratorHarness.Compilation("""
            using Ifx.Generators.Abstractions.Attributes;
            using System;

            namespace Example
            {
                public class Sample
                {
                    [MapFrom("SourceId")]
                    public int Redirected { get; set; }

                    [Obsolete]
                    public int Decorated { get; set; }

                    public int Plain { get; set; }
                }
            }
            """);
        var getMapFromSourceName = GetStaticMethod("GetMapFromSourceName");
        var sampleType = compilation.GetTypeByMetadataName("Example.Sample")!;
        var redirectedProperty = sampleType.GetMembers("Redirected").OfType<IPropertySymbol>().Single();
        var decoratedProperty = sampleType.GetMembers("Decorated").OfType<IPropertySymbol>().Single();
        var plainProperty = sampleType.GetMembers("Plain").OfType<IPropertySymbol>().Single();
        var detachedProperty = new Mock<IPropertySymbol>();
        detachedProperty.Setup(property => property.GetAttributes()).Returns([new StubAttributeData()]);

        Invoke<string?>(getMapFromSourceName, redirectedProperty).Should().Be("SourceId");
        Invoke<string?>(getMapFromSourceName, decoratedProperty).Should().BeNull();
        Invoke<string?>(getMapFromSourceName, plainProperty).Should().BeNull();
        Invoke<string?>(getMapFromSourceName, detachedProperty.Object).Should().BeNull();
    }

    [TestMethod]
    public void GeneratorAttributeHelpersInterpretBidirectionalFlagsAndFallbackLocations()
    {
        var compilation = GeneratorHarness.Compilation("""
            using Ifx.Generators.Abstractions.Attributes;

            namespace Target
            {
                public class One { }
                public class Two { }
                public class Three { }
            }

            namespace Source
            {
                [GenerateMapper(typeof(Target.One))]
                public class OneWay { }

                [GenerateMapper(typeof(Target.Two), Bidirectional = false)]
                public class ExplicitFalse { }

                [GenerateMapper(typeof(Target.Three), Bidirectional = true)]
                public class ExplicitTrue { }
            }
            """);
        var isBidirectional = GetStaticMethod("IsBidirectional");
        var getAttributeLocation = GetStaticMethod("GetAttributeLocation");
        var oneWayAttribute = compilation.GetTypeByMetadataName("Source.OneWay")!.GetAttributes().Single();
        var explicitFalseAttribute = compilation.GetTypeByMetadataName("Source.ExplicitFalse")!.GetAttributes().Single();
        var explicitTrueAttribute = compilation.GetTypeByMetadataName("Source.ExplicitTrue")!.GetAttributes().Single();
        var detachedAttribute = new StubAttributeData();
        var unrelatedNamedArgumentAttribute = new StubAttributeData(
            namedArguments: [new KeyValuePair<string, TypedConstant>("Other", default)]);
        var falseBidirectionalAttribute = new StubAttributeData(
            namedArguments: [new KeyValuePair<string, TypedConstant>("Bidirectional", default)]);

        Invoke<bool>(isBidirectional, oneWayAttribute).Should().BeFalse();
        Invoke<bool>(isBidirectional, explicitFalseAttribute).Should().BeFalse();
        Invoke<bool>(isBidirectional, explicitTrueAttribute).Should().BeTrue();
        Invoke<bool>(isBidirectional, unrelatedNamedArgumentAttribute).Should().BeFalse();
        Invoke<bool>(isBidirectional, falseBidirectionalAttribute).Should().BeFalse();
        Invoke<Location>(getAttributeLocation, explicitTrueAttribute, CancellationToken.None).IsInSource.Should().BeTrue();
        Invoke<Location>(getAttributeLocation, detachedAttribute, CancellationToken.None).Should().BeSameAs(Location.None);
    }

    [TestMethod]
    public void DiagnosticLocationHelperUsesFallbackWhenSymbolHasNoLocations()
    {
        var compilation = GeneratorHarness.Compilation("""
            namespace Example
            {
                public class Sample
                {
                    public int Value { get; set; }
                }
            }
            """);
        var getDiagnosticLocation = GetStaticMethod("GetDiagnosticLocation");
        var property = compilation.GetTypeByMetadataName("Example.Sample")!.GetMembers("Value").OfType<IPropertySymbol>().Single();
        var fallback = Location.None;
        var locationlessSymbol = new Mock<ISymbol>();
        locationlessSymbol.Setup(symbol => symbol.Locations).Returns(ImmutableArray<Location>.Empty);

        Invoke<Location>(getDiagnosticLocation, property, fallback).IsInSource.Should().BeTrue();
        Invoke<Location>(getDiagnosticLocation, locationlessSymbol.Object, fallback).Should().BeSameAs(fallback);
    }

    private static MethodInfo GetStaticMethod(string name) =>
        typeof(MapperGenerator).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;

    private static T Invoke<T>(MethodInfo method, params object?[] args) =>
        (T)method.Invoke(null, args)!;

    private sealed class StubAttributeData(
        INamedTypeSymbol? attributeClass = null,
        SyntaxReference? applicationSyntaxReference = null,
        ImmutableArray<TypedConstant> constructorArguments = default,
        ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments = default) : AttributeData
    {
        protected override INamedTypeSymbol? CommonAttributeClass => attributeClass;
        protected override IMethodSymbol? CommonAttributeConstructor => null;
        protected override SyntaxReference? CommonApplicationSyntaxReference => applicationSyntaxReference;
        protected override ImmutableArray<TypedConstant> CommonConstructorArguments => constructorArguments.IsDefault ? [] : constructorArguments;
        protected override ImmutableArray<KeyValuePair<string, TypedConstant>> CommonNamedArguments => namedArguments.IsDefault ? [] : namedArguments;
    }
}
