using System.Collections.ObjectModel;
using VisionaryCoder.Framework.Extensions;

namespace VisionaryCoder.Framework.Tests.Extensions;

[TestClass]
public class EnumerableExtensionsTests
{
    #region ContainsDuplicates Tests

    [TestMethod]
    public void ContainsDuplicates_WithNullCollection_ShouldReturnFalse()
    {
        // Arrange
        IEnumerable<int>? collection = null;

        // Act
        bool result = collection.ContainsDuplicates();

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void ContainsDuplicates_WithEmptyCollection_ShouldReturnFalse()
    {
        // Arrange
        var collection = new List<int>();

        // Act
        bool result = collection.ContainsDuplicates();

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void ContainsDuplicates_WithSingleItem_ShouldReturnFalse()
    {
        // Arrange
        var collection = new List<int> { 1 };

        // Act
        bool result = collection.ContainsDuplicates();

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void ContainsDuplicates_WithNoDuplicates_ShouldReturnFalse()
    {
        // Arrange
        var collection = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        bool result = collection.ContainsDuplicates();

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void ContainsDuplicates_WithDuplicates_ShouldReturnTrue()
    {
        // Arrange
        var collection = new List<int> { 1, 2, 3, 2, 4 };

        // Act
        bool result = collection.ContainsDuplicates();

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void ContainsDuplicates_WithCustomComparer_ShouldUseComparer()
    {
        // Arrange
        var collection = new List<string> { "Hello", "HELLO", "World" };
        StringComparer comparer = StringComparer.OrdinalIgnoreCase;

        // Act
        bool result = collection.ContainsDuplicates(comparer);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void ContainsDuplicates_WithCustomComparerNoDuplicates_ShouldReturnFalse()
    {
        // Arrange
        var collection = new List<string> { "Hello", "World", "Test" };
        StringComparer comparer = StringComparer.OrdinalIgnoreCase;

        // Act
        bool result = collection.ContainsDuplicates(comparer);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsNullOrEmpty Tests

    [TestMethod]
    public void IsNullOrEmpty_WithNullSource_ShouldReturnTrue()
    {
        // Arrange
        IEnumerable<int>? source = null;

        // Act
        bool result = source.IsNullOrEmpty();

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsNullOrEmpty_WithEmptySource_ShouldReturnTrue()
    {
        // Arrange
        var source = new List<int>();

        // Act
        bool result = source.IsNullOrEmpty();

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsNullOrEmpty_WithNonEmptySource_ShouldReturnFalse()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3 };

        // Act
        bool result = source.IsNullOrEmpty();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ForEach Tests

    [TestMethod]
    public void ForEach_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<int>? source = null;
        var action = new Action<int>(x => { });

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => source!.ForEach(action));
        exception.ParamName.Should().Be("source");
    }

    [TestMethod]
    public void ForEach_WithNullAction_ShouldThrowArgumentNullException()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3 };
        Action<int>? action = null;

        // Act & Assert
        ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(() => source.ForEach(action!));
        exception.ParamName.Should().Be("action");
    }

    [TestMethod]
    public void ForEach_WithValidInputs_ShouldExecuteActionOnEachElement()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3 };
        var results = new List<int>();
        var action = new Action<int>(x => results.Add(x * 2));

        // Act
        source.ForEach(action);

        // Assert
        results.Should().ContainInOrder(2, 4, 6);
    }

    [TestMethod]
    public void ForEach_WithIndexedAction_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<int>? source = null;
        var action = new Action<int, int>((x, i) => { });

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => source!.ForEach(action));
        exception.ParamName.Should().Be("source");
    }

