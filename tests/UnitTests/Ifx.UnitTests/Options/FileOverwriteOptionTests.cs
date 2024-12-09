namespace Ifx.UnitTests.Options;

public class FileOverwriteOptionTests
{
    [Fact]
    public void FileOverwriteOption_ShouldHaveCorrectValues()
    {
        Assert.Equal(0, (int)FileOverwriteOption.Overwrite);
        Assert.Equal(1, (int)FileOverwriteOption.DoNotOverwrite);
    }
}