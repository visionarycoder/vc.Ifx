namespace vc.Ifx.Cli.UnitTests;

public class OverwriteFileTests
{
    [Fact]
    public void OverwriteFile_ShouldHaveCorrectValues()
    {
        Assert.Equal(0, (int)OverwriteFile.No);
        Assert.Equal(1, (int)OverwriteFile.Yes);
    }
}