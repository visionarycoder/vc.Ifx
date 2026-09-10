using VisionaryCoder.Framework.Extensions;

namespace VisionaryCoder.Framework.Tests.Extensions;

/// <summary>
/// Unit tests for DateTimeExtensions to ensure 100% code coverage.
/// </summary>
[TestClass]
public class DateTimeExtensionsTests
{
    #region GetProceedingWeekday Tests

    [TestMethod]
    public void GetProceedingWeekday_WithDefaultSunday_ShouldReturnNextSunday()
    {
        // Arrange - Start with a Wednesday
        var input = new DateTime(2024, 1, 10, 15, 30, 45); // Wednesday

        // Act
        DateTime result = input.GetProceedingWeekday();

        // Assert
        result.Should().Be(new DateTime(2024, 1, 14)); // Next Sunday
        result.DayOfWeek.Should().Be(DayOfWeek.Sunday);
        result.TimeOfDay.Should().Be(TimeSpan.Zero); // Should be date only
    }

    [TestMethod]
    public void GetProceedingWeekday_WithSpecificDayOfWeek_ShouldReturnNextOccurrence()
    {
        // Arrange - Start with a Wednesday
        var input = new DateTime(2024, 1, 10); // Wednesday

        // Act
        DateTime result = input.GetProceedingWeekday(DayOfWeek.Friday);

        // Assert
        result.Should().Be(new DateTime(2024, 1, 12)); // Next Friday
        result.DayOfWeek.Should().Be(DayOfWeek.Friday);
    }

    [TestMethod]
    public void GetProceedingWeekday_SameDayOfWeek_ShouldReturnNextWeek()
    {
        // Arrange - Start with a Wednesday
        var input = new DateTime(2024, 1, 10); // Wednesday

        // Act
        DateTime result = input.GetProceedingWeekday(DayOfWeek.Wednesday);

        // Assert
        result.Should().Be(new DateTime(2024, 1, 17)); // Next Wednesday (7 days later)
        result.DayOfWeek.Should().Be(DayOfWeek.Wednesday);
    }

    [TestMethod]
    public void GetProceedingWeekday_AllDaysOfWeek_ShouldWorkCorrectly()
    {
        // Arrange - Start with a Monday
        var input = new DateTime(2024, 1, 8); // Monday

        // Act & Assert for each day of the week
        input.GetProceedingWeekday(DayOfWeek.Monday).Should().Be(new DateTime(2024, 1, 15)); // Next Monday
        input.GetProceedingWeekday(DayOfWeek.Tuesday).Should().Be(new DateTime(2024, 1, 9)); // Next Tuesday
        input.GetProceedingWeekday(DayOfWeek.Wednesday).Should().Be(new DateTime(2024, 1, 10)); // Next Wednesday
        input.GetProceedingWeekday(DayOfWeek.Thursday).Should().Be(new DateTime(2024, 1, 11)); // Next Thursday
        input.GetProceedingWeekday(DayOfWeek.Friday).Should().Be(new DateTime(2024, 1, 12)); // Next Friday
        input.GetProceedingWeekday(DayOfWeek.Saturday).Should().Be(new DateTime(2024, 1, 13)); // Next Saturday
        input.GetProceedingWeekday(DayOfWeek.Sunday).Should().Be(new DateTime(2024, 1, 14)); // Next Sunday
    }

    #endregion

    #region GetPreviousWeekday Tests

    [TestMethod]
    public void GetPreviousWeekday_WithSpecificDayOfWeek_ShouldReturnPreviousOccurrence()
    {
        // Arrange - Start with a Wednesday
        var input = new DateTime(2024, 1, 10); // Wednesday

        // Act
        DateTime result = input.GetPreviousWeekday(DayOfWeek.Monday);

        // Assert
        result.Should().Be(new DateTime(2024, 1, 8)); // Previous Monday
        result.DayOfWeek.Should().Be(DayOfWeek.Monday);
        result.TimeOfDay.Should().Be(TimeSpan.Zero); // Should be date only
    }

    [TestMethod]
    public void GetPreviousWeekday_SameDayOfWeek_ShouldReturnPreviousWeek()
    {
        // Arrange - Start with a Wednesday
        var input = new DateTime(2024, 1, 10); // Wednesday

        // Act
        DateTime result = input.GetPreviousWeekday(DayOfWeek.Wednesday);

        // Assert
        result.Should().Be(new DateTime(2024, 1, 3)); // Previous Wednesday (7 days earlier)
        result.DayOfWeek.Should().Be(DayOfWeek.Wednesday);
    }

