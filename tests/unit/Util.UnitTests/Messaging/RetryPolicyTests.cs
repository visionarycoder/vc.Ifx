using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Util.Messaging;

namespace Util.UnitTests.Messaging;

[TestClass]
public sealed class RetryPolicyTests
{
    #region ExecuteAsync Tests

    [TestMethod]
    public async Task ExecuteAsync_WhenOperationSucceedsImmediately_ShouldRunOnce()
    {
        var attempts = 0;

        await RetryPolicy.ExecuteAsync(
            () =>
            {
                attempts++;
                return Task.CompletedTask;
            },
            maxAttempts: 3,
            initialDelay: TimeSpan.Zero);

        attempts.Should().Be(1);
    }

    [TestMethod]
    public async Task ExecuteAsync_WhenOperationEventuallySucceeds_ShouldRetryUntilSuccess()
    {
        var attempts = 0;

        await RetryPolicy.ExecuteAsync(
            () =>
            {
                attempts++;

                if (attempts < 3)
                {
                    throw new InvalidOperationException("Transient failure.");
                }

                return Task.CompletedTask;
            },
            maxAttempts: 3,
            initialDelay: TimeSpan.Zero);

        attempts.Should().Be(3);
    }

    [TestMethod]
    public async Task ExecuteAsync_WhenOperationNeverSucceeds_ShouldRethrowLastException()
    {
        var attempts = 0;

        Func<Task> action = () => RetryPolicy.ExecuteAsync(
            () =>
            {
                attempts++;
                throw new InvalidOperationException("Transient failure.");
            },
            maxAttempts: 3,
            initialDelay: TimeSpan.Zero);

        var assertions = await action.Should().ThrowAsync<InvalidOperationException>();

        assertions.Which.Message.Should().Be("Transient failure.");
        attempts.Should().Be(3);
    }

    [TestMethod]
    public async Task ExecuteAsync_WhenInitialDelayIsNegative_ShouldThrowArgumentOutOfRangeException()
    {
        Func<Task> action = () => RetryPolicy.ExecuteAsync(
            () => Task.CompletedTask,
            maxAttempts: 3,
            initialDelay: TimeSpan.FromMilliseconds(-1));

        var assertions = await action.Should().ThrowAsync<ArgumentOutOfRangeException>();

        assertions.Which.ParamName.Should().Be("initialDelay");
    }

    [TestMethod]
    public async Task ExecuteAsync_WhenCancellationAlreadyRequested_ShouldThrowOperationCanceledException()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Func<Task> action = () => RetryPolicy.ExecuteAsync(
            () => Task.CompletedTask,
            maxAttempts: 3,
            initialDelay: TimeSpan.Zero,
            cancellationToken: cancellation.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public async Task ExecuteAsync_WhenMaxAttemptsIsOne_ShouldExecuteOnceWithoutRetryPipeline()
    {
        var attempts = 0;

        Func<Task> action = () => RetryPolicy.ExecuteAsync(
            () =>
            {
                attempts++;
                throw new InvalidOperationException("Immediate failure.");
            },
            maxAttempts: 1,
            initialDelay: TimeSpan.Zero);

        await action.Should().ThrowAsync<InvalidOperationException>();

        attempts.Should().Be(1);
    }

    #endregion
}
