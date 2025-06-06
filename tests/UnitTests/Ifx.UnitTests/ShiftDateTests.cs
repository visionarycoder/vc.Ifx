using vc.Ifx.Extensions;

namespace vc.Ifx.UnitTests;

public class ShiftDateTests
{
    [Fact]
    public void DateIs_ShouldHaveCorrectValues()
    {
        // Assert
        Assert.Equal(0, (int)DateTimeExtensions.ShiftDate.ToFuture);
        Assert.Equal(1, (int)DateTimeExtensions.ShiftDate.ToPast);
    }
}