using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Moq;
using vc.Ifx.Generators;

namespace VisionaryCoder.Framework.Tests.Generators.Implementation;

[TestClass]
public sealed class EndpointMetadataReaderTests
{
    [TestMethod]
    public void UnresolvedAttributeClassIsNotTreatedAsAuthorizationMetadata()
    {
        // A defensive Roslyn metadata boundary test, not a generated-consumer fixture.
        var symbol = new Mock<ISymbol>();
        symbol.Setup(value => value.GetAttributes()).Returns(ImmutableArray.Create(new Mock<AttributeData>().Object));
        var validation = typeof(MinimalEndpointGenerator).Assembly.GetType("vc.Ifx.Generators.EndpointValidation")!;
        var hasAttribute = validation.GetMethod("HasAttribute", BindingFlags.Static | BindingFlags.NonPublic)!;
        hasAttribute.Invoke(null, new object[] { symbol.Object, "Microsoft.AspNetCore.Authorization.IAuthorizeData" }).Should().Be(false);
    }
}
