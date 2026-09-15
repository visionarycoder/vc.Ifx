using System.Reflection;
using Ifx.Generators.Abstractions.Attributes;

namespace Ifx.Tests.Generators.Abstractions;

[TestClass]
public sealed class MapperAttributeTests
{
    [TestMethod]
    public void GenerateMapperAttributePreservesCtorValueAndMutableFlag()
    {
        var attribute = new GenerateMapperAttribute(typeof(RemoteOrder))
        {
            Bidirectional = true
        };

        attribute.TargetType.Should().Be(typeof(RemoteOrder));
        attribute.Bidirectional.Should().BeTrue();

        attribute.Bidirectional = false;
        attribute.Bidirectional.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("OrderId")]
    public void MapFromAttributePreservesMemberNameVerbatim(string? memberName)
    {
        var attribute = new MapFromAttribute(memberName!);

        attribute.MemberName.Should().Be(memberName);
    }

    [TestMethod]
    public void DocumentedMapperAttributesExposeMetadataWithoutNormalization()
    {
        var source = typeof(LocalOrder).GetCustomAttributes<GenerateMapperAttribute>().Single();
        source.TargetType.Should().Be(typeof(RemoteOrder));
        source.Bidirectional.Should().BeTrue();

        var property = typeof(RemoteOrder).GetProperty(nameof(RemoteOrder.OrderId))!;
        var mapFrom = property.GetCustomAttribute<MapFromAttribute>()!;
        mapFrom.MemberName.Should().Be(nameof(LocalOrder.Id));
    }

    [GenerateMapper(typeof(RemoteOrder), Bidirectional = true)]
    private sealed class LocalOrder
    {
        public int Id { get; set; }
    }

    private sealed class RemoteOrder
    {
        [MapFrom(nameof(LocalOrder.Id))]
        public int OrderId { get; set; }
    }
}
