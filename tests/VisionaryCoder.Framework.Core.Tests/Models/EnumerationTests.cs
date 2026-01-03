// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using FluentAssertions;
using VisionaryCoder.Framework.Core.Models;

namespace VisionaryCoder.Framework.Core.Tests.Models;

[TestClass]
public class EnumerationTests
{
    // Test enumeration
    private class TestStatus : Enumeration
    {
        public static readonly TestStatus Pending = new(1, nameof(Pending));
        public static readonly TestStatus Active = new(2, nameof(Active));
        public static readonly TestStatus Completed = new(3, nameof(Completed));

        protected TestStatus(int id, string name) : base(id, name) { }
    }

    [TestMethod]
    public void GetAll_ShouldReturnAllValues()
    {
        // Act
        var all = Enumeration.GetAll<TestStatus>().ToList();

        // Assert
        all.Should().HaveCount(3);
        all.Should().Contain(TestStatus.Pending);
        all.Should().Contain(TestStatus.Active);
        all.Should().Contain(TestStatus.Completed);
    }

    [TestMethod]
    public void FromValue_WithValidId_ShouldReturnCorrectValue()
    {
        // Act
        var status = Enumeration.FromValue<TestStatus>(2);

        // Assert
        status.Should().Be(TestStatus.Active);
        status.Id.Should().Be(2);
        status.Name.Should().Be("Active");
    }

    [TestMethod]
    public void FromValue_WithInvalidId_ShouldThrowInvalidOperationException()
    {
        // Act
        Action act = () => Enumeration.FromValue<TestStatus>(999);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not a valid value*");
    }

    [TestMethod]
    public void FromDisplayName_WithValidName_ShouldReturnCorrectValue()
    {
        // Act
        var status = Enumeration.FromDisplayName<TestStatus>("Pending");

        // Assert
        status.Should().Be(TestStatus.Pending);
        status.Id.Should().Be(1);
    }

    [TestMethod]
    public void FromDisplayName_WithInvalidName_ShouldThrowInvalidOperationException()
    {
        // Act
        Action act = () => Enumeration.FromDisplayName<TestStatus>("Invalid");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not a valid display name*");
    }

    [TestMethod]
    public void TryFromValue_WithValidId_ShouldReturnTrueAndValue()
    {
        // Act
        bool result = Enumeration.TryFromValue<TestStatus>(3, out TestStatus? status);

        // Assert
        result.Should().BeTrue();
        status.Should().Be(TestStatus.Completed);
    }

    [TestMethod]
    public void TryFromValue_WithInvalidId_ShouldReturnFalse()
    {
        // Act
        bool result = Enumeration.TryFromValue<TestStatus>(999, out TestStatus? status);

        // Assert
        result.Should().BeFalse();
        status.Should().BeNull();
    }

    [TestMethod]
    public void TryFromDisplayName_WithValidName_ShouldReturnTrueAndValue()
    {
        // Act
        bool result = Enumeration.TryFromDisplayName<TestStatus>("Active", out TestStatus? status);

        // Assert
        result.Should().BeTrue();
        status.Should().Be(TestStatus.Active);
    }

    [TestMethod]
    public void Equals_SameIdAndType_ShouldReturnTrue()
    {
        // Arrange
        var status1 = TestStatus.Active;
        var status2 = Enumeration.FromValue<TestStatus>(2);

        // Assert
        status1.Equals(status2).Should().BeTrue();
        (status1 == status2).Should().BeTrue();
    }

    [TestMethod]
    public void Equals_DifferentId_ShouldReturnFalse()
    {
        // Arrange
        var status1 = TestStatus.Active;
        var status2 = TestStatus.Pending;

        // Assert
        status1.Equals(status2).Should().BeFalse();
        (status1 == status2).Should().BeFalse();
    }

    [TestMethod]
    public void CompareTo_ShouldCompareById()
    {
        // Assert
        (TestStatus.Pending < TestStatus.Active).Should().BeTrue();
        (TestStatus.Active > TestStatus.Pending).Should().BeTrue();
        (TestStatus.Active <= TestStatus.Completed).Should().BeTrue();
        (TestStatus.Completed >= TestStatus.Active).Should().BeTrue();
    }

    [TestMethod]
    public void ToString_ShouldReturnName()
    {
        // Assert
        TestStatus.Active.ToString().Should().Be("Active");
    }

    [TestMethod]
    public void AbsoluteDifference_ShouldCalculateCorrectly()
    {
        // Act
        int difference = Enumeration.AbsoluteDifference(TestStatus.Pending, TestStatus.Completed);

        // Assert
        difference.Should().Be(2);
    }
}
