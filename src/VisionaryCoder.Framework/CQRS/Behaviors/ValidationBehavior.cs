// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace VisionaryCoder.Framework.CQRS.Behaviors;

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
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IServiceProvider serviceProvider;

    public ValidationBehavior(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
    {
        // Get all validators for this request type
        IEnumerable<IValidator<TRequest>> validators = serviceProvider.GetServices<IValidator<TRequest>>();

        if (!validators.Any())
        {
            // No validators registered, continue pipeline
            return await next();
        }

        // Create validation context
        var context = new ValidationContext<TRequest>(request);

        // Execute all validators
        FluentValidation.Results.ValidationResult[] validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect all failures
        FluentValidation.Results.ValidationFailure[] failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToArray();

        if (failures.Length != 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
