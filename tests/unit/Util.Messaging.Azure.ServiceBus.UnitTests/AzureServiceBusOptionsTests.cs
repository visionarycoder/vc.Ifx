using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Util.Messaging.Azure.ServiceBus;

namespace Util.Messaging.Azure.ServiceBus.UnitTests;

[TestClass]
public sealed class AzureServiceBusOptionsTests
{
    #region Validate Tests

    [TestMethod]
    public void Validate_WhenQueueAndConnectionStringAreConfigured_ShouldReturnSameInstance()
    {
        AzureServiceBusOptions options = new()
        {
            ConnectionString = TestData.ConnectionString,
            QueueName = "orders",
            InitialRetryDelay = TimeSpan.Zero
        };

        AzureServiceBusOptions validated = options.Validate();

        validated.Should().BeSameAs(options);
    }

    [TestMethod]
    public void Validate_WhenAuthenticationPathsConflict_ShouldThrowArgumentException()
    {
        AzureServiceBusOptions options = new()
        {
            ConnectionString = TestData.ConnectionString,
            FullyQualifiedNamespace = "contoso.servicebus.windows.net",
            QueueName = "orders"
        };

        Action action = () => options.Validate();

        action.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be(nameof(AzureServiceBusOptions.ConnectionString));
    }

    [TestMethod]
    public void Validate_WhenEntityPathsAreMissing_ShouldThrowArgumentException()
    {
        AzureServiceBusOptions options = new()
        {
            ConnectionString = TestData.ConnectionString
        };

        Action action = () => options.Validate();

        action.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be(nameof(AzureServiceBusOptions.QueueName));
    }

    #endregion
}
