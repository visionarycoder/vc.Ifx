namespace VisionaryCoder.Framework.Messaging.Abstractions;

/// <summary>
/// Interface for handling received messages.
/// </summary>
/// <typeparam name="TMessage">The type of message to handle.</typeparam>
public interface IMessageHandler<in TMessage> where TMessage : IMessage
{
    /// <summary>
    /// Handles the received message.
    /// </summary>
    /// <param name="message">The message to handle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task HandleAsync(TMessage message, CancellationToken cancellationToken = default);
}