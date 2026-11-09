---
title: Lightweight component starting point
doc_type: guide
status: active
last_updated: 2026-11-08
---

# Lightweight component starting point

Copy Contract.cs.txt and Service.cs.txt into your project as .cs files and rename
the example types. They require only Ifx and the standard task/cancellation APIs.
The service echoes the request so the initial example runs without infrastructure.
Replace that behavior with application work and register the approved typed proxy
at your composition root before connecting components.

The request is a plain record. It needs no logger, disposable base class, database
connection, or interceptor attributes. The marked contract supports Task, Task<T>,
ValueTask and ValueTask<T>, with a request and CancellationToken.

Add caching, retries and authentication only where requirements call for them.
Microsoft cache APIs and token validation APIs remain valid implementation choices.
For HTTP retries, start with Microsoft.Extensions.Http.Resilience configuration;
restrict retries to operations whose repeat execution is safe. Configure token
validation with the application's issuer, audience and signing-key requirements.
The template intentionally supplies no authentication defaults or retry loop.

IFX1500, IFX1501 and IFX1502 are disabled by default. Existing teams can explicitly
enable them in .editorconfig as optional interceptor preferences; they are not
general Microsoft API prohibitions or proof that a component boundary is secure.
Mandatory proxy boundary policies remain separate.
