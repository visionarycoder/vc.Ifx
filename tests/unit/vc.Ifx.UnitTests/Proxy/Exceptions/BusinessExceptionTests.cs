// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using VisionaryCoder.Framework.Proxy.Exceptions;

namespace VisionaryCoder.Framework.Tests.Proxy.Exceptions;

[TestClass]
public sealed class BusinessExceptionTests
{
    [TestMethod]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        string message = "Business rule violation occurred";

        // Act
        var exception = new BusinessException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
    }

    [TestMethod]
    public void Constructor_WithMessageAndInnerException_ShouldSetBoth()
    {
        // Arrange
        string message = "Business operation failed";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new BusinessException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeSameAs(innerException);
    }

    [TestMethod]
    public void BusinessException_ShouldBeProxyException()
    {
        // Arrange & Act
        var exception = new BusinessException("Test");

        // Assert
        exception.Should().BeAssignableTo<ProxyException>();
    }

    [TestMethod]
    public void BusinessException_ShouldBeException()
    {
        // Arrange & Act
        var exception = new BusinessException("Test");

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [TestMethod]
    public void BusinessException_CanBeThrown()
    {
        // Arrange
        string message = "Test business exception";

        // Act
        Action act = () => throw new BusinessException(message);

        // Assert
        act.Should().Throw<BusinessException>()
           .WithMessage(message);
    }

    [TestMethod]
    public void BusinessException_CanBeCaughtAsProxyException()
    {
        // Arrange
        string message = "Business error";

        // Act
        Action act = () => throw new BusinessException(message);

        // Assert
        act.Should().Throw<ProxyException>()
           .Which.Should().BeOfType<BusinessException>()
           .And.Subject.As<BusinessException>().Message.Should().Be(message);
    }

    [TestMethod]
    public void Constructor_WithEmptyMessage_ShouldWork()
    {
        // Arrange & Act
        var exception = new BusinessException(string.Empty);

        // Assert
        exception.Message.Should().Be(string.Empty);
    }

    [TestMethod]
    public void Constructor_WithNullInnerException_ShouldWork()
    {
        // Arrange & Act
        var exception = new BusinessException("Message", null!);

        // Assert
        exception.Message.Should().Be("Message");
        exception.InnerException.Should().BeNull();
    }
}
