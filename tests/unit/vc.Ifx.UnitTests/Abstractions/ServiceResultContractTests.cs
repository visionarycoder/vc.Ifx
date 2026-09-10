using System.Reflection;

namespace VisionaryCoder.Framework.Tests.Abstractions;

[TestClass]
public sealed class ServiceResultContractTests
{
    [TestMethod]
    public void SuccessUsesStateRatherThanValuePresence()
    {
        var empty = ServiceResult.Success();
        Assert.IsTrue(empty.IsSuccess);
        Assert.IsFalse(empty.IsFailure);
        Assert.IsNull(empty.ErrorMessage);
        Assert.IsNull(empty.Exception);
        AssertSuccess(ServiceResult<string?>.Success(null), null);
        AssertSuccess(ServiceResult<int?>.Success(null), null);
        AssertSuccess(ServiceResult<int>.Success(0), 0);
        AssertSuccess(ServiceResult<string>.Success("value"), "value");
        Assert.AreNotSame(ServiceResult.Success(), ServiceResult.Success());
        Assert.AreNotSame(ServiceResult<int>.Success(1), ServiceResult<int>.Success(1));
    }

    [TestMethod]
    public void FailureFactoriesRetainMessagesAndExceptionIdentity()
    {
        var exception = new InvalidOperationException("original");
        AssertFailure(ServiceResult.Failure("message"), "message", null);
        AssertFailure(ServiceResult.Failure(exception), "original", exception);
        AssertFailure(ServiceResult.Failure("custom", exception), "custom", exception);
        AssertFailure(ServiceResult<string>.Failure("message"), "message", null);
        AssertFailure(ServiceResult<string>.Failure(exception), "original", exception);
        AssertFailure(ServiceResult<string>.Failure("custom", exception), "custom", exception);
        Assert.IsNull(ServiceResult<string>.Failure("message").Value);
        Assert.AreEqual(0, ServiceResult<int>.Failure("message").Value);
        var blank = new Exception("");
        AssertFailure(ServiceResult.Failure(blank), "", blank);
        AssertFailure(ServiceResult<int>.Failure(blank), "", blank);
    }

