using vc.Ifx.Helpers;

namespace Ifx.UnitTests.Helpers;

public class ReflectionHelperTests
{
    [Fact]
    public void NameOfCallingClass_ShouldReturnCorrectClassName()
    {
        // Act
        var className = ReflectionHelper.NameOfCallingClass();

        // Assert
        Assert.Equal(nameof(ReflectionHelperTests), className);
    }

    [Fact]
    public void TypeOfCallingClass_ShouldReturnCorrectType()
    {
        // Act
        var type = ReflectionHelper.TypeOfCallingClass();

        // Assert
        Assert.Equal(typeof(ReflectionHelperTests), type);
    }
}