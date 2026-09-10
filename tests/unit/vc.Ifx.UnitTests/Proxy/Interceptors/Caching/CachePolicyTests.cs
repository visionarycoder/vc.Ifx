using Microsoft.Extensions.Caching.Memory;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching;

namespace VisionaryCoder.Framework.Tests.Proxy.Interceptors.Caching;

[TestClass]
public class CachePolicyTests
{
    [TestMethod]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var policy = new CachePolicy();

        // Assert
        policy.IsCachingEnabled.Should().BeTrue();
        policy.Duration.Should().Be(TimeSpan.FromMinutes(5));
        policy.Priority.Should().Be(CacheItemPriority.Normal);
        policy.ShouldCache.Should().NotBeNull();
        policy.ShouldRefresh.Should().NotBeNull();
    }

    [TestMethod]
    public void IsCachingEnabled_ShouldBeSettable()
    {
        // Arrange
        var policy = new CachePolicy();

        // Act
        policy.IsCachingEnabled = false;

        // Assert
        policy.IsCachingEnabled.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(60)]
    [DataRow(1440)]
    public void Duration_ShouldBeSettable(int minutes)
    {
        // Arrange
        var policy = new CachePolicy();
        var duration = TimeSpan.FromMinutes(minutes);

        // Act
        policy.Duration = duration;

        // Assert
        policy.Duration.Should().Be(duration);
    }

    [TestMethod]
    public void Priority_ShouldBeSettableToAllValues()
    {
        // Arrange
        var policy = new CachePolicy();

        // Act & Assert
        policy.Priority = CacheItemPriority.Low;
        policy.Priority.Should().Be(CacheItemPriority.Low);

        policy.Priority = CacheItemPriority.Normal;
        policy.Priority.Should().Be(CacheItemPriority.Normal);

        policy.Priority = CacheItemPriority.High;
        policy.Priority.Should().Be(CacheItemPriority.High);

        policy.Priority = CacheItemPriority.NeverRemove;
        policy.Priority.Should().Be(CacheItemPriority.NeverRemove);
    }

    [TestMethod]
    public void ShouldCache_DefaultPredicate_ShouldReturnTrue()
    {
        // Arrange
        var policy = new CachePolicy();

        // Act
        bool result = policy.ShouldCache(new object());

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void ShouldCache_CustomPredicate_ShouldWork()
    {
        // Arrange
        var policy = new CachePolicy
        {
            ShouldCache = obj => obj is string str && str.Length > 5
        };

        // Act & Assert
        policy.ShouldCache("short").Should().BeFalse();
        policy.ShouldCache("long string").Should().BeTrue();
    }

    [TestMethod]
    public void ShouldRefresh_DefaultPredicate_ShouldReturnFalse()
    {
        // Arrange
        var policy = new CachePolicy();

        // Act
        bool result = policy.ShouldRefresh(new object());

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void ShouldRefresh_CustomPredicate_ShouldWork()
    {
        // Arrange
        var policy = new CachePolicy
        {
            ShouldRefresh = obj => obj is int value && value > 100
        };

        // Act & Assert
        policy.ShouldRefresh(50).Should().BeFalse();
        policy.ShouldRefresh(150).Should().BeTrue();
    }

    [TestMethod]
    public void AllProperties_ShouldBeIndependentlySettable()
    {
        // Arrange & Act
        var policy = new CachePolicy
        {
            IsCachingEnabled = false,
            Duration = TimeSpan.FromHours(1),
            Priority = CacheItemPriority.High,
            ShouldCache = obj => false,
            ShouldRefresh = obj => true
        };

        // Assert
        policy.IsCachingEnabled.Should().BeFalse();
        policy.Duration.Should().Be(TimeSpan.FromHours(1));
        policy.Priority.Should().Be(CacheItemPriority.High);
        policy.ShouldCache(new object()).Should().BeFalse();
        policy.ShouldRefresh(new object()).Should().BeTrue();
    }

    [TestMethod]
    public void Duration_WithZeroTimeSpan_ShouldBeAllowed()
    {
        // Arrange
        var policy = new CachePolicy();

        // Act
        policy.Duration = TimeSpan.Zero;

        // Assert
        policy.Duration.Should().Be(TimeSpan.Zero);
    }

    [TestMethod]
    public void Duration_WithMaxValue_ShouldBeAllowed()
    {
        // Arrange
        var policy = new CachePolicy();

        // Act
        policy.Duration = TimeSpan.MaxValue;

        // Assert
        policy.Duration.Should().Be(TimeSpan.MaxValue);
    }

    [TestMethod]
    public void ShouldCache_WithNullObject_ShouldNotThrow()
    {
        // Arrange
        var policy = new CachePolicy();

        // Act
        bool result = policy.ShouldCache(null!);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void ShouldRefresh_WithNullObject_ShouldNotThrow()
    {
        // Arrange
        var policy = new CachePolicy();

        // Act
        bool result = policy.ShouldRefresh(null!);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void MultipleInstances_ShouldBeIndependent()
    {
        // Act
        var policy1 = new CachePolicy { IsCachingEnabled = false };
        var policy2 = new CachePolicy { IsCachingEnabled = true };

        // Assert
        policy1.IsCachingEnabled.Should().BeFalse();
        policy2.IsCachingEnabled.Should().BeTrue();
    }
}
