using VisionaryCoder.Framework.Models;

namespace VisionaryCoder.Framework.Tests.Extensions;

/// <summary>
/// Unit tests for the Month class to ensure 100% code coverage.
/// </summary>
[TestClass]
public class MonthTests
{
    #region Constructor Tests

    [TestMethod]
    public void DefaultConstructor_ShouldCreateUnknownMonth()
    {
        // Arrange & Act
        var month = new Month();

        // Assert
        month.Name.Should().Be(Month.Unknown.Name);
        month.Ordinal.Should().Be(0);
        month.Index.Should().Be(-1);
        month.Abbrv.Should().Be("Unk");
    }

    [TestMethod]
    [DataRow(0, "Unknown")]
    [DataRow(1, "January")]
    [DataRow(2, "February")]
    [DataRow(3, "March")]
    [DataRow(4, "April")]
    [DataRow(5, "May")]
    [DataRow(6, "June")]
    [DataRow(7, "July")]
    [DataRow(8, "August")]
    [DataRow(9, "September")]
    [DataRow(10, "October")]
    [DataRow(11, "November")]
    [DataRow(12, "December")]
    public void ConstructorWithValidOrdinal_ShouldCreateCorrectMonth(int ordinal, string expectedName)
    {
        // Arrange & Act
        var month = new Month(ordinal);

        // Assert
        month.Ordinal.Should().Be(ordinal);
        month.Name.Should().Be(expectedName);
        month.Index.Should().Be(ordinal - 1);
        month.Abbrv.Should().Be(expectedName[..3]);
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(13)]
    [DataRow(100)]
    public void ConstructorWithInvalidOrdinal_ShouldThrowArgumentOutOfRangeException(int invalidOrdinal)
    {
        // Arrange & Act & Assert
        Func<Month> action = () => new Month(invalidOrdinal);
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("ordinal")
            .WithMessage("Ordinal must be between 0 and 13*");
    }

    [TestMethod]
    [DataRow("Unknown", 0)]
    [DataRow("January", 1)]
    [DataRow("February", 2)]
    [DataRow("March", 3)]
    [DataRow("April", 4)]
    [DataRow("May", 5)]
    [DataRow("June", 6)]
    [DataRow("July", 7)]
    [DataRow("August", 8)]
    [DataRow("September", 9)]
    [DataRow("October", 10)]
    [DataRow("November", 11)]
    [DataRow("December", 12)]
    public void ConstructorWithValidLongName_ShouldCreateCorrectMonth(string name, int expectedOrdinal)
    {
        // Arrange & Act
        var month = new Month(name);

        // Assert
        month.Name.Should().Be(name);
        month.Ordinal.Should().Be(expectedOrdinal);
        month.Index.Should().Be(expectedOrdinal - 1);
    }

    [TestMethod]
    [DataRow("Jan", "January", 1)]
    [DataRow("Feb", "February", 2)]
    [DataRow("Mar", "March", 3)]
    [DataRow("Apr", "April", 4)]
    [DataRow("May", "May", 5)] // May is same in short and long form
    [DataRow("Jun", "June", 6)]
    [DataRow("Jul", "July", 7)]
    [DataRow("Aug", "August", 8)]
    [DataRow("Sep", "September", 9)]
    [DataRow("Oct", "October", 10)]
    [DataRow("Nov", "November", 11)]
    [DataRow("Dec", "December", 12)]
    public void ConstructorWithValidShortName_ShouldCreateCorrectMonth(string shortName, string expectedLongName, int expectedOrdinal)
    {
        // Arrange & Act
        var month = new Month(shortName);

        // Assert
        month.Name.Should().Be(expectedLongName);
        month.Ordinal.Should().Be(expectedOrdinal);
        month.Index.Should().Be(expectedOrdinal - 1);
    }

    [TestMethod]
    public void ConstructorWithNullName_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Func<Month> action = () => new Month((string)null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("name");
    }

    [TestMethod]
    [DataRow("InvalidMonth")]
    [DataRow("")]
    [DataRow("13th")]
    [DataRow("NotAMonth")]
    public void ConstructorWithInvalidName_ShouldThrowArgumentOutOfRangeException(string invalidName)
    {
        // Arrange & Act & Assert
        Func<Month> action = () => new Month(invalidName);
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("name")
            .WithMessage($"Name is not a valid month name: {invalidName}*");
    }

