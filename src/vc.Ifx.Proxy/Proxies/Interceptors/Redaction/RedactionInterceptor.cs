using Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Core;

namespace Wa.Wsdot.Fin.Idl.Ifx.Proxies.Interceptors.Redaction;

/// <summary>
/// Interceptor that redacts sensitive arguments from logging and audit trails.
/// Replaces argument values containing sensitive names (password, token, etc.) with a redaction marker.
/// </summary>
public sealed class RedactionInterceptor : IProxyInterceptor
{
    /// <summary>
    /// List of argument names that should be redacted from logs and audit records.
    /// </summary>
    private static readonly string[] SensitiveNames =
    [
        "password",
        "secret",
        "token",
        "apikey",
        "authorization",
        "credential"
    ];

    /// <summary>
    /// Executes the interceptor to redact sensitive arguments.
    /// </summary>
    public ValueTask<object?> InvokeAsync(MethodContext context, HandlerDelegate next)
    {
        // Create a dictionary of safe arguments with sensitive values redacted
        var safeArguments = context.Arguments.ToDictionary(
            argument => argument.Name,
            argument => ShouldRedact(argument.Name) ? "***REDACTED***" : argument.Value);

        // Store the safe arguments for use by audit and logging interceptors
        context.Items[InterceptorConstants.SafeArguments] = safeArguments;

        return next();
    }

    /// <summary>
    /// Determines whether an argument name matches a sensitive pattern.
    /// </summary>
    private static bool ShouldRedact(string name)
        => SensitiveNames.Any(value => name.Contains(value, StringComparison.OrdinalIgnoreCase));
}
