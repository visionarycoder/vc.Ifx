---
title: Streaming API Problem Details Instruction
description: RFC 9457 Problem Details for streaming and non-streaming Web API endpoints
doc_type: instruction
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 450
prerequisites:
  - .github/instructions/ste-agent-writing-standard.instructions.md
related_docs:
  - .github/instructions/streaming-api-json-seq.instructions.md
related_skills:
  - rfc-9457-compliance
appliesTo: '**/*.md'
tags:
  - webapi
  - problem-details
  - rfc9457
  - error-handling
---
# Streaming API Problem Details

Agent applies this instruction when defining API error contracts, including endpoints that stream responses.

## Core Behavior

Agent returns RFC 9457 Problem Details for error responses.
Agent sets content type `application/problem+json` for problem payloads.
Agent includes at minimum: `type`, `title`, `status`, and `detail` where appropriate.
Agent includes `instance` when it improves traceability.

## Status Code Semantics

| Condition | Status Code | Usage |
|---|---|---|
| Caller-correctable condition | 4xx | Client error |
| Server-side failure | 5xx | Server error |
| Error outcome | 4xx or 5xx | Agent does not return 200 for errors. |

## Security and Privacy

Agent does not expose stack traces, connection strings, SQL text, or secrets.
Agent keeps `detail` safe for external consumers.
Agent logs sensitive diagnostics server-side only.

## Streaming-Specific Rules

Agent performs request validation before starting stream writes.
Agent returns Problem Details response when validation fails before stream start.
Agent terminates stream safely and logs correlation metadata when unrecoverable error occurs after stream start.
Agent does not inject Problem Details into an already-started JSON-seq body as a normal HTTP error replacement.

## Consistency

Agent uses consistent `type` URIs for recurring error categories.
Agent keeps machine-parseable extension members stable when used by clients.
Agent documents error contract changes alongside endpoint changes.
