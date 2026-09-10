using System.Reflection;
using System.Runtime.ExceptionServices;
using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors;
using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Core;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies;

/// <summary>
/// A dynamic proxy that intercepts method calls on a contract interface.
/// Builds a pipeline of interceptors that wrap the actual target method invocation,
/// handling method return types (void, Task, ValueTask, Task&lt;T&gt;, ValueTask&lt;T&gt;).
/// </summary>
/// <typeparam name="TContract">The contract interface type to intercept.</typeparam>
public sealed class BoundaryProxy<TContract> : DispatchProxy
    where TContract : class
{
    private TContract target = null!;

    private IReadOnlyList<IProxyInterceptor> interceptors = [];

    /// <summary>
    /// Creates a new dynamic proxy instance that wraps the target and applies interceptors.
    /// </summary>
    /// <param name="target">The actual implementation instance to wrap.</param>
    /// <param name="interceptors">The interceptors to apply in sequence.</param>
    /// <returns>A proxy instance that implements TContract.</returns>
    /// <exception cref="ArgumentNullException">Thrown if target or interceptors is null.</exception>
    public static TContract Create(
        TContract target,
        IEnumerable<IProxyInterceptor> interceptors)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(interceptors);

        var proxy =
            Create<TContract, BoundaryProxy<TContract>>();

        var interceptor =
            (BoundaryProxy<TContract>)(object)proxy;

        interceptor.target = target;
        interceptor.interceptors = interceptors.ToArray();

        return proxy;
    }

    /// <summary>
    /// Handles method invocations on the proxy.
    /// Builds the interception pipeline and adapts return values to the expected type.
    /// </summary>
    /// <param name="targetMethod">The method being invoked.</param>
    /// <param name="args">The arguments passed to the method.</param>
    /// <returns>The result of the invocation, adapted to the expected return type.</returns>
    protected override object? Invoke(
        MethodInfo? targetMethod,
        object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(targetMethod);

        args ??= [];

        var parameters = targetMethod.GetParameters();

        var arguments = parameters
            .Select((parameter, index) =>
                new MethodArgument(
                    parameter.Name ?? $"arg{index}",
                    parameter.ParameterType,
                    args[index]))
            .ToArray();

        var context = new MethodContext
        {
            ContractType = typeof(TContract),
            MethodInfo = targetMethod,
            Target = target,
            Arguments = arguments
        };

        // Build the initial pipeline that invokes the target method
        HandlerDelegate pipeline =
            () => InvokeTargetAsync(targetMethod, args);

        // Wrap each interceptor in reverse order to create the full pipeline
        for (var index = interceptors.Count - 1;
             index >= 0;
             index--)
        {
            var current = interceptors[index];
            var next = pipeline;

            pipeline = () =>
                current.InvokeAsync(context, next);
        }

        // Adapt the pipeline result to the expected return type
        return AdaptReturnValue(
            targetMethod.ReturnType,
            pipeline);
    }

    /// <summary>
    /// Invokes the target method using reflection and handles any exceptions properly.
    /// </summary>
    private async ValueTask<object?> InvokeTargetAsync(
        MethodInfo method,
        object?[] args)
    {
        object? result;

        try
        {
            result = method.Invoke(target, args);
        }
        catch (TargetInvocationException exception)
            when (exception.InnerException is not null)
        {
            // Unwrap and re-throw the inner exception to preserve the original stack trace
            ExceptionDispatchInfo
                .Capture(exception.InnerException)
                .Throw();

            throw;
        }

        return await AwaitResultAsync(
            method.ReturnType,
            result);
    }

    /// <summary>
    /// Adapts the pipeline result to match the expected return type of the method.
    /// Handles void, Task, ValueTask, Task&lt;T&gt;, and ValueTask&lt;T&gt; return types.
    /// </summary>
    private static object? AdaptReturnValue(Type returnType, HandlerDelegate pipeline)
    {
        if (returnType == typeof(void))
        {
            pipeline()
                .AsTask()
                .GetAwaiter()
                .GetResult();
            return null;
        }

        if (returnType == typeof(Task))
            return ExecuteTaskAsync(pipeline);

        if (returnType == typeof(ValueTask))
            return new ValueTask(ExecuteTaskAsync(pipeline));

        if (!returnType.IsGenericType)
            return pipeline().AsTask().GetAwaiter().GetResult();

        var genericType = returnType.GetGenericTypeDefinition();
        var resultType = returnType.GetGenericArguments()[0];

        if (genericType == typeof(Task<>))
        {
            return typeof(BoundaryProxy<TContract>)
               .GetMethod(nameof(ExecuteTaskAsync), BindingFlags.NonPublic | BindingFlags.Static, [typeof(HandlerDelegate)])!
               .MakeGenericMethod(resultType)
               .Invoke(null, [pipeline]);
        }

        if (genericType == typeof(ValueTask<>))
        {
            return typeof(BoundaryProxy<TContract>)
               .GetMethod(nameof(ExecuteValueTask), BindingFlags.NonPublic | BindingFlags.Static)!
               .MakeGenericMethod(resultType)
               .Invoke(null, [pipeline]);
        }

        // Just in case...
        return pipeline().AsTask().GetAwaiter().GetResult();

    }

    /// <summary>
    /// Awaits the result from the invocation pipeline, handling various async return types.
    /// </summary>
    private static async ValueTask<object?> AwaitResultAsync(Type returnType, object? result)
    {
        if (returnType == typeof(void))
            return null;

        if (returnType == typeof(Task))
        {
            await ((Task)result!).ConfigureAwait(false);
            return null;
        }

        if (returnType == typeof(ValueTask))
        {
            await ((ValueTask)result!).ConfigureAwait(false);
            return null;
        }

        if (!returnType.IsGenericType)
            return result;

        var genericType = returnType.GetGenericTypeDefinition();
        if (genericType == typeof(Task<>))
        {
            var task = (Task)result!;
            await task.ConfigureAwait(false);
            return task.GetType().GetProperty("Result")!.GetValue(task);
        }

        if (genericType == typeof(ValueTask<>))
        {
            var asTaskMethod = returnType.GetMethod("AsTask")!;
            var task = (Task)asTaskMethod.Invoke(result, null)!;
            await task.ConfigureAwait(false);
            return task.GetType().GetProperty("Result")!.GetValue(task);
        }

        // just in case...
        return result;
    }

    /// <summary>
    /// Executes the pipeline and returns a Task without a result.
    /// </summary>
    private static async Task ExecuteTaskAsync(HandlerDelegate pipeline)
    {
        await pipeline().ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the pipeline and returns a Task with a typed result.
    /// </summary>
    private static async Task<TResult?> ExecuteTaskAsync<TResult>(HandlerDelegate pipeline)
    {
        var result = await pipeline().ConfigureAwait(false);

        return result is null
            ? default
            : (TResult)result;
    }

    /// <summary>
    /// Executes the pipeline and returns a ValueTask with a typed result.
    /// </summary>
    private static ValueTask<TResult?> ExecuteValueTask<TResult>(HandlerDelegate pipeline)
        => new(ExecuteTaskAsync<TResult>(pipeline));
}
