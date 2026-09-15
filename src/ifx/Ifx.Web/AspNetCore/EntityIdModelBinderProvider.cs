using Ifx.Primitives;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Ifx.Web.AspNetCore;

public sealed class EntityIdModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        Type type = ctx.Metadata.ModelType;
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(EntityIdentifier<,>) && !type.ContainsGenericParameters
            ? new EntityIdModelBinder()
            : null;
    }
}
