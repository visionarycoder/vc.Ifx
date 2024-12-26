    using vc.Ifx.Extensions;

    namespace vc.Ifx.Cli.UnitTests.Extensions;

    public class CollectionExtensionsTests
    {
        [Fact]
        public void IsNullOrEmpty_ShouldReturnTrue_WhenCollectionIsNull()
        {
            // Arrange
            ICollection<int>? collection = null;

            // Act
            var result = collection.IsNullOrEmpty();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsNullOrEmpty_ShouldReturnTrue_WhenCollectionIsEmpty()
        {
            // Arrange
            var collection = new List<int>();

            // Act
            var result = collection.IsNullOrEmpty();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsNullOrEmpty_ShouldReturnFalse_WhenCollectionIsNotEmpty()
        {
            // Arrange
            var collection = new List<int> { 1, 2, 3 };

            // Act
            var result = collection.IsNullOrEmpty();

            // Assert
            Assert.False(result);
        }
    }