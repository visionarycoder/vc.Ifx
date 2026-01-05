using Microsoft.Extensions.Caching.Memory;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching;

namespace VisionaryCoder.Framework.Tests.Proxy.Interceptors.Caching;

[TestClass]
public class CachingOptionsTests
{
    [TestMethod]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var options = new CachingOptions();

        // Assert
        options.DefaultDuration.Should().Be(TimeSpan.FromMinutes(5));
        options.DefaultPriority.Should().Be(CacheItemPriority.Normal);
        options.EnableEvictionLogging.Should().BeFalse();
        options.OperationPolicies.Should().NotBeNull();
        options.OperationPolicies.Should().BeEmpty();
        options.MaxCacheSize.Should().BeNull();
        options.KeyGenerator.Should().BeNull();
        options.ShouldCache.Should().BeNull();
    }

    [TestMethod]
    public void DefaultDuration_ShouldBeSettable()
    {
        // Arrange
        var options = new CachingOptions();

        // Act
        options.DefaultDuration = TimeSpan.FromMinutes(10);

        // Assert
        options.DefaultDuration.Should().Be(TimeSpan.FromMinutes(10));
    }

    [TestMethod]
    public void DefaultPriority_ShouldBeSettable()
    {
        // Arrange
        var options = new CachingOptions();

        // Act
        options.DefaultPriority = CacheItemPriority.High;

        // Assert
        options.DefaultPriority.Should().Be(CacheItemPriority.High);
    }

    [TestMethod]
    public void EnableEvictionLogging_ShouldBeSettable()
    {
        // Arrange
        var options = new CachingOptions();

        // Act
        options.EnableEvictionLogging = true;

        // Assert
        options.EnableEvictionLogging.Should().BeTrue();
    }

    [TestMethod]
    public void OperationPolicies_ShouldSupportAddingPolicies()
    {
        // Arrange
        var options = new CachingOptions();
        var policy = new CachePolicy { Duration = TimeSpan.FromMinutes(10) };

        // Act
        options.OperationPolicies.Add("GetUser", policy);

        // Assert
        options.OperationPolicies.Should().HaveCount(1);
        options.OperationPolicies["GetUser"].Should().BeSameAs(policy);
    }

    [TestMethod]
    public void OperationPolicies_ShouldSupportMultiplePolicies()
    {
        // Arrange
        var options = new CachingOptions();
        var policy1 = new CachePolicy { Duration = TimeSpan.FromMinutes(5) };
        var policy2 = new CachePolicy { Duration = TimeSpan.FromMinutes(15) };

        // Act
        options.OperationPolicies.Add("Operation1", policy1);
        options.OperationPolicies.Add("Operation2", policy2);

        // Assert
        options.OperationPolicies.Should().HaveCount(2);
        options.OperationPolicies["Operation1"].Duration.Should().Be(TimeSpan.FromMinutes(5));
        options.OperationPolicies["Operation2"].Duration.Should().Be(TimeSpan.FromMinutes(15));
    }

    [TestMethod]
    public void MaxCacheSize_ShouldBeSettable()
    {
        // Arrange
        var options = new CachingOptions();

        // Act
        options.MaxCacheSize = 1000;

        // Assert
        options.MaxCacheSize.Should().Be(1000);
    }

    [TestMethod]
    [DataRow(100)]
    [DataRow(1000)]
    [DataRow(10000)]
    public void MaxCacheSize_WithVariousValues_ShouldStore(int size)
    {
        // Arrange
        var options = new CachingOptions();

        // Act
        options.MaxCacheSize = size;

        // Assert
        options.MaxCacheSize.Should().Be(size);
    }

    [TestMethod]
    public void KeyGenerator_ShouldBeSettable()
    {
        // Arrange
        var options = new CachingOptions();
        Func<ProxyContext, string> generator = ctx => $"custom-{ctx.OperationName}";

        // Act
        options.KeyGenerator = generator;

        // Assert
        options.KeyGenerator.Should().BeSameAs(generator);
    }

    [TestMethod]
    public void KeyGenerator_CustomFunction_ShouldWork()
    {
        // Arrange
        var options = new CachingOptions
        {
            KeyGenerator = ctx => $"{ctx.Method}-{ctx.Url}"
        };
        var context = new ProxyContext
        {
            Method = "GET",
            Url = "https://api.example.com/users"
        };

        // Act
        string key = options.KeyGenerator!(context);

        // Assert
        key.Should().Be("GET-https://api.example.com/users");
    }

    [TestMethod]
    public void ShouldCache_ShouldBeSettable()
    {
        // Arrange
        var options = new CachingOptions();
        Func<ProxyContext, bool> predicate = ctx => ctx.Method == "GET";

        // Act
        options.ShouldCache = predicate;

        // Assert
        options.ShouldCache.Should().BeSameAs(predicate);
    }

    [TestMethod]
    public void ShouldCache_CustomPredicate_ShouldWork()
    {
        // Arrange
        var options = new CachingOptions
        {
            ShouldCache = ctx => ctx.Method == "GET"
        };

        // Act & Assert
        options.ShouldCache!(new ProxyContext { Method = "GET" }).Should().BeTrue();
        options.ShouldCache!(new ProxyContext { Method = "POST" }).Should().BeFalse();
    }

    [TestMethod]
    public void AllProperties_ShouldBeIndependentlySettable()
    {
        // Arrange & Act
        var options = new CachingOptions
        {
            DefaultDuration = TimeSpan.FromHours(1),
            DefaultPriority = CacheItemPriority.Low,
            EnableEvictionLogging = true,
            MaxCacheSize = 500,
            KeyGenerator = ctx => "custom-key",
            ShouldCache = ctx => true
        };

        // Assert
        options.DefaultDuration.Should().Be(TimeSpan.FromHours(1));
        options.DefaultPriority.Should().Be(CacheItemPriority.Low);
        options.EnableEvictionLogging.Should().BeTrue();
        options.MaxCacheSize.Should().Be(500);
        options.KeyGenerator.Should().NotBeNull();
        options.ShouldCache.Should().NotBeNull();
    }

    [TestMethod]
    public void OperationPolicies_ShouldBeReplaceable()
    {
        // Arrange
        var options = new CachingOptions();
        var newPolicies = new Dictionary<string, CachePolicy>
        {
            { "Op1", new CachePolicy() },
            { "Op2", new CachePolicy() }
        };

        // Act
        options.OperationPolicies = newPolicies;

        // Assert
        options.OperationPolicies.Should().BeSameAs(newPolicies);
        options.OperationPolicies.Should().HaveCount(2);
    }

    [TestMethod]
    public void DefaultDuration_WithZeroTimeSpan_ShouldBeAllowed()
    {
        // Arrange
        var options = new CachingOptions();

        // Act
        options.DefaultDuration = TimeSpan.Zero;

        // Assert
        options.DefaultDuration.Should().Be(TimeSpan.Zero);
    }

    [TestMethod]
    public void DefaultDuration_WithMaxValue_ShouldBeAllowed()
    {
        // Arrange
        var options = new CachingOptions();

        // Act
        options.DefaultDuration = TimeSpan.MaxValue;

        // Assert
        options.DefaultDuration.Should().Be(TimeSpan.MaxValue);
    }

    [TestMethod]
    public void MaxCacheSize_SetToNull_ShouldBeAllowed()
    {
        // Arrange
        var options = new CachingOptions { MaxCacheSize = 1000 };

        // Act
        options.MaxCacheSize = null;

        // Assert
        options.MaxCacheSize.Should().BeNull();
    }

    [TestMethod]
    public void KeyGenerator_SetToNull_ShouldBeAllowed()
    {
        // Arrange
        var options = new CachingOptions { KeyGenerator = ctx => "key" };

        // Act
        options.KeyGenerator = null;

        // Assert
        options.KeyGenerator.Should().BeNull();
    }

    [TestMethod]
    public void ShouldCache_SetToNull_ShouldBeAllowed()
    {
        // Arrange
        var options = new CachingOptions { ShouldCache = ctx => true };

        // Act
        options.ShouldCache = null;

        // Assert
        options.ShouldCache.Should().BeNull();
    }

    [TestMethod]
    public void MultipleInstances_ShouldBeIndependent()
    {
        // Act
        var options1 = new CachingOptions { EnableEvictionLogging = true };
        var options2 = new CachingOptions { EnableEvictionLogging = false };

        options1.OperationPolicies.Add("Op1", new CachePolicy());

        // Assert
        options1.EnableEvictionLogging.Should().BeTrue();
        options2.EnableEvictionLogging.Should().BeFalse();
        options1.OperationPolicies.Should().HaveCount(1);
        options2.OperationPolicies.Should().BeEmpty();
    }
}
