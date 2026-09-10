using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VisionaryCoder.Framework.WebApi.ExceptionHandling;
using VisionaryCoder.Framework.WebApi.Resilience;

namespace VisionaryCoder.Framework.WebApi.DependencyInjection;

/// <summary>Registers replaceable Web API hosting infrastructure.</summary>
public static class WebApiServiceCollectionExtensions
{
    /// <summary>Registers error handling and validated resilience options without replacing application services.</summary>
    public static IServiceCollection AddIfxWebApi(
        this IServiceCollection services,
        Action<WebApiExceptionHandlingOptions>? configureExceptionHandling = null,
        Action<WebApiResilienceOptions>? configureResilience = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddProblemDetails();
        services.AddOptions<WebApiExceptionHandlingOptions>()
            .Configure(options =>
            {
                configureExceptionHandling?.Invoke(options);
            })
            .Validate(options => { options.Validate(); return true; })
            .ValidateOnStart();

        services.AddOptions<WebApiResilienceOptions>()
            .Configure(options => configureResilience?.Invoke(options))
            .Validate(options => { options.Validate(); return true; })
            .ValidateOnStart();

        services.TryAddSingleton<IProblemDetailsExceptionMapper, ProblemDetailsExceptionMapper>();
        services.TryAddSingleton<IWebApiResiliencePipelineFactory, WebApiResiliencePipelineFactory>();
        services.TryAddSingleton<IWebApiTransientFailureClassifier, WebApiTransientFailureClassifier>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }

    /// <summary>Adds ASP.NET Core exception handling using the registered handlers.</summary>
    /// <remarks>
    /// Built-in middleware short-circuits aborted cancellation/I/O failures with status 499
    /// before handlers run, and clears unstarted response headers before mapping other errors.
    /// Required headers for mapped errors must be supplied by trusted code after that reset.
    /// Prefer ordinary authentication challenges and routing responses when applicable.
    /// </remarks>
    public static IApplicationBuilder UseIfxWebApiExceptionHandling(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseExceptionHandler();
    }
}
