using System.Collections.Immutable;
using System.Collections.ObjectModel;
using VisionaryCoder.Framework.Extensions;

namespace VisionaryCoder.Framework.Tests.Extensions;

[TestClass]
public class DictionaryExtensionsTests
{
    #region GetValueOrDefault Tests

    [TestMethod]
    public void GetValueOrDefault_WithExistingKey_ShouldReturnValue()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["key1"] = 10, ["key2"] = 20 };

        // Act
        int result = DictionaryExtensions.GetValueOrDefault(dictionary, "key1");

        // Assert
        result.Should().Be(10);
    }

    [TestMethod]
    public void GetValueOrDefault_WithNonExistingKey_ShouldReturnDefault()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["key1"] = 10 };

        // Act
        int result = DictionaryExtensions.GetValueOrDefault(dictionary, "nonexistent");

        // Assert
        result.Should().Be(0); // default for int
    }

    [TestMethod]
    public void GetValueOrDefault_WithCustomDefault_ShouldReturnCustomDefault()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["key1"] = 10 };

        // Act
        int result = DictionaryExtensions.GetValueOrDefault(dictionary, "nonexistent", 42);

        // Assert
        result.Should().Be(42);
    }

    #endregion

    #region GetOrAdd Tests

    [TestMethod]
    public void GetOrAdd_WithNullDictionary_ShouldThrowArgumentNullException()
    {
        // Arrange
        IDictionary<string, int>? dictionary = null;
        var valueFactory = new Func<string, int>(k => 1);

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => dictionary!.GetOrAdd("key", valueFactory));
        exception.ParamName.Should().Be("dictionary");
    }

    [TestMethod]
    public void GetOrAdd_WithNullValueFactory_ShouldThrowArgumentNullException()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>();
        Func<string, int>? valueFactory = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => dictionary.GetOrAdd("key", valueFactory!));
        exception.ParamName.Should().Be("valueFactory");
    }

    [TestMethod]
    public void GetOrAdd_WithExistingKey_ShouldReturnExistingValue()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["key1"] = 10 };
        var valueFactory = new Func<string, int>(k => 99);

        // Act
        int result = dictionary.GetOrAdd("key1", valueFactory);

        // Assert
        result.Should().Be(10);
        dictionary["key1"].Should().Be(10); // Should not have changed
    }

    [TestMethod]
    public void GetOrAdd_WithNewKey_ShouldAddAndReturnNewValue()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>();
        var valueFactory = new Func<string, int>(k => k.Length);

        // Act
        int result = dictionary.GetOrAdd("test", valueFactory);

        // Assert
        result.Should().Be(4); // "test".Length
        dictionary.Should().ContainKey("test");
        dictionary["test"].Should().Be(4);
    }

    #endregion

    #region AddOrUpdate Tests

    [TestMethod]
    public void AddOrUpdate_WithValueFactory_WithNullDictionary_ShouldThrowArgumentNullException()
    {
        // Arrange
        IDictionary<string, int>? dictionary = null;
        var addValueFactory = new Func<string, int>(k => 1);
        var updateValueFactory = new Func<string, int, int>((k, v) => v + 1);

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() =>
            dictionary!.AddOrUpdate("key", addValueFactory, updateValueFactory));
        exception.ParamName.Should().Be("dictionary");
    }

    [TestMethod]
    public void AddOrUpdate_WithValueFactory_WithNullAddValueFactory_ShouldThrowArgumentNullException()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>();
        Func<string, int>? addValueFactory = null;
        var updateValueFactory = new Func<string, int, int>((k, v) => v + 1);

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() =>
            dictionary.AddOrUpdate("key", addValueFactory!, updateValueFactory));
        exception.ParamName.Should().Be("addValueFactory");
    }

    [TestMethod]
    public void AddOrUpdate_WithValueFactory_WithNullUpdateValueFactory_ShouldThrowArgumentNullException()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>();
        var addValueFactory = new Func<string, int>(k => 1);
        Func<string, int, int>? updateValueFactory = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() =>
            dictionary.AddOrUpdate("key", addValueFactory, updateValueFactory!));
        exception.ParamName.Should().Be("updateValueFactory");
    }

    [TestMethod]
    public void AddOrUpdate_WithValueFactory_WithNewKey_ShouldAddValue()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>();
        var addValueFactory = new Func<string, int>(k => k.Length);
        var updateValueFactory = new Func<string, int, int>((k, v) => v + 1);

        // Act
        int result = dictionary.AddOrUpdate("test", addValueFactory, updateValueFactory);

        // Assert
        result.Should().Be(4); // "test".Length
        dictionary["test"].Should().Be(4);
    }

    [TestMethod]
    public void AddOrUpdate_WithValueFactory_WithExistingKey_ShouldUpdateValue()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["test"] = 10 };
        var addValueFactory = new Func<string, int>(k => k.Length);
        var updateValueFactory = new Func<string, int, int>((k, v) => v * 2);

        // Act
        int result = dictionary.AddOrUpdate("test", addValueFactory, updateValueFactory);

        // Assert
        result.Should().Be(20); // 10 * 2
        dictionary["test"].Should().Be(20);
    }

    [TestMethod]
    public void AddOrUpdate_WithAddValue_WithNewKey_ShouldAddValue()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>();
        var updateValueFactory = new Func<string, int, int>((k, v) => v + 1);

        // Act
        int result = dictionary.AddOrUpdate("test", 5, updateValueFactory);

        // Assert
        result.Should().Be(5);
        dictionary["test"].Should().Be(5);
    }

    [TestMethod]
    public void AddOrUpdate_WithAddValue_WithExistingKey_ShouldUpdateValue()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["test"] = 10 };
        var updateValueFactory = new Func<string, int, int>((k, v) => v + 5);

        // Act
        int result = dictionary.AddOrUpdate("test", 99, updateValueFactory);

        // Assert
        result.Should().Be(15); // 10 + 5
        dictionary["test"].Should().Be(15);
    }

    #endregion

    #region ToImmutableDictionary Tests

    [TestMethod]
    public void ToImmutableDictionary_WithNullDictionary_ShouldThrowArgumentNullException()
    {
        // Arrange
        IDictionary<string, int>? dictionary = null;

        // Act & Assert
        ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(() => dictionary!.ToImmutableDictionary());
        exception.ParamName.Should().Be("dictionary");
    }

    [TestMethod]
    public void ToImmutableDictionary_WithValidDictionary_ShouldReturnImmutableDictionary()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["key1"] = 1, ["key2"] = 2 };

        // Act
        IImmutableDictionary<string, int> result = dictionary.ToImmutableDictionary();

        // Assert
        result.Should().BeOfType<ImmutableDictionary<string, int>>();
        result.Should().HaveCount(2);
        result["key1"].Should().Be(1);
        result["key2"].Should().Be(2);
    }

    #endregion

    #region ToReadOnlyDictionary Tests

    [TestMethod]
    public void ToReadOnlyDictionary_WithNullDictionary_ShouldThrowArgumentNullException()
    {
        // Arrange
        IDictionary<string, int>? dictionary = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => dictionary!.ToReadOnlyDictionary());
        exception.ParamName.Should().Be("dictionary");
    }

    [TestMethod]
    public void ToReadOnlyDictionary_WithValidDictionary_ShouldReturnReadOnlyDictionary()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["key1"] = 1, ["key2"] = 2 };

        // Act
        var result = dictionary.ToReadOnlyDictionary();

        // Assert
        result.Should().BeOfType<ReadOnlyDictionary<string, int>>();
        result.Should().HaveCount(2);
        result["key1"].Should().Be(1);
        result["key2"].Should().Be(2);
    }

    #endregion

    #region Merge Tests

    [TestMethod]
    public void Merge_WithNullFirst_ShouldThrowArgumentNullException()
    {
        // Arrange
        IDictionary<string, int>? first = null;
        var second = new Dictionary<string, int>();

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => first!.Merge(second));
        exception.ParamName.Should().Be("first");
    }

    [TestMethod]
    public void Merge_WithNullSecond_ShouldThrowArgumentNullException()
    {
        // Arrange
        var first = new Dictionary<string, int>();
        IDictionary<string, int>? second = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => first.Merge(second!));
        exception.ParamName.Should().Be("second");
    }

    [TestMethod]
    public void Merge_WithNoDuplicateKeys_ShouldCombineDictionaries()
    {
        // Arrange
        var first = new Dictionary<string, int> { ["key1"] = 1, ["key2"] = 2 };
        var second = new Dictionary<string, int> { ["key3"] = 3, ["key4"] = 4 };

        // Act
        Dictionary<string, int> result = first.Merge(second);

        // Assert
        result.Should().HaveCount(4);
        result["key1"].Should().Be(1);
        result["key2"].Should().Be(2);
        result["key3"].Should().Be(3);
        result["key4"].Should().Be(4);
    }

    [TestMethod]
    public void Merge_WithDuplicateKeysNoResolver_ShouldUseSecondValue()
    {
        // Arrange
        var first = new Dictionary<string, int> { ["key1"] = 1, ["key2"] = 2 };
        var second = new Dictionary<string, int> { ["key2"] = 20, ["key3"] = 3 };

        // Act
        Dictionary<string, int> result = first.Merge(second);

        // Assert
        result.Should().HaveCount(3);
        result["key1"].Should().Be(1);
        result["key2"].Should().Be(20); // Second value wins
        result["key3"].Should().Be(3);
    }

    [TestMethod]
    public void Merge_WithDuplicateKeysWithResolver_ShouldUseResolver()
    {
        // Arrange
        var first = new Dictionary<string, int> { ["key1"] = 1, ["key2"] = 2 };
        var second = new Dictionary<string, int> { ["key2"] = 20, ["key3"] = 3 };
        var resolver = new Func<string, int, int, int>((k, v1, v2) => v1 + v2);

        // Act
        Dictionary<string, int> result = first.Merge(second, resolver);

        // Assert
        result.Should().HaveCount(3);
        result["key1"].Should().Be(1);
        result["key2"].Should().Be(22); // 2 + 20
        result["key3"].Should().Be(3);
    }

    #endregion

    #region TransformValues Tests

    [TestMethod]
    public void TransformValues_WithNullDictionary_ShouldThrowArgumentNullException()
    {
        // Arrange
        IDictionary<string, int>? dictionary = null;
        var valueSelector = new Func<int, string>(v => v.ToString());

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => dictionary!.TransformValues(valueSelector));
        exception.ParamName.Should().Be("dictionary");
    }

    [TestMethod]
    public void TransformValues_WithNullValueSelector_ShouldThrowArgumentNullException()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["key1"] = 1 };
        Func<int, string>? valueSelector = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => dictionary.TransformValues(valueSelector!));
        exception.ParamName.Should().Be("valueSelector");
    }

    [TestMethod]
    public void TransformValues_WithValidInputs_ShouldTransformValues()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["one"] = 1, ["two"] = 2, ["three"] = 3 };
        var valueSelector = new Func<int, string>(v => $"Value: {v}");

        // Act
        Dictionary<string, string> result = dictionary.TransformValues(valueSelector);

        // Assert
        result.Should().HaveCount(3);
        result["one"].Should().Be("Value: 1");
        result["two"].Should().Be("Value: 2");
        result["three"].Should().Be("Value: 3");
    }

    #endregion

    #region Where Tests

    [TestMethod]
    public void Where_WithNullDictionary_ShouldThrowArgumentNullException()
    {
        // Arrange
        IDictionary<string, int>? dictionary = null;
        var predicate = new Func<string, int, bool>((k, v) => true);

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => dictionary!.Where(predicate));
        exception.ParamName.Should().Be("dictionary");
    }

    [TestMethod]
    public void Where_WithNullPredicate_ShouldThrowArgumentNullException()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["key1"] = 1 };
        Func<string, int, bool>? predicate = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => dictionary.Where(predicate!));
        exception.ParamName.Should().Be("predicate");
    }

    [TestMethod]
    public void Where_WithFilterPredicate_ShouldReturnFilteredDictionary()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["a"] = 1, ["bb"] = 2, ["ccc"] = 3, ["dddd"] = 4 };
        var predicate = new Func<string, int, bool>((k, v) => k.Length > 2);

        // Act
        var result = dictionary.Where(predicate).ToDictionary(kv => kv.Key, kv => kv.Value);

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainKey("ccc");
        result.Should().ContainKey("dddd");
        result["ccc"].Should().Be(3);
        result["dddd"].Should().Be(4);
    }

    #endregion

    #region ToDictionary Object Tests

    [TestMethod]
    public void ToDictionary_FromObject_WithNullObject_ShouldThrowArgumentNullException()
    {
        // Arrange
        object? obj = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => obj!.ToDictionary());
        exception.ParamName.Should().Be("obj");
    }

    [TestMethod]
    public void ToDictionary_FromObject_WithValidObject_ShouldReturnDictionary()
    {
        // Arrange
        var obj = new { Name = "Test", Age = 25, IsActive = true };

        // Act
        var result = obj.ToDictionary();

        // Assert
        result.Should().BeOfType<Dictionary<string, object?>>();
        result.Should().HaveCount(3);
        result["Name"].Should().Be("Test");
        result["Age"].Should().Be(25);
        result["IsActive"].Should().Be(true);
    }

    #endregion

    #region IsNullOrEmpty Tests

    [TestMethod]
    public void IsNullOrEmpty_WithNullDictionary_ShouldReturnTrue()
    {
        // Arrange
        IDictionary<string, int>? dictionary = null;

        // Act
        bool result = dictionary.IsNullOrEmpty();

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsNullOrEmpty_WithEmptyDictionary_ShouldReturnTrue()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>();

        // Act
        bool result = dictionary.IsNullOrEmpty();

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsNullOrEmpty_WithNonEmptyDictionary_ShouldReturnFalse()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["key1"] = 1 };

        // Act
        bool result = dictionary.IsNullOrEmpty();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region RemoveRange Tests

    [TestMethod]
    public void RemoveRange_WithNullDictionary_ShouldThrowArgumentNullException()
    {
        // Arrange
        IDictionary<string, int>? dictionary = null;
        var keys = new List<string> { "key1" };

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => dictionary!.RemoveRange(keys));
        exception.ParamName.Should().Be("dictionary");
    }

    [TestMethod]
    public void RemoveRange_WithNullKeys_ShouldThrowArgumentNullException()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["key1"] = 1 };
        IEnumerable<string>? keys = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => dictionary.RemoveRange(keys!));
        exception.ParamName.Should().Be("keys");
    }

    [TestMethod]
    public void RemoveRange_WithValidKeys_ShouldRemoveKeysAndReturnCount()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["key1"] = 1, ["key2"] = 2, ["key3"] = 3 };
        var keysToRemove = new List<string> { "key1", "key3", "nonexistent" };

        // Act
        int result = dictionary.RemoveRange(keysToRemove);

        // Assert
        result.Should().Be(2); // Only key1 and key3 were removed
        dictionary.Should().HaveCount(1);
        dictionary.Should().ContainKey("key2");
        dictionary.Should().NotContainKey("key1");
        dictionary.Should().NotContainKey("key3");
    }

    #endregion

    #region TryRemove Tests

    [TestMethod]
    public void TryRemove_WithNullDictionary_ShouldThrowArgumentNullException()
    {
        // Arrange
        IDictionary<string, int>? dictionary = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => dictionary!.TryRemove("key", out _));
        exception.ParamName.Should().Be("dictionary");
    }

    [TestMethod]
    public void TryRemove_WithExistingKey_ShouldRemoveAndReturnTrue()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["key1"] = 1, ["key2"] = 2 };

        // Act
        bool result = dictionary.TryRemove("key1", out int value);

        // Assert
        result.Should().BeTrue();
        value.Should().Be(1);
        dictionary.Should().HaveCount(1);
        dictionary.Should().NotContainKey("key1");
    }

    [TestMethod]
    public void TryRemove_WithNonExistentKey_ShouldReturnFalse()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["key1"] = 1 };

        // Act
        bool result = dictionary.TryRemove("nonexistent", out int value);

        // Assert
        result.Should().BeFalse();
        value.Should().Be(0); // default
        dictionary.Should().HaveCount(1);
    }

    #endregion

    #region TryUpdate Tests

    [TestMethod]
    public void TryUpdate_WithNullDictionary_ShouldThrowArgumentNullException()
    {
        // Arrange
        IDictionary<string, int>? dictionary = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => dictionary!.TryUpdate("key", 1));
        exception.ParamName.Should().Be("dictionary");
    }

    [TestMethod]
    public void TryUpdate_WithExistingKey_ShouldUpdateAndReturnTrue()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["key1"] = 1 };

        // Act
        bool result = dictionary.TryUpdate("key1", 10);

        // Assert
        result.Should().BeTrue();
        dictionary["key1"].Should().Be(10);
    }

    [TestMethod]
    public void TryUpdate_WithNonExistentKey_ShouldReturnFalse()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["key1"] = 1 };

        // Act
        bool result = dictionary.TryUpdate("nonexistent", 10);

        // Assert
        result.Should().BeFalse();
        dictionary.Should().HaveCount(1);
        dictionary.Should().NotContainKey("nonexistent");
    }

    #endregion

    #region ForEach Tests

    [TestMethod]
    public void ForEach_WithNullDictionary_ShouldThrowArgumentNullException()
    {
        // Arrange
        IDictionary<string, int>? dictionary = null;
        var action = new Action<string, int>((k, v) => { });

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => dictionary!.ForEach(action));
        exception.ParamName.Should().Be("dictionary");
    }

    [TestMethod]
    public void ForEach_WithNullAction_ShouldThrowArgumentNullException()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["key1"] = 1 };
        Action<string, int>? action = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => dictionary.ForEach(action!));
        exception.ParamName.Should().Be("action");
    }

    [TestMethod]
    public void ForEach_WithValidInputs_ShouldExecuteActionForEachPair()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var results = new List<string>();
        var action = new Action<string, int>((k, v) => results.Add($"{k}:{v}"));

        // Act
        dictionary.ForEach(action);

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain("a:1");
        results.Should().Contain("b:2");
    }

    #endregion

    #region Invert Tests

    [TestMethod]
    public void Invert_WithNullDictionary_ShouldThrowArgumentNullException()
    {
        // Arrange
        IDictionary<string, int>? dictionary = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => dictionary!.Invert());
        exception.ParamName.Should().Be("dictionary");
    }

    [TestMethod]
    public void Invert_WithUniqueValues_ShouldInvertDictionary()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3 };

        // Act
        Dictionary<int, string> result = dictionary.Invert();

        // Assert
        result.Should().HaveCount(3);
        result[1].Should().Be("a");
        result[2].Should().Be("b");
        result[3].Should().Be("c");
    }

    [TestMethod]
    public void Invert_WithDuplicateValues_ShouldThrowArgumentException()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["a"] = 1, ["b"] = 1 }; // Duplicate value

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => dictionary.Invert());
    }

    #endregion

    #region IncrementValue Tests

    [TestMethod]
    public void IncrementValue_WithNullDictionary_ShouldThrowArgumentNullException()
    {
        // Arrange
        IDictionary<string, int>? dictionary = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => dictionary!.IncrementValue("key"));
        exception.ParamName.Should().Be("dictionary");
    }

    [TestMethod]
    public void IncrementValue_WithNewKey_ShouldSetToIncrement()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>();

        // Act
        int result = dictionary.IncrementValue("counter");

        // Assert
        result.Should().Be(1); // Default increment
        dictionary["counter"].Should().Be(1);
    }

    [TestMethod]
    public void IncrementValue_WithExistingKey_ShouldIncrement()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["counter"] = 5 };

        // Act
        int result = dictionary.IncrementValue("counter", 3);

        // Assert
        result.Should().Be(8); // 5 + 3
        dictionary["counter"].Should().Be(8);
    }

    #endregion

    #region AddToList Tests

    [TestMethod]
    public void AddToList_WithNullDictionary_ShouldThrowArgumentNullException()
    {
        // Arrange
        IDictionary<string, List<int>>? dictionary = null;

        // Act & Assert
        ArgumentNullException? exception = Assert.ThrowsExactly<ArgumentNullException>(() => dictionary!.AddToList("key", 1));
        exception.ParamName.Should().Be("dictionary");
    }

    [TestMethod]
    public void AddToList_WithNewKey_ShouldCreateListAndAddItem()
    {
        // Arrange
        var dictionary = new Dictionary<string, List<int>>();

        // Act
        dictionary.AddToList("items", 1);

        // Assert
        dictionary.Should().ContainKey("items");
        dictionary["items"].Should().ContainSingle().Which.Should().Be(1);
    }

    [TestMethod]
    public void AddToList_WithExistingKey_ShouldAddToExistingList()
    {
        // Arrange
        var dictionary = new Dictionary<string, List<int>> { ["items"] = [1, 2] };

        // Act
        dictionary.AddToList("items", 3);

        // Assert
        dictionary["items"].Should().ContainInOrder(1, 2, 3);
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void DictionaryExtensions_ChainedOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var dictionary = new Dictionary<string, int> { ["apple"] = 5, ["banana"] = 3, ["cherry"] = 8 };

        // Act
        var result = dictionary
            .Where((k, v) => v > 4)
            .TransformValues(v => $"Count: {v}")
            .ToReadOnlyDictionary();

        // Assert
        result.Should().HaveCount(2);
        result["apple"].Should().Be("Count: 5");
        result["cherry"].Should().Be("Count: 8");
        result.Should().NotContainKey("banana");
    }

    [TestMethod]
    public void DictionaryExtensions_WithComplexMergeScenario_ShouldWorkCorrectly()
    {
        // Arrange
        var inventory = new Dictionary<string, int> { ["apples"] = 10, ["bananas"] = 5 };
        var newStock = new Dictionary<string, int> { ["bananas"] = 3, ["oranges"] = 7 };
        var resolver = new Func<string, int, int, int>((k, existing, incoming) => existing + incoming);

        // Act
        Dictionary<string, int> result = inventory.Merge(newStock, resolver);

        // Assert
        result.Should().HaveCount(3);
        result["apples"].Should().Be(10);
        result["bananas"].Should().Be(8); // 5 + 3
        result["oranges"].Should().Be(7);
    }

    #endregion
}
