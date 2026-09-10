using System.Linq.Expressions;
using VisionaryCoder.Framework.Querying;

namespace VisionaryCoder.Framework.Tests.Querying;

/// <summary>
/// Comprehensive unit tests for <see cref="QueryFilterExtensions"/>.
/// Tests composition methods (And/Or/Not), string matching methods (Contains/StartsWith/EndsWith),
/// case-insensitive variants, and IQueryable application methods.
/// </summary>
[TestClass]
public sealed class QueryFilterExtensionsTests
{
    private sealed record TestEntity(int Id, string Name, string Email);

    #region And Composition Tests

    [TestMethod]
    public void And_WithTwoFilters_ShouldCombineWithAndLogic()
    {
        // Arrange
        var filter1 = new QueryFilter<TestEntity>(e => e.Id > 0);
        var filter2 = new QueryFilter<TestEntity>(e => e.Id < 100);

        // Act
        QueryFilter<TestEntity> combined = filter1.And(filter2);

        // Assert
        combined.Should().NotBeNull();
        combined.Predicate.Compile()(new TestEntity(50, "Test", "test@test.com")).Should().BeTrue();
        combined.Predicate.Compile()(new TestEntity(-1, "Test", "test@test.com")).Should().BeFalse();
        combined.Predicate.Compile()(new TestEntity(150, "Test", "test@test.com")).Should().BeFalse();
    }

    [TestMethod]
    public void And_WithNullLeft_ShouldThrowArgumentNullException()
    {
        // Arrange
        QueryFilter<TestEntity> left = null!;
        var right = new QueryFilter<TestEntity>(e => e.Id > 0);

        // Act
        Action act = () => left.And(right);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("left");
    }

    [TestMethod]
    public void And_WithNullRight_ShouldThrowArgumentNullException()
    {
        // Arrange
        var left = new QueryFilter<TestEntity>(e => e.Id > 0);
        QueryFilter<TestEntity> right = null!;

        // Act
        Action act = () => left.And(right);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("right");
    }

    #endregion

    #region Or Composition Tests

    [TestMethod]
    public void Or_WithTwoFilters_ShouldCombineWithOrLogic()
    {
        // Arrange
        var filter1 = new QueryFilter<TestEntity>(e => e.Id < 10);
        var filter2 = new QueryFilter<TestEntity>(e => e.Id > 90);

        // Act
        QueryFilter<TestEntity> combined = filter1.Or(filter2);

        // Assert
        combined.Should().NotBeNull();
        combined.Predicate.Compile()(new TestEntity(5, "Test", "test@test.com")).Should().BeTrue();
        combined.Predicate.Compile()(new TestEntity(95, "Test", "test@test.com")).Should().BeTrue();
        combined.Predicate.Compile()(new TestEntity(50, "Test", "test@test.com")).Should().BeFalse();
    }

    [TestMethod]
    public void Or_WithNullLeft_ShouldThrowArgumentNullException()
    {
        // Arrange
        QueryFilter<TestEntity> left = null!;
        var right = new QueryFilter<TestEntity>(e => e.Id > 0);

        // Act
        Action act = () => left.Or(right);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("left");
    }

    [TestMethod]
    public void Or_WithNullRight_ShouldThrowArgumentNullException()
    {
        // Arrange
        var left = new QueryFilter<TestEntity>(e => e.Id > 0);
        QueryFilter<TestEntity> right = null!;

        // Act
        Action act = () => left.Or(right);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("right");
    }

    #endregion

    #region Not Composition Tests

    [TestMethod]
    public void Not_WithFilter_ShouldNegateLogic()
    {
        // Arrange
        var filter = new QueryFilter<TestEntity>(e => e.Id > 50);

        // Act
        QueryFilter<TestEntity> negated = filter.Not();

        // Assert
        negated.Should().NotBeNull();
        negated.Predicate.Compile()(new TestEntity(60, "Test", "test@test.com")).Should().BeFalse();
        negated.Predicate.Compile()(new TestEntity(40, "Test", "test@test.com")).Should().BeTrue();
    }

    [TestMethod]
    public void Not_WithNullFilter_ShouldThrowArgumentNullException()
    {
        // Arrange
        QueryFilter<TestEntity> filter = null!;

        // Act
        Action act = () => filter.Not();

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("filter");
    }

    #endregion

    #region Contains Tests

    [TestMethod]
    public void Contains_WithValidValue_ShouldCreateFilter()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = e => e.Name;

        // Act
        QueryFilter<TestEntity> filter = QueryFilterExtensions.Contains(selector, "test");

