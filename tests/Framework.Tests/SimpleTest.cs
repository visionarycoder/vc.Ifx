// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Tests;

[TestClass]
public class SimpleTest
{
    [TestMethod]
    public void SimpleTest_ShouldPass()
    {
        // Arrange
        bool expected = true;

        // Act
        bool actual = true;

        // Assert
        Assert.AreEqual(expected, actual);
    }
}
