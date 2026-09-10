using VisionaryCoder.Framework.Primitives;

namespace VisionaryCoder.Framework.Tests.Primitives;

[TestClass]
public class MonthTests
{
    #region Construction

    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(2026, 9)]
    [DataRow(9999, 12)]
    public void Constructor_WithValidValues_ShouldCreateMonth(int year, int number)
    {
        var month = new Month(year, number);

        month.Year.Should().Be(year);
        month.Number.Should().Be(number);
        month.Value.Should().Be((year * 100) + number);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(10000)]
    public void Constructor_WithInvalidYear_ShouldThrowArgumentOutOfRangeException(int year)
    {
        Action act = () => _ = new Month(year, 1);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Year value must be between 1 and 9999*")
            .WithParameterName("year");
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(13)]
    public void Constructor_WithInvalidMonthNumber_ShouldThrowArgumentOutOfRangeException(int number)
    {
        Action act = () => _ = new Month(2026, number);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Month value must be between 1 and 12*")
            .WithParameterName("number");
    }

    [TestMethod]
    public void Create_WithValidValues_ShouldCreateMonth()
    {
        var month = Month.Create(2026, 9);

        month.Year.Should().Be(2026);
        month.Number.Should().Be(9);
    }

    [TestMethod]
    public void FromDate_WithDate_ShouldCreateContainingMonth()
    {
        var date = new DateOnly(2026, 9, 9);

        var month = Month.FromDate(date);

        month.Year.Should().Be(2026);
        month.Number.Should().Be(9);
    }

    #endregion

    #region Calendar bounds

    [TestMethod]
    public void FirstDay_ShouldReturnFirstCalendarDay()
    {
        var month = new Month(2026, 9);

        month.FirstDay.Should().Be(new DateOnly(2026, 9, 1));
    }

    [TestMethod]
    public void LastDay_WithStandardMonth_ShouldReturnLastCalendarDay()
    {
        var month = new Month(2026, 9);

        month.LastDay.Should().Be(new DateOnly(2026, 9, 30));
    }

    [TestMethod]
    public void LastDay_WithLeapYearFebruary_ShouldReturnLeapDay()
    {
        var month = new Month(2024, 2);

        month.LastDay.Should().Be(new DateOnly(2024, 2, 29));
    }

    #endregion

    #region Parsing

    [TestMethod]
    public void Parse_WithValidText_ShouldReturnMonth()
    {
        var month = Month.Parse("2026-09");

        month.Year.Should().Be(2026);
        month.Number.Should().Be(9);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("2026")]
    [DataRow("2026-09-01")]
    [DataRow("abcd-09")]
    [DataRow("2026-ab")]
    [DataRow("0000-01")]
    [DataRow("10000-01")]
    [DataRow("2026-00")]
    [DataRow("2026-13")]
    public void Parse_WithInvalidText_ShouldThrowFormatException(string? text)
    {
        Action act = () => _ = Month.Parse(text);

        act.Should().Throw<FormatException>()
            .WithMessage("*Invalid month value*");
    }

    [TestMethod]
    public void TryParse_WithValidText_ShouldReturnTrue()
    {
        bool result = Month.TryParse(" 2026-09 ", out Month? month);

        result.Should().BeTrue();
        month.Should().NotBeNull();
        month!.Year.Should().Be(2026);
        month.Number.Should().Be(9);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("2026")]
    [DataRow("2026-09-01")]
    [DataRow("abcd-09")]
    [DataRow("2026-ab")]
    [DataRow("0000-01")]
    [DataRow("10000-01")]
    [DataRow("2026-00")]
    [DataRow("2026-13")]
    public void TryParse_WithInvalidText_ShouldReturnFalse(string? text)
    {
        bool result = Month.TryParse(text, out Month? month);

        result.Should().BeFalse();
        month.Should().BeNull();
    }

    #endregion

    #region Conversion and metadata

    [TestMethod]
    public void ToString_ShouldReturnInvariantMonthText()
    {
        var month = new Month(2026, 9);

        month.ToString().Should().Be("2026-09");
    }

    [TestMethod]
    public void IPrimitiveValue_ShouldExposeValueMetadata()
    {
        IPrimitiveValue primitive = new Month(2026, 9);

        primitive.ValueType.Should().Be(typeof(int));
        primitive.BoxedValue.Should().Be(202609);
    }

    [TestMethod]
    public void IPrimitiveValueOfT_ShouldExposeTypedValue()
    {
        IPrimitiveValue<int> primitive = new Month(2026, 9);

        primitive.Value.Should().Be(202609);
    }

    #endregion

    #region Equality

    [TestMethod]
    public void Equals_WithSameYearAndNumber_ShouldReturnTrue()
    {
        var left = new Month(2026, 9);
        var right = new Month(2026, 9);

        left.Equals(right).Should().BeTrue();
        left.Equals((object)right).Should().BeTrue();
        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [TestMethod]
    public void Equals_WithDifferentYearOrNumber_ShouldReturnFalse()
    {
        var month = new Month(2026, 9);
        var differentYear = new Month(2025, 9);
        var differentNumber = new Month(2026, 10);

        month.Equals(differentYear).Should().BeFalse();
        month.Equals(differentNumber).Should().BeFalse();
        month.Equals((Month?)null).Should().BeFalse();
        object.Equals(month, "2026-09").Should().BeFalse();
        (month == differentYear).Should().BeFalse();
        (month != differentYear).Should().BeTrue();
    }

    [TestMethod]
    public void EqualityOperators_WithNullValues_ShouldReturnExpectedResults()
    {
        Month? left = null;
        Month? right = null;
        var month = new Month(2026, 9);

        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
        (left == month).Should().BeFalse();
        (left != month).Should().BeTrue();
    }

    #endregion
}
