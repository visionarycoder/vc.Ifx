namespace Ifx.Tests.Primitives;

[TestClass]
public sealed class ConnectionStringTests
{
    [TestMethod]
    public void Constructor_WithValue_ShouldStoreValue()
    {
        var connectionString = new ConnectionString("Server=(local);Database=Ifx;");

        connectionString.Value.Should().Be("Server=(local);Database=Ifx;");
        connectionString.ToString().Should().Be("Server=(local);Database=Ifx;");
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("\t")]
    public void Constructor_WithNullOrWhitespace_ShouldThrowArgumentException(string? value)
    {
        Action act = () => _ = new ConnectionString(value!);

        act.Should().Throw<ArgumentException>().WithParameterName("connectionString");
    }

    [TestMethod]
    public void EqualityMembers_ShouldUseOrdinalComparison()
    {
        var first = new ConnectionString("Server=(local);Database=Ifx;");
        var second = new ConnectionString("Server=(local);Database=Ifx;");
        var different = new ConnectionString("Server=(local);Database=Other;");

        ConnectionString? nullConnectionString = null;

        first.Equals(second).Should().BeTrue();
        first.Equals((object)second).Should().BeTrue();
        first.Equals(nullConnectionString).Should().BeFalse();
        first.Equals((object)"Server=(local);Database=Ifx;").Should().BeFalse();
        first.GetHashCode().Should().Be(second.GetHashCode());
        first.Should().Be(second);
        (first == second).Should().BeTrue();
        (first != second).Should().BeFalse();
        (first == different).Should().BeFalse();
        (first != different).Should().BeTrue();
    }

    [TestMethod]
    public void EqualityOperators_ShouldHandleNullValues()
    {
        ConnectionString? left = null;
        ConnectionString? right = null;
        var value = new ConnectionString("Server=(local);Database=Ifx;");

        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
        (left == value).Should().BeFalse();
        (left != value).Should().BeTrue();
        (value == left).Should().BeFalse();
        (value != left).Should().BeTrue();
    }

    [TestMethod]
    public void ConversionOperators_ShouldRoundTripValue()
    {
        ConnectionString connectionString = (ConnectionString)"Server=(local);Database=Ifx;";
        string text = connectionString;

        connectionString.Value.Should().Be("Server=(local);Database=Ifx;");
        text.Should().Be("Server=(local);Database=Ifx;");
    }
}
