using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace VisionaryCoder.Framework.Primitives.Web.AspNetCore;
public sealed class EntityIdModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        Type type = ctx.ModelType;
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(EntityId<,>) || type.ContainsGenericParameters)
            throw new ArgumentException("The model must be a closed EntityId type.", nameof(ctx));

        ValueProviderResult supplied = ctx.ValueProvider.GetValue(ctx.ModelName);
        if (supplied == ValueProviderResult.None)
            return Task.CompletedTask;

        ctx.ModelState.SetModelValue(ctx.ModelName, supplied);
        object?[] arguments = [supplied.FirstValue, null];
        bool parsed = (bool)type.GetMethod("TryParse")!.Invoke(null, arguments)!;
        if (parsed)
            ctx.Result = ModelBindingResult.Success(arguments[1]);
        else
        {
            ctx.ModelState.TryAddModelError(ctx.ModelName, "The identifier is invalid.");
            ctx.Result = ModelBindingResult.Failed();
        }
        return Task.CompletedTask;
    }
}
