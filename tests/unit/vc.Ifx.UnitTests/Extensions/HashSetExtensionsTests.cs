using VisionaryCoder.Framework.Extensions;

namespace VisionaryCoder.Framework.Tests.Extensions;

[TestClass]
public class HashSetExtensionsTests
{
    #region AddRange Tests

    [TestMethod]
    public void AddRange_WithNullTarget_ShouldThrowArgumentNullException()
    {
        // Arrange
        HashSet<int>? target = null;
        var collection = new List<int> { 1, 2, 3 };

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => target!.AddRange(collection));
        exception.ParamName.Should().Be("target");
    }

    [TestMethod]
    public void AddRange_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Arrange
        var target = new HashSet<int>();
        ICollection<int>? collection = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => target.AddRange(collection!));
        exception.ParamName.Should().Be("collection");
    }

    [TestMethod]
    public void AddRange_WithValidInputs_ShouldAddAllElements()
    {
        // Arrange
        var target = new HashSet<int> { 1, 2 };
        var collection = new List<int> { 3, 4, 5 };

        // Act
        target.AddRange(collection);

        // Assert
        target.Should().HaveCount(5);
        target.Should().Contain([1, 2, 3, 4, 5]);
    }

    [TestMethod]
    public void AddRange_WithDuplicates_ShouldNotAddDuplicates()
    {
        // Arrange
        var target = new HashSet<int> { 1, 2, 3 };
        var collection = new List<int> { 2, 3, 4 };

        // Act
        target.AddRange(collection);

        // Assert
        target.Should().HaveCount(4);
        target.Should().Contain([1, 2, 3, 4]);
    }

    [TestMethod]
    public void AddRange_WithEmptyCollection_ShouldNotChangeTarget()
    {
        // Arrange
        var target = new HashSet<int> { 1, 2 };
        var collection = new List<int>();

        // Act
        target.AddRange(collection);

        // Assert
        target.Should().HaveCount(2);
        target.Should().Contain([1, 2]);
    }

    [TestMethod]
    public void AddRange_WithEmptyTarget_ShouldAddAllElements()
    {
        // Arrange
        var target = new HashSet<int>();
        var collection = new List<int> { 1, 2, 3 };

        // Act
        target.AddRange(collection);

        // Assert
        target.Should().HaveCount(3);
        target.Should().Contain([1, 2, 3]);
    }

    #endregion

    #region RemoveRange Tests

    [TestMethod]
    public void RemoveRange_WithNullTarget_ShouldThrowArgumentNullException()
    {
        // Arrange
        HashSet<int>? target = null;
        var collection = new List<int> { 1, 2, 3 };

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => target!.RemoveRange(collection));
        exception.ParamName.Should().Be("target");
    }

    [TestMethod]
    public void RemoveRange_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Arrange
        var target = new HashSet<int> { 1, 2, 3 };
        ICollection<int>? collection = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => target.RemoveRange(collection!));
        exception.ParamName.Should().Be("collection");
    }

    [TestMethod]
    public void RemoveRange_WithValidInputs_ShouldRemoveMatchingElements()
    {
        // Arrange
        var target = new HashSet<int> { 1, 2, 3, 4, 5 };
        var collection = new List<int> { 2, 4, 6 }; // 6 is not in target

        // Act
        target.RemoveRange(collection);

        // Assert
        target.Should().HaveCount(3);
        target.Should().Contain([1, 3, 5]);
        target.Should().NotContain([2, 4]);
    }

    [TestMethod]
    public void RemoveRange_WithNonExistentElements_ShouldNotChangeTarget()
    {
        // Arrange
        var target = new HashSet<int> { 1, 2, 3 };
        var collection = new List<int> { 4, 5, 6 };

        // Act
        target.RemoveRange(collection);

        // Assert
        target.Should().HaveCount(3);
        target.Should().Contain([1, 2, 3]);
    }

    [TestMethod]
    public void RemoveRange_WithEmptyCollection_ShouldNotChangeTarget()
    {
        // Arrange
        var target = new HashSet<int> { 1, 2, 3 };
        var collection = new List<int>();

        // Act
        target.RemoveRange(collection);

        // Assert
        target.Should().HaveCount(3);
        target.Should().Contain([1, 2, 3]);
    }

    [TestMethod]
    public void RemoveRange_WithAllElements_ShouldEmptyTarget()
    {
        // Arrange
        var target = new HashSet<int> { 1, 2, 3 };
        var collection = new List<int> { 1, 2, 3 };

        // Act
        target.RemoveRange(collection);

        // Assert
        target.Should().BeEmpty();
    }

    #endregion

    #region ContainsAll Tests

    [TestMethod]
    public void ContainsAll_WithNullTarget_ShouldThrowArgumentNullException()
    {
        // Arrange
        HashSet<int>? target = null;
        var collection = new List<int> { 1, 2, 3 };

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => target!.ContainsAll(collection));
        exception.ParamName.Should().Be("target");
    }

    [TestMethod]
    public void ContainsAll_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Arrange
        var target = new HashSet<int> { 1, 2, 3 };
        ICollection<int>? collection = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => target.ContainsAll(collection!));
        exception.ParamName.Should().Be("collection");
    }

    [TestMethod]
    public void ContainsAll_WithAllElementsPresent_ShouldReturnTrue()
    {
        // Arrange
        var target = new HashSet<int> { 1, 2, 3, 4, 5 };
        var collection = new List<int> { 2, 4, 5 };

        // Act
        bool result = target.ContainsAll(collection);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void ContainsAll_WithSomeElementsMissing_ShouldReturnFalse()
    {
        // Arrange
        var target = new HashSet<int> { 1, 2, 3 };
        var collection = new List<int> { 2, 4, 5 }; // 4 and 5 are missing

        // Act
        bool result = target.ContainsAll(collection);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void ContainsAll_WithEmptyCollection_ShouldReturnTrue()
    {
        // Arrange
        var target = new HashSet<int> { 1, 2, 3 };
        var collection = new List<int>();

        // Act
        bool result = target.ContainsAll(collection);

        // Assert
        result.Should().BeTrue(); // Empty collection is considered a subset
    }

    [TestMethod]
    public void ContainsAll_WithEmptyTarget_ShouldReturnFalseForNonEmptyCollection()
    {
        // Arrange
        var target = new HashSet<int>();
        var collection = new List<int> { 1, 2 };

        // Act
        bool result = target.ContainsAll(collection);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void ContainsAll_WithEmptyTargetAndEmptyCollection_ShouldReturnTrue()
    {
        // Arrange
        var target = new HashSet<int>();
        var collection = new List<int>();

        // Act
        bool result = target.ContainsAll(collection);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void ContainsAll_WithIdenticalSets_ShouldReturnTrue()
    {
        // Arrange
        var target = new HashSet<int> { 1, 2, 3 };
        var collection = new List<int> { 1, 2, 3 };

        // Act
        bool result = target.ContainsAll(collection);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region ContainsAny Tests

    [TestMethod]
    public void ContainsAny_WithNullTarget_ShouldThrowArgumentNullException()
    {
        // Arrange
        HashSet<int>? target = null;
        var collection = new List<int> { 1, 2, 3 };

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => target!.ContainsAny(collection));
        exception.ParamName.Should().Be("target");
    }

    [TestMethod]
    public void ContainsAny_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Arrange
        var target = new HashSet<int> { 1, 2, 3 };
        ICollection<int>? collection = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => target.ContainsAny(collection!));
        exception.ParamName.Should().Be("collection");
    }

    [TestMethod]
    public void ContainsAny_WithSomeElementsPresent_ShouldReturnTrue()
    {
        // Arrange
        var target = new HashSet<int> { 1, 2, 3 };
        var collection = new List<int> { 3, 4, 5 }; // Only 3 is present

        // Act
        bool result = target.ContainsAny(collection);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void ContainsAny_WithNoElementsPresent_ShouldReturnFalse()
    {
        // Arrange
        var target = new HashSet<int> { 1, 2, 3 };
        var collection = new List<int> { 4, 5, 6 };

        // Act
        bool result = target.ContainsAny(collection);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void ContainsAny_WithEmptyCollection_ShouldReturnFalse()
    {
        // Arrange
        var target = new HashSet<int> { 1, 2, 3 };
        var collection = new List<int>();

        // Act
        bool result = target.ContainsAny(collection);

        // Assert
        result.Should().BeFalse(); // No elements to check
    }

    [TestMethod]
    public void ContainsAny_WithEmptyTarget_ShouldReturnFalse()
    {
        // Arrange
        var target = new HashSet<int>();
        var collection = new List<int> { 1, 2, 3 };

        // Act
        bool result = target.ContainsAny(collection);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void ContainsAny_WithEmptyTargetAndEmptyCollection_ShouldReturnFalse()
    {
        // Arrange
        var target = new HashSet<int>();
        var collection = new List<int>();

        // Act
        bool result = target.ContainsAny(collection);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void ContainsAny_WithAllElementsPresent_ShouldReturnTrue()
    {
        // Arrange
        var target = new HashSet<int> { 1, 2, 3, 4, 5 };
        var collection = new List<int> { 2, 4 };

        // Act
        bool result = target.ContainsAny(collection);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void HashSetExtensions_CombinedOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var target = new HashSet<int> { 1, 2, 3 };
        var toAdd = new List<int> { 4, 5, 6 };
        var toRemove = new List<int> { 2, 7 }; // 7 doesn't exist
        var toCheck = new List<int> { 1, 4 };

        // Act
        target.AddRange(toAdd);
        target.RemoveRange(toRemove);
        bool containsAll = target.ContainsAll(toCheck);
        bool containsAny = target.ContainsAny(new List<int> { 7, 8, 1 });

        // Assert
        target.Should().Contain([1, 3, 4, 5, 6]);
        target.Should().NotContain(2);
        target.Should().HaveCount(5);
        containsAll.Should().BeTrue(); // Contains both 1 and 4
        containsAny.Should().BeTrue(); // Contains 1 (but not 7 or 8)
    }

    [TestMethod]
    public void HashSetExtensions_WithStrings_ShouldWorkCorrectly()
    {
        // Arrange
        var target = new HashSet<string> { "apple", "banana" };
        var newFruits = new List<string> { "cherry", "date", "apple" }; // apple is duplicate

        // Act
        target.AddRange(newFruits);
        bool hasCommonFruits = target.ContainsAny(new List<string> { "grape", "cherry" });
        bool hasAllCitrus = target.ContainsAll(new List<string> { "lemon", "lime" });

        // Assert
        target.Should().HaveCount(4); // No duplicate apple
        target.Should().Contain(["apple", "banana", "cherry", "date"]);
        hasCommonFruits.Should().BeTrue(); // Contains cherry
        hasAllCitrus.Should().BeFalse(); // Missing lemon and lime
    }

    [TestMethod]
    public void HashSetExtensions_WithComplexObjects_ShouldWorkCorrectly()
    {
        // Arrange
        var person1 = new { Name = "Alice", Age = 30 };
        var person2 = new { Name = "Bob", Age = 25 };
        var person3 = new { Name = "Charlie", Age = 35 };

        var target = new HashSet<object> { person1, person2 };
        var newPeople = new List<object> { person3 };
        var searchPeople = new List<object> { person1, person3 };

        // Act
        target.AddRange(newPeople);
        bool containsAllSearched = target.ContainsAll(searchPeople);

        // Assert
        target.Should().HaveCount(3);
        target.Should().Contain(person1);
        target.Should().Contain(person2);
        target.Should().Contain(person3);
        containsAllSearched.Should().BeTrue();
    }

    #endregion
}
