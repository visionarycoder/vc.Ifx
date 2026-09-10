---
title: Repository and Unit of Work Examples
doc_type: reference
status: active
last_updated: 2026-08-30
target_audience: ai
estimated_tokens: 146
related_docs:
  - ../SKILL.md
---
# Repository and Unit of Work Examples

| Example | Focus |
|---|---|
| `IUnitOfWork` contract | Centralized `SaveChangesAsync` and transaction start |
| Aggregate repository | Intent-focused retrieval methods and add or remove operations |
| Coordinated handler | One explicit transaction across several repositories |
| Read-side handler | Direct `DbContext` projection for caller-shaped DTOs |
