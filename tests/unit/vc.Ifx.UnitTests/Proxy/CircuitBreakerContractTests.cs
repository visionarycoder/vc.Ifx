using Microsoft.Extensions.Logging.Abstractions;
using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Exceptions;
using VisionaryCoder.Framework.Proxy.Interceptors.Retries;

namespace VisionaryCoder.Framework.Tests.Proxy;

[TestClass]
public sealed class CircuitBreakerContractTests
{
    [TestMethod]
    public async Task ConsecutiveFailuresOpenAndSuccessResetsTheCounter()
    {
        var breaker = new CircuitBreakerInterceptor(NullLogger<CircuitBreakerInterceptor>.Instance, 2);
        var context = new ProxyContext();
        Task<ProxyResponse<int>> Fail(ProxyContext request, CancellationToken token) => throw new InvalidOperationException("failure");
        Task<ProxyResponse<int>> Success(ProxyContext request, CancellationToken token) => Task.FromResult(ProxyResponse<int>.Success(1));
        Func<Task> failure = () => breaker.InvokeAsync<int>(context, Fail);
        await failure.Should().ThrowAsync<InvalidOperationException>();
        breaker.State.Should().Be(CircuitBreakerState.Closed);
        await breaker.InvokeAsync<int>(context, Success);
        await failure.Should().ThrowAsync<InvalidOperationException>();
        breaker.State.Should().Be(CircuitBreakerState.Closed);
        await failure.Should().ThrowAsync<InvalidOperationException>();
        breaker.State.Should().Be(CircuitBreakerState.Open);
        Func<Task> open = () => breaker.InvokeAsync<int>(context, Success);
        await open.Should().ThrowAsync<TransientProxyException>();
        context.Metadata["CircuitBreakerState"].Should().Be("Open");
    }

    [TestMethod]
    public async Task CancellationDoesNotTripAndArgumentsAreValidated()
    {
        Action missing = () => new CircuitBreakerInterceptor(null!);
        missing.Should().Throw<ArgumentNullException>();
        Action threshold = () => new CircuitBreakerInterceptor(NullLogger<CircuitBreakerInterceptor>.Instance, 0);
        threshold.Should().Throw<ArgumentOutOfRangeException>();
        Action duration = () => new CircuitBreakerInterceptor(NullLogger<CircuitBreakerInterceptor>.Instance, 1, TimeSpan.Zero);
        duration.Should().Throw<ArgumentOutOfRangeException>();
        var breaker = new CircuitBreakerInterceptor(NullLogger<CircuitBreakerInterceptor>.Instance, 1, TimeSpan.FromMinutes(1));
        Task<ProxyResponse<int>> Canceled(ProxyContext request, CancellationToken token) => throw new OperationCanceledException();
        Func<Task> cancellation = () => breaker.InvokeAsync<int>(new ProxyContext(), Canceled);
        await cancellation.Should().ThrowAsync<OperationCanceledException>();
        breaker.State.Should().Be(CircuitBreakerState.Closed);
        Func<Task> missingContext = () => breaker.InvokeAsync<int>(null!, Canceled);
        await missingContext.Should().ThrowAsync<ArgumentNullException>();
        Func<Task> missingNext = () => breaker.InvokeAsync<int>(new ProxyContext(), null!);
        await missingNext.Should().ThrowAsync<ArgumentNullException>();
        Func<Task> preCanceled = () => breaker.InvokeAsync<int>(new ProxyContext(), Canceled, new CancellationToken(true));
        await preCanceled.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public async Task HalfOpenPermitsOneTrialAndRecoversOnSuccess()
    {
        var breaker = new CircuitBreakerInterceptor(NullLogger<CircuitBreakerInterceptor>.Instance, 1, TimeSpan.FromSeconds(1));
        Func<Task> failure = () => breaker.InvokeAsync<int>(new ProxyContext(), (context, token) => throw new InvalidOperationException());
        await failure.Should().ThrowAsync<InvalidOperationException>();
        // This checks the externally observable timed contract without altering Polly's global clock.
        await Task.Delay(1100);
        breaker.State.Should().Be(CircuitBreakerState.HalfOpen);
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<ProxyResponse<int>> trial = breaker.InvokeAsync<int>(new ProxyContext(), async (context, token) =>
        {
            entered.SetResult();
            await release.Task;
            return ProxyResponse<int>.Success(42);
        });
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        try
        {
            Func<Task> competing = () => breaker.InvokeAsync<int>(new ProxyContext(), (context, token) => Task.FromResult(ProxyResponse<int>.Success(0)));
            await competing.Should().ThrowAsync<TransientProxyException>();
        }
        finally { release.SetResult(); }
        (await trial).Data.Should().Be(42);
        breaker.State.Should().Be(CircuitBreakerState.Closed);
    }
}
