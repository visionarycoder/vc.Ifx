using VisionaryCoder.Framework.Proxy.Interceptors.Security;

namespace VisionaryCoder.Framework.Tests.Proxy.Interceptors.Security;

[TestClass]
public class AuthorizationResultTests
{
    [TestMethod]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Act
        var result = new AuthorizationResult();

        // Assert
        result.IsAuthorized.Should().BeFalse();
        result.FailureReason.Should().BeNull();
        result.Context.Should().NotBeNull();
        result.Context.Should().BeEmpty();
    }

    [TestMethod]
    public void Success_ShouldCreateAuthorizedResult()
    {
        // Act
        var result = AuthorizationResult.Success();

        // Assert
        result.IsAuthorized.Should().BeTrue();
        result.FailureReason.Should().BeNull();
        result.Context.Should().NotBeNull();
    }

    [TestMethod]
    public void Failure_WithReason_ShouldCreateUnauthorizedResult()
    {
        // Arrange
        string reason = "Insufficient permissions";

        // Act
        var result = AuthorizationResult.Failure(reason);

        // Assert
        result.IsAuthorized.Should().BeFalse();
        result.FailureReason.Should().Be(reason);
        result.Context.Should().NotBeNull();
    }

    [TestMethod]
    [DataRow("Access denied")]
    [DataRow("Token expired")]
    [DataRow("Invalid credentials")]
    [DataRow("")]
    public void Failure_WithVariousReasons_ShouldStoreReason(string reason)
    {
        // Act
        var result = AuthorizationResult.Failure(reason);

        // Assert
        result.FailureReason.Should().Be(reason);
        result.IsAuthorized.Should().BeFalse();
    }

    [TestMethod]
    public void IsAuthorized_ShouldBeSettable()
    {
        // Arrange
        var result = new AuthorizationResult();

        // Act
        result.IsAuthorized = true;

        // Assert
        result.IsAuthorized.Should().BeTrue();
    }

    [TestMethod]
    public void FailureReason_ShouldBeSettable()
    {
        // Arrange
        var result = new AuthorizationResult();

        // Act
        result.FailureReason = "Custom reason";

        // Assert
        result.FailureReason.Should().Be("Custom reason");
    }

    [TestMethod]
    public void Context_ShouldBeSettable()
    {
        // Arrange
        var result = new AuthorizationResult();
        var context = new Dictionary<string, object>
        {
            { "UserId", "123" },
            { "Role", "Admin" }
        };

        // Act
        result.Context = context;

        // Assert
        result.Context.Should().BeSameAs(context);
        result.Context.Should().HaveCount(2);
    }

    [TestMethod]
    public void Context_ShouldSupportAddingItems()
    {
        // Arrange
        var result = AuthorizationResult.Success();

        // Act
        result.Context.Add("Key1", "Value1");
        result.Context.Add("Key2", 42);

        // Assert
        result.Context.Should().HaveCount(2);
        result.Context["Key1"].Should().Be("Value1");
        result.Context["Key2"].Should().Be(42);
    }

    [TestMethod]
    public void Success_CalledMultipleTimes_ShouldReturnIndependentInstances()
    {
        // Act
        var result1 = AuthorizationResult.Success();
        var result2 = AuthorizationResult.Success();

        result1.Context.Add("Key1", "Value1");

        // Assert
        result1.Should().NotBeSameAs(result2);
        result1.Context.Should().HaveCount(1);
        result2.Context.Should().BeEmpty();
    }

    [TestMethod]
    public void Failure_CalledMultipleTimes_ShouldReturnIndependentInstances()
    {
        // Act
        var result1 = AuthorizationResult.Failure("Reason1");
        var result2 = AuthorizationResult.Failure("Reason2");

        result1.Context.Add("Key1", "Value1");

        // Assert
        result1.Should().NotBeSameAs(result2);
        result1.FailureReason.Should().Be("Reason1");
        result2.FailureReason.Should().Be("Reason2");
        result1.Context.Should().HaveCount(1);
        result2.Context.Should().BeEmpty();
    }

    [TestMethod]
    public void Context_WithComplexObjects_ShouldStoreCorrectly()
    {
        // Arrange
        var result = new AuthorizationResult();
        var complexObject = new { Name = "Test", Value = 123 };

        // Act
        result.Context.Add("ComplexKey", complexObject);

        // Assert
        result.Context["ComplexKey"].Should().Be(complexObject);
    }

    [TestMethod]
    public void Failure_WithNullReason_ShouldAllowNull()
    {
        // Act
        var result = AuthorizationResult.Failure(null!);

        // Assert
        result.IsAuthorized.Should().BeFalse();
        result.FailureReason.Should().BeNull();
    }

    [TestMethod]
    public void Failure_WithVeryLongReason_ShouldStoreCompletely()
    {
        // Arrange
        string longReason = new('A', 10000);

        // Act
        var result = AuthorizationResult.Failure(longReason);

        // Assert
        result.FailureReason.Should().HaveLength(10000);
        result.FailureReason.Should().Be(longReason);
    }

    [TestMethod]
    public void Failure_WithUnicodeReason_ShouldPreserveCharacters()
    {
        // Arrange
        string unicodeReason = "授权失败 🔒 Access denied";

        // Act
        var result = AuthorizationResult.Failure(unicodeReason);

        // Assert
        result.FailureReason.Should().Be(unicodeReason);
    }
}
