---
title: Util.Messaging.RabbitMQ
doc_type: readme
status: active
last_updated: 2026-09-13
---

# Util.Messaging.RabbitMQ

`Util.Messaging.RabbitMQ` provides a durable RabbitMQ transport for `Ifx.Messaging.Abstractions`.

## Options

`RabbitMqOptions` supports:

- `ConnectionString` or `HostName` + `Port` + `VirtualHost` + `UserName` + `Password`
- `ExchangeName`
- `ExchangeType`
- `QueueName`
- `DeadLetterExchangeName`
- `DeadLetterQueueName`
- `PrefetchCount`
- `MaxRetryAttempts`
- `InitialRetryDelay`
- `ConsumerDispatchConcurrency`
- `ClientProvidedName`

When dead-letter names are omitted, the provider derives them from the primary exchange and queue names.

## Routing model

Messages publish to the configured exchange with the message type full name as the routing key.

Consumers bind a durable queue named `<QueueName>.<subscriptionName>.<messageType>` and a dead-letter queue named `<DeadLetterQueueName>.<subscriptionName>.<messageType>`.

## Usage

```csharp
using Ifx.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Util.Messaging.RabbitMQ;

var services = new ServiceCollection();

services.AddRabbitMqMessaging(options =>
{
    options.HostName = "rabbitmq.internal";
    options.Port = 5672;
    options.VirtualHost = "/";
    options.UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME");
    options.Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");
    options.ExchangeName = "ifx.messages";
    options.ExchangeType = "direct";
    options.QueueName = "ifx.messages";
});

ServiceProvider provider = services.BuildServiceProvider();
IMessagePublisher publisher = provider.GetRequiredService<IMessagePublisher>();
```