    [TestMethod]
    public void ForEach_WithIndexedAction_WithNullAction_ShouldThrowArgumentNullException()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3 };
        Action<int, int>? action = null;

        // Act & Assert
        ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(() => source.ForEach(action!));
        exception.ParamName.Should().Be("action");
    }

    [TestMethod]
    public void ForEach_WithIndexedAction_ShouldExecuteActionWithElementAndIndex()
    {
        // Arrange
        var source = new List<string> { "a", "b", "c" };
        var results = new List<string>();
        var action = new Action<string, int>((x, i) => results.Add($"{x}{i}"));

        // Act
        source.ForEach(action);

        // Assert
        results.Should().ContainInOrder("a0", "b1", "c2");
    }

    #endregion

    #region DistinctBy Tests

    [TestMethod]
    public void DistinctBy_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<int>? source = null;
        var keySelector = new Func<int, int>(x => x);

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => EnumerableExtensions.DistinctBy(source!, keySelector).ToList());
        exception.ParamName.Should().Be("source");
    }

    [TestMethod]
    public void DistinctBy_WithNullKeySelector_ShouldThrowArgumentNullException()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3 };
        Func<int, int>? keySelector = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => EnumerableExtensions.DistinctBy(source, keySelector!).ToList());
        exception.ParamName.Should().Be("keySelector");
    }

    [TestMethod]
    public void DistinctBy_WithDuplicateKeys_ShouldReturnDistinctElements()
    {
        // Arrange
        var source = new List<(int id, string name)>
        {
            (1, "Alice"),
            (2, "Bob"),
            (1, "Charlie"), // Duplicate ID
            (3, "David")
        };

        // Act
        var result = EnumerableExtensions.DistinctBy(source, x => x.id).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().ContainInOrder((1, "Alice"), (2, "Bob"), (3, "David"));
    }

    [TestMethod]
    public void DistinctBy_WithNoDuplicates_ShouldReturnAllElements()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var result = EnumerableExtensions.DistinctBy(source, x => x).ToList();

        // Assert
        result.Should().HaveCount(5);
        result.Should().ContainInOrder(1, 2, 3, 4, 5);
    }

    #endregion

    #region Batch Tests

    [TestMethod]
    public void Batch_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<int>? source = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => source!.Batch(2).ToList());
        exception.ParamName.Should().Be("source");
    }

    [TestMethod]
    public void Batch_WithZeroSize_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3 };

        // Act & Assert
        ArgumentOutOfRangeException? exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => source.Batch(0).ToList());
        exception.ParamName.Should().Be("size");
    }

    [TestMethod]
    public void Batch_WithNegativeSize_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3 };

        // Act & Assert
        ArgumentOutOfRangeException? exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => source.Batch(-1).ToList());
        exception.ParamName.Should().Be("size");
    }

    [TestMethod]
    public void Batch_WithValidSize_ShouldReturnBatches()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5, 6, 7 };

        // Act
        var result = source.Batch(3).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Should().ContainInOrder(1, 2, 3);
        result[1].Should().ContainInOrder(4, 5, 6);
        result[2].Should().ContainInOrder(7);
    }

    [TestMethod]
    public void Batch_WithEmptySource_ShouldReturnEmptySequence()
    {
        // Arrange
        var source = new List<int>();

        // Act
        var result = source.Batch(3).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Shuffle Tests

    [TestMethod]
    public void Shuffle_WithCustomRandom_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<int>? source = null;
        var random = new Random(42);

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => source!.Shuffle(random).ToList());
        exception.ParamName.Should().Be("source");
    }

    [TestMethod]
    public void Shuffle_WithCustomRandom_WithNullRandom_ShouldThrowArgumentNullException()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3 };
        Random? random = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => source.Shuffle(random!).ToList());
        exception.ParamName.Should().Be("random");
    }

    [TestMethod]
    public void Shuffle_WithCustomRandom_ShouldBeReproducible()
    {
        // Arrange
        var source = Enumerable.Range(1, 10).ToList();
        var random1 = new Random(42);
        var random2 = new Random(42);

        // Act
        var result1 = source.Shuffle(random1).ToList();
        var result2 = source.Shuffle(random2).ToList();

        // Assert
        result1.Should().ContainInOrder(result2);
    }

    #endregion

    #region TryFirst Tests

    [TestMethod]
    public void TryFirst_WithNullSource_ShouldReturnFalse()
    {
        // Arrange
        IEnumerable<int>? source = null;

        // Act
        bool result = source.TryFirst(out int value);

        // Assert
        result.Should().BeFalse();
        value.Should().Be(default(int));
    }

    [TestMethod]
    public void TryFirst_WithEmptySource_ShouldReturnFalse()
    {
        // Arrange
        var source = new List<int>();

        // Act
        bool result = source.TryFirst(out int value);

        // Assert
        result.Should().BeFalse();
        value.Should().Be(default(int));
    }

    [TestMethod]
    public void TryFirst_WithNonEmptySource_ShouldReturnTrueAndFirstElement()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3 };

        // Act
        bool result = source.TryFirst(out int value);

        // Assert
        result.Should().BeTrue();
        value.Should().Be(1);
    }

    #endregion

    #region TryLast Tests

    [TestMethod]
    public void TryLast_WithNullSource_ShouldReturnFalse()
    {
        // Arrange
        IEnumerable<int>? source = null;

        // Act
        bool result = source.TryLast(out int value);

        // Assert
        result.Should().BeFalse();
        value.Should().Be(default(int));
    }

    [TestMethod]
    public void TryLast_WithEmptySource_ShouldReturnFalse()
    {
        // Arrange
        var source = new List<int>();

        // Act
        bool result = source.TryLast(out int value);

        // Assert
        result.Should().BeFalse();
        value.Should().Be(default(int));
    }

    [TestMethod]
    public void TryLast_WithList_ShouldReturnTrueAndLastElement()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3 };

        // Act
        bool result = source.TryLast(out int value);

        // Assert
        result.Should().BeTrue();
        value.Should().Be(3);
    }

    [TestMethod]
    public void TryLast_WithArray_ShouldReturnTrueAndLastElement()
    {
        // Arrange
        int[] source = [1, 2, 3];

        // Act
        bool result = source.TryLast(out int value);

        // Assert
        result.Should().BeTrue();
        value.Should().Be(3);
    }

    [TestMethod]
    public void TryLast_WithEnumerable_ShouldReturnTrueAndLastElement()
    {
        // Arrange
        IEnumerable<int> source = Enumerable.Range(1, 3);

        // Act
        bool result = source.TryLast(out int value);

        // Assert
        result.Should().BeTrue();
        value.Should().Be(3);
    }

    #endregion

    #region ToDelimitedString Tests

    [TestMethod]
    public void ToDelimitedString_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<int>? source = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => source!.ToDelimitedString());
        exception.ParamName.Should().Be("source");
    }

    [TestMethod]
    public void ToDelimitedString_WithDefaultSeparator_ShouldUseCommaSpace()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3 };

        // Act
        string result = source.ToDelimitedString();

        // Assert
        result.Should().Be("1, 2, 3");
    }

    [TestMethod]
    public void ToDelimitedString_WithCustomSeparator_ShouldUseCustomSeparator()
    {
        // Arrange
        var source = new List<string> { "a", "b", "c" };

        // Act
        string result = source.ToDelimitedString(" | ");

        // Assert
        result.Should().Be("a | b | c");
    }

    [TestMethod]
    public void ToDelimitedString_WithEmptySource_ShouldReturnEmptyString()
    {
        // Arrange
        var source = new List<int>();

        // Act
        string result = source.ToDelimitedString();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region ToReadOnlyCollection Tests

    [TestMethod]
    public void ToReadOnlyCollection_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<int>? source = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => source!.ToReadOnlyCollection());
        exception.ParamName.Should().Be("source");
    }

    [TestMethod]
    public void ToReadOnlyCollection_WithValidSource_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3 };

        // Act
        var result = source.ToReadOnlyCollection();

        // Assert
        result.Should().BeOfType<ReadOnlyCollection<int>>();
        result.Should().ContainInOrder(1, 2, 3);
        result.Count.Should().Be(3);
    }

    #endregion

    #region WithIndex Tests

    [TestMethod]
    public void WithIndex_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<string>? source = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => source!.WithIndex().ToList());
        exception.ParamName.Should().Be("source");
    }

    [TestMethod]
    public void WithIndex_WithValidSource_ShouldReturnElementsWithIndices()
    {
        // Arrange
        var source = new List<string> { "a", "b", "c" };

        // Act
        var result = source.WithIndex().ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Should().Be(("a", 0));
        result[1].Should().Be(("b", 1));
        result[2].Should().Be(("c", 2));
    }

    #endregion

    #region ToDictionary Tests

    [TestMethod]
    public void ToDictionary_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<KeyValuePair<string, int>>? source = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => EnumerableExtensions.ToDictionary(source!));
        exception.ParamName.Should().Be("source");
    }

    [TestMethod]
    public void ToDictionary_WithValidKeyValuePairs_ShouldReturnDictionary()
    {
        // Arrange
        var source = new List<KeyValuePair<string, int>>
        {
            new("key1", 1),
            new("key2", 2),
            new("key3", 3)
        };

        // Act
        var result = EnumerableExtensions.ToDictionary(source);

        // Assert
        result.Should().BeOfType<Dictionary<string, int>>();
        result.Should().HaveCount(3);
        result["key1"].Should().Be(1);
        result["key2"].Should().Be(2);
        result["key3"].Should().Be(3);
    }

    #endregion

    #region IndexOf Tests

    [TestMethod]
    public void IndexOf_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<int>? source = null;
        var predicate = new Func<int, bool>(x => x > 2);

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => source!.IndexOf(predicate));
        exception.ParamName.Should().Be("source");
    }

    [TestMethod]
    public void IndexOf_WithNullPredicate_ShouldThrowArgumentNullException()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3 };
        Func<int, bool>? predicate = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => source.IndexOf(predicate!));
        exception.ParamName.Should().Be("predicate");
    }

    [TestMethod]
    public void IndexOf_WithMatchingElement_ShouldReturnCorrectIndex()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var predicate = new Func<int, bool>(x => x > 3);

        // Act
        int result = source.IndexOf(predicate);

        // Assert
        result.Should().Be(3); // Index of element 4
    }

    [TestMethod]
    public void IndexOf_WithNoMatchingElement_ShouldReturnMinusOne()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3 };
        var predicate = new Func<int, bool>(x => x > 10);

        // Act
        int result = source.IndexOf(predicate);

        // Assert
        result.Should().Be(-1);
    }

    [TestMethod]
    public void IndexOf_WithEmptySource_ShouldReturnMinusOne()
    {
        // Arrange
        var source = new List<int>();
        var predicate = new Func<int, bool>(x => x > 0);

        // Act
        int result = source.IndexOf(predicate);

        // Assert
        result.Should().Be(-1);
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void EnumerableExtensions_ChainedOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var source = new List<int> { 1, 2, 2, 3, 4, 4, 5 };

        // Act
        var result = EnumerableExtensions.DistinctBy(source, x => x)
            .Batch(2)
            .Select(batch => batch.ToDelimitedString("-"))
            .ToReadOnlyCollection();

        // Assert
        result.Should().HaveCount(3);
        result[0].Should().Be("1-2");
        result[1].Should().Be("3-4");
        result[2].Should().Be("5");
    }

    [TestMethod]
    public void EnumerableExtensions_WithComplexObjects_ShouldWorkCorrectly()
    {
        // Arrange
        var source = new List<(string name, int age)>
        {
            ("Alice", 30),
            ("Bob", 25),
            ("Charlie", 30),
            ("David", 35)
        };

        // Act
        string adultNames = EnumerableExtensions.DistinctBy(source
                .Where(p => p.age >= 30), p => p.age)
            .WithIndex()
            .Select(item => $"{item.index}: {item.item.name}")
            .ToDelimitedString(" | ");

        // Assert
        adultNames.Should().Be("0: Alice | 1: David");
    }

    #endregion
}
