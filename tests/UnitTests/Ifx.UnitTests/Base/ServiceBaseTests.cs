using Microsoft.Extensions.Logging;
using Moq;
using vc.Ifx.Base;

namespace Ifx.UnitTests.Base
{
    public class ServiceBaseTests
    {
        private class TestService : ServiceBase<TestService>
        {
            public TestService(ILogger<TestService> logger) : base(logger)
            {
            }
        }

        [Fact]
        public void ServiceBase_ShouldInitializeLogger()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TestService>>();

            // Act
            var service = new TestService(mockLogger.Object);

            // Assert
            Assert.NotNull(service.logger);
        }

        [Fact]
        public void ServiceBase_ShouldGenerateNewInstanceGuid()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TestService>>();

            // Act
            var service = new TestService(mockLogger.Object);

            // Assert
            Assert.NotEqual(Guid.Empty, service.Instance);
        }

        [Fact]
        public void ServiceBase_ShouldSetCreatedAtToUtcNow()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TestService>>();
            var beforeCreation = DateTime.UtcNow;

            // Act
            var service = new TestService(mockLogger.Object);
            var afterCreation = DateTime.UtcNow;

            // Assert
            Assert.True(service.CreatedAt >= beforeCreation && service.CreatedAt <= afterCreation);
        }
    }
}

