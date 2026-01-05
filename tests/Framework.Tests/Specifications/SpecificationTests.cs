using FluentAssertions;
using VisionaryCoder.Framework.Patterns.Specifications;

namespace VisionaryCoder.Framework.Tests.Specifications;

/// <summary>
/// Unit tests for Specification pattern with 100% coverage.
/// </summary>
[TestClass]
public class SpecificationTests
{
    [TestMethod]
    public void Specification_ToExpression_ShouldReturnValidExpression()
    {
        // Arrange
        var spec = new TestSpecification(x => x > 10);

        // Act
        var expression = spec.ToExpression();

        // Assert
        expression.Should().NotBeNull();
        var func = expression.Compile();
        func(15).Should().BeTrue();
        func(5).Should().BeFalse();
    }

    [TestMethod]
    public void Specification_IsSatisfiedBy_ShouldEvaluateCorrectly()
    {
        // Arrange
        var spec = new TestSpecification(x => x > 10);

        // Act & Assert
        spec.IsSatisfiedBy(15).Should().BeTrue();
        spec.IsSatisfiedBy(5).Should().BeFalse();
        spec.IsSatisfiedBy(10).Should().BeFalse();
    }

    [TestMethod]
    public void Specification_And_ShouldCombineTwoSpecifications()
    {
        // Arrange
        var spec1 = new TestSpecification(x => x > 10);
        var spec2 = new TestSpecification(x => x < 20);

        // Act
        var combined = spec1.And(spec2);

        // Assert
        combined.IsSatisfiedBy(15).Should().BeTrue();
        combined.IsSatisfiedBy(5).Should().BeFalse();
        combined.IsSatisfiedBy(25).Should().BeFalse();
    }

    [TestMethod]
    public void Specification_Or_ShouldCombineTwoSpecifications()
    {
        // Arrange
        var spec1 = new TestSpecification(x => x < 10);
        var spec2 = new TestSpecification(x => x > 20);

        // Act
        var combined = spec1.Or(spec2);

        // Assert
        combined.IsSatisfiedBy(5).Should().BeTrue();
        combined.IsSatisfiedBy(25).Should().BeTrue();
        combined.IsSatisfiedBy(15).Should().BeFalse();
    }

    [TestMethod]
    public void Specification_Not_ShouldNegateSpecification()
    {
        // Arrange
        var spec = new TestSpecification(x => x > 10);

        // Act
        var negated = spec.Not();

        // Assert
        negated.IsSatisfiedBy(5).Should().BeTrue();
        negated.IsSatisfiedBy(15).Should().BeFalse();
    }

    [TestMethod]
    public void Specification_ComplexCombination_ShouldWorkCorrectly()
    {
        // Arrange
        var greaterThan10 = new TestSpecification(x => x > 10);
        var lessThan20 = new TestSpecification(x => x < 20);
        var notEqualTo15 = new TestSpecification(x => x != 15);

        // Act
        var combined = greaterThan10.And(lessThan20).And(notEqualTo15);

        // Assert
        combined.IsSatisfiedBy(12).Should().BeTrue();
        combined.IsSatisfiedBy(18).Should().BeTrue();
        combined.IsSatisfiedBy(15).Should().BeFalse();
        combined.IsSatisfiedBy(5).Should().BeFalse();
        combined.IsSatisfiedBy(25).Should().BeFalse();
    }

    [TestMethod]
    public void Specification_ImplicitConversion_ToExpression_ShouldWork()
    {
        // Arrange
        var spec = new TestSpecification(x => x > 10);

        // Act
        System.Linq.Expressions.Expression<Func<int, bool>> expression = spec;

        // Assert
        expression.Should().NotBeNull();
        var func = expression.Compile();
        func(15).Should().BeTrue();
        func(5).Should().BeFalse();
    }

