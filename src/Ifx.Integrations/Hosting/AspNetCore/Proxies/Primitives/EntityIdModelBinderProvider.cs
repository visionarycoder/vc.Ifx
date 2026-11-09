using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Ifx.Web.AspNetCore;

/// <summary>Selects the typed entity identifier model binder.</summary>
public sealed class EntityIdModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return EntityIdModelBinder.IsIdentifier(context.Metadata.ModelType) ? new EntityIdModelBinder() : null;
    }
}
