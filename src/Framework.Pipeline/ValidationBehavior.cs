using FluentValidation;
using FluentValidation.Results;

using Microsoft.Extensions.DependencyInjection;

using VisionaryCoder.Framework.Pipeline.Abstractions;

using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace VisionaryCoder.Framework.Pipeline;

/// <summary>
/// Pipeline behavior that validates commands and queries using FluentValidation.
/// Automatically discovers and executes all registered validators for the request type.
/// </summary>
/// <typeparam name="TRequest">The type of request being validated.</typeparam>
/// <typeparam name="TResponse">The type of response being returned.</typeparam>
/// <remarks>
/// To use this behavior:
/// 1. Install FluentValidation package
/// 2. Create validators: public class MyCommandValidator : AbstractValidator&lt;MyCommand&gt;
/// 3. Register validators in DI
/// 4. Add this behavior: services.AddPipelineBehavior&lt;ValidationBehavior&lt;,&gt;&gt;()
/// </remarks>
public class ValidationBehavior<TRequest, TResponse>(IServiceProvider serviceProvider)
    : IPipelineBehavior<TRequest, TResponse>
{

    private readonly IServiceProvider serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
    {
        // Get all validators for this request type
        var validators = serviceProvider.GetServices<IValidator<TRequest>>().ToList();
        if (validators.Count == 0)
        {
            // No validators registered, continue pipeline
            return await next().ConfigureAwait(false);
        }

        // Create validation context
        var context = new ValidationContext<TRequest>(request);

        // Execute all validators
        FluentValidation.Results.ValidationResult[] validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken))).ConfigureAwait(false);

        // Collect all failures
        var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).Cast<ValidationResult>().ToList();
        if (failures.Count != 0)
        {
            // TODO: Create a custom ValidationException that can hold FluentValidation errors.  Consider an aggregate that inherits from ValidationResult.
            throw new ValidationException(failures);
        }
        return await next().ConfigureAwait(false);
    }
}
