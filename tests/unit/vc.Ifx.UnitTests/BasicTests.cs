// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace VisionaryCoder.Framework.Tests;

/// <summary>
/// Simple test to verify test framework is working correctly.
/// </summary>
[TestClass]
public class BasicTests
{
    [TestMethod]
    public void ServiceCollection_ShouldBeCreatable()
    {
        // Arrange & Act
        var services = new ServiceCollection();

        // Assert
        services.Should().NotBeNull();
        services.Should().BeEmpty();
    }

    [TestMethod]
    public void ServiceCollection_AddService_ShouldRegisterService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTransient<IServiceCollection, ServiceCollection>();

        // Assert
        services.Should().HaveCount(1);
    }
}
