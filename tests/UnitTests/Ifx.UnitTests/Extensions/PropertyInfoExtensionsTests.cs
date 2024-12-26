    using vc.Ifx.Extensions;

    namespace vc.Ifx.Cli.UnitTests.Extensions;

    public class PropertyInfoExtensionsTests
    {
        private class TestClass
        {
            public string Name { get; set; } = string.Empty;
            public List<int> Numbers { get; set; } = new List<int>();
        }

        [Fact]
        public void IsNonStringEnumerable_ShouldReturnTrue_ForNonStringEnumerableProperty()
        {
            // Arrange
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.Numbers));

            // Act
            var result = propertyInfo.IsNonStringEnumerable();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsNonStringEnumerable_ShouldReturnFalse_ForStringProperty()
        {
            // Arrange
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.Name));

            // Act
            var result = propertyInfo.IsNonStringEnumerable();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsNonStringEnumerable_ShouldReturnTrue_ForNonStringEnumerableInstance()
        {
            // Arrange
            var instance = new List<int>();

            // Act
            var result = instance.IsNonStringEnumerable();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsNonStringEnumerable_ShouldReturnFalse_ForStringInstance()
        {
            // Arrange
            var instance = "test";

            // Act
            var result = instance.IsNonStringEnumerable();

            // Assert
            Assert.False(result);
        }
    }