    [TestMethod]
    public void Specification_WithReferenceType_ShouldWork()
    {
        // Arrange
        var spec = new PersonActiveSpecification();
        var activePerson = new Person { IsActive = true, IsDeleted = false };
        var inactivePerson = new Person { IsActive = false, IsDeleted = false };
        var deletedPerson = new Person { IsActive = true, IsDeleted = true };

        // Act & Assert
        spec.IsSatisfiedBy(activePerson).Should().BeTrue();
        spec.IsSatisfiedBy(inactivePerson).Should().BeFalse();
        spec.IsSatisfiedBy(deletedPerson).Should().BeFalse();
    }

    [TestMethod]
    public void Specification_WithComplexObject_AndOr_ShouldWork()
    {
        // Arrange
        var activeSpec = new PersonActiveSpecification();
        var premiumSpec = new PersonPremiumSpecification();

        var activePremium = new Person { IsActive = true, IsDeleted = false, IsPremium = true };
        var activeNonPremium = new Person { IsActive = true, IsDeleted = false, IsPremium = false };
        var inactivePremium = new Person { IsActive = false, IsDeleted = false, IsPremium = true };

        // Act
        var activePremiumSpec = activeSpec.And(premiumSpec);
        var activeOrPremiumSpec = activeSpec.Or(premiumSpec);

        // Assert - AND
        activePremiumSpec.IsSatisfiedBy(activePremium).Should().BeTrue();
        activePremiumSpec.IsSatisfiedBy(activeNonPremium).Should().BeFalse();
        activePremiumSpec.IsSatisfiedBy(inactivePremium).Should().BeFalse();

        // Assert - OR
        activeOrPremiumSpec.IsSatisfiedBy(activePremium).Should().BeTrue();
        activeOrPremiumSpec.IsSatisfiedBy(activeNonPremium).Should().BeTrue();
        activeOrPremiumSpec.IsSatisfiedBy(inactivePremium).Should().BeTrue();
    }

    [TestMethod]
    public void Specification_MultipleNot_ShouldNegateCorrectly()
    {
        // Arrange
        var spec = new TestSpecification(x => x > 10);

        // Act
        var doubleNegated = spec.Not().Not();

        // Assert
        doubleNegated.IsSatisfiedBy(15).Should().BeTrue();
        doubleNegated.IsSatisfiedBy(5).Should().BeFalse();
    }

    [TestMethod]
    public void Specification_ChainedOperations_ShouldMaintainCorrectness()
    {
        // Arrange
        var spec = new TestSpecification(x => x > 0)
            .And(new TestSpecification(x => x < 100))
            .Or(new TestSpecification(x => x == 200))
            .And(new TestSpecification(x => x != 50));

        // Act & Assert
        spec.IsSatisfiedBy(25).Should().BeTrue();   // 0 < 25 < 100 && 25 != 50
        spec.IsSatisfiedBy(200).Should().BeTrue();  // == 200
        spec.IsSatisfiedBy(50).Should().BeFalse();  // Would match first part but == 50
        spec.IsSatisfiedBy(-10).Should().BeFalse(); // < 0
        spec.IsSatisfiedBy(150).Should().BeFalse(); // > 100 and != 200
    }

    // Test specifications
    private class TestSpecification : Specification<int>
    {
        private readonly Func<int, bool> predicate;

        public TestSpecification(Func<int, bool> predicate)
        {
            this.predicate = predicate;
        }

        public override System.Linq.Expressions.Expression<Func<int, bool>> ToExpression()
        {
            return x => predicate(x);
        }
    }

    private class PersonActiveSpecification : Specification<Person>
    {
        public override System.Linq.Expressions.Expression<Func<Person, bool>> ToExpression()
        {
            return person => person.IsActive && !person.IsDeleted;
        }
    }

    private class PersonPremiumSpecification : Specification<Person>
    {
        public override System.Linq.Expressions.Expression<Func<Person, bool>> ToExpression()
        {
            return person => person.IsPremium;
        }
    }

    private class Person
    {
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsPremium { get; set; }
    }
}
