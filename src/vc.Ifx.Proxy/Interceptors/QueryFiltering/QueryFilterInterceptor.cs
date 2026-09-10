using System.Reflection;

using VisionaryCoder.Framework.Querying;
using VisionaryCoder.Framework.Querying.Serialization;

namespace VisionaryCoder.Framework.Proxy.Interceptors.QueryFiltering;

public sealed class QueryFilterInterceptor : IProxyInterceptor
{
    public async Task<ProxyResponse<T>> InvokeAsync<T>(
        ProxyContext context,
        ProxyDelegate<T> next,
        CancellationToken cancellationToken = default)
    {
        // Example: assume filters are passed in context.Body as JSON
        if (context.Body is string json)
        {
            // Validate against schema
            IReadOnlyList<string> validationErrors = QueryFilterSchemaValidator.Validate(json);
            if (validationErrors.Count > 0)
            {
                throw new ArgumentException($"Invalid query filter JSON: {string.Join(", ", validationErrors)}");
            }

            // Deserialize and rehydrate
            FilterNode? node = QueryFilterSerializer.Deserialize(json);
            if (node != null && typeof(T).IsGenericType &&
                typeof(T).GetGenericTypeDefinition() == typeof(QueryFilter<>))
            {
                // Rehydrate into QueryFilter<TInner>
                Type innerType = typeof(T).GetGenericArguments()[0];
                MethodInfo method = typeof(QueryFilterRehydrator)
                    .GetMethod(nameof(QueryFilterRehydrator.ToQueryFilter))!
                    .MakeGenericMethod(innerType);

                object rehydrated = method.Invoke(null, [node])!;
                return ProxyResponse<T>.Success((T)rehydrated, 200);
            }
        }

        // Pass through if not a filter payload
        return await next(context, cancellationToken);
    }
}
