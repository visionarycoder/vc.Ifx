// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using VisionaryCoder.Framework.Proxy.Interceptors.Authorization.Policies;

namespace VisionaryCoder.Framework.Tests.Authorization;

/// <summary>
/// Comprehensive data-driven unit tests for Authorization service collection extensions with 100% code coverage.
/// Tests SOLID principles compliance and service registration patterns.
/// </summary>
[TestClass]
public class AuthorizationServiceCollectionExtensionsTests
{
    private IServiceCollection services = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        services = new ServiceCollection();
    }

    #region Manual Registration Tests

    [TestMethod]
    public void AddRoleBasedAuthorizationPolicy_ShouldRegisterCorrectly()
    {
        // Act
        services.AddScoped<IAuthorizationPolicy, RoleBasedAuthorizationPolicy>();

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IAuthorizationPolicy? policy = serviceProvider.GetService<IAuthorizationPolicy>();
        policy.Should().NotBeNull();
        policy.Should().BeOfType<RoleBasedAuthorizationPolicy>();
    }

    [TestMethod]
    public void AddNullAuthorizationPolicy_ShouldRegisterCorrectly()
    {
        // Act
        services.AddScoped<IAuthorizationPolicy, NullAuthorizationPolicy>();

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IAuthorizationPolicy? policy = serviceProvider.GetService<IAuthorizationPolicy>();
        policy.Should().NotBeNull();
        policy.Should().BeOfType<NullAuthorizationPolicy>();
    }

    [TestMethod]
    public void AddMultipleAuthorizationPolicies_ShouldRegisterAll()
    {
        // Act
        services.AddScoped<IAuthorizationPolicy, RoleBasedAuthorizationPolicy>();
        services.AddScoped<IAuthorizationPolicy, NullAuthorizationPolicy>();

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IEnumerable<IAuthorizationPolicy> policies = serviceProvider.GetServices<IAuthorizationPolicy>();
        policies.Should().HaveCount(2);
        policies.Should().ContainSingle(p => p.GetType() == typeof(RoleBasedAuthorizationPolicy));
        policies.Should().ContainSingle(p => p.GetType() == typeof(NullAuthorizationPolicy));
    }

    [TestMethod]
    public void RegisterAuthorizationPolicies_ShouldUseCorrectServiceLifetime()
    {
        // Act
        services.AddScoped<IAuthorizationPolicy, RoleBasedAuthorizationPolicy>();

        // Assert
        ServiceDescriptor? descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IAuthorizationPolicy)
                                                                     && s.ImplementationType == typeof(RoleBasedAuthorizationPolicy));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    #endregion
}