    [TestMethod]
    public void GetPreviousWeekday_AllDaysOfWeek_ShouldWorkCorrectly()
    {
        // Arrange - Start with a Friday
        var input = new DateTime(2024, 1, 12); // Friday

        // Act & Assert for each day of the week
        input.GetPreviousWeekday(DayOfWeek.Monday).Should().Be(new DateTime(2024, 1, 8)); // Previous Monday
        input.GetPreviousWeekday(DayOfWeek.Tuesday).Should().Be(new DateTime(2024, 1, 9)); // Previous Tuesday
        input.GetPreviousWeekday(DayOfWeek.Wednesday).Should().Be(new DateTime(2024, 1, 10)); // Previous Wednesday
        input.GetPreviousWeekday(DayOfWeek.Thursday).Should().Be(new DateTime(2024, 1, 11)); // Previous Thursday
        input.GetPreviousWeekday(DayOfWeek.Friday).Should().Be(new DateTime(2024, 1, 5)); // Previous Friday
        input.GetPreviousWeekday(DayOfWeek.Saturday).Should().Be(new DateTime(2024, 1, 6)); // Previous Saturday
        input.GetPreviousWeekday(DayOfWeek.Sunday).Should().Be(new DateTime(2024, 1, 7)); // Previous Sunday
    }

    #endregion

    #region GetDateOnly Tests

