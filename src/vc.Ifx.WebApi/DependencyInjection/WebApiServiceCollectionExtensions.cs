using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VisionaryCoder.Framework.WebApi.ExceptionHandling;
using VisionaryCoder.Framework.WebApi.Resilience;

namespace VisionaryCoder.Framework.WebApi.DependencyInjection;

public static class WebApiServiceCollectionExtensions
{
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
                options.AddDefaultMappings();
                configureExceptionHandling?.Invoke(options);
            });

        services.AddOptions<WebApiResilienceOptions>()
            .Configure(options => configureResilience?.Invoke(options));

        services.TryAddSingleton<IProblemDetailsExceptionMapper, ProblemDetailsExceptionMapper>();
        services.TryAddSingleton<IWebApiResiliencePipelineFactory, WebApiResiliencePipelineFactory>();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }

    public static IApplicationBuilder UseIfxWebApiExceptionHandling(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseExceptionHandler();
    }
}
