# Ifx.Messaging.Abstractions

Provider-neutral messaging contracts for Ifx applications.

## Contents

- `IMessage` / `MessageBase` — marker contract and base record carrying `MessageId`,
  `CorrelationId`, and `CreatedAt`.
- `IMessagePublisher` — publish single, batch, and scheduled messages.
- `IMessageConsumer` / `IMessageHandler<TMessage>` — start/stop consumption and per-message
  handling.
- `IMessageBus` — combines a publisher with per-message-type consumer creation.

## Usage

Concrete messaging packages (for example `Ifx.Messaging.Azure.Queues`) may implement
these contracts to expose a transport-neutral publish/consume surface. Packages whose
transport does not fit the publish/subscribe shape are not required to implement them.