        // Assert
        filter.Should().NotBeNull();
        filter.Predicate.Compile()(new TestEntity(1, "testing", "email")).Should().BeTrue();
        filter.Predicate.Compile()(new TestEntity(1, "other", "email")).Should().BeFalse();
    }

    [TestMethod]
    public void Contains_WithNullValue_ShouldReturnAlwaysTrueFilter()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = e => e.Name;

        // Act
        QueryFilter<TestEntity> filter = QueryFilterExtensions.Contains(selector, null);

        // Assert
        filter.Should().NotBeNull();
        filter.Predicate.Compile()(new TestEntity(1, "any", "email")).Should().BeTrue();
    }

    [TestMethod]
    public void Contains_WithEmptyValue_ShouldReturnAlwaysTrueFilter()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = e => e.Name;

        // Act
        QueryFilter<TestEntity> filter = QueryFilterExtensions.Contains(selector, "");

        // Assert
        filter.Should().NotBeNull();
        filter.Predicate.Compile()(new TestEntity(1, "any", "email")).Should().BeTrue();
    }

    [TestMethod]
    public void Contains_WithWhitespaceValue_ShouldReturnAlwaysTrueFilter()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = e => e.Name;

        // Act
        QueryFilter<TestEntity> filter = QueryFilterExtensions.Contains(selector, "   ");

        // Assert
        filter.Should().NotBeNull();
        filter.Predicate.Compile()(new TestEntity(1, "any", "email")).Should().BeTrue();
    }

    [TestMethod]
    public void Contains_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = null!;

        // Act
        Action act = () => QueryFilterExtensions.Contains(selector, "test");

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("selector");
    }

    #endregion

    #region StartsWith Tests

    [TestMethod]
    public void StartsWith_WithValidValue_ShouldCreateFilter()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = e => e.Name;

        // Act
        QueryFilter<TestEntity> filter = QueryFilterExtensions.StartsWith(selector, "test");

        // Assert
        filter.Should().NotBeNull();
        filter.Predicate.Compile()(new TestEntity(1, "testing", "email")).Should().BeTrue();
        filter.Predicate.Compile()(new TestEntity(1, "other", "email")).Should().BeFalse();
    }

    [TestMethod]
    public void StartsWith_WithNullValue_ShouldReturnAlwaysTrueFilter()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = e => e.Name;

        // Act
        QueryFilter<TestEntity> filter = QueryFilterExtensions.StartsWith(selector, null);

        // Assert
        filter.Should().NotBeNull();
        filter.Predicate.Compile()(new TestEntity(1, "any", "email")).Should().BeTrue();
    }

    [TestMethod]
    public void StartsWith_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = null!;

        // Act
        Action act = () => QueryFilterExtensions.StartsWith(selector, "test");

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("selector");
    }

    #endregion

    #region EndsWith Tests

    [TestMethod]
    public void EndsWith_WithValidValue_ShouldCreateFilter()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = e => e.Email;

        // Act
        QueryFilter<TestEntity> filter = QueryFilterExtensions.EndsWith(selector, ".com");

        // Assert
        filter.Should().NotBeNull();
        filter.Predicate.Compile()(new TestEntity(1, "name", "test@test.com")).Should().BeTrue();
        filter.Predicate.Compile()(new TestEntity(1, "name", "test@test.org")).Should().BeFalse();
    }

    [TestMethod]
    public void EndsWith_WithNullValue_ShouldReturnAlwaysTrueFilter()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = e => e.Email;

        // Act
        QueryFilter<TestEntity> filter = QueryFilterExtensions.EndsWith(selector, null);

        // Assert
        filter.Should().NotBeNull();
        filter.Predicate.Compile()(new TestEntity(1, "name", "any@email")).Should().BeTrue();
    }

    [TestMethod]
    public void EndsWith_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = null!;

        // Act
        Action act = () => QueryFilterExtensions.EndsWith(selector, ".com");

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("selector");
    }

    #endregion

    #region ContainsIgnoreCase Tests

    [TestMethod]
    public void ContainsIgnoreCase_WithValidValue_ShouldCreateCaseInsensitiveFilter()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = e => e.Name;

        // Act
        QueryFilter<TestEntity> filter = QueryFilterExtensions.ContainsIgnoreCase(selector, "TEST");

        // Assert
        filter.Should().NotBeNull();
        filter.Predicate.Compile()(new TestEntity(1, "testing", "email")).Should().BeTrue();
        filter.Predicate.Compile()(new TestEntity(1, "TESTING", "email")).Should().BeTrue();
        filter.Predicate.Compile()(new TestEntity(1, "TeSt", "email")).Should().BeTrue();
        filter.Predicate.Compile()(new TestEntity(1, "other", "email")).Should().BeFalse();
    }

    [TestMethod]
    public void ContainsIgnoreCase_WithNullPropertyValue_ShouldReturnFalse()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = e => e.Name;

        // Act
        QueryFilter<TestEntity> filter = QueryFilterExtensions.ContainsIgnoreCase(selector, "test");

        // Assert - This will check the null-safety in the expression
        filter.Should().NotBeNull();
        filter.Predicate.Compile()(new TestEntity(1, null!, "email")).Should().BeFalse();
    }

    [TestMethod]
    public void ContainsIgnoreCase_WithNullValue_ShouldReturnAlwaysTrueFilter()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = e => e.Name;

        // Act
        QueryFilter<TestEntity> filter = QueryFilterExtensions.ContainsIgnoreCase(selector, null);

        // Assert
        filter.Should().NotBeNull();
        filter.Predicate.Compile()(new TestEntity(1, "any", "email")).Should().BeTrue();
    }

    [TestMethod]
    public void ContainsIgnoreCase_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = null!;

        // Act
        Action act = () => QueryFilterExtensions.ContainsIgnoreCase(selector, "test");

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("selector");
    }

    #endregion

    #region StartsWithIgnoreCase Tests

    [TestMethod]
    public void StartsWithIgnoreCase_WithValidValue_ShouldCreateCaseInsensitiveFilter()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = e => e.Name;

        // Act
        QueryFilter<TestEntity> filter = QueryFilterExtensions.StartsWithIgnoreCase(selector, "TEST");

        // Assert
        filter.Should().NotBeNull();
        filter.Predicate.Compile()(new TestEntity(1, "testing", "email")).Should().BeTrue();
        filter.Predicate.Compile()(new TestEntity(1, "TESTING", "email")).Should().BeTrue();
        filter.Predicate.Compile()(new TestEntity(1, "TeSt", "email")).Should().BeTrue();
        filter.Predicate.Compile()(new TestEntity(1, "other", "email")).Should().BeFalse();
    }

    [TestMethod]
    public void StartsWithIgnoreCase_WithNullPropertyValue_ShouldReturnFalse()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = e => e.Name;

        // Act
        QueryFilter<TestEntity> filter = QueryFilterExtensions.StartsWithIgnoreCase(selector, "test");

        // Assert
        filter.Should().NotBeNull();
        filter.Predicate.Compile()(new TestEntity(1, null!, "email")).Should().BeFalse();
    }

    [TestMethod]
    public void StartsWithIgnoreCase_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = null!;

        // Act
        Action act = () => QueryFilterExtensions.StartsWithIgnoreCase(selector, "test");

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("selector");
    }

    #endregion

    #region EndsWithIgnoreCase Tests

    [TestMethod]
    public void EndsWithIgnoreCase_WithValidValue_ShouldCreateCaseInsensitiveFilter()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = e => e.Email;

        // Act
        QueryFilter<TestEntity> filter = QueryFilterExtensions.EndsWithIgnoreCase(selector, ".COM");

        // Assert
        filter.Should().NotBeNull();
        filter.Predicate.Compile()(new TestEntity(1, "name", "test@test.com")).Should().BeTrue();
        filter.Predicate.Compile()(new TestEntity(1, "name", "test@test.COM")).Should().BeTrue();
        filter.Predicate.Compile()(new TestEntity(1, "name", "test@test.CoM")).Should().BeTrue();
        filter.Predicate.Compile()(new TestEntity(1, "name", "test@test.org")).Should().BeFalse();
    }

    [TestMethod]
    public void EndsWithIgnoreCase_WithNullPropertyValue_ShouldReturnFalse()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = e => e.Email;

        // Act
        QueryFilter<TestEntity> filter = QueryFilterExtensions.EndsWithIgnoreCase(selector, ".com");

        // Assert
        filter.Should().NotBeNull();
        filter.Predicate.Compile()(new TestEntity(1, "name", null!)).Should().BeFalse();
    }

    [TestMethod]
    public void EndsWithIgnoreCase_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Arrange
        Expression<Func<TestEntity, string>> selector = null!;

        // Act
        Action act = () => QueryFilterExtensions.EndsWithIgnoreCase(selector, ".com");

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("selector");
    }

    #endregion

    #region Join Tests

    [TestMethod]
    public void Join_WithMultipleFiltersAndTrue_ShouldCombineWithAnd()
    {
        // Arrange
        QueryFilter<TestEntity>[] filters =
        [
            new QueryFilter<TestEntity>(e => e.Id > 0),
            new QueryFilter<TestEntity>(e => e.Id < 100),
            new QueryFilter<TestEntity>(e => e.Name != null)
        ];

        // Act
        QueryFilter<TestEntity> combined = filters.Join(useAnd: true);

        // Assert
        combined.Should().NotBeNull();
        combined.Predicate.Compile()(new TestEntity(50, "Test", "email")).Should().BeTrue();
        combined.Predicate.Compile()(new TestEntity(-1, "Test", "email")).Should().BeFalse();
    }

    [TestMethod]
    public void Join_WithMultipleFiltersAndFalse_ShouldCombineWithOr()
    {
        // Arrange
        QueryFilter<TestEntity>[] filters =
        [
            new QueryFilter<TestEntity>(e => e.Id < 10),
            new QueryFilter<TestEntity>(e => e.Id > 90)
        ];

        // Act
        QueryFilter<TestEntity> combined = filters.Join(useAnd: false);

        // Assert
        combined.Should().NotBeNull();
        combined.Predicate.Compile()(new TestEntity(5, "Test", "email")).Should().BeTrue();
        combined.Predicate.Compile()(new TestEntity(95, "Test", "email")).Should().BeTrue();
        combined.Predicate.Compile()(new TestEntity(50, "Test", "email")).Should().BeFalse();
    }

    [TestMethod]
    public void Join_WithEmptySequence_ShouldReturnAlwaysTrueFilter()
    {
        // Arrange
        QueryFilter<TestEntity>[] filters = [];

        // Act
        QueryFilter<TestEntity> combined = filters.Join();

        // Assert
        combined.Should().NotBeNull();
        combined.Predicate.Compile()(new TestEntity(1, "any", "email")).Should().BeTrue();
    }

    [TestMethod]
    public void Join_WithNullSequence_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<QueryFilter<TestEntity>> filters = null!;

        // Act
        Action act = () => filters.Join();

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("filters");
    }

    [TestMethod]
    public void Join_ParamsOverload_WithMultipleFilters_ShouldCombine()
    {
        // Arrange
        var filter1 = new QueryFilter<TestEntity>(e => e.Id > 0);
        var filter2 = new QueryFilter<TestEntity>(e => e.Id < 100);

        // Act
        QueryFilter<TestEntity> combined = QueryFilterExtensions.Join(true, filter1, filter2);

        // Assert
        combined.Should().NotBeNull();
        combined.Predicate.Compile()(new TestEntity(50, "Test", "email")).Should().BeTrue();
        combined.Predicate.Compile()(new TestEntity(150, "Test", "email")).Should().BeFalse();
    }

    [TestMethod]
    public void Join_ParamsOverload_WithNullArray_ShouldReturnAlwaysTrueFilter()
    {
        // Act
        QueryFilter<TestEntity> combined = QueryFilterExtensions.Join<TestEntity>(true, null!);

        // Assert
        combined.Should().NotBeNull();
        combined.Predicate.Compile()(new TestEntity(1, "any", "email")).Should().BeTrue();
    }

    #endregion

    #region Apply Tests

    [TestMethod]
    public void Apply_WithValidFilter_ShouldFilterQueryable()
    {
        // Arrange
        IQueryable<TestEntity> data = new[]
        {
            new TestEntity(1, "Alice", "alice@test.com"),
            new TestEntity(2, "Bob", "bob@test.com"),
            new TestEntity(3, "Charlie", "charlie@test.com")
        }.AsQueryable();

        var filter = new QueryFilter<TestEntity>(e => e.Id > 1);

        // Act
        var result = data.Apply(filter).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(e => e.Name == "Bob");
        result.Should().Contain(e => e.Name == "Charlie");
    }

    [TestMethod]
    public void Apply_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        IQueryable<TestEntity> source = null!;
        var filter = new QueryFilter<TestEntity>(e => e.Id > 0);

        // Act
        Action act = () => source.Apply(filter);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("source");
    }

    [TestMethod]
    public void Apply_WithNullFilter_ShouldThrowArgumentNullException()
    {
        // Arrange
        IQueryable<TestEntity> source = Array.Empty<TestEntity>().AsQueryable();
        QueryFilter<TestEntity> filter = null!;

        // Act
        Action act = () => source.Apply(filter);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("filter");
    }

    #endregion

    #region ApplyAll Tests

    [TestMethod]
    public void ApplyAll_WithMultipleFilters_ShouldApplyAllSequentially()
    {
        // Arrange
        IQueryable<TestEntity> data = new[]
        {
            new TestEntity(1, "Alice", "alice@test.com"),
            new TestEntity(2, "Bob", "bob@test.com"),
            new TestEntity(3, "Charlie", "charlie@test.com"),
            new TestEntity(4, "David", "david@test.com")
        }.AsQueryable();

        QueryFilter<TestEntity>[] filters =
        [
            new QueryFilter<TestEntity>(e => e.Id > 1),
            new QueryFilter<TestEntity>(e => e.Id < 4)
        ];

        // Act
        var result = data.ApplyAll(filters).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(e => e.Name == "Bob");
        result.Should().Contain(e => e.Name == "Charlie");
    }

    [TestMethod]
    public void ApplyAll_WithNullFiltersInSequence_ShouldSkipNulls()
    {
        // Arrange
        IQueryable<TestEntity> data = new[]
        {
            new TestEntity(1, "Alice", "alice@test.com"),
            new TestEntity(2, "Bob", "bob@test.com")
        }.AsQueryable();

        QueryFilter<TestEntity>?[] filters =
        [
            new QueryFilter<TestEntity>(e => e.Id > 0),
            null,
            new QueryFilter<TestEntity>(e => e.Id < 10)
        ];

        // Act
        var result = data.ApplyAll(filters!).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    [TestMethod]
    public void ApplyAll_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        IQueryable<TestEntity> source = null!;
        QueryFilter<TestEntity>[] filters = [new QueryFilter<TestEntity>(e => e.Id > 0)];

        // Act
        Action act = () => source.ApplyAll(filters);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("source");
    }

    [TestMethod]
    public void ApplyAll_WithNullFiltersCollection_ShouldThrowArgumentNullException()
    {
        // Arrange
        IQueryable<TestEntity> source = Array.Empty<TestEntity>().AsQueryable();
        IEnumerable<QueryFilter<TestEntity>> filters = null!;

        // Act
        Action act = () => source.ApplyAll(filters);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("filters");
    }

    #endregion

    #region Complex Composition Tests

    [TestMethod]
    public void QueryFilter_ComplexComposition_ShouldWork()
    {
        // Arrange
        IQueryable<TestEntity> data = new[]
        {
            new TestEntity(1, "Alice Anderson", "alice@gmail.com"),
            new TestEntity(2, "Bob Brown", "bob@yahoo.com"),
            new TestEntity(3, "Charlie Chen", "charlie@gmail.com"),
            new TestEntity(4, "David Davis", "david@test.org")
        }.AsQueryable();

        QueryFilter<TestEntity> hasA = QueryFilterExtensions.ContainsIgnoreCase<TestEntity>(e => e.Name, "a");
        QueryFilter<TestEntity> gmail = QueryFilterExtensions.EndsWithIgnoreCase<TestEntity>(e => e.Email, "@gmail.com");
        QueryFilter<TestEntity> filter = hasA.And(gmail);

        // Act
        var result = data.Apply(filter).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(e => e.Name == "Alice Anderson");
        result.Should().Contain(e => e.Name == "Charlie Chen");
    }

    [TestMethod]
    public void QueryFilter_MultipleOrConditions_ShouldWork()
    {
        // Arrange
        IQueryable<TestEntity> data = new[]
        {
            new TestEntity(1, "Alice", "alice@gmail.com"),
            new TestEntity(2, "Bob", "bob@yahoo.com"),
            new TestEntity(3, "Charlie", "charlie@hotmail.com")
        }.AsQueryable();

        QueryFilter<TestEntity> gmail = QueryFilterExtensions.EndsWithIgnoreCase<TestEntity>(e => e.Email, "@gmail.com");
        QueryFilter<TestEntity> yahoo = QueryFilterExtensions.EndsWithIgnoreCase<TestEntity>(e => e.Email, "@yahoo.com");
        QueryFilter<TestEntity> filter = gmail.Or(yahoo);

        // Act
        var result = data.Apply(filter).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(e => e.Name == "Alice");
        result.Should().Contain(e => e.Name == "Bob");
    }

    [TestMethod]
    public void QueryFilter_NotWithCombination_ShouldWork()
    {
        // Arrange
        IQueryable<TestEntity> data = new[]
        {
            new TestEntity(1, "Alice", "alice@test.com"),
            new TestEntity(2, "Bob", "bob@test.com"),
            new TestEntity(3, "Charlie", "charlie@test.com")
        }.AsQueryable();

        QueryFilter<TestEntity> hasAlice = QueryFilterExtensions.ContainsIgnoreCase<TestEntity>(e => e.Name, "alice");
        QueryFilter<TestEntity> notAlice = hasAlice.Not();

        // Act
        var result = data.Apply(notAlice).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain(e => e.Name == "Alice");
    }

    #endregion
}
