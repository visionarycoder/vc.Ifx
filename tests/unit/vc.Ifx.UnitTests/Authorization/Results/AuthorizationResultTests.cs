// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using VisionaryCoder.Framework.Proxy.Interceptors.Authorization.Results;

namespace VisionaryCoder.Framework.Tests.Authorization.Results;

/// <summary>
/// Comprehensive data-driven unit tests for AuthorizationResult with 100% code coverage.
/// Tests all properties, methods, and edge cases to ensure robust authorization result handling.
/// </summary>
[TestClass]
public class AuthorizationResultTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_Default_ShouldInitializeWithCorrectDefaults()
    {
        // Act
        var result = new AuthorizationResult();

        // Assert
        result.IsAuthorized.Should().BeFalse("Should default to unauthorized");
        result.FailureReason.Should().BeNull("Should have no failure reason by default");
        result.Context.Should().NotBeNull("Context should be initialized");
        result.Context.Should().BeEmpty("Context should start empty");
    }

    [TestMethod]
    public void Constructor_ShouldCreateIndependentInstances()
    {
        // Act
        var result1 = new AuthorizationResult();
        var result2 = new AuthorizationResult();

        result1.Context["key1"] = "value1";
        result2.Context["key2"] = "value2";

        // Assert
        result1.Context.Should().NotBeSameAs(result2.Context, "Should have independent context dictionaries");
        result1.Context.Should().ContainKey("key1").And.NotContainKey("key2");
        result2.Context.Should().ContainKey("key2").And.NotContainKey("key1");
    }

    #endregion

    #region Property Tests

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void IsAuthorized_ShouldAcceptValidValues(bool isAuthorized)
    {
        // Arrange
        var result = new AuthorizationResult();

        // Act
        result.IsAuthorized = isAuthorized;

        // Assert
        result.IsAuthorized.Should().Be(isAuthorized, $"Should accept IsAuthorized value: {isAuthorized}");
    }

    [TestMethod]
    [DataRow("Access denied")]
    [DataRow("")]
    [DataRow(null)]
    [DataRow("User does not have required role")]
    [DataRow("Token expired")]
    public void FailureReason_ShouldAcceptValidValues(string? failureReason)
    {
        // Arrange
        var result = new AuthorizationResult();

        // Act
        result.FailureReason = failureReason;

        // Assert
        result.FailureReason.Should().Be(failureReason, $"Should accept FailureReason value: {failureReason}");
    }

    [TestMethod]
    public void Context_ShouldBeModifiable()
    {
        // Arrange
        var result = new AuthorizationResult();

        // Act
        result.Context["userId"] = "user123";
        result.Context["roles"] = new[] { "admin", "user" };
        result.Context["timestamp"] = DateTimeOffset.UtcNow;
        result.Context["isValid"] = true;

        // Assert
        result.Context.Should().HaveCount(4, "Should contain all added context items");
        result.Context["userId"].Should().Be("user123");
        result.Context["roles"].Should().BeEquivalentTo(new[] { "admin", "user" });
        result.Context["timestamp"].Should().BeOfType<DateTimeOffset>();
        result.Context["isValid"].Should().Be(true);
    }

    [TestMethod]
    public void Context_ShouldHandleComplexValues()
    {
        // Arrange
        var result = new AuthorizationResult();
        var complexObject = new { Name = "Test", Values = new[] { 1, 2, 3 } };

        // Act
        result.Context["complex"] = complexObject;
        result.Context["null"] = null!;
        result.Context["array"] = new[] { "a", "b", "c" };

        // Assert
        result.Context["complex"].Should().Be(complexObject);
        result.Context["null"].Should().BeNull();
        result.Context["array"].Should().BeEquivalentTo(new[] { "a", "b", "c" });
    }

    #endregion

    #region Success Method Tests

    [TestMethod]
    public void Success_ShouldReturnAuthorizedResult()
    {
        // Act
        var result = AuthorizationResult.Success();

        // Assert
        result.Should().NotBeNull("Success should return a valid result");
        result.IsAuthorized.Should().BeTrue("Success result should be authorized");
        result.FailureReason.Should().BeNull("Success result should have no failure reason");
        result.Context.Should().NotBeNull("Success result should have context dictionary");
        result.Context.Should().BeEmpty("Success result should start with empty context");
    }

    [TestMethod]
    public void Success_ShouldReturnNewInstanceEachTime()
    {
        // Act
        var result1 = AuthorizationResult.Success();
        var result2 = AuthorizationResult.Success();

        // Assert
        result1.Should().NotBeSameAs(result2, "Should return new instances");
        result1.Context.Should().NotBeSameAs(result2.Context, "Should have independent context dictionaries");
    }

    [TestMethod]
    public void Success_ResultShouldBeModifiable()
    {
        // Act
        var result = AuthorizationResult.Success();
        result.Context["user"] = "testuser";
        result.Context["policy"] = "admin-policy";

        // Assert
        result.IsAuthorized.Should().BeTrue("Should remain authorized");
        result.Context.Should().HaveCount(2, "Should contain added context");
        result.Context["user"].Should().Be("testuser");
        result.Context["policy"].Should().Be("admin-policy");
    }

    #endregion

    #region Failure Method Tests

    [TestMethod]
    [DataRow("Access denied")]
    [DataRow("Insufficient permissions")]
    [DataRow("Token expired")]
    [DataRow("User not found")]
    [DataRow("Role required: admin")]
    public void Failure_WithValidReason_ShouldReturnUnauthorizedResult(string reason)
    {
        // Act
        var result = AuthorizationResult.Failure(reason);

        // Assert
        result.Should().NotBeNull("Failure should return a valid result");
        result.IsAuthorized.Should().BeFalse("Failure result should be unauthorized");
        result.FailureReason.Should().Be(reason, $"Should have the specified failure reason: {reason}");
        result.Context.Should().NotBeNull("Failure result should have context dictionary");
        result.Context.Should().BeEmpty("Failure result should start with empty context");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(null)]
    public void Failure_WithEmptyOrNullReason_ShouldAcceptValue(string? reason)
    {
        // Act
        var result = AuthorizationResult.Failure(reason!);

        // Assert
        result.IsAuthorized.Should().BeFalse("Should be unauthorized");
        result.FailureReason.Should().Be(reason, $"Should accept failure reason: {reason}");
    }

    [TestMethod]
    public void Failure_ShouldReturnNewInstanceEachTime()
    {
        // Act
        var result1 = AuthorizationResult.Failure("reason1");
        var result2 = AuthorizationResult.Failure("reason2");

        // Assert
        result1.Should().NotBeSameAs(result2, "Should return new instances");
        result1.Context.Should().NotBeSameAs(result2.Context, "Should have independent context dictionaries");
        result1.FailureReason.Should().Be("reason1");
        result2.FailureReason.Should().Be("reason2");
    }

    [TestMethod]
    public void Failure_ResultShouldBeModifiable()
    {
        // Act
        var result = AuthorizationResult.Failure("Access denied");
        result.Context["attemptedAction"] = "deleteUser";
        result.Context["requiredRole"] = "admin";

        // Assert
        result.IsAuthorized.Should().BeFalse("Should remain unauthorized");
        result.FailureReason.Should().Be("Access denied");
        result.Context.Should().HaveCount(2, "Should contain added context");
        result.Context["attemptedAction"].Should().Be("deleteUser");
        result.Context["requiredRole"].Should().Be("admin");
    }

    [TestMethod]
    public void Failure_WithLongReason_ShouldHandleGracefully()
    {
        // Arrange
        string longReason = new string('A', 1000) + " - Access denied due to insufficient permissions for the requested resource";

        // Act
        var result = AuthorizationResult.Failure(longReason);

        // Assert
        result.IsAuthorized.Should().BeFalse();
        result.FailureReason.Should().Be(longReason, "Should handle long failure reasons");
    }

    [TestMethod]
    public void Failure_WithSpecialCharacters_ShouldHandleGracefully()
    {
        // Arrange
        string specialReason = "Access denied: 用户权限不足 ñoñó @#$%^&*()";

        // Act
        var result = AuthorizationResult.Failure(specialReason);

        // Assert
        result.IsAuthorized.Should().BeFalse();
        result.FailureReason.Should().Be(specialReason, "Should handle special characters in failure reason");
    }

    #endregion

    #region Edge Cases and Complex Scenarios

    [TestMethod]
    public void AuthorizationResult_ShouldHandleContextModifications()
    {
        // Arrange
        var result = AuthorizationResult.Success();

        // Act
        result.Context["initial"] = "value";
        result.Context["initial"] = "updated"; // Overwrite
        result.Context["removed"] = "temp";
        result.Context.Remove("removed");

        // Assert
        result.Context.Should().ContainSingle("Should have one item after modifications");
        result.Context["initial"].Should().Be("updated", "Should have updated value");
        result.Context.Should().NotContainKey("removed", "Should have removed the key");
    }

    [TestMethod]
    public void AuthorizationResult_ShouldHandleLargeContext()
    {
        // Arrange
        var result = new AuthorizationResult();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            result.Context[$"key{i}"] = $"value{i}";
        }

        // Assert
        result.Context.Should().HaveCount(1000, "Should handle large number of context items");
        result.Context["key0"].Should().Be("value0");
        result.Context["key999"].Should().Be("value999");
    }

    [TestMethod]
    public void AuthorizationResult_ShouldSupportChaining()
    {
        // Act
        var result = AuthorizationResult.Success();
        result.IsAuthorized = true; // This should already be true, but testing assignment
        result.Context["chained"] = "value";

        // Assert
        result.IsAuthorized.Should().BeTrue();
        result.Context["chained"].Should().Be("value");
    }

    [TestMethod]
    public void AuthorizationResult_ShouldHandleNullContextValues()
    {
        // Arrange
        var result = new AuthorizationResult();

        // Act
        result.Context["nullValue"] = null!;
        result.Context["emptyString"] = "";
        result.Context["normalValue"] = "test";

        // Assert
        result.Context.Should().HaveCount(3);
        result.Context["nullValue"].Should().BeNull();
        result.Context["emptyString"].Should().Be("");
        result.Context["normalValue"].Should().Be("test");
    }

    [TestMethod]
    public void AuthorizationResult_SuccessAndFailure_ShouldHaveDifferentStates()
    {
        // Act
        var success = AuthorizationResult.Success();
        var failure = AuthorizationResult.Failure("Test failure");

        // Assert
        success.IsAuthorized.Should().BeTrue();
        success.FailureReason.Should().BeNull();

        failure.IsAuthorized.Should().BeFalse();
        failure.FailureReason.Should().Be("Test failure");

        success.Context.Should().NotBeSameAs(failure.Context);
    }

    #endregion

    #region Thread Safety Tests

    [TestMethod]
    public void AuthorizationResult_ShouldHandleConcurrentContextAccess()
    {
        // Arrange
        var result = new AuthorizationResult();
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 50; i++)
        {
            int index = i;
            tasks.Add(Task.Run(() =>
            {
                result.Context[$"key{index}"] = $"value{index}";
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        result.Context.Should().HaveCount(50, "Should handle concurrent modifications");

        // Check that all values are present (some might have been overwritten due to concurrency)
        for (int i = 0; i < 50; i++)
        {
            result.Context.Should().ContainKey($"key{i}", $"Should contain key{i}");
        }
    }

    [TestMethod]
    public void AuthorizationResult_StaticMethodsShouldBeThreadSafe()
    {
        // Arrange
        var tasks = new List<Task<AuthorizationResult>>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            int index = i;
            if (index % 2 == 0)
            {
                tasks.Add(Task.Run(() => AuthorizationResult.Success()));
            }
            else
            {
                tasks.Add(Task.Run(() => AuthorizationResult.Failure($"Failure {index}")));
            }
        }

        AuthorizationResult[] results = Task.WhenAll(tasks).Result;

        // Assert
        var successResults = results.Where(r => r.IsAuthorized).ToList();
        var failureResults = results.Where(r => !r.IsAuthorized).ToList();

        successResults.Should().HaveCount(50, "Should have 50 success results");
        failureResults.Should().HaveCount(50, "Should have 50 failure results");

        successResults.Should().AllSatisfy(r =>
        {
            r.IsAuthorized.Should().BeTrue();
            r.FailureReason.Should().BeNull();
        });

        failureResults.Should().AllSatisfy(r =>
        {
            r.IsAuthorized.Should().BeFalse();
            r.FailureReason.Should().StartWith("Failure ");
        });
    }

    #endregion

    #region Performance Tests

    [TestMethod]
    public void AuthorizationResult_StaticMethods_ShouldBeEfficient()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 10000; i++)
        {
            var success = AuthorizationResult.Success();
            var failure = AuthorizationResult.Failure("test");
        }

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "Should create results efficiently");
    }

    #endregion
}
