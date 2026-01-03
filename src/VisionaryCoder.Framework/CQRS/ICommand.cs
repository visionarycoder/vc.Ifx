// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.CQRS;

/// <summary>
/// Marker interface for commands that modify state but do not return a value.
/// Commands represent actions or intents to change system state.
/// </summary>
/// <remarks>
/// Use ICommand for operations that:
/// - Modify state (Create, Update, Delete)
/// - Have side effects
/// - Do not need to return data
/// Example: CreateOrderCommand, UpdateUserCommand, DeleteProductCommand
/// </remarks>
public interface ICommand
{
}

/// <summary>
/// Marker interface for commands that modify state and return a result.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned by the command.</typeparam>
/// <remarks>
/// Use ICommand&lt;TResponse&gt; for operations that:
/// - Modify state AND need to return data
/// - Example: CreateOrderCommand returning OrderId
/// - Example: ProcessPaymentCommand returning PaymentResult
/// </remarks>
public interface ICommand<out TResponse>
{
}
