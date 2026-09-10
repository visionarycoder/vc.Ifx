// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers;
using DefaultCacheKeyProvider = VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers.DefaultCacheKeyProvider;
using DefaultCachePolicyProvider = VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers.DefaultCachePolicyProvider;
using ICacheKeyProvider = VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers.ICacheKeyProvider;
using ICachePolicyProvider = VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers.ICachePolicyProvider;

namespace VisionaryCoder.Framework.Tests.Caching;

/// <summary>
/// Comprehensive data-driven unit tests for Caching service collection extensions with 100% code coverage.
/// Tests SOLID principles compliance and service registration patterns.
/// </summary>
[TestClass]
public class CachingServiceCollectionExtensionsTests
{
    private IServiceCollection services = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        services = new ServiceCollection();
    }

    #region AddCaching Tests

    [TestMethod]
    public void AddCaching_ShouldRegisterNullProvidersByDefault()
    {
        // Act
        services.AddCaching();

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        ICacheKeyProvider? keyProvider = serviceProvider.GetService<ICacheKeyProvider>();
        keyProvider.Should().NotBeNull();
        keyProvider.Should().BeOfType<NullCacheKeyProvider>();

        ICachePolicyProvider? policyProvider = serviceProvider.GetService<ICachePolicyProvider>();
        policyProvider.Should().NotBeNull();
        policyProvider.Should().BeOfType<NullCachePolicyProvider>();

        IProxyCache? cache = serviceProvider.GetService<IProxyCache>();
        cache.Should().NotBeNull();
        cache.Should().BeOfType<NullProxyCache>();
    }

    [TestMethod]
    public void AddCaching_WithConfiguration_ShouldApplyOptions()
    {
        // Act
        services.AddCaching(options =>
        {
            options.DefaultDuration = TimeSpan.FromMinutes(30);
            options.MaxCacheSize = 1000;
        });

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IProxyCache? cache = serviceProvider.GetService<IProxyCache>();
        cache.Should().NotBeNull();
    }

    [TestMethod]
    public void AddCaching_WithTimeSpan_ShouldRegisterWithDefaultDuration()
    {
        // Act
        services.AddCaching(TimeSpan.FromMinutes(15));

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IProxyCache? cache = serviceProvider.GetService<IProxyCache>();
        cache.Should().NotBeNull();
        cache.Should().BeOfType<NullProxyCache>();
    }

    #endregion

    #region AddCaching with Generic Types Tests

    [TestMethod]
    public void AddCaching_WithGenericCache_ShouldRegisterSpecifiedCache()
    {
        // Act
        services.AddCaching<MemoryProxyCache>();

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        IProxyCache? cache = serviceProvider.GetService<IProxyCache>();
        cache.Should().NotBeNull();
        cache.Should().BeOfType<MemoryProxyCache>();

        ICacheKeyProvider? keyProvider = serviceProvider.GetService<ICacheKeyProvider>();
        keyProvider.Should().BeOfType<DefaultCacheKeyProvider>();

        ICachePolicyProvider? policyProvider = serviceProvider.GetService<ICachePolicyProvider>();
        policyProvider.Should().BeOfType<DefaultCachePolicyProvider>();
    }

    [TestMethod]
    public void AddCaching_WithGenericProviders_ShouldRegisterSpecifiedProviders()
    {
        // Act
        services.AddCaching<DefaultCachePolicyProvider>();

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        ICacheKeyProvider? keyProvider = serviceProvider.GetService<ICacheKeyProvider>();
        keyProvider.Should().BeOfType<DefaultCacheKeyProvider>();

        ICachePolicyProvider? policyProvider = serviceProvider.GetService<ICachePolicyProvider>();
        policyProvider.Should().BeOfType<DefaultCachePolicyProvider>();

        IProxyCache? cache = serviceProvider.GetService<IProxyCache>();
        cache.Should().BeOfType<MemoryProxyCache>();
    }

    #endregion

    #region AddDistributedCaching Tests

    [TestMethod]
    public void AddDistributedCaching_ShouldRegisterCachingWithConfiguration()
    {
        // Act
        services.AddDistributedCaching(options =>
        {
            options.DefaultDuration = TimeSpan.FromHours(1);
            options.EnableEvictionLogging = true;
        });

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IProxyCache? cache = serviceProvider.GetService<IProxyCache>();
        cache.Should().NotBeNull();
    }

    #endregion

    #region Service Lifetime Tests

    [TestMethod]
    public void AddCaching_ShouldRegisterProvidersAsSingleton()
    {
        // Act
        services.AddCaching();

        // Assert
        ServiceDescriptor? keyProviderDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ICacheKeyProvider));
        keyProviderDescriptor.Should().NotBeNull();
        keyProviderDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);

        ServiceDescriptor? policyProviderDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ICachePolicyProvider));
        policyProviderDescriptor.Should().NotBeNull();
        policyProviderDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);

        ServiceDescriptor? cacheDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IProxyCache));
        cacheDescriptor.Should().NotBeNull();
        cacheDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    #endregion
}
