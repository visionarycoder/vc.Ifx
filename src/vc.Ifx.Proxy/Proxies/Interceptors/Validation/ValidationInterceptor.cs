using System.ComponentModel.DataAnnotations;
using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Core;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Validation;

/// <summary>
/// Validates all method arguments using data annotations.
/// Throws ValidationException if any argument fails validation.
/// </summary>
public sealed class ValidationInterceptor : IProxyInterceptor
{
    /// <summary>
    /// Executes the interceptor to validate all arguments before method invocation.
    /// </summary>
    public ValueTask<object?> InvokeAsync(MethodContext context, HandlerDelegate next)
    {
        // Validate each non-null argument
        foreach (var argument in context.Arguments)
        {
            if (argument.Value is null)
                continue;

            Validator.ValidateObject(
                argument.Value,
                new ValidationContext(argument.Value),
                validateAllProperties: true);
        }

        return next();
    }
}
