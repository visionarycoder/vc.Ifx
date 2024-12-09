namespace Ifx.UnitTests.Options;

public class IgnoreCaseOptionTests
{
    [Fact]
    public void IgnoreCase_ShouldHaveCorrectValues()
    {
        Assert.Equal(0, (int)IgnoreCase.Yes);
        Assert.Equal(1, (int)IgnoreCase.No);
    }
}