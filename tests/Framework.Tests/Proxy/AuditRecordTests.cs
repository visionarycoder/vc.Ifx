using VisionaryCoder.Framework.Proxy.Interceptors.Auditing;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public class AuditRecordTests
{
    [TestMethod]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Act
        var record = new AuditRecord();

        // Assert
        record.OperationId.Should().BeEmpty();
        record.MethodName.Should().BeEmpty();
        record.OperationName.Should().BeEmpty();
        record.Result.Should().BeNull();
        record.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        record.Duration.Should().Be(TimeSpan.Zero);
        record.Success.Should().BeFalse();
        record.ErrorMessage.Should().BeNull();
        record.CorrelationId.Should().BeNull();
        record.RequestId.Should().BeNull();
        record.CompletedAt.Should().BeNull();
        record.ExceptionType.Should().BeNull();
        record.UserId.Should().BeNull();
        record.UserAgent.Should().BeNull();
        record.IpAddress.Should().BeNull();
        record.Method.Should().BeNull();
        record.Url.Should().BeNull();
        record.StartedAt.Should().BeNull();
        record.Headers.Should().BeNull();
        record.RequestSize.Should().BeNull();
        record.ResponseSize.Should().BeNull();
    }

    [TestMethod]
    public void OperationName_ShouldBeAliasForMethodName()
    {
        // Arrange
        var record = new AuditRecord();

        // Act
        record.OperationName = "TestOperation";

        // Assert
        record.OperationName.Should().Be("TestOperation");
        record.MethodName.Should().Be("TestOperation");
    }

    [TestMethod]
    public void MethodName_ChangingShouldUpdateOperationName()
    {
        // Arrange
        var record = new AuditRecord();

        // Act
        record.MethodName = "TestMethod";

        // Assert
        record.MethodName.Should().Be("TestMethod");
        record.OperationName.Should().Be("TestMethod");
    }

    [TestMethod]
    public void AllProperties_ShouldBeSettable()
    {
        // Arrange
        var headers = new Dictionary<string, string> { { "Authorization", "Bearer token" } };
        var result = new { Data = "test" };

        // Act
        var record = new AuditRecord
        {
            OperationId = "op-123",
            MethodName = "GetUser",
            Result = result,
            Duration = TimeSpan.FromSeconds(5),
            Success = true,
            ErrorMessage = "No error",
            CorrelationId = "corr-456",
            RequestId = "req-789",
            CompletedAt = DateTime.UtcNow,
            ExceptionType = "None",
            UserId = "user-001",
            UserAgent = "TestAgent/1.0",
            IpAddress = "192.168.1.1",
            Method = "GET",
            Url = "https://api.test.com",
            StartedAt = DateTime.UtcNow.AddSeconds(-5),
            Headers = headers,
            RequestSize = 1024,
            ResponseSize = 2048
        };

        // Assert
        record.OperationId.Should().Be("op-123");
        record.MethodName.Should().Be("GetUser");
        record.Result.Should().BeSameAs(result);
        record.Duration.Should().Be(TimeSpan.FromSeconds(5));
        record.Success.Should().BeTrue();
        record.ErrorMessage.Should().Be("No error");
        record.CorrelationId.Should().Be("corr-456");
        record.RequestId.Should().Be("req-789");
        record.CompletedAt.Should().NotBeNull();
        record.ExceptionType.Should().Be("None");
        record.UserId.Should().Be("user-001");
        record.UserAgent.Should().Be("TestAgent/1.0");
        record.IpAddress.Should().Be("192.168.1.1");
        record.Method.Should().Be("GET");
        record.Url.Should().Be("https://api.test.com");
        record.StartedAt.Should().NotBeNull();
        record.Headers.Should().BeSameAs(headers);
        record.RequestSize.Should().Be(1024);
        record.ResponseSize.Should().Be(2048);
    }

    [TestMethod]
    public void Timestamp_ShouldBeInitializedToUtcNow()
    {
        // Act
        var record = new AuditRecord();

        // Assert
        record.Timestamp.Kind.Should().Be(DateTimeKind.Utc);
        record.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public void RequestSize_WithLargeValue_ShouldStore()
    {
        // Arrange
        var record = new AuditRecord();

        // Act
        record.RequestSize = long.MaxValue;

        // Assert
        record.RequestSize.Should().Be(long.MaxValue);
    }

    [TestMethod]
    public void ResponseSize_WithLargeValue_ShouldStore()
    {
        // Arrange
        var record = new AuditRecord();

        // Act
        record.ResponseSize = long.MaxValue;

        // Assert
        record.ResponseSize.Should().Be(long.MaxValue);
    }

    [TestMethod]
    [DataRow("IPv4", "192.168.1.1")]
    [DataRow("IPv6", "2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
    [DataRow("Localhost", "127.0.0.1")]
    public void IpAddress_WithVariousFormats_ShouldStore(string _, string ipAddress)
    {
        // Arrange
        var record = new AuditRecord();

        // Act
        record.IpAddress = ipAddress;

        // Assert
        record.IpAddress.Should().Be(ipAddress);
    }

    [TestMethod]
    public void Headers_WithMultipleEntries_ShouldStore()
    {
        // Arrange
        var record = new AuditRecord();
        var headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" },
            { "Authorization", "Bearer token" },
            { "X-Custom", "value" }
        };

        // Act
        record.Headers = headers;

        // Assert
        record.Headers.Should().HaveCount(3);
        record.Headers!["Content-Type"].Should().Be("application/json");
    }

    [TestMethod]
    public void Duration_WithMaxValue_ShouldStore()
    {
        // Arrange
        var record = new AuditRecord();

        // Act
        record.Duration = TimeSpan.MaxValue;

        // Assert
        record.Duration.Should().Be(TimeSpan.MaxValue);
    }

    [TestMethod]
    public void MultipleInstances_ShouldBeIndependent()
    {
        // Act
        var record1 = new AuditRecord { OperationId = "op-1", Success = true };
        var record2 = new AuditRecord { OperationId = "op-2", Success = false };

        // Assert
        record1.OperationId.Should().Be("op-1");
        record1.Success.Should().BeTrue();
        record2.OperationId.Should().Be("op-2");
        record2.Success.Should().BeFalse();
    }

    [TestMethod]
    public void Url_WithUnicode_ShouldStore()
    {
        // Arrange
        var record = new AuditRecord();
        string unicodeUrl = "https://api.example.com/用户/123";

        // Act
        record.Url = unicodeUrl;

        // Assert
        record.Url.Should().Be(unicodeUrl);
    }

    [TestMethod]
    public void UserAgent_WithLongString_ShouldStore()
    {
        // Arrange
        var record = new AuditRecord();
        string longUserAgent = new('A', 10000);

        // Act
        record.UserAgent = longUserAgent;

        // Assert
        record.UserAgent.Should().HaveLength(10000);
    }

    [TestMethod]
    public void ErrorMessage_WithUnicode_ShouldPreserve()
    {
        // Arrange
        var record = new AuditRecord();
        string unicodeError = "エラーが発生しました";

        // Act
        record.ErrorMessage = unicodeError;

        // Assert
        record.ErrorMessage.Should().Be(unicodeError);
    }
}
