using System.Linq.Expressions;
using VisionaryCoder.Framework.Querying;

namespace VisionaryCoder.Framework.Tests.Querying;

/// <summary>
/// Data-driven unit tests for the <see cref="QueryFilter{T}"/> class.
/// Tests query filter wrapper functionality with various scenarios.
/// </summary>
[TestClass]
public class QueryFilterTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_WithValidPredicate_ShouldSetPredicate()
    {
        // Arrange
        Expression<Func<TestUser, bool>> predicate = u => u.Age > 18;

        // Act
        var filter = new QueryFilter<TestUser>(predicate);

        // Assert
        filter.Predicate.Should().NotBeNull();
        filter.Predicate.Should().BeSameAs(predicate);
    }

    [TestMethod]
    public void Constructor_WithNullPredicate_ShouldThrowArgumentNullException()
    {
        // Arrange
        Expression<Func<TestUser, bool>>? predicate = null;

        // Act
        Action act = () => new QueryFilter<TestUser>(predicate!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("predicate");
    }

    [TestMethod]
    public void Constructor_WithSimplePredicate_ShouldAccept()
    {
        // Arrange
        Expression<Func<TestUser, bool>> predicate = u => true;

        // Act
        var filter = new QueryFilter<TestUser>(predicate);

        // Assert
        filter.Predicate.Should().NotBeNull();
    }

    [TestMethod]
    public void Constructor_WithComplexPredicate_ShouldAccept()
    {
        // Arrange
        Expression<Func<TestUser, bool>> predicate = u =>
            u.Age > 18 && u.Name.StartsWith("A") && !u.IsDeleted;

        // Act
        var filter = new QueryFilter<TestUser>(predicate);

        // Assert
        filter.Predicate.Should().NotBeNull();
        filter.Predicate.Should().BeSameAs(predicate);
    }

    #endregion

    #region Predicate Property Tests

    [TestMethod]
    public void Predicate_ShouldReturnSameInstanceAsConstructed()
    {
        // Arrange
        Expression<Func<TestUser, bool>> predicate = u => u.Age >= 21;
        var filter = new QueryFilter<TestUser>(predicate);

        // Act
        Expression<Func<TestUser, bool>> retrievedPredicate = filter.Predicate;

        // Assert
        retrievedPredicate.Should().BeSameAs(predicate);
    }

    [TestMethod]
    public void Predicate_ShouldBeUsableWithLinq()
    {
        // Arrange
        Expression<Func<TestUser, bool>> predicate = u => u.Age > 25;
        var filter = new QueryFilter<TestUser>(predicate);
        IQueryable<TestUser> users = new List<TestUser>
        {
            new("John", 30),
            new("Jane", 20),
            new("Bob", 28)
        }.AsQueryable();

        // Act
        var result = users.Where(filter.Predicate).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Name == "John");
        result.Should().Contain(u => u.Name == "Bob");
    }

    [TestMethod]
    public void Predicate_WithCompile_ShouldWorkAsFunc()
    {
        // Arrange
        Expression<Func<TestUser, bool>> predicate = u => u.Age < 25;
        var filter = new QueryFilter<TestUser>(predicate);
        Func<TestUser, bool> compiledFunc = filter.Predicate.Compile();
        var testUser = new TestUser("Test", 20);

        // Act
        bool result = compiledFunc(testUser);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Different Predicate Types Tests

    [TestMethod]
    public void QueryFilter_WithEquality_ShouldWork()
    {
        // Arrange
        Expression<Func<TestUser, bool>> predicate = u => u.Name == "John";
        var filter = new QueryFilter<TestUser>(predicate);
        var users = new List<TestUser>
        {
            new("John", 25),
            new("Jane", 30)
        };

        // Act
        var result = users.Where(filter.Predicate.Compile()).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("John");
    }

    [TestMethod]
    public void QueryFilter_WithStringOperations_ShouldWork()
    {
        // Arrange
        Expression<Func<TestUser, bool>> predicate = u => u.Name.Contains("oh");
        var filter = new QueryFilter<TestUser>(predicate);
        var users = new List<TestUser>
        {
            new("John", 25),
            new("Jane", 30),
            new("Johnny", 35)
        };

        // Act
        var result = users.Where(filter.Predicate.Compile()).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    [TestMethod]
    public void QueryFilter_WithNumericComparison_ShouldWork()
    {
        // Arrange
        Expression<Func<TestUser, bool>> predicate = u => u.Age >= 30;
        var filter = new QueryFilter<TestUser>(predicate);
        var users = new List<TestUser>
        {
            new("John", 25),
            new("Jane", 30),
            new("Bob", 35)
        };

        // Act
        var result = users.Where(filter.Predicate.Compile()).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    [TestMethod]
    public void QueryFilter_WithLogicalOperators_ShouldWork()
    {
        // Arrange
        Expression<Func<TestUser, bool>> predicate = u => u.Age > 20 && u.Age < 30;
        var filter = new QueryFilter<TestUser>(predicate);
        var users = new List<TestUser>
        {
            new("John", 25),
            new("Jane", 35),
            new("Bob", 19)
        };

        // Act
        var result = users.Where(filter.Predicate.Compile()).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("John");
    }

    #endregion

    #region Multiple Instance Tests

    [TestMethod]
    public void QueryFilter_MultipleInstances_ShouldBeIndependent()
    {
        // Arrange
        Expression<Func<TestUser, bool>> predicate1 = u => u.Age > 25;
        Expression<Func<TestUser, bool>> predicate2 = u => u.Age < 25;
        var filter1 = new QueryFilter<TestUser>(predicate1);
        var filter2 = new QueryFilter<TestUser>(predicate2);

        // Assert
        filter1.Predicate.Should().NotBeSameAs(filter2.Predicate);
    }

    [TestMethod]
    public void QueryFilter_SamePredicateInstance_CanBeShared()
    {
        // Arrange
        Expression<Func<TestUser, bool>> predicate = u => u.Age > 18;

        // Act
        var filter1 = new QueryFilter<TestUser>(predicate);
        var filter2 = new QueryFilter<TestUser>(predicate);

        // Assert
        filter1.Predicate.Should().BeSameAs(filter2.Predicate);
        filter1.Predicate.Should().BeSameAs(predicate);
        filter2.Predicate.Should().BeSameAs(predicate);
    }

    #endregion

    #region Generic Type Tests

    [TestMethod]
    public void QueryFilter_WithDifferentGenericTypes_ShouldWork()
    {
        // Arrange
        Expression<Func<TestUser, bool>> userPredicate = u => u.Age > 18;
        Expression<Func<TestProduct, bool>> productPredicate = p => p.Price > 100;

        // Act
        var userFilter = new QueryFilter<TestUser>(userPredicate);
        var productFilter = new QueryFilter<TestProduct>(productPredicate);

        // Assert
        userFilter.Predicate.Should().NotBeNull();
        productFilter.Predicate.Should().NotBeNull();
    }

    [TestMethod]
    public void QueryFilter_WithComplexType_ShouldWork()
    {
        // Arrange
        Expression<Func<TestUser, bool>> predicate = u => u.Name != null && u.Age > 0;
        var filter = new QueryFilter<TestUser>(predicate);
        var user = new TestUser("Test", 25);

        // Act
        bool result = filter.Predicate.Compile()(user);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Edge Cases Tests

    [TestMethod]
    public void QueryFilter_WithAlwaysTruePredicate_ShouldWork()
    {
        // Arrange
        Expression<Func<TestUser, bool>> predicate = u => true;
        var filter = new QueryFilter<TestUser>(predicate);
        var users = new List<TestUser>
        {
            new("John", 25),
            new("Jane", 30)
        };

        // Act
        var result = users.Where(filter.Predicate.Compile()).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    [TestMethod]
    public void QueryFilter_WithAlwaysFalsePredicate_ShouldWork()
    {
        // Arrange
        Expression<Func<TestUser, bool>> predicate = u => false;
        var filter = new QueryFilter<TestUser>(predicate);
        var users = new List<TestUser>
        {
            new("John", 25),
            new("Jane", 30)
        };

        // Act
        var result = users.Where(filter.Predicate.Compile()).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    public void QueryFilter_WithEmptyCollection_ShouldReturnEmpty()
    {
        // Arrange
        Expression<Func<TestUser, bool>> predicate = u => u.Age > 18;
        var filter = new QueryFilter<TestUser>(predicate);
        var users = new List<TestUser>();

        // Act
        var result = users.Where(filter.Predicate.Compile()).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Type System Tests

    [TestMethod]
    public void QueryFilter_ShouldBeSealed()
    {
        // Arrange & Act
        Type type = typeof(QueryFilter<TestUser>);

        // Assert
        type.IsSealed.Should().BeTrue();
    }

    [TestMethod]
    public void QueryFilter_ShouldBeClass()
    {
        // Arrange & Act
        Type type = typeof(QueryFilter<TestUser>);

        // Assert
        type.IsClass.Should().BeTrue();
    }

    #endregion

    #region Test Helper Classes

    private record TestUser(string Name, int Age, bool IsDeleted = false);
    private record TestProduct(string Name, decimal Price);

    #endregion
}
