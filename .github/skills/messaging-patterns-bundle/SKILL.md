---
name: messaging-patterns-bundle
title: Messaging Patterns Bundle
description: Bundle routing skill for messaging infrastructure including Azure Service Bus, RabbitMQ, message patterns, and Redis pub/sub.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: low
estimated_tokens: 850
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - azure-service-bus-patterns
  - rabbitmq-patterns
  - message-patterns-dotnet
  - redis-patterns-dotnet
  - background-services-channels
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - messaging
  - bundle
  - asynchronous
  - integration
---
# Messaging Patterns Bundle

Agent uses this bundle for message-based integration including Azure Service Bus, RabbitMQ, idempotency patterns, and Redis pub/sub.

## Activation

| Prompt Scope | Use This Bundle | Route Detail |
|---|---|---|
| Integrate Azure Service Bus | Yes | Agent uses azure-service-bus-patterns |
| Integrate RabbitMQ | Yes | Agent uses rabbitmq-patterns |
| Implement message patterns (pub/sub, idempotency, outbox) | Yes | Agent uses message-patterns-dotnet |
| Use Redis for messaging | Yes | Agent uses redis-patterns-dotnet |
| In-process async work queue | No | Agent routes to background-services-channels |

## Coverage Matrix

| Track | Specialist Skill | Agent Uses When |
|---|---|---|
| Azure Service Bus | `azure-service-bus-patterns` | Queues, topics, sessions, dead-letter in scope |
| RabbitMQ | `rabbitmq-patterns` | Exchanges, bindings, consumers in scope |
| Message patterns | `message-patterns-dotnet` | Pub/sub, idempotency, outbox, saga in scope |
| Redis messaging | `redis-patterns-dotnet` | Redis pub/sub, streams in scope |

## Decision Order

| Decision | Agent Action |
|---|---|
| Azure cloud deployment | Agent prefers azure-service-bus-patterns |
| On-premises or multi-cloud | Agent uses rabbitmq-patterns |
| Lightweight pub/sub without durability | Agent uses redis-patterns-dotnet |
| Idempotency or outbox pattern | Agent uses message-patterns-dotnet |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Classify messaging need | Agent maps prompt to message broker and pattern. | Agent lists broker and patterns in scope. | Every messaging concern maps to one specialist skill. |
| 2. Apply specialist | Agent applies matching specialist skill. | Agent records applied skill. | One specialist skill applied per messaging technology. |
| 3. Add message contracts | Agent creates message DTOs with clear boundaries. | Inspect message classes. | Message contracts exist with serialization attributes. |
| 4. Verify reliability | Agent tests message delivery and failure handling. | Simulate broker unavailability. | Application handles transient failures gracefully. |

## Pattern Selection Guide

| Requirement | Recommended Track | Notes |
|---|---|---|
| Azure-native integration | Azure Service Bus | Managed service, Azure AD integration |
| On-premises control | RabbitMQ | Self-hosted, multi-protocol support |
| High-throughput, low-latency | Redis | In-memory, limited durability |
| Exactly-once semantics | Message patterns + outbox | Cross-cutting pattern |
| Distributed transactions | Message patterns + saga | Cross-cutting pattern |

## Verification Matrix

| Track | Test | Pass |
|---|---|---|
| Azure Service Bus | Send message, verify receipt. | Message processed successfully. |
| RabbitMQ | Send message, verify acknowledgment. | Message acknowledged and processed. |
| Idempotency | Send duplicate message. | Second message skipped or deduplicated. |
| Dead-letter | Send poison message. | Message moves to dead-letter queue. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| No dead-letter handling | Agent adds dead-letter queue configuration. |
| Missing idempotency keys | Agent adds message ID or hash-based deduplication. |
| Synchronous message processing | Agent uses async handlers throughout. |
| No retry policy | Agent configures exponential backoff retry. |

## Outputs

- Configured message broker client
- Message contracts and handlers
- Reliability patterns (retries, dead-letter, idempotency)
- Integration test evidence
