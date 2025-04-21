#pragma warning disable ClassMethodMissingInterface

namespace vc.Ifx.UnitTests.Base;

public class DtoBaseTests
{
    private class TestDto : DtoBase
    {
        public int TestProperty { get; init; }
    }

    [Fact]
    public void TestDto_ShouldHaveDefaultValues()
    {
        // Arrange
        var dto = new TestDto();

        // Act & Assert
        Assert.NotEqual(Guid.Empty, dto.InstanceId);
    }

    [Fact]
    public void TestDto_ShouldSetProperties()
    {
        // Arrange
        var instanceId = Guid.NewGuid();
        var testProperty = 456;

        // Act
        var dto = new TestDto
        {
            InstanceId = instanceId,
            TestProperty = testProperty
        };

        // Assert
        Assert.Equal(instanceId, dto.InstanceId);
        Assert.Equal(testProperty, dto.TestProperty);
    }
}