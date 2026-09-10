# vc.Ifx.Pipeline.Grpc

Network transport adapter for vc.Ifx.Pipeline on .NET 10 / C# 14. The direct
Pipeline and protobuf/gRPC package dependencies are unchanged.

## Stable Contracts

Existing GrpcRemoteDispatcher(ISerializer), GenericGrpcClient(GrpcChannel),
and no-token DispatchAsync/InvokeAsync signatures remain. Required-token
overloads forward cancellation to the generated unary call. IRequest,
IRemoteDispatcher, ISerializer and EndpointResolution are unchanged.

The protobuf service remains GenericInvoker/Invoke. InvokeRequest keeps
requestType field 1 and payload field 2; InvokeResponse keeps payload field 1.
Generated global C# types and the request wire discriminator typeof(TRequest).Name
are retained. Hosts must keep simple request names unique; changing the
discriminator or generated namespaces requires a separate protocol migration.

The serializer defines the payload format; the adapter does not introduce a
second JSON mapper or change returned values. Request and endpoint are required.
Remote endpoints must have an absolute HTTP(S) URI and IsLocal=false. HTTP is
supported for explicitly selected development/h2c endpoints, not secure defaults.
Missing URI retains the existing InvalidOperationException behavior. Arguments,
serializer failures and RPC status failures propagate without ordinary-failure
conversion or an additional retry layer.

## Transport Ownership

The default dispatcher owns and disposes a per-call GrpcChannel, preserving the
existing lifecycle. The GrpcChannelOptions overload supports host-selected
transport, credentials and call configuration, including fake HttpMessageHandler
tests without network I/O. SDK options govern handler/client ownership; avoid
sharing a handler configured for disposal across per-call channels.

For production channel reuse, supply the generated-client factory overload.
Its clients/channels are borrowed and must be managed by the host. Factory null
results fail explicitly. GenericGrpcClient also accepts a borrowed generated
client for testing/reuse; it never disposes the supplied channel or client.
Every unary AsyncUnaryCall is disposed on success, failure and cancellation.

Caller cancellation is checked before transport and before deserialization.
A canceled RPC becomes OperationCanceledException only when the caller token is
canceled, preserving that token and the original RPC exception. Server-reported
Cancelled without caller cancellation and DeadlineExceeded remain RpcException.
Successful responses arriving after cancellation are not deserialized.
The default channel requests normal .NET cancellation exceptions. Supplied
channels/factories retain their SDK policy, and the wrapper normalizes caller
cancellation consistently. Configure deadlines/budgets in the host and avoid
stacking a Pipeline retry policy with gRPC service-config retries.

## Generated Boundaries

Generated protobuf/gRPC C# stays under obj and is never hand-edited. The current
generator emits its own warning pragmas including CS8981. The 2026-09-10 metadata
recheck confirms the redundant package-wide CS8981 suppression has been removed.
Shared coverage exclusions remain limited to obj/generated-code boundaries.
No handwritten adapter code
may be excluded to satisfy coverage.

## Verification

```powershell
pwsh -NoProfile -File scripts/Invoke-FrameworkTests.ps1 -CoveragePackage vc.Ifx.Pipeline.Grpc -TestSourceScope PipelineGrpc -TestPackage vc.Ifx.Pipeline.Grpc -Filter FullyQualifiedName~Tests.PipelineGrpc -WarningsAsErrors
```

Verified 2026-09-09 with the strict command above: 12/12 tests, 53/53 lines and
18/18 branches (100%), with warnings treated as errors. Evidence:
`TestResults/coverage/vc.Ifx.Pipeline.Grpc/efef7400610c4b3f825f9ac7e30e9c72/summary.json`
and the adjacent `vc.Ifx.UnitTests/tests.trx`. Coverage includes both handwritten
adapter files; existing shared obj/generated-code exclusions remain unchanged.
Tests cover generated-client fakes, protobuf framing through an in-memory HTTP
handler, and PipelineInvoker-to-gRPC in-flight cancellation without network I/O.

The stale suppression-removal request is resolved; the evidence above is the
recorded local run, not a fresh measurement from documentation reconciliation.
Final consumer integration, packaging and
repository-wide gates remain with the orchestrator. No whole-solution check or
live endpoint test was run by this worker.

Primary references: [gRPC cancellation](https://learn.microsoft.com/en-us/aspnet/core/grpc/deadlines-cancellation?view=aspnetcore-10.0)
and [testing generated clients](https://learn.microsoft.com/en-us/aspnet/core/grpc/test-services?view=aspnetcore-10.0).
