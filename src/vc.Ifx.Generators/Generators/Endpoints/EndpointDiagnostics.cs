using Microsoft.CodeAnalysis;

namespace vc.Ifx.Generators;

internal static class EndpointDiagnostics
{
    internal static readonly DiagnosticDescriptor Declaration = Create("IFX2000", "Invalid endpoint declaration");
    internal static readonly DiagnosticDescriptor Handler = Create("IFX2001", "Unsupported endpoint handler");
    internal static readonly DiagnosticDescriptor Route = Create("IFX2002", "Duplicate endpoint route");
    internal static readonly DiagnosticDescriptor Name = Create("IFX2003", "Duplicate endpoint name");
    internal static readonly DiagnosticDescriptor Authorization = Create("IFX2004", "Conflicting endpoint authorization");
    internal static readonly DiagnosticDescriptor Hosting = Create("IFX2005", "Missing endpoint hosting reference");
    internal static readonly DiagnosticDescriptor Payload = Create("IFX2006", "Unsupported endpoint payload");
    internal static readonly DiagnosticDescriptor Collision = Create("IFX2007", "Generated registry name collision");

    private static DiagnosticDescriptor Create(string id, string title) => new(
        id, title, "{0}", "Generation", DiagnosticSeverity.Error, true,
        helpLinkUri: "https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/minimal-api-generator-contract.md#duplicate-policy-and-diagnostics");
}
