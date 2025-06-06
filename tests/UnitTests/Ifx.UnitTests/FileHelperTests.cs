using vc.Ifx.Helpers;

namespace vc.Ifx.UnitTests;

public class FileHelperTests
{
    [Fact]
    public void Load_ShouldReturnFileContent_WhenFileExists()
    {
        // Arrange
        var filePath = "testfile.txt";
        var expectedContent = "Hello, World!";
        File.WriteAllText(filePath, expectedContent);

        // Act
        var content = FileHelper.Load(filePath);

        // Assert
        Assert.Equal(expectedContent, content);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void Load_ShouldReturnEmptyString_WhenFileDoesNotExist()
    {
        // Arrange
        var filePath = "nonexistentfile.txt";

        // Act
        var content = FileHelper.Load(filePath);

        // Assert
        Assert.Equal(string.Empty, content);
    }

    [Fact]
    public async Task LoadAsync_ShouldReturnFileContent_WhenFileExists()
    {
        // Arrange
        var filePath = "testfile.txt";
        var expectedContent = "Hello, World!";
        await File.WriteAllTextAsync(filePath, expectedContent);

        // Act
        var content = await FileHelper.LoadAsync(filePath);

        // Assert
        Assert.Equal(expectedContent, content);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public async Task LoadAsync_ShouldReturnEmptyString_WhenFileDoesNotExist()
    {
        // Arrange
        var filePath = "nonexistentfile.txt";

        // Act
        var content = await FileHelper.LoadAsync(filePath);

        // Assert
        Assert.Equal(string.Empty, content);
    }
}