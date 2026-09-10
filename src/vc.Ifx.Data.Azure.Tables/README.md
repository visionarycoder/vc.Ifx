# vc.Ifx.Data.Azure.Tables

Azure Table Storage boundary for .NET 10 / C# 14. Existing
`ITableStorageProvider` signatures and SDK entity types remain compatible.
Dependencies remain Abstractions, Azure.Data.Tables, Azure.Identity and logging.
The SDK owns serialization, transport, authentication and retries; the provider
owns validation, table initialization, paging and concurrency decisions.

## Construction and Ownership

The existing options/logger constructor creates SDK clients without network I/O.
The new options/logger/TableServiceClient constructor borrows a configured SDK
client; its virtual GetTableClient supplies the replaceable TableClient seam.
No redundant SDK interface, emulator or live Azure account is needed for tests.
Credentials in options are validated even when a client is supplied, but are not
used to replace its credentials or retry policy. Hosts own injected clients.

When CreateTableIfNotExists is enabled, the first entity operation creates the
table lazily. Initialization is serialized, successful initialization is reused,
and failed/canceled initialization can be tried again by a later operation.
TableExists never creates a table. Async initialization and SDK calls receive
the caller token; streaming also observes cancellation between returned entities.
Dispose prevents subsequent operations; callers must quiesce outstanding work
before disposal. Disposal releases the initialization semaphore and is idempotent.
The provider does not dispose borrowed SDK clients or logger. Async operations
also observe cancellation after SDK completion, including before translating a
404 to a missing entity.

## Data and Concurrency

Table names use ASCII letters/digits, start with a letter, contain 3-63
characters, and cannot be the reserved name Tables (case-insensitive).
Keys are non-null strings of at most 1024 characters; empty keys and spaces are
valid. Slash, backslash, hash, question mark and prohibited control characters
are rejected. Apostrophes are allowed and SDK CreateQueryFilter escapes them.
Arbitrary query filters are caller-supplied OData, not concatenated user input.

Entities are passed to the SDK without custom serialization or mutation. The SDK
and service validate supported property types, timestamps, property counts,
entity size and serialized transaction payload limits. Do not mutate entities
while an operation is outstanding. Get returns null for any SDK 404, including
a missing table; other failures (including 409/412) propagate unchanged.

An explicit nonempty ETag, including ETag.All, is honored. Default and empty-string
ETags are treated as omitted, never as an implicit wildcard. With optimistic concurrency
enabled (default), Update uses an explicit ETag or the entity's non-wildcard
ETag; Delete requires an explicit ETag. Missing conditional ETags fail before
I/O. With concurrency disabled, omitted ETags mean unconditional writes.
Upsert is inherently unconditional and defaults to Replace, as before.
Merge/Replace are the only accepted update modes. Operations do not refresh the
caller's ETag; read again before the next conditional update.

Transactions retain input order, require 1..MaxEntitiesPerBatch actions (at most
100), one partition and unique row keys, and validate action kinds/keys/ETags
before sending. The provider snapshots action metadata, never splits an atomic
batch and never retries it independently of the SDK. Conditional action ETags
follow the same rules as single updates/deletes, using the action entity ETag
when available. The service enforces the 4 MiB serialized transaction limit.

## Queries and Retry Ownership

MaxEntitiesPerQuery and maxPerPage are page-size hints in 1..1000, not total
result caps. List queries consume all continuation pages, including empty pages
with continuation tokens. EnumerateEntitiesAsync streams all pages and disposes
the SDK enumerator on completion, early exit, failure or cancellation. No sorting,
deduplication, buffering of all pages, or synthetic continuation tokens are added.

Options.CreateClientOptions creates fresh SDK options: exponential retries,
MaxRetries = MaxRetryAttempts, Retry.Delay = RetryDelayMilliseconds, and
Retry.NetworkTimeout = TimeoutMilliseconds. NetworkTimeout is per network
operation, not an end-to-end deadline. Use caller cancellation for an overall
budget. Supplied clients retain their own settings. No Polly policy is stacked.
UseManagedIdentity uses DefaultAzureCredential and requires an HTTPS account URI
without user information, query or fragment; connection-string mode delegates
credential parsing to the SDK. DefaultAzureCredential includes developer
credentials; inject a client with an explicit credential for stricter hosts.

Migration: constructor-time creation moves to the first entity operation;
EnableOptimisticConcurrency is now enforced, so callers intentionally replacing
rows unconditionally must pass ETag.All or disable concurrency. Invalid names,
keys, modes, batches and page sizes fail early. Empty keys become consistently
supported. Partition filters now escape literals. Retry/timeout options now
actually configure provider-created SDK clients.

## Verification

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Data.Azure.Tables -TestSourceScope Data/AzureTables -TestPackage vc.Ifx.Data.Azure.Tables -Filter FullyQualifiedName~Data.AzureTables -WarningsAsErrors
```

On 2026-09-09 the strict scoped command passed 25 tests with 100% line coverage
(281/281), branch coverage (98/98), and method coverage. Warnings are treated as
errors. Global integration/release gates belong to Orchestrator; package refresh
belongs to the metadata owner. No tests contact Azure or an emulator.

Primary references: [Table data model](https://learn.microsoft.com/en-us/rest/api/storageservices/understanding-the-table-service-data-model),
[atomic transactions](https://learn.microsoft.com/en-us/rest/api/storageservices/performing-entity-group-transactions),
and [SDK filter escaping](https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables.tableclient.createqueryfilter?view=azure-dotnet).
