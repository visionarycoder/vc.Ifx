# Windows integration source

MSMQ adapters are compiled into the `Ifx` assembly's `net10.0-windows` target.
The Windows-only source and `Experimental.System.Messaging` dependency are
excluded from the cross-platform `net10.0` target. Existing provider namespaces
are retained. Applications reference the `Ifx` package.

See the [Ifx package guide](../ifx/README.md) for the consolidated dependency
footprint and [project ownership](../../docs/architecture/root-library-consolidation.md).
