using Ifx.Primitives;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Ifx.Web.AspNetCore;

public sealed class EntityIdModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        Type type = ctx.ModelType;
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(EntityIdentifier<,>) || type.ContainsGenericParameters)
            throw new ArgumentException("The model must be a closed EntityIdentifier type.", nameof(ctx));

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