    #endregion

    #region Property Tests

    [TestMethod]
    public void Name_ShouldReturnCorrectValue()
    {
        // Arrange
        var month = new Month(Month.January);

        // Act & Assert
        month.Name.Should().Be(Month.January.Name);
    }

    [TestMethod]
    public void Ordinal_ShouldReturnCorrectValue()
    {
        // Arrange
        var month = new Month(5);

        // Act & Assert
        month.Ordinal.Should().Be(5);
    }

    [TestMethod]
    public void Index_ShouldReturnOrdinalMinusOne()
    {
        // Arrange
        var month = new Month(5);

        // Act & Assert
        month.Index.Should().Be(4);
    }

    [TestMethod]
    public void Abbrv_ShouldReturnFirstThreeCharacters()
    {
        // Arrange
        var month = new Month(Month.January);

        // Act & Assert
        month.Abbrv.Should().Be("Jan");
    }

    [TestMethod]
    public void Abbrv_ForUnknown_ShouldReturnFirstThreeCharacters()
    {
        // Arrange
        var month = new Month();

        // Act & Assert
        month.Abbrv.Should().Be("Unk");
    }

    #endregion

    #region Method Tests

    [TestMethod]
    public void ToString_ShouldReturnName()
    {
        // Arrange
        var month = new Month(Month.March);

        // Act
        string result = month.ToString();

        // Assert
        result.Should().Be(Month.March.Name);
    }

    [TestMethod]
    public void ToString_ForUnknown_ShouldReturnUnknown()
    {
        // Arrange
        var month = new Month();

        // Act
        string result = month.ToString();

        // Assert
        result.Should().Be(Month.Unknown.Name);
    }

    #endregion

    #region Constant Tests

    [TestMethod]
    public void Constants_ShouldHaveCorrectValues()
    {
        // Assert - Test all month constants
        Month.Unknown.Name.Should().Be("Unknown");
        Month.January.Name.Should().Be("January");
        Month.February.Name.Should().Be("February");
        Month.March.Name.Should().Be("March");
        Month.April.Name.Should().Be("April");
        Month.May.Name.Should().Be("May");
        Month.June.Name.Should().Be("June");
        Month.July.Name.Should().Be("July");
        Month.August.Name.Should().Be("August");
        Month.September.Name.Should().Be("September");
        Month.October.Name.Should().Be("October");
        Month.November.Name.Should().Be("November");
        Month.December.Name.Should().Be("December");

        // Short month constants
        Month.Jan.Abbrv.Should().Be("Jan");
        Month.Feb.Abbrv.Should().Be("Feb");
        Month.Mar.Abbrv.Should().Be("Mar");
        Month.Apr.Abbrv.Should().Be("Apr");
        Month.Jun.Abbrv.Should().Be("Jun");
        Month.Jul.Abbrv.Should().Be("Jul");
        Month.Aug.Abbrv.Should().Be("Aug");
        Month.Sep.Abbrv.Should().Be("Sep");
        Month.Oct.Abbrv.Should().Be("Oct");
        Month.Nov.Abbrv.Should().Be("Nov");
        Month.Dec.Abbrv.Should().Be("Dec");
    }

    #endregion

    #region Edge Case Tests

    [TestMethod]
    public void Constructor_WithBoundaryValues_ShouldWorkCorrectly()
    {
        // Test first valid value
        var firstMonth = new Month(0);
        firstMonth.Name.Should().Be(Month.Unknown.Name);
        firstMonth.Ordinal.Should().Be(0);

        // Test last valid value
        var lastMonth = new Month(12);
        lastMonth.Name.Should().Be(Month.December.Name);
        lastMonth.Ordinal.Should().Be(12);
    }

    [TestMethod]
    public void Constructor_WithCaseSensitiveNames_ShouldThrowForIncorrectCase()
    {
        // Arrange & Act & Assert
        Func<Month> action = () => new Month("january"); // lowercase
        action.Should().Throw<ArgumentOutOfRangeException>();

        Func<Month> action2 = () => new Month("JANUARY"); // uppercase
        action2.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion
}
