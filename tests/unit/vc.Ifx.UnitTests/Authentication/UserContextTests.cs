// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using VisionaryCoder.Framework.Proxy.Interceptors.Authentication;

namespace VisionaryCoder.Framework.Tests.Authentication;

/// <summary>
/// Comprehensive data-driven unit tests for UserContext with 100% code coverage.
/// Tests all properties, methods, and edge cases to ensure robust user context handling.
/// </summary>
[TestClass]
public class UserContextTests
{
    private UserContext userContext = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        userContext = new UserContext();
    }

    #region Property Tests

    [TestMethod]
    public void UserId_ShouldHaveDefaultEmptyString()
    {
        // Assert
        userContext.UserId.Should().Be(string.Empty, "UserId should default to empty string");
    }

    [TestMethod]
    [DataRow("user123")]
    [DataRow("")]
    [DataRow("very-long-user-id-with-special-characters-123456789")]
    [DataRow("user@domain.com")]
    public void UserId_ShouldAcceptValidValues(string userId)
    {
        // Act
        userContext.UserId = userId;

        // Assert
        userContext.UserId.Should().Be(userId, $"UserId should accept value: {userId}");
    }

    [TestMethod]
    public void UserName_ShouldHaveDefaultEmptyString()
    {
        // Assert
        userContext.UserName.Should().Be(string.Empty, "UserName should default to empty string");
    }

    [TestMethod]
    [DataRow("john.doe")]
    [DataRow("")]
    [DataRow("user with spaces")]
    [DataRow("用户名")]
    [DataRow("user123!@#")]
    public void UserName_ShouldAcceptValidValues(string userName)
    {
        // Act
        userContext.UserName = userName;

        // Assert
        userContext.UserName.Should().Be(userName, $"UserName should accept value: {userName}");
    }

    [TestMethod]
    public void Email_ShouldHaveDefaultNull()
    {
        // Assert
        userContext.Email.Should().BeNull("Email should default to null");
    }

    [TestMethod]
    [DataRow("user@example.com")]
    [DataRow("")]
    [DataRow(null)]
    [DataRow("user.name+tag@domain.co.uk")]
    [DataRow("invalid-email-format")]
    public void Email_ShouldAcceptValidValues(string? email)
    {
        // Act
        userContext.Email = email;

        // Assert
        userContext.Email.Should().Be(email, $"Email should accept value: {email}");
    }

    [TestMethod]
    public void Roles_ShouldHaveDefaultEmptyCollection()
    {
        // Assert
        userContext.Roles.Should().NotBeNull("Roles should not be null");
        userContext.Roles.Should().BeEmpty("Roles should default to empty collection");
    }

    [TestMethod]
    public void Roles_ShouldBeModifiable()
    {
        // Act
        userContext.Roles.Add("Admin");
        userContext.Roles.Add("User");

        // Assert
        userContext.Roles.Should().HaveCount(2, "Should contain added roles");
        userContext.Roles.Should().Contain("Admin", "Should contain Admin role");
        userContext.Roles.Should().Contain("User", "Should contain User role");
    }

    [TestMethod]
    public void Permissions_ShouldHaveDefaultEmptyCollection()
    {
        // Assert
        userContext.Permissions.Should().NotBeNull("Permissions should not be null");
        userContext.Permissions.Should().BeEmpty("Permissions should default to empty collection");
    }

    [TestMethod]
    public void Permissions_ShouldBeModifiable()
    {
        // Act
        userContext.Permissions.Add("read:users");
        userContext.Permissions.Add("write:users");

        // Assert
        userContext.Permissions.Should().HaveCount(2, "Should contain added permissions");
        userContext.Permissions.Should().Contain("read:users", "Should contain read permission");
        userContext.Permissions.Should().Contain("write:users", "Should contain write permission");
    }

    [TestMethod]
    public void Claims_ShouldHaveDefaultEmptyDictionary()
    {
        // Assert
        userContext.Claims.Should().NotBeNull("Claims should not be null");
        userContext.Claims.Should().BeEmpty("Claims should default to empty dictionary");
    }

    [TestMethod]
    public void Claims_ShouldBeModifiable()
    {
        // Act
        userContext.Claims["department"] = "Engineering";
        userContext.Claims["level"] = 5;
        userContext.Claims["isActive"] = true;

        // Assert
        userContext.Claims.Should().HaveCount(3, "Should contain added claims");
        userContext.Claims["department"].Should().Be("Engineering");
        userContext.Claims["level"].Should().Be(5);
        userContext.Claims["isActive"].Should().Be(true);
    }

    [TestMethod]
    public void AuthenticatedAt_ShouldHaveReasonableDefault()
    {
        // Arrange
        DateTimeOffset beforeCreation = DateTimeOffset.UtcNow.AddSeconds(-1);
        DateTimeOffset afterCreation = DateTimeOffset.UtcNow.AddSeconds(1);

        // Act
        var newContext = new UserContext();

        // Assert
        newContext.AuthenticatedAt.Should().BeAfter(beforeCreation, "Should be set around creation time");
        newContext.AuthenticatedAt.Should().BeBefore(afterCreation, "Should be set around creation time");
    }

    [TestMethod]
    public void AuthenticatedAt_ShouldBeModifiable()
    {
        // Arrange
        var specificTime = new DateTimeOffset(2023, 10, 15, 14, 30, 0, TimeSpan.Zero);

        // Act
        userContext.AuthenticatedAt = specificTime;

        // Assert
        userContext.AuthenticatedAt.Should().Be(specificTime, "Should accept custom authentication time");
    }

    #endregion

    #region HasRole Method Tests

    [TestMethod]
    public void HasRole_WithExistingRole_ShouldReturnTrue()
    {
        // Arrange
        userContext.Roles.Add("Admin");
        userContext.Roles.Add("User");

        // Act & Assert
        userContext.HasRole("Admin").Should().BeTrue("Should return true for existing role");
        userContext.HasRole("User").Should().BeTrue("Should return true for existing role");
    }

    [TestMethod]
    public void HasRole_WithNonExistingRole_ShouldReturnFalse()
    {
        // Arrange
        userContext.Roles.Add("User");

        // Act & Assert
        userContext.HasRole("Admin").Should().BeFalse("Should return false for non-existing role");
        userContext.HasRole("SuperUser").Should().BeFalse("Should return false for non-existing role");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(null)]
    public void HasRole_WithNullOrEmptyRole_ShouldReturnFalse(string? role)
    {
        // Arrange
        userContext.Roles.Add("Admin");

        // Act & Assert
        userContext.HasRole(role!).Should().BeFalse($"Should return false for role: {role}");
    }

    [TestMethod]
    public void HasRole_ShouldBeCaseInsensitive()
    {
        // Arrange
        userContext.Roles.Add("Admin");

        // Act & Assert
        userContext.HasRole("admin").Should().BeTrue("Should be case insensitive");
        userContext.HasRole("ADMIN").Should().BeTrue("Should be case insensitive");
        userContext.HasRole("AdMiN").Should().BeTrue("Should be case insensitive");
    }

    #endregion

    #region HasPermission Method Tests

    [TestMethod]
    public void HasPermission_WithExistingPermission_ShouldReturnTrue()
    {
        // Arrange
        userContext.Permissions.Add("read:users");
        userContext.Permissions.Add("write:users");

        // Act & Assert
        userContext.HasPermission("read:users").Should().BeTrue("Should return true for existing permission");
        userContext.HasPermission("write:users").Should().BeTrue("Should return true for existing permission");
    }

    [TestMethod]
    public void HasPermission_WithNonExistingPermission_ShouldReturnFalse()
    {
        // Arrange
        userContext.Permissions.Add("read:users");

        // Act & Assert
        userContext.HasPermission("delete:users").Should().BeFalse("Should return false for non-existing permission");
        userContext.HasPermission("admin:all").Should().BeFalse("Should return false for non-existing permission");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(null)]
    public void HasPermission_WithNullOrEmptyPermission_ShouldReturnFalse(string? permission)
    {
        // Arrange
        userContext.Permissions.Add("read:users");

        // Act & Assert
        userContext.HasPermission(permission!).Should().BeFalse($"Should return false for permission: {permission}");
    }

    [TestMethod]
    public void HasPermission_ShouldBeCaseInsensitive()
    {
        // Arrange
        userContext.Permissions.Add("Read:Users");

        // Act & Assert
        userContext.HasPermission("read:users").Should().BeTrue("Should be case insensitive");
        userContext.HasPermission("READ:USERS").Should().BeTrue("Should be case insensitive");
        userContext.HasPermission("ReAd:UsErS").Should().BeTrue("Should be case insensitive");
    }

    #endregion

    #region IsValid Method Tests

    [TestMethod]
    public void IsValid_WithMinimalValidData_ShouldReturnTrue()
    {
        // Arrange
        userContext.UserId = "user123";
        userContext.UserName = "testuser";

        // Act & Assert
        userContext.IsValid.Should().BeTrue("Should be valid with minimal required data");
    }

    [TestMethod]
    public void IsValid_WithEmptyUserId_ShouldReturnFalse()
    {
        // Arrange
        userContext.UserId = "";
        userContext.UserName = "testuser";

        // Act & Assert
        userContext.IsValid.Should().BeFalse("Should be invalid with empty UserId");
    }

    [TestMethod]
    public void IsValid_WithEmptyUserName_ShouldReturnTrue()
    {
        // Arrange
        userContext.UserId = "user123";
        userContext.UserName = "";

        // Act & Assert
        userContext.IsValid.Should().BeTrue("Should be valid when UserId is provided (UserName is not required for validity)");
    }

    [TestMethod]
    public void IsValid_WithBothEmptyUserIdAndUserName_ShouldReturnFalse()
    {
        // Arrange
        userContext.UserId = "";
        userContext.UserName = "";

        // Act & Assert
        userContext.IsValid.Should().BeFalse("Should be invalid with empty UserId (UserName doesn't affect validity)");
    }

    [TestMethod]
    public void IsValid_WithCompleteData_ShouldReturnTrue()
    {
        // Arrange
        userContext.UserId = "user123";
        userContext.UserName = "testuser";
        userContext.Email = "test@example.com";
        userContext.Roles.Add("User");
        userContext.Permissions.Add("read:data");
        userContext.Claims["department"] = "IT";

        // Act & Assert
        userContext.IsValid.Should().BeTrue("Should be valid with complete data");
    }

    #endregion

    #region Edge Cases and Complex Scenarios

    [TestMethod]
    public void UserContext_ShouldHandleCollectionModifications()
    {
        // Arrange
        userContext.Roles.Add("Admin");
        userContext.Permissions.Add("read:all");

        // Act
        userContext.Roles.Remove("Admin");
        userContext.Roles.Add("User");
        userContext.Permissions.Clear();
        userContext.Permissions.Add("read:limited");

        // Assert
        userContext.Roles.Should().ContainSingle("User");
        userContext.Roles.Should().NotContain("Admin");
        userContext.Permissions.Should().ContainSingle("read:limited");
        userContext.Permissions.Should().NotContain("read:all");
    }

    [TestMethod]
    public void UserContext_ShouldHandleDuplicateRoles()
    {
        // Act
        userContext.Roles.Add("Admin");
        userContext.Roles.Add("Admin"); // Duplicate

        // Assert
        userContext.Roles.Should().HaveCount(2, "Collection allows duplicates");
        userContext.HasRole("Admin").Should().BeTrue("Should still find the role");
    }

    [TestMethod]
    public void UserContext_ShouldHandleDuplicatePermissions()
    {
        // Act
        userContext.Permissions.Add("read:users");
        userContext.Permissions.Add("read:users"); // Duplicate

        // Assert
        userContext.Permissions.Should().HaveCount(2, "Collection allows duplicates");
        userContext.HasPermission("read:users").Should().BeTrue("Should still find the permission");
    }

    [TestMethod]
    public void UserContext_ShouldHandleComplexClaimValues()
    {
        // Arrange
        var complexObject = new { Name = "Test", Values = new[] { 1, 2, 3 } };
        DateTimeOffset dateTime = DateTimeOffset.UtcNow;

        // Act
        userContext.Claims["complex"] = complexObject;
        userContext.Claims["datetime"] = dateTime;
        userContext.Claims["array"] = new[] { "a", "b", "c" };
        userContext.Claims["null"] = null!;

        // Assert
        userContext.Claims["complex"].Should().Be(complexObject);
        userContext.Claims["datetime"].Should().Be(dateTime);
        userContext.Claims["array"].Should().BeEquivalentTo(new[] { "a", "b", "c" });
        userContext.Claims["null"].Should().BeNull();
    }

    [TestMethod]
    public void UserContext_ShouldBeIndependentInstances()
    {
        // Arrange
        var context1 = new UserContext { UserId = "user1", UserName = "User One" };
        var context2 = new UserContext { UserId = "user2", UserName = "User Two" };

        context1.Roles.Add("Admin");
        context2.Roles.Add("User");

        // Assert
        context1.UserId.Should().NotBe(context2.UserId);
        context1.UserName.Should().NotBe(context2.UserName);
        context1.Roles.Should().NotBeEquivalentTo(context2.Roles);
        context1.Roles.Should().Contain("Admin");
        context2.Roles.Should().Contain("User");
    }

    #endregion

    #region Thread Safety Tests

    [TestMethod]
    public void UserContext_ShouldHandleConcurrentAccess()
    {
        // Arrange
        var context = new UserContext
        {
            UserId = "concurrent-user",
            UserName = "Concurrent User"
        };

        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            int index = i;
            tasks.Add(Task.Run(() =>
            {
                context.Roles.Add($"Role{index}");
                context.Permissions.Add($"permission{index}");
                context.Claims[$"claim{index}"] = index;
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        context.Roles.Should().HaveCount(10, "Should have all added roles");
        context.Permissions.Should().HaveCount(10, "Should have all added permissions");
        context.Claims.Should().HaveCount(10, "Should have all added claims");
    }

    #endregion
}
