using VisionaryCoder.Framework.Proxy.Interceptors.Auditing;

namespace VisionaryCoder.Framework.Tests.Proxy.Interceptors.Security;

[TestClass]
public class AuditingOptionsTests
{
    [TestMethod]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var options = new AuditingOptions();

        // Assert
        options.IncludeHeaders.Should().BeTrue();
        options.IncludeErrorDetails.Should().BeTrue();
        options.IncludeResponseData.Should().BeFalse();
    }

    [TestMethod]
    public void IncludeHeaders_ShouldBeSettable()
    {
        // Arrange
        var options = new AuditingOptions();

        // Act
        options.IncludeHeaders = false;

        // Assert
        options.IncludeHeaders.Should().BeFalse();
    }

    [TestMethod]
    public void IncludeErrorDetails_ShouldBeSettable()
    {
        // Arrange
        var options = new AuditingOptions();

        // Act
        options.IncludeErrorDetails = false;

        // Assert
        options.IncludeErrorDetails.Should().BeFalse();
    }

    [TestMethod]
    public void IncludeResponseData_ShouldBeSettable()
    {
        // Arrange
        var options = new AuditingOptions();

        // Act
        options.IncludeResponseData = true;

        // Assert
        options.IncludeResponseData.Should().BeTrue();
    }

    [TestMethod]
    public void AllProperties_ShouldBeIndependentlySettable()
    {
        // Arrange & Act
        var options = new AuditingOptions
        {
            IncludeHeaders = false,
            IncludeErrorDetails = false,
            IncludeResponseData = true
        };

        // Assert
        options.IncludeHeaders.Should().BeFalse();
        options.IncludeErrorDetails.Should().BeFalse();
        options.IncludeResponseData.Should().BeTrue();
    }
}
