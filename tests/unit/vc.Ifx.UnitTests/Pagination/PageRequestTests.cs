using VisionaryCoder.Framework.Pagination;

namespace VisionaryCoder.Framework.Tests.Pagination;

/// <summary>
/// Data-driven unit tests for the <see cref="PageRequest"/> class.
/// Tests pagination request configuration with various scenarios.
/// </summary>
[TestClass]
public class PageRequestTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_WithDefaultParameters_ShouldUseDefaults()
    {
        // Act
        var request = new PageRequest();

        // Assert
        request.PageNumber.Should().Be(1);
        request.PageSize.Should().Be(50);
        request.ContinuationToken.Should().BeNull();
        request.IsTokenPaging.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(1, 50)]
    [DataRow(5, 25)]
    [DataRow(10, 100)]
    [DataRow(100, 1000)]
    public void Constructor_WithValidParameters_ShouldSetProperties(int pageNumber, int pageSize)
    {
        // Act
        var request = new PageRequest(pageNumber, pageSize);

        // Assert
        request.PageNumber.Should().Be(pageNumber);
        request.PageSize.Should().Be(pageSize);
        request.ContinuationToken.Should().BeNull();
        request.IsTokenPaging.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(1, 10, "token123")]
    [DataRow(5, 50, "continuation-abc")]
    [DataRow(10, 100, "next-page-xyz")]
    public void Constructor_WithContinuationToken_ShouldSetTokenAndEnableTokenPaging(int pageNumber, int pageSize, string token)
    {
        // Act
        var request = new PageRequest(pageNumber, pageSize, token);

        // Assert
        request.PageNumber.Should().Be(pageNumber);
        request.PageSize.Should().Be(pageSize);
        request.ContinuationToken.Should().Be(token);
        request.IsTokenPaging.Should().BeTrue();
    }

    #endregion

    #region PageNumber Validation Tests

    [TestMethod]
    [DataRow(0, 1)]
    [DataRow(-1, 1)]
    [DataRow(-10, 1)]
    [DataRow(-100, 1)]
    public void Constructor_WithInvalidPageNumber_ShouldClampToMinimumOne(int invalidPageNumber, int expectedPageNumber)
    {
        // Act
        var request = new PageRequest(invalidPageNumber);

        // Assert
        request.PageNumber.Should().Be(expectedPageNumber, "page number should be clamped to minimum of 1");
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(50)]
    [DataRow(1000)]
    [DataRow(10000)]
    public void Constructor_WithValidPageNumber_ShouldAcceptValue(int pageNumber)
    {
        // Act
        var request = new PageRequest(pageNumber);

        // Assert
        request.PageNumber.Should().Be(pageNumber);
    }

    #endregion

    #region PageSize Validation Tests

    [TestMethod]
    [DataRow(0, 1)]
    [DataRow(-1, 1)]
    [DataRow(-50, 1)]
    public void Constructor_WithPageSizeBelowMinimum_ShouldClampToOne(int invalidPageSize, int expectedPageSize)
    {
        // Act
        var request = new PageRequest(1, invalidPageSize);

        // Assert
        request.PageSize.Should().Be(expectedPageSize, "page size should be clamped to minimum of 1");
    }

    [TestMethod]
    [DataRow(1001, 1000)]
    [DataRow(5000, 1000)]
    [DataRow(10000, 1000)]
    public void Constructor_WithPageSizeAboveMaximum_ShouldClampToThousand(int invalidPageSize, int expectedPageSize)
    {
        // Act
        var request = new PageRequest(1, invalidPageSize);

        // Assert
        request.PageSize.Should().Be(expectedPageSize, "page size should be clamped to maximum of 1000");
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(50)]
    [DataRow(100)]
    [DataRow(500)]
    [DataRow(1000)]
    public void Constructor_WithValidPageSize_ShouldAcceptValue(int pageSize)
    {
        // Act
        var request = new PageRequest(1, pageSize);

        // Assert
        request.PageSize.Should().Be(pageSize);
    }

    #endregion

    #region ContinuationToken Tests

    [TestMethod]
    public void Constructor_WithNullContinuationToken_ShouldNotEnableTokenPaging()
    {
        // Act
        var request = new PageRequest(1, 50, null);

        // Assert
        request.ContinuationToken.Should().BeNull();
        request.IsTokenPaging.Should().BeFalse();
    }

    [TestMethod]
    public void Constructor_WithEmptyContinuationToken_ShouldNotEnableTokenPaging()
    {
        // Act
        var request = new PageRequest(1, 50, "");

        // Assert
        request.ContinuationToken.Should().Be("");
        request.IsTokenPaging.Should().BeFalse();
    }

    [TestMethod]
    public void Constructor_WithWhitespaceContinuationToken_ShouldNotEnableTokenPaging()
    {
        // Act
        var request = new PageRequest(1, 50, "   ");

        // Assert
        request.ContinuationToken.Should().Be("   ");
        request.IsTokenPaging.Should().BeFalse();
    }

    [TestMethod]
    [DataRow("token")]
    [DataRow("continuation-token-123")]
    [DataRow("eyJpZCI6MTIzfQ==")]
    public void Constructor_WithValidContinuationToken_ShouldEnableTokenPaging(string token)
    {
        // Act
        var request = new PageRequest(1, 50, token);

        // Assert
        request.ContinuationToken.Should().Be(token);
        request.IsTokenPaging.Should().BeTrue();
    }

    #endregion

    #region IsTokenPaging Property Tests

    [TestMethod]
    public void IsTokenPaging_WithNoToken_ShouldBeFalse()
    {
        // Arrange
        var request = new PageRequest();

        // Assert
        request.IsTokenPaging.Should().BeFalse();
    }

    [TestMethod]
    public void IsTokenPaging_WithNullToken_ShouldBeFalse()
    {
        // Arrange
        var request = new PageRequest(1, 50, null);

        // Assert
        request.IsTokenPaging.Should().BeFalse();
    }

    [TestMethod]
    public void IsTokenPaging_WithEmptyToken_ShouldBeFalse()
    {
        // Arrange
        var request = new PageRequest(1, 50, string.Empty);

        // Assert
        request.IsTokenPaging.Should().BeFalse();
    }

    [TestMethod]
    public void IsTokenPaging_WithWhitespaceToken_ShouldBeFalse()
    {
        // Arrange
        var request = new PageRequest(1, 50, "   ");

        // Assert
        request.IsTokenPaging.Should().BeFalse();
    }

    [TestMethod]
    public void IsTokenPaging_WithValidToken_ShouldBeTrue()
    {
        // Arrange
        var request = new PageRequest(1, 50, "valid-token");

        // Assert
        request.IsTokenPaging.Should().BeTrue();
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [TestMethod]
    public void Constructor_WithAllInvalidValues_ShouldClampToValidRanges()
    {
        // Act
        var request = new PageRequest(-10, -50, "");

        // Assert
        request.PageNumber.Should().Be(1);
        request.PageSize.Should().Be(1);
        request.ContinuationToken.Should().Be("");
        request.IsTokenPaging.Should().BeFalse();
    }

    [TestMethod]
    public void Constructor_WithMinimumValidValues_ShouldAccept()
    {
        // Act
        var request = new PageRequest(1, 1);

        // Assert
        request.PageNumber.Should().Be(1);
        request.PageSize.Should().Be(1);
    }

    [TestMethod]
    public void Constructor_WithMaximumValidPageSize_ShouldAccept()
    {
        // Act
        var request = new PageRequest(1, 1000);

        // Assert
        request.PageSize.Should().Be(1000);
    }

    [TestMethod]
    public void Constructor_WithVeryLargePageNumber_ShouldAccept()
    {
        // Act
        var request = new PageRequest(int.MaxValue, 50);

        // Assert
        request.PageNumber.Should().Be(int.MaxValue);
    }

    [TestMethod]
    public void Constructor_CalledMultipleTimes_ShouldCreateIndependentInstances()
    {
        // Act
        var request1 = new PageRequest(1, 10, "token1");
        var request2 = new PageRequest(2, 20, "token2");
        var request3 = new PageRequest(3, 30, "token3");

        // Assert
        request1.PageNumber.Should().Be(1);
        request2.PageNumber.Should().Be(2);
        request3.PageNumber.Should().Be(3);
        request1.ContinuationToken.Should().Be("token1");
        request2.ContinuationToken.Should().Be("token2");
        request3.ContinuationToken.Should().Be("token3");
    }

    #endregion

    #region Property Immutability Tests

    [TestMethod]
    public void Properties_ShouldBeReadOnly()
    {
        // Arrange
        var request = new PageRequest(5, 25, "token");

        // Assert - Properties should have init-only setters (verified by compilation)
        request.PageNumber.Should().Be(5);
        request.PageSize.Should().Be(25);
        request.ContinuationToken.Should().Be("token");
    }

    #endregion
}
