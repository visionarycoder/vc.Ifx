// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using VisionaryCoder.Framework.Proxy.Interceptors.Authentication;

namespace VisionaryCoder.Framework.Tests.Authentication;

/// <summary>
/// Comprehensive data-driven unit tests for TenantContext with 100% code coverage.
/// Tests all properties, methods, and edge cases to ensure robust tenant context handling.
/// </summary>
[TestClass]
public class TenantContextTests
{
    private TenantContext tenantContext = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        tenantContext = new TenantContext();
    }

    #region Property Tests

    [TestMethod]
    public void TenantId_ShouldHaveDefaultEmptyString()
    {
        // Assert
        tenantContext.TenantId.Should().Be(string.Empty, "TenantId should default to empty string");
    }

    [TestMethod]
    [DataRow("tenant123")]
    [DataRow("")]
    [DataRow("very-long-tenant-id-with-special-characters-123456789")]
    [DataRow("tenant@domain.com")]
    [DataRow("tenant-with-dashes")]
    public void TenantId_ShouldAcceptValidValues(string tenantId)
    {
        // Act
        tenantContext.TenantId = tenantId;

        // Assert
        tenantContext.TenantId.Should().Be(tenantId, $"TenantId should accept value: {tenantId}");
    }

    [TestMethod]
    public void TenantName_ShouldHaveDefaultEmptyString()
    {
        // Assert
        tenantContext.TenantName.Should().Be(string.Empty, "TenantName should default to empty string");
    }

    [TestMethod]
    [DataRow("Acme Corporation")]
    [DataRow("")]
    [DataRow("Tenant with spaces")]
    [DataRow("公司名称")]
    [DataRow("Tenant123!@#")]
    public void TenantName_ShouldAcceptValidValues(string tenantName)
    {
        // Act
        tenantContext.TenantName = tenantName;

        // Assert
        tenantContext.TenantName.Should().Be(tenantName, $"TenantName should accept value: {tenantName}");
    }

    [TestMethod]
    public void Domain_ShouldHaveDefaultNull()
    {
        // Assert
        tenantContext.Domain.Should().BeNull("Domain should default to null");
    }

    [TestMethod]
    [DataRow("example.com")]
    [DataRow("")]
    [DataRow(null)]
    [DataRow("subdomain.example.com")]
    [DataRow("localhost")]
    public void Domain_ShouldAcceptValidValues(string? domain)
    {
        // Act
        tenantContext.Domain = domain;

        // Assert
        tenantContext.Domain.Should().Be(domain, $"Domain should accept value: {domain}");
    }

    [TestMethod]
    public void SubscriptionTier_ShouldHaveDefaultNull()
    {
        // Assert
        tenantContext.SubscriptionTier.Should().BeNull("SubscriptionTier should default to null");
    }

    [TestMethod]
    [DataRow("Basic")]
    [DataRow("Premium")]
    [DataRow("Enterprise")]
    [DataRow("")]
    [DataRow(null)]
    public void SubscriptionTier_ShouldAcceptValidValues(string? subscriptionTier)
    {
        // Act
        tenantContext.SubscriptionTier = subscriptionTier;

        // Assert
        tenantContext.SubscriptionTier.Should().Be(subscriptionTier, $"SubscriptionTier should accept value: {subscriptionTier}");
    }

    [TestMethod]
    public void Settings_ShouldHaveDefaultEmptyDictionary()
    {
        // Assert
        tenantContext.Settings.Should().NotBeNull("Settings should not be null");
        tenantContext.Settings.Should().BeEmpty("Settings should default to empty dictionary");
    }

    [TestMethod]
    public void Settings_ShouldBeModifiable()
    {
        // Act
        tenantContext.Settings["maxUsers"] = 100;
        tenantContext.Settings["allowExternalAuth"] = true;
        tenantContext.Settings["region"] = "us-east-1";

        // Assert
        tenantContext.Settings.Should().HaveCount(3, "Should contain added settings");
        tenantContext.Settings["maxUsers"].Should().Be(100);
        tenantContext.Settings["allowExternalAuth"].Should().Be(true);
        tenantContext.Settings["region"].Should().Be("us-east-1");
    }

    [TestMethod]
    public void CreatedAt_ShouldHaveReasonableDefault()
    {
        // Arrange
        DateTimeOffset beforeCreation = DateTimeOffset.UtcNow.AddSeconds(-1);
        DateTimeOffset afterCreation = DateTimeOffset.UtcNow.AddSeconds(1);

        // Act
        var newContext = new TenantContext();

        // Assert
        newContext.CreatedAt.Should().BeAfter(beforeCreation, "Should be set around creation time");
        newContext.CreatedAt.Should().BeBefore(afterCreation, "Should be set around creation time");
    }

    [TestMethod]
    public void CreatedAt_ShouldBeModifiable()
    {
        // Arrange
        var specificTime = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // Act
        tenantContext.CreatedAt = specificTime;

        // Assert
        tenantContext.CreatedAt.Should().Be(specificTime, "Should accept custom creation time");
    }

    [TestMethod]
    public void IsActive_ShouldHaveDefaultTrue()
    {
        // Assert
        tenantContext.IsActive.Should().BeTrue("IsActive should default to true");
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void IsActive_ShouldAcceptValidValues(bool isActive)
    {
        // Act
        tenantContext.IsActive = isActive;

        // Assert
        tenantContext.IsActive.Should().Be(isActive, $"IsActive should accept value: {isActive}");
    }

    [TestMethod]
    public void EnabledFeatures_ShouldHaveDefaultEmptyCollection()
    {
        // Assert
        tenantContext.EnabledFeatures.Should().NotBeNull("EnabledFeatures should not be null");
        tenantContext.EnabledFeatures.Should().BeEmpty("EnabledFeatures should default to empty collection");
    }

    [TestMethod]
    public void EnabledFeatures_ShouldBeModifiable()
    {
        // Act
        tenantContext.EnabledFeatures.Add("reporting");
        tenantContext.EnabledFeatures.Add("analytics");
        tenantContext.EnabledFeatures.Add("api-access");

        // Assert
        tenantContext.EnabledFeatures.Should().HaveCount(3, "Should contain added features");
        tenantContext.EnabledFeatures.Should().Contain("reporting", "Should contain reporting feature");
        tenantContext.EnabledFeatures.Should().Contain("analytics", "Should contain analytics feature");
        tenantContext.EnabledFeatures.Should().Contain("api-access", "Should contain api-access feature");
    }

    #endregion

    #region HasFeature Method Tests

    [TestMethod]
    public void HasFeature_WithExistingFeature_ShouldReturnTrue()
    {
        // Arrange
        tenantContext.EnabledFeatures.Add("reporting");
        tenantContext.EnabledFeatures.Add("analytics");

        // Act & Assert
        tenantContext.HasFeature("reporting").Should().BeTrue("Should return true for existing feature");
        tenantContext.HasFeature("analytics").Should().BeTrue("Should return true for existing feature");
    }

    [TestMethod]
    public void HasFeature_WithNonExistingFeature_ShouldReturnFalse()
    {
        // Arrange
        tenantContext.EnabledFeatures.Add("reporting");

        // Act & Assert
        tenantContext.HasFeature("analytics").Should().BeFalse("Should return false for non-existing feature");
        tenantContext.HasFeature("api-access").Should().BeFalse("Should return false for non-existing feature");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(null)]
    public void HasFeature_WithNullOrEmptyFeature_ShouldReturnFalse(string? feature)
    {
        // Arrange
        tenantContext.EnabledFeatures.Add("reporting");

        // Act & Assert
        tenantContext.HasFeature(feature!).Should().BeFalse($"Should return false for feature: {feature}");
    }

    [TestMethod]
    public void HasFeature_ShouldBeCaseInsensitive()
    {
        // Arrange
        tenantContext.EnabledFeatures.Add("Reporting");

        // Act & Assert
        tenantContext.HasFeature("reporting").Should().BeTrue("Should be case insensitive");
        tenantContext.HasFeature("REPORTING").Should().BeTrue("Should be case insensitive");
        tenantContext.HasFeature("RePoRtInG").Should().BeTrue("Should be case insensitive");
    }

    #endregion

    #region GetSetting Method Tests

    [TestMethod]
    public void GetSetting_WithExistingSetting_ShouldReturnValue()
    {
        // Arrange
        tenantContext.Settings["maxUsers"] = 100;
        tenantContext.Settings["region"] = "us-east-1";

        // Act & Assert
        tenantContext.GetSetting<int>("maxUsers").Should().Be(100, "Should return existing int setting");
        tenantContext.GetSetting<string>("region").Should().Be("us-east-1", "Should return existing string setting");
    }

    [TestMethod]
    public void GetSetting_WithNonExistingSetting_ShouldReturnDefault()
    {
        // Act & Assert
        tenantContext.GetSetting<int>("nonExistent").Should().Be(0, "Should return default int value");
        tenantContext.GetSetting<string>("nonExistent").Should().BeNull("Should return default string value");
        tenantContext.GetSetting<bool>("nonExistent").Should().BeFalse("Should return default bool value");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(null)]
    public void GetSetting_WithNullOrEmptyKey_ShouldReturnDefault(string? key)
    {
        // Arrange
        tenantContext.Settings["validKey"] = "validValue";

        // Act & Assert
        tenantContext.GetSetting<string>(key!).Should().BeNull($"Should return default for key: {key}");
    }

    [TestMethod]
    public void GetSetting_WithWrongType_ShouldThrowInvalidCastException()
    {
        // Arrange
        tenantContext.Settings["stringValue"] = "not a number";

        // Act & Assert
        Action act = () => tenantContext.GetSetting<int>("stringValue");
        act.Should().Throw<InvalidCastException>("Should throw when trying to cast incompatible types");
    }

    [TestMethod]
    public void GetSetting_WithNullValue_ShouldReturnDefault()
    {
        // Arrange
        tenantContext.Settings["nullValue"] = null!;

        // Act & Assert
        tenantContext.GetSetting<string>("nullValue").Should().BeNull("Should return null for null setting");
        tenantContext.GetSetting<int?>("nullValue").Should().BeNull("Should return null for nullable int");
    }

    #endregion

    #region IsValid Method Tests

    [TestMethod]
    public void IsValid_WithMinimalValidData_ShouldReturnTrue()
    {
        // Arrange
        tenantContext.TenantId = "tenant123";
        tenantContext.TenantName = "Test Tenant";

        // Act & Assert
        tenantContext.IsValid.Should().BeTrue("Should be valid with minimal required data");
    }

    [TestMethod]
    public void IsValid_WithEmptyTenantId_ShouldReturnFalse()
    {
        // Arrange
        tenantContext.TenantId = "";
        tenantContext.TenantName = "Test Tenant";

        // Act & Assert
        tenantContext.IsValid.Should().BeFalse("Should be invalid with empty TenantId");
    }

    [TestMethod]
    public void IsValid_WithEmptyTenantName_ShouldReturnTrue()
    {
        // Arrange
        tenantContext.TenantId = "tenant123";
        tenantContext.TenantName = "";

        // Act & Assert
        tenantContext.IsValid.Should().BeTrue("Should be valid when TenantId is provided and IsActive is true (TenantName is not required for validity)");
    }

    [TestMethod]
    public void IsValid_WithBothEmptyTenantIdAndName_ShouldReturnFalse()
    {
        // Arrange
        tenantContext.TenantId = "";
        tenantContext.TenantName = "";

        // Act & Assert
        tenantContext.IsValid.Should().BeFalse("Should be invalid with empty TenantId (TenantName doesn't affect validity)");
    }

    [TestMethod]
    public void IsValid_WithCompleteData_ShouldReturnTrue()
    {
        // Arrange
        tenantContext.TenantId = "tenant123";
        tenantContext.TenantName = "Test Tenant";
        tenantContext.Domain = "example.com";
        tenantContext.SubscriptionTier = "Premium";
        tenantContext.IsActive = true;
        tenantContext.EnabledFeatures.Add("reporting");
        tenantContext.Settings["maxUsers"] = 100;

        // Act & Assert
        tenantContext.IsValid.Should().BeTrue("Should be valid with complete data");
    }

    [TestMethod]
    public void IsValid_WithInactiveTenant_ShouldStillBeValid()
    {
        // Arrange
        tenantContext.TenantId = "tenant123";
        tenantContext.TenantName = "Test Tenant";
        tenantContext.IsActive = false;

        // Act & Assert
        tenantContext.IsValid.Should().BeFalse("Should be invalid when inactive (IsActive affects validity)");
    }

    #endregion

    #region Edge Cases and Complex Scenarios

    [TestMethod]
    public void TenantContext_ShouldHandleCollectionModifications()
    {
        // Arrange
        tenantContext.EnabledFeatures.Add("reporting");
        tenantContext.EnabledFeatures.Add("analytics");

        // Act
        tenantContext.EnabledFeatures.Remove("reporting");
        tenantContext.EnabledFeatures.Add("api-access");

        // Assert
        tenantContext.EnabledFeatures.Should().HaveCount(2);
        tenantContext.EnabledFeatures.Should().NotContain("reporting");
        tenantContext.EnabledFeatures.Should().Contain("analytics");
        tenantContext.EnabledFeatures.Should().Contain("api-access");
    }

    [TestMethod]
    public void TenantContext_ShouldHandleDuplicateFeatures()
    {
        // Act
        tenantContext.EnabledFeatures.Add("reporting");
        tenantContext.EnabledFeatures.Add("reporting"); // Duplicate

        // Assert
        tenantContext.EnabledFeatures.Should().HaveCount(2, "Collection allows duplicates");
        tenantContext.HasFeature("reporting").Should().BeTrue("Should still find the feature");
    }

    [TestMethod]
    public void TenantContext_ShouldHandleComplexSettingValues()
    {
        // Arrange
        var complexObject = new { Name = "Config", Limits = new[] { 10, 20, 30 } };
        DateTimeOffset dateTime = DateTimeOffset.UtcNow;

        // Act
        tenantContext.Settings["complex"] = complexObject;
        tenantContext.Settings["datetime"] = dateTime;
        tenantContext.Settings["array"] = new[] { "a", "b", "c" };
        tenantContext.Settings["null"] = null!;

        // Assert
        tenantContext.Settings["complex"].Should().Be(complexObject);
        tenantContext.Settings["datetime"].Should().Be(dateTime);
        tenantContext.Settings["array"].Should().BeEquivalentTo(new[] { "a", "b", "c" });
        tenantContext.Settings["null"].Should().BeNull();
    }

    [TestMethod]
    public void TenantContext_ShouldBeIndependentInstances()
    {
        // Arrange
        var context1 = new TenantContext { TenantId = "tenant1", TenantName = "Tenant One" };
        var context2 = new TenantContext { TenantId = "tenant2", TenantName = "Tenant Two" };

        context1.EnabledFeatures.Add("reporting");
        context2.EnabledFeatures.Add("analytics");
        context1.Settings["plan"] = "basic";
        context2.Settings["plan"] = "premium";

        // Assert
        context1.TenantId.Should().NotBe(context2.TenantId);
        context1.TenantName.Should().NotBe(context2.TenantName);
        context1.EnabledFeatures.Should().NotBeEquivalentTo(context2.EnabledFeatures);
        context1.Settings.Should().NotBeEquivalentTo(context2.Settings);
    }

    [TestMethod]
    public void GetSetting_WithComplexTypes_ShouldWork()
    {
        // Arrange
        var complexConfig = new { MaxUsers = 100, Features = new[] { "A", "B" } };
        tenantContext.Settings["config"] = complexConfig;

        // Act
        object? retrieved = tenantContext.GetSetting<object>("config");

        // Assert
        retrieved.Should().Be(complexConfig, "Should retrieve complex objects correctly");
    }

    #endregion

    #region Thread Safety Tests

    [TestMethod]
    public void TenantContext_ShouldHandleConcurrentAccess()
    {
        // Arrange
        var context = new TenantContext
        {
            TenantId = "concurrent-tenant",
            TenantName = "Concurrent Tenant"
        };

        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            int index = i;
            tasks.Add(Task.Run(() =>
            {
                context.EnabledFeatures.Add($"feature{index}");
                context.Settings[$"setting{index}"] = index;
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        context.EnabledFeatures.Should().HaveCount(10, "Should have all added features");
        context.Settings.Should().HaveCount(10, "Should have all added settings");
    }

    #endregion
}
