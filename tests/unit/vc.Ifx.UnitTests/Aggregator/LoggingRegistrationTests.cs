using Microsoft.Extensions.DependencyInjection;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Logging;
using LoggingRegistration = VisionaryCoder.Framework.Logging.LoggingExtensions;

namespace VisionaryCoder.Framework.Tests.Aggregator;

[TestClass]
public sealed class LoggingRegistrationTests
{
    [TestMethod]
    public void ConfiguredLoggingIncludesBothInterceptorsAndSnapshotsThresholds()
    {
        var services = Services();
        VisionaryCoder.Framework.Logging.LoggingOptions? captured = null;
        Assert.AreSame(services, LoggingRegistration.AddLogging(services, options =>
        {
            Assert.IsTrue(options.EnableStandardLogging);
            Assert.IsTrue(options.EnableTiming);
            Assert.AreEqual(1000L, options.SlowOperationThresholdMs);
            Assert.AreEqual(5000L, options.CriticalOperationThresholdMs);
            options.SlowOperationThresholdMs = 10;
            options.CriticalOperationThresholdMs = 20;
            captured = options;
        }));
        captured!.SlowOperationThresholdMs = 30;
        using var provider = services.BuildServiceProvider();
        var interceptors = provider.GetServices<IOrderedProxyInterceptor>().ToArray();
        Assert.AreEqual(2, interceptors.Length);
        Assert.IsInstanceOfType<LoggingInterceptor>(interceptors[0]);
        var timing = (TimingInterceptor)interceptors[1];
        Assert.AreEqual(10L, timing.SlowOperationThresholdMs);
        Assert.AreEqual(20L, timing.CriticalOperationThresholdMs);
    }

    [TestMethod]
    public void DisabledOptionsDoNotRegisterInterceptors()
    {
        var services = Services();
        LoggingRegistration.AddLogging(services, options =>
        {
            options.EnableStandardLogging = false;
            options.EnableTiming = false;
        });
        using var provider = services.BuildServiceProvider();
        Assert.AreEqual(0, provider.GetServices<IOrderedProxyInterceptor>().Count());
    }

    [TestMethod]
    public void AddMethodsAreIdempotentPerImplementationAndPreserveOtherInterceptors()
    {
        var services = Services();
        Assert.AreSame(services, LoggingRegistration.AddLogging(services));
        Assert.AreSame(services, LoggingRegistration.AddNullLogging(services));
        Assert.AreSame(services, LoggingRegistration.AddLoggingInterceptor(services));
        LoggingRegistration.AddLoggingInterceptor(services);
        Assert.AreSame(services, LoggingRegistration.AddTimingInterceptor(services, 12, 24));
        LoggingRegistration.AddTimingInterceptor(services, 15, 30);
        Assert.AreSame(services, LoggingRegistration.AddLogging<NullLoggingInterceptor>(services));
        using var provider = services.BuildServiceProvider();
        var interceptors = provider.GetServices<IOrderedProxyInterceptor>().ToArray();
        Assert.AreEqual(3, interceptors.Length);
        Assert.IsInstanceOfType<NullLoggingInterceptor>(interceptors[0]);
        var timing = interceptors.OfType<TimingInterceptor>().Single();
        Assert.AreEqual(12L, timing.SlowOperationThresholdMs);
        Assert.AreEqual(24L, timing.CriticalOperationThresholdMs);
        Assert.AreSame(timing, provider.GetServices<IOrderedProxyInterceptor>().OfType<TimingInterceptor>().Single());
    }

    [TestMethod]
    public void ExplicitUseMethodsAppendAndDoNotReplaceUnrelatedRegistrations()
    {
        var services = Services();
        Assert.AreSame(services, LoggingRegistration.UseDefaultLoggingInterceptors(services));
        Assert.AreSame(services, LoggingRegistration.UseLoggingInterceptor(services));
        Assert.AreSame(services, LoggingRegistration.UseTimingInterceptor(services, 7, 14));
        using var provider = services.BuildServiceProvider();
        var interceptors = provider.GetServices<IOrderedProxyInterceptor>().ToArray();
        Assert.AreEqual(4, interceptors.Length);
        Assert.AreEqual(2, interceptors.OfType<LoggingInterceptor>().Count());
        var timing = interceptors.OfType<TimingInterceptor>().ToArray();
        Assert.AreEqual(1000L, timing[0].SlowOperationThresholdMs);
        Assert.AreEqual(5000L, timing[0].CriticalOperationThresholdMs);
        Assert.AreEqual(7L, timing[1].SlowOperationThresholdMs);
        Assert.AreEqual(14L, timing[1].CriticalOperationThresholdMs);
    }

    [TestMethod]
    public void NullRegistrationArgumentsAreRejected()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => LoggingRegistration.AddLogging(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => LoggingRegistration.AddNullLogging(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => LoggingRegistration.AddLoggingInterceptor(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => LoggingRegistration.AddTimingInterceptor(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => LoggingRegistration.AddLogging<NullLoggingInterceptor>(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => LoggingRegistration.AddLogging(null!, options => { }));
        Assert.ThrowsExactly<ArgumentNullException>(() => LoggingRegistration.AddLogging(new ServiceCollection(), null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => LoggingRegistration.UseLoggingInterceptor(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => LoggingRegistration.UseTimingInterceptor(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => LoggingRegistration.UseDefaultLoggingInterceptors(null!));
    }

    private static ServiceCollection Services()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        return services;
    }
}
