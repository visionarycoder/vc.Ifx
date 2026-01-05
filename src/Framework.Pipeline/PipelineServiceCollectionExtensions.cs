using Microsoft.Extensions.DependencyInjection;

using VisionaryCoder.Framework.Pipeline.Abstractions;

namespace VisionaryCoder.Framework.Pipeline;

public static class PipelineServiceCollectionExtensions
{
    /// <summary>
    /// Adds a pipeline behavior that will execute for all commands and queries.
    /// Behaviors execute in the order they are registered.
    /// </summary>
    /// <typeparam name="TBehavior">The behavior type to add.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPipelineBehavior<TBehavior>(this IServiceCollection services)
        where TBehavior : class
    {
        // Register behavior for all pipeline behavior interfaces it implements
        foreach (Type interfaceType in typeof(TBehavior).GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>)))
        {
            services.AddScoped(interfaceType, typeof(TBehavior));
        }

        return services;
    }


}
