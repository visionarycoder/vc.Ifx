# Ifx.Integrations

General adapters grouped by capability: EF Core querying, FTP storage, RabbitMQ
messaging, gRPC transport, SevenZip archiving, ASP.NET Core hosting, and shared
interceptors. Targets net10.0 and requires the ASP.NET Core shared framework.

Azure and Windows adapters live in separate child projects. This project has no
Azure SDK or Windows messaging package references. It references Ifx; Ifx never
references Integrations. Public namespaces are retained.

See [ownership and migration](../../docs/architecture/root-library-consolidation.md).
