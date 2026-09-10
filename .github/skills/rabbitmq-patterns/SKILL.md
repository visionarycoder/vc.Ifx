---
name: rabbitmq-patterns
title: RabbitMQ Patterns
description: Implement RabbitMQ exchanges, bindings, acknowledgments, prefetch, and reliable consumer flows in .NET.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: high
estimated_tokens: 1974
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - message-patterns-dotnet
  - background-services-channels
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - rabbitmq
  - amqp
  - messaging
  - consumers
  - reliability
---
# RabbitMQ Patterns

Agent implements RabbitMQ messaging with explicit topology, bounded consumer concurrency, acknowledgments, and replay-safe handlers.

## When to Use

| Condition | Use |
|---|---|
| Deployment needs self-hosted or multi-cloud broker | Use this skill |
| Flow needs exchange and binding control | Use this skill |
| Consumer uses acknowledgments, prefetch, or requeue policy | Use this skill |
| Workload uses direct, topic, or fanout routing | Use this skill |

## When Not to Use

| Condition | Route |
|---|---|
| Deployment is Azure-native managed messaging | Use `azure-service-bus-patterns` |
| Work stays inside one process | Use `background-services-channels` |
| Need is pub/sub pattern guidance without broker choice | Use `message-patterns-dotnet` |
| Need is cache or lightweight ephemeral pub/sub | Use `redis-patterns-dotnet` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Exchange type | Yes | Direct, topic, or fanout |
| Queue and binding keys | Yes | Stable routing contract |
| Delivery guarantee | Yes | At-least-once, replay-safe |
| Ack and retry path | Yes | Ack, nack-requeue, or dead-letter |
| Prefetch target | Yes | Match handler throughput and latency |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent selects exchange type and queue topology. | Review routing rules. | Exchange and binding fit message fan-out shape. |
| 2 | Agent registers durable connection, channel, exchange, and queue declarations. | Inspect startup code. | Topology is explicit and durable where required. |
| 3 | Agent implements async consumer with explicit ack path. | Inspect consumer code. | Success and failure branches settle delivery explicitly. |
| 4 | Agent configures `BasicQos` and dead-letter routing. | Review channel setup. | Prefetch and retry path are bounded. |
| 5 | Agent adds publisher confirms where publish guarantee matters. | Inspect publish code. | Publisher receives explicit broker confirmation. |
| 6 | Agent verifies routing, retries, and poison-message behavior. | Run targeted tests. | Delivery and failure flows pass. |

## Implementation Patterns

| Pattern | Use | Avoid |
|---|---|---|
| Direct exchange | Exact routing key maps to one queue group | Topic exchange for exact-match only |
| Topic exchange | Wildcard routing fits bounded contract | Over-broad `#` bindings across unrelated domains |
| Fanout exchange | Broadcast event fits all bound queues | Queue-per-subscriber logic inside consumer |
| Manual ack | Handler has business side effects | Auto-ack |
| Dead-letter exchange | Retries need separate queue path | Infinite requeue loop |

### Durable Topology and Connection

```csharp
using RabbitMQ.Client;

builder.Services.AddSingleton<IConnection>(_ =>
{
    var factory = new ConnectionFactory
    {
        HostName = builder.Configuration["Messaging:RabbitMq:Host"],
        UserName = builder.Configuration["Messaging:RabbitMq:Username"],
        Password = builder.Configuration["Messaging:RabbitMq:Password"],
        DispatchConsumersAsync = true
    };

    return factory.CreateConnection("billing-service");
});
```

```csharp
using var channel = connection.CreateModel();

channel.ExchangeDeclare("billing.events", ExchangeType.Topic, durable: true, autoDelete: false);
channel.QueueDeclare("billing.invoice.posted", durable: true, exclusive: false, autoDelete: false);
channel.QueueBind("billing.invoice.posted", "billing.events", "invoice.posted");
channel.BasicQos(prefetchSize: 0, prefetchCount: 20, global: false);
```

### Async Consumer with Ack and Dead-Letter Path

```csharp
public sealed class InvoicePostedConsumer(IConnection connection, InvoicePostedHandler handler)
{
    public void Start(CancellationToken cancellationToken)
    {
        var channel = connection.CreateModel();
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.Received += async (_, args) =>
        {
            var message = JsonSerializer.Deserialize<InvoicePostedMessage>(args.Body.Span)!;
            var outcome = await handler.HandleAsync(message, cancellationToken);

            switch (outcome)
            {
                case ConsumeOutcome.Ack:
                    channel.BasicAck(args.DeliveryTag, multiple: false);
                    break;
                case ConsumeOutcome.Reject:
                    channel.BasicReject(args.DeliveryTag, requeue: false);
                    break;
                case ConsumeOutcome.Requeue:
                    channel.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
                    break;
            }
        };

        channel.BasicConsume("billing.invoice.posted", autoAck: false, consumer);
    }
}
```

### Publish with Confirm

```csharp
channel.ConfirmSelect();
channel.BasicPublish(
    exchange: "billing.events",
    routingKey: "invoice.posted",
    mandatory: true,
    basicProperties: channel.CreateBasicProperties(),
    body: JsonSerializer.SerializeToUtf8Bytes(message));

channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));
```

## Rules

| Rule | Agent Verifies | Fix |
|---|---|---|
| RMQ-001 | Topology is declared explicitly in code or infrastructure | Add exchange, queue, and binding declarations |
| RMQ-002 | Consumers use manual acknowledgments for side-effecting handlers | Set `autoAck: false` and settle explicitly |
| RMQ-003 | Prefetch is bounded | Add `BasicQos` |
| RMQ-004 | Poison messages exit hot queue path | Add dead-letter exchange and reject path |
| RMQ-005 | Handlers are idempotent for replay-safe at-least-once delivery | Add idempotency key storage |
| RMQ-006 | Publish guarantee uses confirms where loss is unacceptable | Enable confirm mode |
| RMQ-007 | Routing keys stay stable and documented | Centralize keys in contract constants |

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Build | `dotnet build [project].csproj` | Zero compile errors |
| Exchange routing | Publish one message per route | Expected queue receives message |
| Ack path | Handle valid message | Delivery leaves queue once |
| Poison path | Send invalid payload | Message routes to dead-letter path |
| Requeue path | Throw transient failure | Message reappears after nack |
| Prefetch path | Load consumer with many messages | In-flight count stays near prefetch limit |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| `autoAck: true` with side effects | Agent switches to manual ack |
| Topic binding uses broad wildcard for unrelated events | Agent narrows binding keys |
| Infinite requeue loop | Agent adds retry queue or dead-letter path |
| No publisher confirm on critical publish | Agent enables confirm mode |
| Unbounded consumer fetch | Agent sets `BasicQos` |

## MCP Hooks

| Need | GitHub MCP hook | Agent action |
|---|---|---|
| Find existing RabbitMQ topology | `search_code` | Agent searches for `ExchangeDeclare`, `QueueBind`, `BasicConsume`, and routing-key constants before editing. |
| Review broker PR changes | `pull_request_read` | Agent reads changed publishers, consumers, and dead-letter configuration before correcting topology or settlement flow. |
| Verify reliability coverage | `search_code` | Agent searches for `BasicAck`, `BasicNack`, confirm mode, and poison-message tests so consumer behavior stays aligned. |
