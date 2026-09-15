using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Util.Messaging;

namespace Util.UnitTests.Messaging;

[TestClass]
public sealed class MessagingOptionsValidationExtensionsTests
{
    #region ValidateNotNullOrWhiteSpace Tests

    [DataTestMethod]
    [DataRow("orders")]
    [DataRow("Endpoint=sb://contoso.servicebus.windows.net/")]
    public void ValidateNotNullOrWhiteSpace_WhenValueIsValid_ShouldReturnOriginalValue(string value)
    {
        string validated = value.ValidateNotNullOrWhiteSpace("QueueName");

        validated.Should().Be(value);
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void ValidateNotNullOrWhiteSpace_WhenValueIsMissing_ShouldThrowArgumentException(string? value)
    {
        Action action = () => value.ValidateNotNullOrWhiteSpace("QueueName");

        action.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be("QueueName");
    }

    #endregion
}
