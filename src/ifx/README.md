# Ifx

The `Ifx` package contains the cross-platform runtime, general integrations,
Azure integrations, and Windows MSMQ integration. The project builds for
`net10.0` and `net10.0-windows`; Windows-only MSMQ source and its dependency are
included only in the Windows target. Existing provider namespaces remain
unchanged.

The package includes framework contracts and primitives, filtering and query
composition, pipelines and proxies, archiving, local storage and secrets,
in-memory messaging, EF Core, FTP, S3, Kafka, RabbitMQ, Redis, gRPC, OpenTelemetry,
Azure configuration and identity, Azure storage and messaging, Key Vault, and
MSMQ support. The unified package has a broader dependency graph than the former
provider packages. Review the provider guides before registering integrations.

- [General integrations](../Ifx.Integrations/README.md)
- [Azure integrations](../Ifx.Integrations.Azure/README.md)
- [Windows integrations](../Ifx.Integrations.Windows/README.md)
- [Project ownership and migration](../../docs/architecture/root-library-consolidation.md)
