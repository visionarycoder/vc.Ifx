using VisionaryCoder.Framework.Proxy.Interceptors.Security;

namespace VisionaryCoder.Framework.Tests.Proxy.Interceptors.Security;

[TestClass]
public class TenantContextTests
{
    [TestMethod]
    public void Constructor_ShouldInitializeWithEmptyStrings()
    {
        // Act
        var context = new TenantContext();

        // Assert
        context.TenantId.Should().BeEmpty();
        context.TenantName.Should().BeEmpty();
    }

    [TestMethod]
    public void TenantId_ShouldBeSettable()
    {
        // Arrange
        var context = new TenantContext();

        // Act
        context.TenantId = "tenant-123";

        // Assert
        context.TenantId.Should().Be("tenant-123");
    }

    [TestMethod]
    public void TenantName_ShouldBeSettable()
    {
        // Arrange
        var context = new TenantContext();

        // Act
        context.TenantName = "Acme Corporation";

        // Assert
        context.TenantName.Should().Be("Acme Corporation");
    }

    [TestMethod]
    public void Properties_ShouldBeIndependentlySettable()
    {
        // Arrange & Act
        var context = new TenantContext
        {
            TenantId = "tenant-456",
            TenantName = "Test Tenant"
        };

        // Assert
        context.TenantId.Should().Be("tenant-456");
        context.TenantName.Should().Be("Test Tenant");
    }

    [TestMethod]
    [DataRow("tenant-001", "Company A")]
    [DataRow("tenant-002", "Company B")]
    [DataRow("", "")]
    [DataRow("guid-12345", "Unicode 公司")]
    public void Properties_WithVariousValues_ShouldStoreCorrectly(string tenantId, string tenantName)
    {
        // Act
        var context = new TenantContext
        {
            TenantId = tenantId,
            TenantName = tenantName
        };

        // Assert
        context.TenantId.Should().Be(tenantId);
        context.TenantName.Should().Be(tenantName);
    }

    [TestMethod]
    public void TenantId_WithGuid_ShouldStore()
    {
        // Arrange
        string guid = Guid.NewGuid().ToString();
        var context = new TenantContext();

        // Act
        context.TenantId = guid;

        // Assert
        context.TenantId.Should().Be(guid);
    }

    [TestMethod]
    public void TenantName_WithVeryLongName_ShouldStoreCompletely()
    {
        // Arrange
        string longName = new('A', 10000);
        var context = new TenantContext();

        // Act
        context.TenantName = longName;

        // Assert
        context.TenantName.Should().HaveLength(10000);
        context.TenantName.Should().Be(longName);
    }

    [TestMethod]
    public void TenantName_WithUnicode_ShouldPreserveCharacters()
    {
        // Arrange
        string unicodeName = "テスト会社 🏢 Test Company";
        var context = new TenantContext();

        // Act
        context.TenantName = unicodeName;

        // Assert
        context.TenantName.Should().Be(unicodeName);
    }

    [TestMethod]
    public void TenantId_WithSpecialCharacters_ShouldStore()
    {
        // Arrange
        string specialId = "tenant-123!@#$%^&*()";
        var context = new TenantContext();

        // Act
        context.TenantId = specialId;

        // Assert
        context.TenantId.Should().Be(specialId);
    }

    [TestMethod]
    public void MultipleInstances_ShouldBeIndependent()
    {
        // Act
        var context1 = new TenantContext { TenantId = "tenant-1", TenantName = "Tenant One" };
        var context2 = new TenantContext { TenantId = "tenant-2", TenantName = "Tenant Two" };

        // Assert
        context1.TenantId.Should().Be("tenant-1");
        context1.TenantName.Should().Be("Tenant One");
        context2.TenantId.Should().Be("tenant-2");
        context2.TenantName.Should().Be("Tenant Two");
    }
}
