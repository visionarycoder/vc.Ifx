using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Framework;


[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Ifx001Signature : DiagnosticAnalyzer
{

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticIds.Ifx001ProxyContractSignature,
        title: "Proxy contract method signature",
        messageFormat: "Method '{0}' in a [ProxyContract] interface must be Task/Task<T>(ServiceRequest request, CancellationToken ct)",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Update the method signature to return Task or Task<T> and accept ServiceRequest plus CancellationToken.",
        helpLinkUri: "Docs/Framework/ifx001.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext ctx)
    {

        ctx.EnableConcurrentExecution();
        ctx.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        ctx.RegisterSymbolAction(c =>
        {
            var namedTypeSymbol = (INamedTypeSymbol)c.Symbol;
            var hasAttr = namedTypeSymbol.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "Ifx.Proxy.ProxyContractAttribute");
            if (!hasAttr)
            {
                return;
            }

            foreach (var m in namedTypeSymbol.GetMembers().OfType<IMethodSymbol>())
            {
                if (m.MethodKind != MethodKind.Ordinary)
                {
                    continue;
                }

                var okReturn = m.ReturnType.ToDisplayString().StartsWith("System.Threading.Tasks.Task");
                var ps = m.Parameters;
                var okParams = ps.Length == 2 && (ps[0].Type.Name == "ServiceRequest" || ps[0].Type.ToDisplayString() == "VisionaryCoder.Framework.ServiceRequest") && ps[1].Type.ToDisplayString() == "System.Threading.CancellationToken";
                if (!(okReturn && okParams))
                {
                    c.ReportDiagnostic(Diagnostic.Create(Rule, m.Locations.FirstOrDefault(), m.Name));
                }
            }
        }, SymbolKind.NamedType);
    }
}