    [TestMethod]
    public void FailureFactoriesValidateExplicitArguments()
    {
        foreach (string? message in new[] {null, "", " ", "\t"})
        {
            Assert.Throws<ArgumentException>(() => ServiceResult.Failure(message!));
            Assert.Throws<ArgumentException>(() => ServiceResult<int>.Failure(message!));
            Assert.Throws<ArgumentException>(() => ServiceResult.Failure(message!, new Exception()));
            Assert.Throws<ArgumentException>(() => ServiceResult<int>.Failure(message!, new Exception()));
        }
        Assert.ThrowsExactly<ArgumentNullException>(() => ServiceResult.Failure((Exception)null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => ServiceResult<int>.Failure((Exception)null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => ServiceResult.Failure("error", null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => ServiceResult<int>.Failure("error", null!));
    }

    [TestMethod]
    public void MatchSelectsExactlyOneBranchIncludingNullSuccess()
    {
        int calls = 0;
        ServiceResult.Success().Match(() => calls++, (message, exception) => Assert.Fail());
        Assert.AreEqual(1, calls);
        ServiceResult<string?>.Success(null).Match(value =>
        {
            Assert.IsNull(value);
            calls++;
        }, (message, exception) => Assert.Fail());
        Assert.AreEqual(2, calls);
        var original = new Exception("error");
        ServiceResult.Failure(original).Match(() => Assert.Fail(), (message, exception) =>
        {
            Assert.AreEqual("error", message);
            Assert.AreSame(original, exception);
            calls++;
        });
        ServiceResult<int>.Failure("custom").Match(value => Assert.Fail(), (message, exception) =>
        {
            Assert.AreEqual("custom", message);
            Assert.IsNull(exception);
            calls++;
        });
        Assert.AreEqual(4, calls);
    }

    [TestMethod]
    public void MatchValidatesBothDelegatesRegardlessOfState()
    {
        foreach (var result in new[] {ServiceResult.Success(), ServiceResult.Failure("error")})
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => result.Match(null!, (message, exception) => { }));
            Assert.ThrowsExactly<ArgumentNullException>(() => result.Match(() => { }, null!));
        }
        foreach (var result in new[] {ServiceResult<string?>.Success(null), ServiceResult<string?>.Failure("error")})
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => result.Match(null!, (message, exception) => { }));
            Assert.ThrowsExactly<ArgumentNullException>(() => result.Match(value => { }, null!));
        }
    }

    [TestMethod]
    public void MatchDelegateExceptionsPropagate()
    {
        foreach (Exception failure in new Exception[] {new InvalidOperationException(), new OperationCanceledException()})
        {
            Assert.AreSame(failure, Assert.Throws<Exception>(() => ServiceResult.Success().Match(() => throw failure, (message, exception) => { })));
            Assert.AreSame(failure, Assert.Throws<Exception>(() => ServiceResult.Failure("error").Match(() => { }, (message, exception) => throw failure)));
            Assert.AreSame(failure, Assert.Throws<Exception>(() => ServiceResult<int>.Success(1).Match(value => throw failure, (message, exception) => { })));
            Assert.AreSame(failure, Assert.Throws<Exception>(() => ServiceResult<int>.Failure("error").Match(value => { }, (message, exception) => throw failure)));
        }
    }

    [TestMethod]
    public async Task MappingSupportsNullableReferencesNullableValuesAndNullOutput()
    {
        AssertSuccess(ServiceResult<string?>.Success(null).Map(value => value?.Length ?? 0), 0);
        AssertSuccess(ServiceResult<int?>.Success(null).Map(value => value ?? 42), 42);
        AssertSuccess(ServiceResult<int>.Success(0).Map<string?>(value => null), null);
        AssertSuccess(await ServiceResult<string?>.Success(null).MapAsync(value => Task.FromResult(value?.Length ?? 0)), 0);
        AssertSuccess(await ServiceResult<int?>.Success(null).MapAsync(value => Task.FromResult(value ?? 42)), 42);
        AssertSuccess(await ServiceResult<int>.Success(0).MapAsync(value => Task.FromResult<string?>(null)), null);
        var completion = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<ServiceResult<string>> pending = ServiceResult<int>.Success(1).MapAsync(value => completion.Task);
        Assert.IsFalse(pending.IsCompleted);
        completion.SetResult("completed");
        AssertSuccess(await pending, "completed");
    }

    [TestMethod]
    public async Task FailuresShortCircuitAndCopyMetadataWithoutRevalidation()
    {
        var original = new Exception("original");
        var empty = new Exception("");
        foreach (var result in new[]
        {
            ServiceResult<int>.Failure("message"), ServiceResult<int>.Failure(original),
            ServiceResult<int>.Failure("custom", original), ServiceResult<int>.Failure(empty)
        })
        {
            var mapped = result.Map<string>(value => throw new AssertFailedException("Mapper must not run."));
            var asyncMapped = await result.MapAsync<string>(value => throw new AssertFailedException("Mapper must not run."));
            AssertFailure(mapped, result.ErrorMessage!, result.Exception);
            AssertFailure(asyncMapped, result.ErrorMessage!, result.Exception);
            Assert.IsNull(mapped.Value);
            Assert.IsNull(asyncMapped.Value);
        }
    }

    [TestMethod]
    public async Task MappingValidatesDelegatesBeforeShortCircuiting()
    {
        foreach (var result in new[] {ServiceResult<int>.Success(1), ServiceResult<int>.Failure("error")})
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => result.Map<string>(null!));
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => result.MapAsync<string>(null!));
        }
    }

    [TestMethod]
    public async Task OrdinaryMapperFailuresBecomeResults()
    {
        var failure = new InvalidOperationException("private");
        var source = ServiceResult<int>.Success(42);
        AssertFailure(source.Map<string>(value => throw failure), "private", failure);
        AssertFailure(await source.MapAsync<string>(value => throw failure), "private", failure);
        AssertFailure(await source.MapAsync(value => Task.FromException<string>(failure)), "private", failure);
        var nullTask = await source.MapAsync<string>(value => null!);
        Assert.IsTrue(nullTask.IsFailure);
        Assert.IsInstanceOfType<NullReferenceException>(nullTask.Exception);
    }

    [TestMethod]
    public async Task CancellationIsNeverAnOrdinaryFailure()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        foreach (var cancellation in new OperationCanceledException[]
        {
            new OperationCanceledException(source.Token), new TaskCanceledException("canceled", null, source.Token)
        })
        {
            Assert.AreSame(cancellation, Assert.Throws<OperationCanceledException>(() => ServiceResult.Failure(cancellation)));
            Assert.AreSame(cancellation, Assert.Throws<OperationCanceledException>(() => ServiceResult.Failure("error", cancellation)));
            Assert.AreSame(cancellation, Assert.Throws<OperationCanceledException>(() => ServiceResult<int>.Failure(cancellation)));
            Assert.AreSame(cancellation, Assert.Throws<OperationCanceledException>(() => ServiceResult<int>.Failure("error", cancellation)));
            Assert.AreSame(cancellation, Assert.Throws<OperationCanceledException>(() =>
                ServiceResult<int>.Success(1).Map<int>(value => throw cancellation)));
            Assert.AreSame(cancellation, await Assert.ThrowsAsync<OperationCanceledException>(() =>
                ServiceResult<int>.Success(1).MapAsync<int>(value => throw cancellation)));
            Assert.AreSame(cancellation, await Assert.ThrowsAsync<OperationCanceledException>(() =>
                ServiceResult<int>.Success(1).MapAsync(value => Task.FromException<int>(cancellation))));
        }
        Task<ServiceResult<int>> canceled = ServiceResult<int>.Success(1).MapAsync(value => Task.FromCanceled<int>(source.Token));
        var thrown = await Assert.ThrowsAsync<OperationCanceledException>(() => canceled);
        Assert.AreEqual(source.Token, thrown.CancellationToken);
        Assert.IsTrue(canceled.IsCanceled);
    }

    [TestMethod]
    public void LegacyPrivateStateStillUsesDefensiveMatchFallback()
    {
        // Preserve the pre-existing defensive behavior for private-constructor callers.
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var untyped = (ServiceResult)typeof(ServiceResult).GetConstructors(flags).Single().Invoke([false, null, null]);
        var typed = (ServiceResult<string>)typeof(ServiceResult<string>).GetConstructors(flags).Single().Invoke([false, null, null, null]);
        untyped.Match(() => Assert.Fail(), (message, exception) => Assert.AreEqual("Unknown error", message));
        typed.Match(value => Assert.Fail(), (message, exception) => Assert.AreEqual("Unknown error", message));
    }

    private static void AssertSuccess<T>(ServiceResult<T> result, T? value)
    {
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.IsFailure);
        Assert.AreEqual(value, result.Value);
        Assert.IsNull(result.ErrorMessage);
        Assert.IsNull(result.Exception);
    }

    private static void AssertFailure(ServiceResultBase result, string message, Exception? exception)
    {
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(message, result.ErrorMessage);
        Assert.AreSame(exception, result.Exception);
    }
}
