using VisionaryCoder.Framework.Proxy;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public class ProxyContextTests
{
    [TestMethod]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Act
        var context = new ProxyContext();

        // Assert
        context.OperationId.Should().NotBeNullOrWhiteSpace();
        Guid.TryParse(context.OperationId, out _).Should().BeTrue();
        context.MethodName.Should().BeNull();
        context.ServiceName.Should().BeNull();
        context.Properties.Should().NotBeNull().And.BeEmpty();
        context.CorrelationId.Should().BeNull();
        context.StartTime.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        context.Method.Should().BeNull();
        context.Url.Should().BeNull();
        context.Headers.Should().NotBeNull().And.BeEmpty();
        context.Request.Should().BeNull();
        context.Items.Should().NotBeNull().And.BeEmpty();
        context.Metadata.Should().NotBeNull().And.BeEmpty();
        context.OperationName.Should().BeNull();
        context.ResultType.Should().BeNull();
        context.RequestId.Should().BeNull();
        context.CancellationToken.Should().Be(default(CancellationToken));
    }

    [TestMethod]
    public void OperationId_ShouldBeUniquePerInstance()
    {
        // Act
        var context1 = new ProxyContext();
        var context2 = new ProxyContext();

        // Assert
        context1.OperationId.Should().NotBe(context2.OperationId);
    }

    [TestMethod]
    public void OperationId_ShouldBeSettable()
    {
        // Arrange
        var context = new ProxyContext();
        string customId = "custom-operation-id";

        // Act
        context.OperationId = customId;

        // Assert
        context.OperationId.Should().Be(customId);
    }

    [TestMethod]
    [DataRow("GetUser")]
    [DataRow("CreateOrder")]
    [DataRow(null)]
    public void MethodName_ShouldBeSettable(string? methodName)
    {
        // Arrange
        var context = new ProxyContext();

        // Act
        context.MethodName = methodName;

        // Assert
        context.MethodName.Should().Be(methodName);
    }

    [TestMethod]
    [DataRow("UserService")]
    [DataRow("OrderService")]
    [DataRow(null)]
    public void ServiceName_ShouldBeSettable(string? serviceName)
    {
        // Arrange
        var context = new ProxyContext();

        // Act
        context.ServiceName = serviceName;

        // Assert
        context.ServiceName.Should().Be(serviceName);
    }

    [TestMethod]
    public void Properties_ShouldSupportAddingItems()
    {
        // Arrange
        var context = new ProxyContext();

        // Act
        context.Properties.Add("Key1", "Value1");
        context.Properties.Add("Key2", 42);
        context.Properties.Add("Key3", null);

        // Assert
        context.Properties.Should().HaveCount(3);
        context.Properties["Key1"].Should().Be("Value1");
        context.Properties["Key2"].Should().Be(42);
        context.Properties["Key3"].Should().BeNull();
    }

    [TestMethod]
    public void CorrelationId_ShouldBeSettable()
    {
        // Arrange
        var context = new ProxyContext();
        string correlationId = Guid.NewGuid().ToString();

        // Act
        context.CorrelationId = correlationId;

        // Assert
        context.CorrelationId.Should().Be(correlationId);
    }

    [TestMethod]
    public void StartTime_ShouldBeSettable()
    {
        // Arrange
        var context = new ProxyContext();
        var customStartTime = new DateTimeOffset(2025, 10, 24, 12, 0, 0, TimeSpan.Zero);

        // Act
        context.StartTime = customStartTime;

        // Assert
        context.StartTime.Should().Be(customStartTime);
    }

    [TestMethod]
    [DataRow("GET")]
    [DataRow("POST")]
    [DataRow("PUT")]
    [DataRow("DELETE")]
    public void Method_ShouldBeSettable(string method)
    {
        // Arrange
        var context = new ProxyContext();

        // Act
        context.Method = method;

        // Assert
        context.Method.Should().Be(method);
    }

    [TestMethod]
    [DataRow("https://api.example.com/users")]
    [DataRow("https://api.example.com/orders/123")]
    public void Url_ShouldBeSettable(string url)
    {
        // Arrange
        var context = new ProxyContext();

        // Act
        context.Url = url;

        // Assert
        context.Url.Should().Be(url);
    }

    [TestMethod]
    public void Headers_ShouldSupportAddingItems()
    {
        // Arrange
        var context = new ProxyContext();

        // Act
        context.Headers.Add("Authorization", "Bearer token");
        context.Headers.Add("Content-Type", "application/json");

        // Assert
        context.Headers.Should().HaveCount(2);
        context.Headers["Authorization"].Should().Be("Bearer token");
        context.Headers["Content-Type"].Should().Be("application/json");
    }

    [TestMethod]
    public void Request_ShouldBeSettable()
    {
        // Arrange
        var context = new ProxyContext();
        var request = new { UserId = 123, Name = "John" };

        // Act
        context.Request = request;

        // Assert
        context.Request.Should().BeSameAs(request);
    }

    [TestMethod]
    public void Items_ShouldSupportAddingItems()
    {
        // Arrange
        var context = new ProxyContext();

        // Act
        context.Items.Add("Custom1", "Value1");
        context.Items.Add("Custom2", 42);

        // Assert
        context.Items.Should().HaveCount(2);
    }

    [TestMethod]
    public void Metadata_ShouldSupportAddingItems()
    {
        // Arrange
        var context = new ProxyContext();

        // Act
        context.Metadata.Add("Meta1", "Value1");
        context.Metadata.Add("Meta2", true);

        // Assert
        context.Metadata.Should().HaveCount(2);
    }

    [TestMethod]
    public void OperationName_ShouldBeSettable()
    {
        // Arrange
        var context = new ProxyContext();

        // Act
        context.OperationName = "GetUserById";

        // Assert
        context.OperationName.Should().Be("GetUserById");
    }

    [TestMethod]
    public void ResultType_ShouldBeSettable()
    {
        // Arrange
        var context = new ProxyContext();

        // Act
        context.ResultType = typeof(string);

        // Assert
        context.ResultType.Should().Be(typeof(string));
    }

    [TestMethod]
    public void RequestId_ShouldBeSettable()
    {
        // Arrange
        var context = new ProxyContext();
        string requestId = Guid.NewGuid().ToString();

        // Act
        context.RequestId = requestId;

        // Assert
        context.RequestId.Should().Be(requestId);
    }

    [TestMethod]
    public void CancellationToken_ShouldBeSettable()
    {
        // Arrange
        var context = new ProxyContext();
        var cts = new CancellationTokenSource();

        // Act
        context.CancellationToken = cts.Token;

        // Assert
        context.CancellationToken.Should().Be(cts.Token);
    }

    [TestMethod]
    public void AllProperties_ShouldBeIndependentlySettable()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var request = new { Id = 1 };

        // Act
        var context = new ProxyContext
        {
            OperationId = "custom-id",
            MethodName = "TestMethod",
            ServiceName = "TestService",
            CorrelationId = "correlation-123",
            Method = "POST",
            Url = "https://api.test.com",
            Request = request,
            OperationName = "TestOperation",
            ResultType = typeof(int),
            RequestId = "request-456",
            CancellationToken = cts.Token
        };

        context.Properties.Add("Prop1", "Value1");
        context.Headers.Add("Header1", "Value1");
        context.Items.Add("Item1", "Value1");
        context.Metadata.Add("Meta1", "Value1");

        // Assert
        context.OperationId.Should().Be("custom-id");
        context.MethodName.Should().Be("TestMethod");
        context.ServiceName.Should().Be("TestService");
        context.CorrelationId.Should().Be("correlation-123");
        context.Method.Should().Be("POST");
        context.Url.Should().Be("https://api.test.com");
        context.Request.Should().BeSameAs(request);
        context.OperationName.Should().Be("TestOperation");
        context.ResultType.Should().Be(typeof(int));
        context.RequestId.Should().Be("request-456");
        context.CancellationToken.Should().Be(cts.Token);
        context.Properties.Should().HaveCount(1);
        context.Headers.Should().HaveCount(1);
        context.Items.Should().HaveCount(1);
        context.Metadata.Should().HaveCount(1);
    }

    [TestMethod]
    public void MultipleInstances_ShouldBeIndependent()
    {
        // Act
        var context1 = new ProxyContext { MethodName = "Method1" };
        var context2 = new ProxyContext { MethodName = "Method2" };

        context1.Properties.Add("Key1", "Value1");

        // Assert
        context1.MethodName.Should().Be("Method1");
        context2.MethodName.Should().Be("Method2");
        context1.Properties.Should().HaveCount(1);
        context2.Properties.Should().BeEmpty();
    }

    [TestMethod]
    public void Url_WithUnicode_ShouldStore()
    {
        // Arrange
        var context = new ProxyContext();
        string unicodeUrl = "https://api.example.com/用户/123";

        // Act
        context.Url = unicodeUrl;

        // Assert
        context.Url.Should().Be(unicodeUrl);
    }

    [TestMethod]
    public void Headers_WithUnicodeValues_ShouldStore()
    {
        // Arrange
        var context = new ProxyContext();

        // Act
        context.Headers.Add("X-Custom", "値123");

        // Assert
        context.Headers["X-Custom"].Should().Be("値123");
    }
}
