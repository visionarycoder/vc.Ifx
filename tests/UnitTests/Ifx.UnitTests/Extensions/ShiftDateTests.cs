using vc.Ifx.Extensions;

namespace vc.Ifx.Cli.UnitTests.Extensions;

public class ShiftDateTests
{
    [Fact]
    public void DateIs_ShouldHaveCorrectValues()
    {
        // Assert
        Assert.Equal(0, (int)DateTimeExtensions.ShiftDate.IntoTheFuture);
        Assert.Equal(1, (int)DateTimeExtensions.ShiftDate.IntoThePast);
    }
}