using System.Reflection;
using vc.Ifx.Generators.Abstractions.Attributes;

namespace VisionaryCoder.Framework.Tests.Generators.Abstractions;

[TestClass]
public sealed class LegacyAttributeCompatibilityTests
{
    [TestMethod]
    public void EnumerationDefaultsAndNameAliasRemainCompatible()
    {
        var attribute = new GenerateEnumerationAttribute(name: "OrderStatus");

        attribute.ClassName.Should().Be("OrderStatus");
        attribute.Name.Should().Be(attribute.ClassName);
        attribute.Namespace.Should().BeEmpty();
        attribute.DefaultName.Should().BeEmpty();
        attribute.EnumerationNamespace.Should().Be("vc.Ifx.Primitives");
        attribute.EnumerationTypeName.Should().Be("Enumeration");
    }

    [TestMethod]
    public void EnumerationTwoArgumentConstructorAndSettersRemainCompatible()
    {
        var attribute = new GenerateEnumerationAttribute(name: "OrderStatus", defaultName: "Ready");
        attribute.DefaultName.Should().Be("Ready");
        attribute.Namespace = "Example.Generated";
        attribute.DefaultName = "Busy";
        attribute.EnumerationNamespace = "Example.Primitives";
        attribute.EnumerationTypeName = "Code";

        attribute.Namespace.Should().Be("Example.Generated");
        attribute.DefaultName.Should().Be("Busy");
        attribute.EnumerationNamespace.Should().Be("Example.Primitives");
        attribute.EnumerationTypeName.Should().Be("Code");
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void EnumerationRetainsLegacyInvalidAndNullInputs(string? text)
    {
        var attribute = new GenerateEnumerationAttribute(text!, text!)
        {
            Namespace = text!,
            EnumerationNamespace = text!,
            EnumerationTypeName = text!
        };

        attribute.ClassName.Should().Be(text);
        attribute.Name.Should().Be(text);
        attribute.DefaultName.Should().Be(text);
        attribute.Namespace.Should().Be(text);
        attribute.EnumerationNamespace.Should().Be(text);
        attribute.EnumerationTypeName.Should().Be(text);
    }

    [TestMethod]
    public void InterceptorDefaultsAndParamsRemainCompatible()
    {
        var attribute = new GenerateInterceptorsAttribute(activitySourceName: "Orders");

        attribute.ActivitySourceName.Should().Be("Orders");
        attribute.ServiceTypes.Should().BeEmpty();
        attribute.InterceptorSuffix.Should().Be("OpenTelemetryInterceptor");
        attribute.DecoratorSuffix.Should().Be(attribute.InterceptorSuffix);
    }

    [TestMethod]
    public void InterceptorServiceArrayRetainsCallerOwnership()
    {
        Type[] types = [typeof(IDisposable), typeof(IAsyncDisposable)];
        var attribute = new GenerateInterceptorsAttribute("Orders", types);

        attribute.ServiceTypes.Should().BeSameAs(types);
        types[0] = typeof(IComparable);
        attribute.ServiceTypes.Should().Equal(typeof(IComparable), typeof(IAsyncDisposable));
    }

    [TestMethod]
    public void InterceptorSuffixAliasesRemainBidirectional()
    {
        var attribute = new GenerateInterceptorsAttribute("Orders", typeof(IDisposable));
        attribute.InterceptorSuffix = "Trace";
        attribute.DecoratorSuffix.Should().Be("Trace");
        attribute.DecoratorSuffix = "Decorator";
        attribute.InterceptorSuffix.Should().Be("Decorator");
        attribute.DecoratorSuffix = null!;
        attribute.InterceptorSuffix.Should().BeNull();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void InterceptorRetainsLegacyInvalidAndNullInputs(string? text)
    {
        var attribute = new GenerateInterceptorsAttribute(text!, null!) { InterceptorSuffix = text! };

        attribute.ActivitySourceName.Should().Be(text);
        attribute.ServiceTypes.Should().BeNull();
        attribute.DecoratorSuffix.Should().Be(text);
    }

    [TestMethod]
    public void NullServiceArrayElementsRemainRepresentable()
    {
        var attribute = new GenerateInterceptorsAttribute("Orders", new Type[] { null! });
        attribute.ServiceTypes.Should().ContainSingle().Which.Should().BeNull();
    }

    [TestMethod]
    public void LegacyAttributesRetainSystemAttributeEquality()
    {
        var enumeration = new GenerateEnumerationAttribute("Status", "Ready");
        enumeration.Equals(new GenerateEnumerationAttribute("Status", "Ready")).Should().BeTrue();
        enumeration.Equals(new GenerateEnumerationAttribute("Other")).Should().BeFalse();

        var interceptor = new GenerateInterceptorsAttribute("Orders", typeof(IDisposable));
        interceptor.Equals(new GenerateInterceptorsAttribute("Orders", typeof(IDisposable))).Should().BeTrue();
        interceptor.Equals(new GenerateInterceptorsAttribute("Other", typeof(IDisposable))).Should().BeFalse();
    }

    [TestMethod]
    public void DocumentedLegacyDeclarationsCompileAndExposeMetadata()
    {
        var enumeration = typeof(StatusCode).GetCustomAttribute<GenerateEnumerationAttribute>()!;
        enumeration.ClassName.Should().Be("Status");
        enumeration.DefaultName.Should().Be("Ready");
        var interceptor = typeof(OrderService).GetCustomAttribute<GenerateInterceptorsAttribute>()!;
        interceptor.ActivitySourceName.Should().Be("Orders");
        interceptor.ServiceTypes.Should().Equal(typeof(IDisposable));
    }

    [GenerateEnumeration("Status", "Ready")]
    private enum StatusCode { Ready, Busy }

    [GenerateInterceptors("Orders", typeof(IDisposable))]
    private sealed class OrderService;
}
