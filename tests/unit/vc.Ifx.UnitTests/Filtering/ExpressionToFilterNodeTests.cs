using System.Linq.Expressions;
using VisionaryCoder.Framework.Filtering;
using VisionaryCoder.Framework.Filtering.Abstractions;

namespace VisionaryCoder.Framework.Tests.Filtering;

/// <summary>
/// Comprehensive unit tests for <see cref="ExpressionToFilterNode"/>.
/// Tests translation of LINQ expressions to FilterNode structures including
/// basic comparisons, string operations, logical operators, and collection methods.
/// </summary>
[TestClass]
public sealed class ExpressionToFilterNodeTests
{
    #region Test Models

    private sealed record TestEntity(
        int Id,
        string Name,
        string Email,
        int Age,
        bool IsActive,
        List<string> Tags,
        List<TestChild> Children);

    private sealed record TestChild(string Name, int Value, bool IsActive);

    #endregion

    #region Basic Comparison Tests

    [TestMethod]
    public void Translate_WithEqualsExpression_ShouldCreateFilterCondition()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = e => e.Name == "test";

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterCondition>();
        var condition = (FilterCondition)result;
        condition.Path.Should().Be("Name");
        condition.Operator.Should().Be(FilterOperation.Equals);
        condition.Value.Should().Be("test");
    }

    [TestMethod]
    public void Translate_WithNotEqualsExpression_ShouldCreateFilterCondition()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = e => e.Age != 25;

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterCondition>();
        var condition = (FilterCondition)result;
        condition.Path.Should().Be("Age");
        condition.Operator.Should().Be(FilterOperation.NotEquals);
        condition.Value.Should().Be("25");
    }

    [TestMethod]
    public void Translate_WithGreaterThanExpression_ShouldCreateFilterCondition()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = e => e.Age > 18;

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterCondition>();
        var condition = (FilterCondition)result;
        condition.Path.Should().Be("Age");
        condition.Operator.Should().Be(FilterOperation.GreaterThan);
        condition.Value.Should().Be("18");
    }

    [TestMethod]
    public void Translate_WithLessThanOrEqualExpression_ShouldCreateFilterCondition()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = e => e.Age <= 65;

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterCondition>();
        var condition = (FilterCondition)result;
        condition.Path.Should().Be("Age");
        condition.Operator.Should().Be(FilterOperation.LessOrEqual);
        condition.Value.Should().Be("65");
    }

    #endregion

    #region String Method Tests

    [TestMethod]
    public void Translate_WithStringContainsExpression_ShouldCreateFilterCondition()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = e => e.Name.Contains("test");

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterCondition>();
        var condition = (FilterCondition)result;
        condition.Path.Should().Be("Name");
        condition.Operator.Should().Be(FilterOperation.Contains);
        condition.Value.Should().Be("test");
    }

    [TestMethod]
    public void Translate_WithStringStartsWithExpression_ShouldCreateFilterCondition()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = e => e.Email.StartsWith("admin");

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterCondition>();
        var condition = (FilterCondition)result;
        condition.Path.Should().Be("Email");
        condition.Operator.Should().Be(FilterOperation.StartsWith);
        condition.Value.Should().Be("admin");
    }

    [TestMethod]
    public void Translate_WithStringEndsWithExpression_ShouldCreateFilterCondition()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = e => e.Email.EndsWith(".com");

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterCondition>();
        var condition = (FilterCondition)result;
        condition.Path.Should().Be("Email");
        condition.Operator.Should().Be(FilterOperation.EndsWith);
        condition.Value.Should().Be(".com");
    }

    #endregion

    #region Logical Operator Tests

    [TestMethod]
    public void Translate_WithAndAlsoExpression_ShouldCreateFilterGroup()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = e => e.Age > 18 && e.IsActive;

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterGroup>();
        var group = (FilterGroup)result;
        group.Combination.Should().Be(FilterCombination.And);
        group.Children.Should().HaveCount(2);
    }

    [TestMethod]
    public void Translate_WithOrElseExpression_ShouldCreateFilterGroup()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = e => e.Age < 18 || e.Age > 65;

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterGroup>();
        var group = (FilterGroup)result;
        group.Combination.Should().Be(FilterCombination.Or);
        group.Children.Should().HaveCount(2);
    }

    [TestMethod]
    public void Translate_WithNotExpression_ShouldNegateOperator()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = e => !(e.Age > 18);

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterCondition>();
        var condition = (FilterCondition)result;
        condition.Path.Should().Be("Age");
        condition.Operator.Should().Be(FilterOperation.LessOrEqual); // Negation of >
        condition.Value.Should().Be("18");
    }

    #endregion

    #region Collection Method Tests - Any()

    [TestMethod]
    public void Translate_WithAnyWithoutPredicate_ShouldCreateCollectionCondition()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = e => e.Tags.Any();

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterCollectionCondition>();
        var condition = (FilterCollectionCondition)result;
        condition.Path.Should().Be("Tags");
        condition.Operator.Should().Be(FilterOperation.HasElements);
        condition.Predicate.Should().BeNull();
    }

    [TestMethod]
    public void Translate_WithAnyWithSimplePredicate_ShouldCreateCollectionCondition()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = e => e.Children.Any(c => c.Value > 10);

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterCollectionCondition>();
        var condition = (FilterCollectionCondition)result;
        condition.Path.Should().Be("Children");
        condition.Operator.Should().Be(FilterOperation.Any);
        condition.Predicate.Should().NotBeNull();
        condition.Predicate.Should().BeOfType<FilterCondition>();

        var predicateCondition = (FilterCondition)condition.Predicate!;
        predicateCondition.Path.Should().Be("Value");
        predicateCondition.Operator.Should().Be(FilterOperation.GreaterThan);
        predicateCondition.Value.Should().Be("10");
    }

    [TestMethod]
    public void Translate_WithAnyWithStringPredicate_ShouldCreateCollectionCondition()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = e => e.Children.Any(c => c.Name.Contains("test"));

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterCollectionCondition>();
        var condition = (FilterCollectionCondition)result;
        condition.Path.Should().Be("Children");
        condition.Operator.Should().Be(FilterOperation.Any);
        condition.Predicate.Should().NotBeNull();
        condition.Predicate.Should().BeOfType<FilterCondition>();

        var predicateCondition = (FilterCondition)condition.Predicate!;
        predicateCondition.Path.Should().Be("Name");
        predicateCondition.Operator.Should().Be(FilterOperation.Contains);
        predicateCondition.Value.Should().Be("test");
    }

    [TestMethod]
    public void Translate_WithAnyWithComplexPredicate_ShouldCreateCollectionCondition()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = e => e.Children.Any(c => c.Value > 5 && c.IsActive);

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterCollectionCondition>();
        var condition = (FilterCollectionCondition)result;
        condition.Path.Should().Be("Children");
        condition.Operator.Should().Be(FilterOperation.Any);
        condition.Predicate.Should().NotBeNull();
        condition.Predicate.Should().BeOfType<FilterGroup>();

        var predicateGroup = (FilterGroup)condition.Predicate!;
        predicateGroup.Combination.Should().Be(FilterCombination.And);
        predicateGroup.Children.Should().HaveCount(2);
    }

    #endregion

    #region Collection Method Tests - All()

    [TestMethod]
    public void Translate_WithAllWithPredicate_ShouldCreateCollectionCondition()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = e => e.Children.All(c => c.Value > 0);

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterCollectionCondition>();
        var condition = (FilterCollectionCondition)result;
        condition.Path.Should().Be("Children");
        condition.Operator.Should().Be(FilterOperation.All);
        condition.Predicate.Should().NotBeNull();
        condition.Predicate.Should().BeOfType<FilterCondition>();

        var predicateCondition = (FilterCondition)condition.Predicate!;
        predicateCondition.Path.Should().Be("Value");
        predicateCondition.Operator.Should().Be(FilterOperation.GreaterThan);
        predicateCondition.Value.Should().Be("0");
    }

    [TestMethod]
    public void Translate_WithAllWithComplexPredicate_ShouldCreateCollectionCondition()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = e => e.Children.All(c => c.IsActive && c.Value >= 0);

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterCollectionCondition>();
        var condition = (FilterCollectionCondition)result;
        condition.Path.Should().Be("Children");
        condition.Operator.Should().Be(FilterOperation.All);
        condition.Predicate.Should().NotBeNull();
        condition.Predicate.Should().BeOfType<FilterGroup>();
    }

    #endregion

    #region Collection Method Tests - Contains()

    [TestMethod]
    public void Translate_WithEnumerableContains_ShouldCreateFilterCondition()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = e => e.Tags.Contains("important");

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterCondition>();
        var condition = (FilterCondition)result;
        condition.Path.Should().Be("Tags");
        condition.Operator.Should().Be(FilterOperation.Contains);
        condition.Value.Should().Be("important");
    }

    #endregion

    #region Complex Composition Tests

    [TestMethod]
    public void Translate_WithCombinedAnyAndComparison_ShouldCreateFilterGroup()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = e => e.Age > 18 && e.Children.Any(c => c.IsActive);

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterGroup>();
        var group = (FilterGroup)result;
        group.Combination.Should().Be(FilterCombination.And);
        group.Children.Should().HaveCount(2);
        group.Children[0].Should().BeOfType<FilterCondition>();
        group.Children[1].Should().BeOfType<FilterCollectionCondition>();
    }

    [TestMethod]
    public void Translate_WithMultipleAnyConditions_ShouldCreateFilterGroup()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = e =>
            e.Children.Any(c => c.Value > 10) || e.Children.Any(c => c.Name.StartsWith("A"));

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterGroup>();
        var group = (FilterGroup)result;
        group.Combination.Should().Be(FilterCombination.Or);
        group.Children.Should().HaveCount(2);
        group.Children.Should().AllBeOfType<FilterCollectionCondition>();
    }

    [TestMethod]
    public void Translate_WithNestedCollectionOperations_ShouldCreateComplexStructure()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = e =>
            e.Name.Contains("test") &&
            e.Tags.Any() &&
            e.Children.All(c => c.Value >= 0);

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterGroup>();
        var group = (FilterGroup)result;
        group.Combination.Should().Be(FilterCombination.And);
        group.Children.Should().HaveCount(3);
        group.Children[0].Should().BeOfType<FilterCondition>(); // Name.Contains
        group.Children[1].Should().BeOfType<FilterCollectionCondition>(); // Tags.Any()
        group.Children[2].Should().BeOfType<FilterCollectionCondition>(); // Children.All
    }

    #endregion

    #region Edge Cases and Error Handling

    [TestMethod]
    public void Translate_WithNullExpression_ShouldThrowException()
    {
        // Arrange
        Expression<Func<TestEntity, bool>>? expression = null;

        // Act
        Action act = () => ExpressionToFilterNode.Translate(expression!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void Translate_WithUnsupportedExpression_ShouldThrowNotSupportedException()
    {
        // Arrange - using a method that's not supported
        Expression<Func<TestEntity, bool>> expression = e => e.Name.GetHashCode() > 0;

        // Act
        Action act = () => ExpressionToFilterNode.Translate(expression);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    #endregion

    #region Inverted Comparison Tests

    [TestMethod]
    public void Translate_WithInvertedComparison_ShouldNormalizeCorrectly()
    {
        // Arrange - constant on left side
        Expression<Func<TestEntity, bool>> expression = e => 18 < e.Age;

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterCondition>();
        var condition = (FilterCondition)result;
        condition.Path.Should().Be("Age");
        condition.Operator.Should().Be(FilterOperation.GreaterThan);
        condition.Value.Should().Be("18");
    }

    [TestMethod]
    public void Translate_WithInvertedEquality_ShouldNormalizeCorrectly()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expression = e => "test" == e.Name;

        // Act
        FilterNode result = ExpressionToFilterNode.Translate(expression);

        // Assert
        result.Should().BeOfType<FilterCondition>();
        var condition = (FilterCondition)result;
        condition.Path.Should().Be("Name");
        condition.Operator.Should().Be(FilterOperation.Equals);
        condition.Value.Should().Be("test");
    }

    #endregion
}
