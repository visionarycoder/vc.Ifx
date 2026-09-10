using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace VisionaryCoder.Framework.Primitives.Web.AspNetCore;
public sealed class EntityIdModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext ctx)
    {
        string? text = ctx.ValueProvider.GetValue(ctx.ModelName).FirstValue;
        Type? t = ctx.ModelType; // EntityId<TEntity,TKey>
        if (string.IsNullOrWhiteSpace(text)) { ctx.Result = ModelBindingResult.Failed(); return Task.CompletedTask; }
        MethodInfo parse = t.GetMethod("Parse", [typeof(string)])!;
        object? value = parse.Invoke(null, [text]);
        ctx.Result = ModelBindingResult.Success(value);
        return Task.CompletedTask;
    }
}
