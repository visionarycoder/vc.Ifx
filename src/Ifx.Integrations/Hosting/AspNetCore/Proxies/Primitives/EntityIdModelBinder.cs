using System.Globalization;
using Ifx.Primitives;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Ifx.Web.AspNetCore;

/// <summary>Binds scalar route or query values to typed entity identifiers.</summary>
public sealed class EntityIdModelBinder : IModelBinder
{
    internal static bool IsIdentifier(Type type) => type.IsGenericType && !type.ContainsGenericParameters
        && type.GetGenericTypeDefinition() == typeof(EntityIdentifier<,>);

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);
        Type type = bindingContext.ModelType;
        if (!IsIdentifier(type)) throw new ArgumentException("A closed EntityIdentifier type is required.", nameof(bindingContext));
        ValueProviderResult supplied = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (supplied == ValueProviderResult.None) return Task.CompletedTask;
        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, supplied);
        string? text = supplied.FirstValue;
        Type key = type.GetGenericArguments()[1];
        try
        {
            if (string.IsNullOrWhiteSpace(text)) throw new FormatException();
            object value = key == typeof(Guid) ? Guid.Parse(text)
                : key == typeof(string) ? text
                : key == typeof(int) ? (object)int.Parse(text, CultureInfo.InvariantCulture)
                : key == typeof(long) ? (object)long.Parse(text, CultureInfo.InvariantCulture)
                : key == typeof(short) ? (object)short.Parse(text, CultureInfo.InvariantCulture)
                : throw new NotSupportedException();
            bindingContext.Result = ModelBindingResult.Success(Activator.CreateInstance(type, value));
        }
        catch (Exception exception) when (exception is FormatException or OverflowException or NotSupportedException)
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "The identifier is invalid.");
        }
        return Task.CompletedTask;
    }
}
