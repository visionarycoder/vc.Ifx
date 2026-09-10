---
title: Streaming API JSON Sequence Instruction
description: RFC 8091 JSON-seq streaming with explicit framing and error handling
doc_type: instruction
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 450
prerequisites:
  - .github/instructions/ste-agent-writing-standard.instructions.md
related_docs:
  - .github/instructions/streaming-api-problem-details.instructions.md
related_skills:
  - rfc-8091-compliance
appliesTo: '**/*.md'
tags:
  - webapi
  - streaming
  - json-seq
  - rfc8091
---
# Streaming API JSON Sequence

Agent applies this instruction when implementing or modifying HTTP endpoints that stream multiple JSON payloads over time.

## Core Behavior

Agent sets content type `application/json-seq` for JSON text sequences.
Agent emits one complete JSON object per sequence frame.
Agent verifies frames are independently parseable JSON texts.
Agent flushes the response stream per frame when low-latency delivery is required.
Agent preserves cancellation support via request cancellation tokens.

## Framing Requirements

Agent uses record-separator framing per RFC 8091 for JSON text sequences.
Agent does not concatenate arbitrary JSON fragments.
Agent avoids buffering the entire stream in memory before writing.

## Contract and Compatibility

Agent keeps stable envelope fields for streamed records.
Agent adds new fields as optional to preserve backward compatibility.
Agent versions stream schemas only when contract changes are breaking.

## Reliability and Diagnostics

| Check | Test | Pass Criteria |
|---|---|---|
| Payload shape | Agent validates outbound payload shape before write when feasible. | Payload matches schema. |
| Lifecycle logging | Agent logs stream lifecycle events: start, frame count milestones, completion, cancellation, failures. | Log entries exist for each lifecycle event. |
| Security | Agent does not leak internal exception details to clients. | Client response contains no stack traces or internal paths. |

## Error Handling

Agent returns normal error responses before writing stream bytes when pre-stream validation fails.
Agent stops streaming on unrecoverable errors after streaming has begun and logs with correlation data.
Agent uses consistent correlation identifiers across logs and stream-producing operations.