    [TestMethod]
    public void GetDateOnly_WithToPastDefault_ShouldSubtractOffset()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 10, 15, 30, 45);
        var offset = TimeSpan.FromDays(3);

        // Act
        DateTime result = dateTime.GetDateOnly(offset);

        // Assert
        result.Should().Be(new DateTime(2024, 1, 7)); // 3 days earlier, date only
        result.TimeOfDay.Should().Be(TimeSpan.Zero);
    }

    [TestMethod]
    public void GetDateOnly_WithToFuture_ShouldAddOffset()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 10, 15, 30, 45);
        var offset = TimeSpan.FromDays(5);

        // Act
        DateTime result = dateTime.GetDateOnly(offset, DateTimeExtensions.ShiftDate.ToFuture);

        // Assert
        result.Should().Be(new DateTime(2024, 1, 15)); // 5 days later, date only
        result.TimeOfDay.Should().Be(TimeSpan.Zero);
    }

    [TestMethod]
    public void GetDateOnly_WithHourOffset_ShouldWorkCorrectly()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 10, 15, 30, 45);
        var offset = TimeSpan.FromHours(20);

        // Act
        DateTime resultPast = dateTime.GetDateOnly(offset, DateTimeExtensions.ShiftDate.ToPast);
        DateTime resultFuture = dateTime.GetDateOnly(offset, DateTimeExtensions.ShiftDate.ToFuture);

        // Assert
        resultPast.Should().Be(new DateTime(2024, 1, 9)); // Previous day due to hour offset
        resultFuture.Should().Be(new DateTime(2024, 1, 11)); // Next day due to hour offset
    }

    [TestMethod]
    public void GetDateOnly_WithZeroOffset_ShouldReturnSameDate()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 10, 15, 30, 45);
        TimeSpan offset = TimeSpan.Zero;

        // Act
        DateTime result = dateTime.GetDateOnly(offset);

        // Assert
        result.Should().Be(new DateTime(2024, 1, 10)); // Same date, time stripped
        result.TimeOfDay.Should().Be(TimeSpan.Zero);
    }

    #endregion

    #region IsWeekend Tests

    [TestMethod]
    public void IsWeekend_WithSaturday_ShouldReturnTrue()
    {
        // Arrange
        var saturday = new DateTime(2024, 1, 13); // Saturday

        // Act
        bool result = saturday.IsWeekend();

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsWeekend_WithSunday_ShouldReturnTrue()
    {
        // Arrange
        var sunday = new DateTime(2024, 1, 14); // Sunday

        // Act
        bool result = sunday.IsWeekend();

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsWeekend_WithWeekdays_ShouldReturnFalse()
    {
        // Arrange & Act & Assert
        new DateTime(2024, 1, 8).IsWeekend().Should().BeFalse(); // Monday
        new DateTime(2024, 1, 9).IsWeekend().Should().BeFalse(); // Tuesday
        new DateTime(2024, 1, 10).IsWeekend().Should().BeFalse(); // Wednesday
        new DateTime(2024, 1, 11).IsWeekend().Should().BeFalse(); // Thursday
        new DateTime(2024, 1, 12).IsWeekend().Should().BeFalse(); // Friday
    }

    #endregion

    #region IsWeekday Tests

    [TestMethod]
    public void IsWeekday_WithWeekdays_ShouldReturnTrue()
    {
        // Arrange & Act & Assert
        new DateTime(2024, 1, 8).IsWeekday().Should().BeTrue(); // Monday
        new DateTime(2024, 1, 9).IsWeekday().Should().BeTrue(); // Tuesday
        new DateTime(2024, 1, 10).IsWeekday().Should().BeTrue(); // Wednesday
        new DateTime(2024, 1, 11).IsWeekday().Should().BeTrue(); // Thursday
        new DateTime(2024, 1, 12).IsWeekday().Should().BeTrue(); // Friday
    }

    [TestMethod]
    public void IsWeekday_WithWeekendDays_ShouldReturnFalse()
    {
        // Arrange
        var saturday = new DateTime(2024, 1, 13); // Saturday
        var sunday = new DateTime(2024, 1, 14); // Sunday

        // Act & Assert
        saturday.IsWeekday().Should().BeFalse();
        sunday.IsWeekday().Should().BeFalse();
    }

    #endregion

    #region GetStartOfWeek Tests

    [TestMethod]
    public void GetStartOfWeek_WithDefaultMondayStart_ShouldReturnMonday()
    {
        // Arrange - Test with different days of the week
        var wednesday = new DateTime(2024, 1, 10, 15, 30, 45); // Wednesday

        // Act
        DateTime result = wednesday.GetStartOfWeek();

        // Assert
        result.Should().Be(new DateTime(2024, 1, 8)); // Monday of that week
        result.DayOfWeek.Should().Be(DayOfWeek.Monday);
        result.TimeOfDay.Should().Be(TimeSpan.Zero);
    }

    [TestMethod]
    public void GetStartOfWeek_WithSundayStart_ShouldReturnSunday()
    {
        // Arrange
        var wednesday = new DateTime(2024, 1, 10); // Wednesday

        // Act
        DateTime result = wednesday.GetStartOfWeek(DayOfWeek.Sunday);

        // Assert
        result.Should().Be(new DateTime(2024, 1, 7)); // Sunday of that week
        result.DayOfWeek.Should().Be(DayOfWeek.Sunday);
    }

    [TestMethod]
    public void GetStartOfWeek_WithSameDayAsStart_ShouldReturnSameDate()
    {
        // Arrange
        var monday = new DateTime(2024, 1, 8, 10, 15, 30); // Monday

        // Act
        DateTime result = monday.GetStartOfWeek(DayOfWeek.Monday);

        // Assert
        result.Should().Be(new DateTime(2024, 1, 8)); // Same Monday, time stripped
        result.TimeOfDay.Should().Be(TimeSpan.Zero);
    }

    [TestMethod]
    public void GetStartOfWeek_AllStartDays_ShouldWorkCorrectly()
    {
        // Arrange - Use Wednesday as test date
        var wednesday = new DateTime(2024, 1, 10); // Wednesday

        // Act & Assert for each possible start day
        wednesday.GetStartOfWeek(DayOfWeek.Sunday).Should().Be(new DateTime(2024, 1, 7)); // Previous Sunday
        wednesday.GetStartOfWeek(DayOfWeek.Monday).Should().Be(new DateTime(2024, 1, 8)); // Previous Monday
        wednesday.GetStartOfWeek(DayOfWeek.Tuesday).Should().Be(new DateTime(2024, 1, 9)); // Previous Tuesday
        wednesday.GetStartOfWeek(DayOfWeek.Wednesday).Should().Be(new DateTime(2024, 1, 10)); // Same Wednesday
        wednesday.GetStartOfWeek(DayOfWeek.Thursday).Should().Be(new DateTime(2024, 1, 4)); // Previous Thursday
        wednesday.GetStartOfWeek(DayOfWeek.Friday).Should().Be(new DateTime(2024, 1, 5)); // Previous Friday
        wednesday.GetStartOfWeek(DayOfWeek.Saturday).Should().Be(new DateTime(2024, 1, 6)); // Previous Saturday
    }

    #endregion

    #region ShiftDate Enum Tests

    [TestMethod]
    public void ShiftDate_EnumValues_ShouldHaveCorrectValues()
    {
        // Assert
        ((int)DateTimeExtensions.ShiftDate.ToFuture).Should().Be(0);
        ((int)DateTimeExtensions.ShiftDate.ToPast).Should().Be(1);
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void DateTimeExtensions_ChainedOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 10, 14, 30, 45); // Wednesday with time

        // Act - Chain multiple operations
        DateTime result = startDate
            .GetStartOfWeek(DayOfWeek.Monday)
            .GetProceedingWeekday(DayOfWeek.Friday);

        // Assert
        result.Should().Be(new DateTime(2024, 1, 12)); // Friday of the same week
        result.DayOfWeek.Should().Be(DayOfWeek.Friday);
    }

    [TestMethod]
    public void DateTimeExtensions_EdgeCaseMonthBoundary_ShouldWorkCorrectly()
    {
        // Arrange - Last day of January
        var lastDay = new DateTime(2024, 1, 31); // Wednesday

        // Act
        DateTime nextSunday = lastDay.GetProceedingWeekday(DayOfWeek.Sunday);
        DateTime previousMonday = lastDay.GetPreviousWeekday(DayOfWeek.Monday);

        // Assert
        nextSunday.Should().Be(new DateTime(2024, 2, 4)); // First Sunday of February
        previousMonday.Should().Be(new DateTime(2024, 1, 29)); // Last Monday of January
    }

    [TestMethod]
    public void DateTimeExtensions_LeapYearBoundary_ShouldWorkCorrectly()
    {
        // Arrange - February 28, 2024 (leap year)
        var feb28 = new DateTime(2024, 2, 28); // Wednesday

        // Act
        DateTime nextSunday = feb28.GetProceedingWeekday(DayOfWeek.Sunday);
        DateTime dateWithOffset = feb28.GetDateOnly(TimeSpan.FromDays(2), DateTimeExtensions.ShiftDate.ToFuture);

        // Assert
        nextSunday.Should().Be(new DateTime(2024, 3, 3)); // First Sunday of March
        dateWithOffset.Should().Be(new DateTime(2024, 3, 1)); // March 1st (leap day handled)
    }

    #endregion
}
