namespace Ifx.UnitTests.Options
{
    public class DateIsOptionTests
    {
        [Fact]
        public void DateIs_ShouldHaveCorrectValues()
        {
            // Assert
            Assert.Equal(0, (int)DateIs.InThePast);
            Assert.Equal(1, (int)DateIs.InTheFuture);
        }
    }
}

