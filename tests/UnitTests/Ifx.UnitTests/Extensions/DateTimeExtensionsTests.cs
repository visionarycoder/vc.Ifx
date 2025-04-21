using vc.Ifx.Extensions;

namespace vc.Ifx.UnitTests.Extensions;

public class DateTimeExtensionsTests
{
    [Fact]
    public void GetProceedingWeekday_ShouldReturnCorrectDate()
    {
        // Arrange
        var inputDate = new DateTime(2023, 10, 1); // Sunday
        var expectedDate = new DateTime(2023, 10, 2); // Monday

        // Act
        var result = inputDate.GetProceedingWeekday(DayOfWeek.Monday);

        // Assert
        Assert.Equal(expectedDate, result);
    }

    [Fact]
    public void GetDateOnly_ShouldReturnCorrectDate_WhenInThePast()
    {
        // Arrange
        var inputDate = new DateTime(2023, 10, 1, 12, 0, 0); // 1st October 2023, 12:00 PM
        var offset = TimeSpan.FromDays(1);
        var expectedDate = new DateTime(2023, 9, 30); // 30th September 2023

        // Act
        var result = inputDate.GetDateOnly(offset, DateTimeExtensions.ShiftDate.IntoThePast);

        // Assert
        Assert.Equal(expectedDate, result);
    }

    [Fact]
    public void GetDateOnly_ShouldReturnCorrectDate_WhenInTheFuture()
    {
        // Arrange
        var inputDate = new DateTime(2023, 10, 1, 12, 0, 0); // 1st October 2023, 12:00 PM
        var offset = TimeSpan.FromDays(1);
        var expectedDate = new DateTime(2023, 10, 2); // 2nd October 2023

        // Act
        var result = inputDate.GetDateOnly(offset, DateTimeExtensions.ShiftDate.AfterToday);

        // Assert
        Assert.Equal(expectedDate, result);
    }
}