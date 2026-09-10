using System.Reflection;
using vc.Ifx.Generators.Abstractions.Attributes;

namespace VisionaryCoder.Framework.Tests.Generators.Abstractions;

[TestClass]
public sealed class GenerateEndpointAttributeTests
{
    [TestMethod]
    public void ConstructorPreservesRouteMethodAndOptionalDefaults()
    {
        var attribute = new GenerateEndpointAttribute("/orders/{id:int}", "get");

        attribute.Route.Should().Be("/orders/{id:int}");
        attribute.HttpMethod.Should().Be("get");
        attribute.Name.Should().BeNull();
        attribute.Summary.Should().BeNull();
        attribute.Description.Should().BeNull();
        attribute.Tags.Should().BeEmpty();
        attribute.RequireAuthorization.Should().BeFalse();
        attribute.AuthorizationPolicy.Should().BeNull();
        attribute.AllowAnonymous.Should().BeFalse();
        attribute.ExcludeFromDescription.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(null, null)]
    [DataRow("", "")]
    [DataRow(" ", " ")]
    [DataRow("relative", "GET,POST")]
    [DataRow("/path", "CUSTOM")]
    public void InvalidConstructorMetadataIsRetainedForGeneratorDiagnostics(string? route, string? method)
    {
        var attribute = new GenerateEndpointAttribute(route!, method!);

        attribute.Route.Should().Be(route);
        attribute.HttpMethod.Should().Be(method);
    }

    [TestMethod]
    public void OptionalMetadataRoundTripsWithoutNormalization()
    {
        string[] tags = ["Orders", "Public"];
        var attribute = new GenerateEndpointAttribute("/orders", "POST")
        {
            Name = "CreateOrder",
            Summary = "Create an order",
            Description = "First line\nSecond line",
            Tags = tags,
            RequireAuthorization = true,
            AuthorizationPolicy = "Orders.Write",
            AllowAnonymous = true,
            ExcludeFromDescription = true
        };

        attribute.Name.Should().Be("CreateOrder");
        attribute.Summary.Should().Be("Create an order");
        attribute.Description.Should().Be("First line\nSecond line");
        attribute.Tags.Should().BeSameAs(tags);
        attribute.RequireAuthorization.Should().BeTrue();
        attribute.AuthorizationPolicy.Should().Be("Orders.Write");
        attribute.AllowAnonymous.Should().BeTrue();
        attribute.ExcludeFromDescription.Should().BeTrue();

        attribute.Name = null;
        attribute.Summary = null;
        attribute.Description = null;
        attribute.AuthorizationPolicy = null;
        attribute.RequireAuthorization = false;
        attribute.AllowAnonymous = false;
        attribute.ExcludeFromDescription = false;

        attribute.Name.Should().BeNull();
        attribute.Summary.Should().BeNull();
        attribute.Description.Should().BeNull();
        attribute.AuthorizationPolicy.Should().BeNull();
        attribute.RequireAuthorization.Should().BeFalse();
        attribute.AllowAnonymous.Should().BeFalse();
        attribute.ExcludeFromDescription.Should().BeFalse();
    }

    [TestMethod]
    public void MutableTagOwnershipAndNullMetadataAreExplicit()
    {
        var attribute = new GenerateEndpointAttribute("/orders", "GET");
        string[] tags = ["", null!, "Orders", "Orders"];
        attribute.Tags = tags;
        tags[0] = "Changed";
        attribute.Tags[0].Should().Be("Changed");
        attribute.Tags[1].Should().BeNull();
        attribute.Tags.Should().HaveCount(4);
        attribute.Tags = null!;
        attribute.Tags.Should().BeNull();

        var untouched = new GenerateEndpointAttribute("/orders", "GET");
        untouched.Tags.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    public void InvalidOptionalTextIsRetainedForGeneratorDiagnostics(string text)
    {
        var attribute = new GenerateEndpointAttribute("/orders", "GET")
        {
            Name = text,
            Summary = text,
            Description = text,
            AuthorizationPolicy = text
        };

        attribute.Name.Should().Be(text);
        attribute.Summary.Should().Be(text);
        attribute.Description.Should().Be(text);
        attribute.AuthorizationPolicy.Should().Be(text);
    }

    [TestMethod]
    public void EqualAttributeValuesUseSystemAttributeEquality()
    {
        var left = new GenerateEndpointAttribute("/orders", "GET") { Tags = ["Orders"] };
        var right = new GenerateEndpointAttribute("/orders", "GET") { Tags = ["Orders"] };

        left.Equals(right).Should().BeTrue();
        left.GetHashCode().Should().Be(right.GetHashCode());
        left.Equals(new GenerateEnumerationAttribute("Orders")).Should().BeFalse();
        right.Name = "Different";
        left.Equals(right).Should().BeFalse();
        left.Equals(null).Should().BeFalse();
    }

    [TestMethod]
    public void DocumentedUsageCompilesAndExposesItsMetadata()
    {
        var method = typeof(EndpointExamples).GetMethod(nameof(EndpointExamples.ReadHealth), BindingFlags.Static | BindingFlags.NonPublic)!;
        var attribute = method.GetCustomAttribute<GenerateEndpointAttribute>()!;

        attribute.Route.Should().Be("/health");
        attribute.HttpMethod.Should().Be("GET");
        attribute.Name.Should().Be("ReadHealth");
        attribute.Summary.Should().Be("Read service health");
        attribute.Tags.Should().Equal("Health");
        attribute.AllowAnonymous.Should().BeTrue();
        EndpointExamples.ReadHealth(CancellationToken.None).Should().Be("healthy");
    }

    internal static class EndpointExamples
    {
        [GenerateEndpoint("/health", "GET", Name = "ReadHealth",
            Summary = "Read service health", Tags = new[] { "Health" }, AllowAnonymous = true)]
        internal static string ReadHealth(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return "healthy";
        }
    }
}
