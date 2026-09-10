using vc.Ifx.Abstractions.Time;

namespace VisionaryCoder.Framework.Tests.Abstractions;

[TestClass]
public sealed class RequestContractTests
{
    [TestMethod]
    public void RequestsKeepTheirEmptyInheritableReferenceShapes()
    {
        var request = new ServiceRequest();
        var typed = new ServiceRequest<string>();
        Assert.IsInstanceOfType<ServiceRequest>(typed);
        Assert.AreEqual(0, typeof(ServiceRequest).GetProperties().Length);
        Assert.AreEqual(0, typeof(ServiceRequest<string>).GetProperties().Length);
        Assert.IsNotNull(typeof(ServiceRequest).GetConstructor(Type.EmptyTypes));
        Assert.IsNotNull(typeof(ServiceRequest<string>).GetConstructor(Type.EmptyTypes));
        Assert.AreNotEqual(request, new ServiceRequest());
        Assert.AreNotEqual(typed, new ServiceRequest<string>());
        Assert.IsInstanceOfType<ServiceRequest>(new DerivedRequest());
        Assert.IsInstanceOfType<ServiceRequest<int>>(new TypedRequest());
    }

    [TestMethod]
    public void ClockContractRemainsReplaceableWithoutAProviderDependency()
    {
        var expected = new DateTimeOffset(2026, 9, 9, 0, 0, 0, TimeSpan.Zero);
        IClock clock = new FixedClock(expected);
        Assert.AreEqual(expected, clock.UtcNow);
    }

    private sealed class DerivedRequest : ServiceRequest;
    private sealed class TypedRequest : ServiceRequest<int>;
    private sealed class FixedClock(DateTimeOffset value) : IClock
    {
        public DateTimeOffset UtcNow => value;
    }
}
