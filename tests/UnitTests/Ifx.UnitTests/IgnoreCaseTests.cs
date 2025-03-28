namespace vc.Ifx.Cli.UnitTests;

public class IgnoreCaseTests
{
    [Fact]
    public void IgnoreCase_ShouldHaveCorrectValues()
    {
        Assert.Equal(0, (int)IgnoreCase.Yes);
        Assert.Equal(1, (int)IgnoreCase.No);
    }
}