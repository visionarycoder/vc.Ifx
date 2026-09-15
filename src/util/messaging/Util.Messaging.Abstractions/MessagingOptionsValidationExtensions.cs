namespace Util.Messaging;

/// <summary>
/// Provides guard-clause helpers for messaging provider option validation.
/// </summary>
public static class MessagingOptionsValidationExtensions
{
    /// <summary>
    /// Validates that an option value is not null, empty, or whitespace.
    /// </summary>
    /// <param name="value">The option value to validate.</param>
    /// <param name="optionName">The option name to report in failures.</param>
    /// <returns>The validated option value.</returns>
    public static string ValidateNotNullOrWhiteSpace(this string? value, string optionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(optionName);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{optionName} must not be null, empty, or whitespace.", optionName);
        }

        return value;
    }
}
