---
name: realtime-communication-bundle
title: Real-Time Communication Bundle
description: Bundle routing skill for real-time communication patterns including SignalR hubs and WebSocket connections.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: low
estimated_tokens: 650
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - signalr-patterns
  - dotnet-webapi
appliesTo: '**/*.{cs,csproj,json}'
tags:
  - signalr
  - websockets
  - bundle
  - realtime
---
# Real-Time Communication Bundle

Agent uses this bundle for real-time bidirectional communication using SignalR or WebSockets.

## Activation

| Prompt Scope | Use This Bundle | Route Detail |
|---|---|---|
| Add SignalR to application | Yes | Agent uses signalr-patterns |
| Real-time notifications to clients | Yes | Agent uses signalr-patterns |
| Bidirectional communication | Yes | Agent uses signalr-patterns |
| Simple Server-Sent Events | No | Agent uses built-in SSE without SignalR |

## Coverage Matrix

| Track | Specialist Skill | Agent Uses When |
|---|---|---|
| SignalR | `signalr-patterns` | Hubs, connection lifecycle, groups, scaling in scope |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Classify need | Agent determines if SignalR fits the requirement. | Agent verifies bidirectional or broadcast need. | Requirement matches SignalR capabilities. |
| 2. Apply SignalR skill | Agent uses signalr-patterns specialist skill. | Agent records skill application. | SignalR configured correctly. |
| 3. Configure scaling | Agent adds Redis/Service Bus backplane when multi-server. | Inspect backplane configuration. | Backplane configured for scale-out. |
| 4. Verify connectivity | Agent tests hub connection and message flow. | Connect client and send/receive messages. | Messages flow bidirectionally. |

## Pattern Selection Guide

| Requirement | Recommended Approach | Notes |
|---|---|---|
| Bidirectional RPC | SignalR | Supports multiple transports, automatic reconnection |
| Server-to-client only | Server-Sent Events or SignalR | SSE simpler for one-way push |
| High-performance binary | SignalR with MessagePack | Use MessagePack protocol |
| Multi-server deployment | SignalR + Redis/Service Bus | Required for scale-out |

## Verification Matrix

| Test | Action | Pass |
|---|---|---|
| Connection | Client connects to hub. | Connection established successfully. |
| Server-to-client | Server invokes client method. | Client receives invocation. |
| Client-to-server | Client invokes hub method. | Server executes method. |
| Reconnection | Disconnect and reconnect client. | Client reconnects automatically. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| No backplane on multi-server | Agent adds Redis or Azure Service Bus backplane. |
| Missing CORS configuration | Agent adds SignalR-compatible CORS policy. |
| Synchronous hub methods | Agent converts hub methods to async. |
| No authentication | Agent adds hub authorization attributes. |

## Outputs

- SignalR hub implementation
- Client connection configuration
- Backplane setup for scale-out (when needed)
- Real-time message flow verification
