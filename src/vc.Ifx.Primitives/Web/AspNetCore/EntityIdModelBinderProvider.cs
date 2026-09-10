using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace VisionaryCoder.Framework.Primitives.Web.AspNetCore;

public sealed class EntityIdModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        Type type = ctx.Metadata.ModelType;
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(EntityId<,>) && !type.ContainsGenericParameters
            ? new EntityIdModelBinder()
            : null;
    }
}
