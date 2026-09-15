using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Util.Messaging.Msmq;

namespace Util.Messaging.Msmq.UnitTests;

[TestClass]
public sealed class MsmqOptionsTests
{
    #region Validate Tests

    [TestMethod]
    public void Validate_WhenOptionsAreValid_ShouldSucceed()
    {
        MsmqOptions options = new()
        {
            QueuePath = @".\private$\orders-{subscriptionName}",
            MaxRetryAttempts = 3,
            InitialRetryDelay = TimeSpan.Zero,
            ReceiveTimeout = TimeSpan.FromSeconds(1)
        };

        Action action = options.Validate;

        action.Should().NotThrow();
        options.ResolveQueuePath("billing").Should().Be(@".\private$\orders-billing");
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Validate_WhenQueuePathMissing_ShouldThrow(string? queuePath)
    {
        MsmqOptions options = new()
        {
            QueuePath = queuePath,
            ReceiveTimeout = TimeSpan.FromSeconds(1)
        };

        Action action = options.Validate;

        action.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be(nameof(MsmqOptions.QueuePath));
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void Validate_WhenMaxRetryAttemptsLessThanOne_ShouldThrow(int maxRetryAttempts)
    {
        MsmqOptions options = new()
        {
            QueuePath = @".\private$\orders",
            MaxRetryAttempts = maxRetryAttempts,
            ReceiveTimeout = TimeSpan.FromSeconds(1)
        };

        Action action = options.Validate;

        action.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be(nameof(MsmqOptions.MaxRetryAttempts));
    }

    [TestMethod]
    public void Validate_WhenInitialRetryDelayIsNegative_ShouldThrow()
    {
        MsmqOptions options = new()
        {
            QueuePath = @".\private$\orders",
            InitialRetryDelay = TimeSpan.FromMilliseconds(-1),
            ReceiveTimeout = TimeSpan.FromSeconds(1)
        };

        Action action = options.Validate;

        action.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be(nameof(MsmqOptions.InitialRetryDelay));
    }

    [TestMethod]
    public void Validate_WhenReceiveTimeoutIsNotPositive_ShouldThrow()
    {
        MsmqOptions options = new()
        {
            QueuePath = @".\private$\orders",
            ReceiveTimeout = TimeSpan.Zero
        };

        Action action = options.Validate;

        action.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be(nameof(MsmqOptions.ReceiveTimeout));
    }

    [TestMethod]
    public void ResolveQueuePath_WhenTemplateDoesNotContainSubscriptionPlaceholder_ShouldReturnOriginalQueuePath()
    {
        MsmqOptions options = new()
        {
            QueuePath = @".\private$\orders",
            ReceiveTimeout = TimeSpan.FromSeconds(1)
        };

        string resolvedPath = options.ResolveQueuePath("billing");

        resolvedPath.Should().Be(@".\private$\orders");
    }

    #endregion
}
