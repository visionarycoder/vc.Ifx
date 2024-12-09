    namespace Ifx.UnitTests.Extensions;

    public class HashSetExtensionsTests
    {
        [Fact]
        public void AddRange_ShouldAddElementsToHashSet()
        {
            // Arrange
            var hashSet = new HashSet<int>();
            var collection = new List<int> { 1, 2, 3 };

            // Act
            hashSet.AddRange(collection);

            // Assert
            Assert.Equal(3, hashSet.Count);
            Assert.Contains(1, hashSet);
            Assert.Contains(2, hashSet);
            Assert.Contains(3, hashSet);
        }

        [Fact]
        public void AddRange_ShouldNotAddDuplicateElementsToHashSet()
        {
            // Arrange
            var hashSet = new HashSet<int> { 1, 2 };
            var collection = new List<int> { 2, 3, 4 };

            // Act
            hashSet.AddRange(collection);

            // Assert
            Assert.Equal(4, hashSet.Count);
            Assert.Contains(1, hashSet);
            Assert.Contains(2, hashSet);
            Assert.Contains(3, hashSet);
            Assert.Contains(4, hashSet);
        }
    }