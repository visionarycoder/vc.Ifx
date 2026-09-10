using VisionaryCoder.Framework.Proxy;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public class ProxyOptionsTests
{
    [TestMethod]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var options = new ProxyOptions();

        // Assert
        options.Timeout.Should().Be(TimeSpan.FromSeconds(30));
        options.CircuitBreakerFailures.Should().Be(5);
        options.CircuitBreakerDuration.Should().Be(TimeSpan.FromMinutes(1));
        options.MaxRetries.Should().Be(3);
        options.MaxRetryAttempts.Should().Be(3);
        options.RetryDelay.Should().Be(TimeSpan.FromSeconds(1));
        options.CachingEnabled.Should().BeTrue();
        options.AuditingEnabled.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(10)]
    [DataRow(30)]
    [DataRow(60)]
    public void Timeout_ShouldBeSettable(int seconds)
    {
        // Arrange
        var options = new ProxyOptions();

        // Act
        options.Timeout = TimeSpan.FromSeconds(seconds);

        // Assert
        options.Timeout.Should().Be(TimeSpan.FromSeconds(seconds));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(10)]
    public void CircuitBreakerFailures_ShouldBeSettable(int failures)
    {
        // Arrange
        var options = new ProxyOptions();

        // Act
        options.CircuitBreakerFailures = failures;

        // Assert
        options.CircuitBreakerFailures.Should().Be(failures);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(10)]
    public void CircuitBreakerDuration_ShouldBeSettable(int minutes)
    {
        // Arrange
        var options = new ProxyOptions();

        // Act
        options.CircuitBreakerDuration = TimeSpan.FromMinutes(minutes);

        // Assert
        options.CircuitBreakerDuration.Should().Be(TimeSpan.FromMinutes(minutes));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(3)]
    [DataRow(5)]
    public void MaxRetries_ShouldBeSettable(int retries)
    {
        // Arrange
        var options = new ProxyOptions();

        // Act
        options.MaxRetries = retries;

        // Assert
        options.MaxRetries.Should().Be(retries);
    }

    [TestMethod]
    public void MaxRetryAttempts_ShouldBeAliasForMaxRetries()
    {
        // Arrange
        var options = new ProxyOptions();

        // Act
        options.MaxRetryAttempts = 10;

        // Assert
        options.MaxRetryAttempts.Should().Be(10);
        options.MaxRetries.Should().Be(10);
    }

    [TestMethod]
    public void MaxRetries_ChangingShouldUpdateMaxRetryAttempts()
    {
        // Arrange
        var options = new ProxyOptions();

        // Act
        options.MaxRetries = 7;

        // Assert
        options.MaxRetries.Should().Be(7);
        options.MaxRetryAttempts.Should().Be(7);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(5)]
    public void RetryDelay_ShouldBeSettable(int seconds)
    {
        // Arrange
        var options = new ProxyOptions();

        // Act
        options.RetryDelay = TimeSpan.FromSeconds(seconds);

        // Assert
        options.RetryDelay.Should().Be(TimeSpan.FromSeconds(seconds));
    }

    [TestMethod]
    public void CachingEnabled_ShouldBeSettable()
    {
        // Arrange
        var options = new ProxyOptions();

        // Act
        options.CachingEnabled = false;

        // Assert
        options.CachingEnabled.Should().BeFalse();
    }

    [TestMethod]
    public void AuditingEnabled_ShouldBeSettable()
    {
        // Arrange
        var options = new ProxyOptions();

        // Act
        options.AuditingEnabled = false;

        // Assert
        options.AuditingEnabled.Should().BeFalse();
    }

    [TestMethod]
    public void AllProperties_ShouldBeIndependentlySettable()
    {
        // Arrange & Act
        var options = new ProxyOptions
        {
            Timeout = TimeSpan.FromMinutes(2),
            CircuitBreakerFailures = 10,
            CircuitBreakerDuration = TimeSpan.FromMinutes(5),
            MaxRetries = 5,
            RetryDelay = TimeSpan.FromSeconds(2),
            CachingEnabled = false,
            AuditingEnabled = false
        };

        // Assert
        options.Timeout.Should().Be(TimeSpan.FromMinutes(2));
        options.CircuitBreakerFailures.Should().Be(10);
        options.CircuitBreakerDuration.Should().Be(TimeSpan.FromMinutes(5));
        options.MaxRetries.Should().Be(5);
        options.MaxRetryAttempts.Should().Be(5);
        options.RetryDelay.Should().Be(TimeSpan.FromSeconds(2));
        options.CachingEnabled.Should().BeFalse();
        options.AuditingEnabled.Should().BeFalse();
    }

    [TestMethod]
    public void Timeout_WithZeroTimeSpan_ShouldBeAllowed()
    {
        // Arrange
        var options = new ProxyOptions();

        // Act
        options.Timeout = TimeSpan.Zero;

        // Assert
        options.Timeout.Should().Be(TimeSpan.Zero);
    }

    [TestMethod]
    public void Timeout_WithMaxValue_ShouldBeAllowed()
    {
        // Arrange
        var options = new ProxyOptions();

        // Act
        options.Timeout = TimeSpan.MaxValue;

        // Assert
        options.Timeout.Should().Be(TimeSpan.MaxValue);
    }

    [TestMethod]
    public void CircuitBreakerFailures_WithZero_ShouldBeAllowed()
    {
        // Arrange
        var options = new ProxyOptions();

        // Act
        options.CircuitBreakerFailures = 0;

        // Assert
        options.CircuitBreakerFailures.Should().Be(0);
    }

    [TestMethod]
    public void MaxRetries_WithZero_ShouldDisableRetries()
    {
        // Arrange
        var options = new ProxyOptions();

        // Act
        options.MaxRetries = 0;

        // Assert
        options.MaxRetries.Should().Be(0);
        options.MaxRetryAttempts.Should().Be(0);
    }

    [TestMethod]
    public void MultipleInstances_ShouldBeIndependent()
    {
        // Act
        var options1 = new ProxyOptions { CachingEnabled = false };
        var options2 = new ProxyOptions { CachingEnabled = true };

        // Assert
        options1.CachingEnabled.Should().BeFalse();
        options2.CachingEnabled.Should().BeTrue();
    }

    [TestMethod]
    public void RetryDelay_WithNegativeValue_ShouldBeAllowed()
    {
        // Arrange
        var options = new ProxyOptions();

        // Act
        options.RetryDelay = TimeSpan.FromSeconds(-1);

        // Assert
        options.RetryDelay.Should().Be(TimeSpan.FromSeconds(-1));
    }

    [TestMethod]
    public void CircuitBreakerFailures_WithLargeValue_ShouldStore()
    {
        // Arrange
        var options = new ProxyOptions();

        // Act
        options.CircuitBreakerFailures = int.MaxValue;

        // Assert
        options.CircuitBreakerFailures.Should().Be(int.MaxValue);
    }

    [TestMethod]
    public void MaxRetries_WithLargeValue_ShouldStore()
    {
        // Arrange
        var options = new ProxyOptions();

        // Act
        options.MaxRetries = 1000;

        // Assert
        options.MaxRetries.Should().Be(1000);
        options.MaxRetryAttempts.Should().Be(1000);
    }
}
