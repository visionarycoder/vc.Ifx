using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Moq;
using Ifx.Generators;

namespace Ifx.Tests.Generators.Implementation;

[TestClass]
public sealed class EndpointMetadataReaderTests
{
    [TestMethod]
    public void UnresolvedAttributeClassIsNotTreatedAsAuthorizationMetadata()
    {
        // A defensive Roslyn metadata boundary test, not a generated-consumer fixture.
        var symbol = new Mock<ISymbol>();
        symbol.Setup(value => value.GetAttributes()).Returns([new Mock<AttributeData>().Object]);
        var validation = typeof(MinimalEndpointGenerator).Assembly.GetType("Ifx.Generators.Endpoints.EndpointValidation")!;
        var hasAttribute = validation.GetMethod("HasAttribute", BindingFlags.Static | BindingFlags.NonPublic)!;
        hasAttribute.Invoke(null, [symbol.Object, "Microsoft.AspNetCore.Authorization.IAuthorizeData"]).Should().Be(false);
    }
}
