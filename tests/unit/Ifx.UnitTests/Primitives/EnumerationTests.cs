namespace Ifx.Tests.Primitives;

[TestClass]
public sealed class EnumerationTests
{
    [TestMethod]
    public void Constructor_ShouldExposeIdNameAndToString()
    {
        var value = new SampleEnumeration(7, "Seven");

        value.Id.Should().Be(7);
        value.Name.Should().Be("Seven");
        value.ToString().Should().Be("Seven");
    }

    private sealed class SampleEnumeration(int id, string name) : Enumeration(id, name);
}
