---

name: rfc-8725-compliance

title: JWT RFC 8725 Best Current Practices

description: Enforce explicit JWT algorithm, key, claim, and validation rules for issuing and consuming bearer tokens.

doc_type: skill

status: active

last_updated: 2026-07-29

target_audience: ai

complexity: medium

estimated_tokens: 1080

prerequisites:

  - ste-agent-writing-standard

  - terminology-dictionary

related_skills:

  - rfc-fixes-bundle

  - rfc-9457-compliance

  - rfc-8259-compliance

  - webapi-authz-hardening

appliesTo: '**/*.{cs,ts,js,json,yml,yaml,md}'

tags:

  - rfc

  - 8725

  - jwt

  - security

---



# JWT RFC 8725 Best Current Practices



Agent uses this skill for JWT issuance, validation, storage, and transport decisions.



## When to Use



| Condition | Agent Action |

|---|---|

| Service issues JWTs | Use this skill |

| API validates bearer tokens | Use this skill |

| Session cookie flow without JWTs | Do not use this skill |



## Core Rules



| Rule | Agent Verifies | Fix |

|---|---|---|

| RFC8725-001 | Allowed algorithms are explicit | Configure algorithm allow-list |

| RFC8725-002 | `alg: none` and confusion paths are blocked | Reject unsigned or mismatched token types |

| RFC8725-003 | Verification binds issuer, audience, key, and lifetime | Set all validator parameters explicitly |

| RFC8725-004 | Symmetric and asymmetric keys stay separated by purpose | Use distinct key material per token class |

| RFC8725-005 | Token lifetime stays bounded | Set short expiry and verify clock skew deliberately |

| RFC8725-006 | Sensitive claims carry least data | Remove secrets and unnecessary PII |

| RFC8725-007 | Transport and storage stay hardened | Use headers and secure runtime storage; avoid query-string transport |



## Workflow



| Step | Agent Action | Test | Pass |

|---|---|---|---|

| 1. Classify token paths | Agent identifies issuers, validators, and token consumers in scope. | Agent lists changed token paths. | Every changed token path appears once. |

| 2. Lock verification rules | Agent sets explicit algorithm, issuer, audience, key, and lifetime checks. | Inspect changed token configuration or tests. | Zero implicit validation defaults remain in changed scope. |

| 3. Reduce token risk | Agent removes excess claims and insecure transport or storage patterns. | Search changed scope for query-string or local-storage token flow when relevant. | Zero insecure token transport or storage patterns remain in changed scope. |

| 4. Verify auth outcomes | Agent runs existing authentication tests in scope. | Run existing build and auth test commands in scope. | Zero build errors and zero failing auth tests. |



## Verification Matrix



| Test | Run | Pass |

|---|---|---|

| Tampered token verification | Run existing auth tests with invalid signature or wrong key paths. | Invalid token is rejected. |

| Claim verification | Run existing tests for issuer, audience, and expiry failures. | Mismatched issuer, audience, or expired token is rejected. |

| Build verification | Run existing build command for touched projects. | Zero build errors. |



## Guardrails



- Agent does not relax signature validation for local or test convenience in production code.

- Agent does not trust token headers to select keys without server-side constraints.

- Agent keeps authorization policy work aligned with `webapi-authz-hardening`.

