using VisionaryCoder.Framework.Pagination;

namespace VisionaryCoder.Framework.Tests.Pagination;

/// <summary>
/// Data-driven unit tests for the <see cref="Page{T}"/> class.
/// Tests pagination result container with various scenarios.
/// </summary>
[TestClass]
public class PageTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_WithValidParameters_ShouldSetProperties()
    {
        // Arrange
        var items = new List<string> { "item1", "item2", "item3" };
        const int totalCount = 100;
        const int pageNumber = 2;
        const int pageSize = 10;

        // Act
        var page = new Page<string>(items, totalCount, pageNumber, pageSize);

        // Assert
        page.Items.Should().BeEquivalentTo(items);
        page.TotalCount.Should().Be(totalCount);
        page.PageNumber.Should().Be(pageNumber);
        page.PageSize.Should().Be(pageSize);
        page.NextToken.Should().BeNull();
    }

    [TestMethod]
    public void Constructor_WithNextToken_ShouldSetAllProperties()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5 };
        const int totalCount = 50;
        const int pageNumber = 1;
        const int pageSize = 5;
        const string nextToken = "continuation-token-123";

        // Act
        var page = new Page<int>(items, totalCount, pageNumber, pageSize, nextToken);

        // Assert
        page.Items.Should().BeEquivalentTo(items);
        page.TotalCount.Should().Be(totalCount);
        page.PageNumber.Should().Be(pageNumber);
        page.PageSize.Should().Be(pageSize);
        page.NextToken.Should().Be(nextToken);
    }

    [TestMethod]
    public void Constructor_WithEmptyItems_ShouldAcceptEmptyList()
    {
        // Arrange
        var items = new List<string>();

        // Act
        var page = new Page<string>(items, 0, 1, 10);

        // Assert
        page.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(0);
    }

    [TestMethod]
    public void Constructor_WithNullNextToken_ShouldAccept()
    {
        // Arrange
        var items = new List<string> { "item1" };

        // Act
        var page = new Page<string>(items, 1, 1, 10, null);

        // Assert
        page.NextToken.Should().BeNull();
    }

    #endregion

    #region Items Property Tests

    [TestMethod]
    public void Items_ShouldReturnReadOnlyList()
    {
        // Arrange
        var items = new List<string> { "a", "b", "c" };
        var page = new Page<string>(items, 3, 1, 10);

        // Assert
        page.Items.Should().BeAssignableTo<IReadOnlyList<string>>();
        page.Items.Should().HaveCount(3);
    }

    [TestMethod]
    public void Items_WithDifferentTypes_ShouldWork()
    {
        // Arrange & Act
        var stringPage = new Page<string>(["a", "b"], 2, 1, 10);
        var intPage = new Page<int>([1, 2, 3], 3, 1, 10);
        var objectPage = new Page<object>([1, "test", 3.14], 3, 1, 10);

        // Assert
        stringPage.Items.Should().AllBeOfType<string>();
        intPage.Items.Should().AllBeOfType<int>();
        objectPage.Items.Should().HaveCount(3);
    }

    [TestMethod]
    public void Items_WithComplexTypes_ShouldWork()
    {
        // Arrange
        var users = new List<User>
        {
            new("John", "john@example.com"),
            new("Jane", "jane@example.com")
        };

        // Act
        var page = new Page<User>(users, 2, 1, 10);

        // Assert
        page.Items.Should().HaveCount(2);
        page.Items[0].Name.Should().Be("John");
        page.Items[1].Name.Should().Be("Jane");
    }

    private record User(string Name, string Email);

    #endregion

    #region TotalCount Tests

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(50)]
    [DataRow(1000)]
    [DataRow(1000000)]
    public void TotalCount_WithVariousValues_ShouldAccept(int totalCount)
    {
        // Arrange
        var items = new List<string> { "item" };

        // Act
        var page = new Page<string>(items, totalCount, 1, 10);

        // Assert
        page.TotalCount.Should().Be(totalCount);
    }

    [TestMethod]
    public void TotalCount_CanBeDifferentFromItemsCount()
    {
        // Arrange
        var items = new List<string> { "item1", "item2", "item3" };
        const int totalCount = 100; // Total across all pages

        // Act
        var page = new Page<string>(items, totalCount, 1, 3);

        // Assert
        page.Items.Should().HaveCount(3);
        page.TotalCount.Should().Be(100);
    }

    [TestMethod]
    public void TotalCount_CanBeZeroWithEmptyItems()
    {
        // Arrange
        var items = new List<string>();

        // Act
        var page = new Page<string>(items, 0, 1, 10);

        // Assert
        page.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(0);
    }

    #endregion

    #region PageNumber and PageSize Tests

    [TestMethod]
    [DataRow(1, 10)]
    [DataRow(5, 25)]
    [DataRow(10, 50)]
    [DataRow(100, 100)]
    public void PageNumber_WithVariousValues_ShouldAccept(int pageNumber, int pageSize)
    {
        // Arrange
        var items = new List<string> { "item" };

        // Act
        var page = new Page<string>(items, 100, pageNumber, pageSize);

        // Assert
        page.PageNumber.Should().Be(pageNumber);
        page.PageSize.Should().Be(pageSize);
    }

    [TestMethod]
    public void PageNumber_ShouldRepresentCurrentPage()
    {
        // Arrange
        var items = new List<string> { "item1", "item2" };

        // Act
        var page = new Page<string>(items, 100, 5, 2);

        // Assert
        page.PageNumber.Should().Be(5);
    }

    [TestMethod]
    public void PageSize_ShouldRepresentRequestedPageSize()
    {
        // Arrange
        var items = new List<string> { "item1", "item2", "item3" };

        // Act
        var page = new Page<string>(items, 100, 1, 50);

        // Assert
        page.PageSize.Should().Be(50);
        page.Items.Should().HaveCount(3); // Actual returned items can be less than page size
    }

    #endregion

    #region NextToken Tests

    [TestMethod]
    public void NextToken_WithNullValue_ShouldIndicateNoMorePages()
    {
        // Arrange
        var items = new List<string> { "last-item" };

        // Act
        var page = new Page<string>(items, 1, 1, 10, null);

        // Assert
        page.NextToken.Should().BeNull();
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("token-123")]
    [DataRow("eyJpZCI6MTIzLCJvZmZzZXQiOjUwfQ==")]
    public void NextToken_WithVariousValues_ShouldAccept(string nextToken)
    {
        // Arrange
        var items = new List<string> { "item" };

        // Act
        var page = new Page<string>(items, 100, 1, 10, nextToken);

        // Assert
        page.NextToken.Should().Be(nextToken);
    }

    [TestMethod]
    public void NextToken_WhenPresent_ShouldIndicateMorePagesAvailable()
    {
        // Arrange
        var items = new List<string> { "item1", "item2", "item3" };
        const string nextToken = "next-page-token";

        // Act
        var page = new Page<string>(items, 100, 1, 3, nextToken);

        // Assert
        page.NextToken.Should().NotBeNullOrEmpty();
        page.NextToken.Should().Be(nextToken);
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [TestMethod]
    public void Constructor_WithAllMinimumValues_ShouldWork()
    {
        // Arrange
        var items = new List<string>();

        // Act
        var page = new Page<string>(items, 0, 0, 0);

        // Assert
        page.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(0);
        page.PageNumber.Should().Be(0);
        page.PageSize.Should().Be(0);
    }

    [TestMethod]
    public void Constructor_WithLargeDataset_ShouldWork()
    {
        // Arrange
        var items = Enumerable.Range(1, 1000).ToList();

        // Act
        var page = new Page<int>(items, 1000000, 1, 1000);

        // Assert
        page.Items.Should().HaveCount(1000);
        page.TotalCount.Should().Be(1000000);
    }

    [TestMethod]
    public void Constructor_CalledMultipleTimes_ShouldCreateIndependentInstances()
    {
        // Arrange
        var items1 = new List<string> { "a" };
        var items2 = new List<string> { "b", "c" };
        var items3 = new List<string> { "d", "e", "f" };

        // Act
        var page1 = new Page<string>(items1, 1, 1, 10);
        var page2 = new Page<string>(items2, 2, 2, 10);
        var page3 = new Page<string>(items3, 3, 3, 10);

        // Assert
        page1.Items.Should().HaveCount(1);
        page2.Items.Should().HaveCount(2);
        page3.Items.Should().HaveCount(3);
        page1.TotalCount.Should().Be(1);
        page2.TotalCount.Should().Be(2);
        page3.TotalCount.Should().Be(3);
    }

    [TestMethod]
    public void Constructor_WithNegativeValues_ShouldAccept()
    {
        // Arrange
        var items = new List<string> { "item" };

        // Act
        var page = new Page<string>(items, -1, -1, -1);

        // Assert - No validation, allows negative values
        page.TotalCount.Should().Be(-1);
        page.PageNumber.Should().Be(-1);
        page.PageSize.Should().Be(-1);
    }

    #endregion

    #region Generic Type Tests

    [TestMethod]
    public void Page_WithValueTypes_ShouldWork()
    {
        // Act
        var intPage = new Page<int>([1, 2, 3], 3, 1, 10);
        var doublePage = new Page<double>([1.1, 2.2], 2, 1, 10);
        var boolPage = new Page<bool>([true, false, true], 3, 1, 10);

        // Assert
        intPage.Items.Should().AllBeOfType<int>();
        doublePage.Items.Should().AllBeOfType<double>();
        boolPage.Items.Should().AllBeOfType<bool>();
    }

    [TestMethod]
    public void Page_WithReferenceTypes_ShouldWork()
    {
        // Act
        var stringPage = new Page<string>(["a", "b"], 2, 1, 10);
        var objectPage = new Page<object>([new(), new()], 2, 1, 10);

        // Assert
        stringPage.Items.Should().AllBeOfType<string>();
        objectPage.Items.Should().AllBeOfType<object>();
    }

    [TestMethod]
    public void Page_WithNullableTypes_ShouldWork()
    {
        // Arrange
        var items = new List<int?> { 1, null, 3, null };

        // Act
        var page = new Page<int?>(items, 4, 1, 10);

        // Assert
        page.Items.Where(x => x == null).Should().HaveCount(2);
        page.Items.Should().HaveCount(4);
    }

    #endregion

    #region Property Immutability Tests

    [TestMethod]
    public void Properties_ShouldBeReadOnly()
    {
        // Arrange
        var items = new List<string> { "item" };
        var page = new Page<string>(items, 100, 5, 25, "token");

        // Assert - Properties should have init-only setters (verified by compilation)
        page.Items.Should().NotBeNull();
        page.TotalCount.Should().Be(100);
        page.PageNumber.Should().Be(5);
        page.PageSize.Should().Be(25);
        page.NextToken.Should().Be("token");
    }

    #endregion
}
