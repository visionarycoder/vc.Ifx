using System.Collections.Immutable;
using System.Linq;
using Ifx.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Ifx.Analyzers.Rules;


[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Ifx001Signature : DiagnosticAnalyzer
{

    public static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticIds.Ifx001ProxyContractSignature,
        title: "Proxy contract method signature",
        messageFormat: "Method '{0}' in a [ProxyContract] interface must return Task, Task<T>, ValueTask or ValueTask<T> and accept a request plus CancellationToken",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Marked contracts use one typed request and a CancellationToken. Request DTOs do not require a framework base class.",
        helpLinkUri: "https://github.com/visionarycoder/Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#legacy-ifx-inventory");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext ctx)
    {

        ctx.EnableConcurrentExecution();
        ctx.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        ctx.RegisterSymbolAction(c =>
        {
            c.CancellationToken.ThrowIfCancellationRequested();
            var namedTypeSymbol = (INamedTypeSymbol)c.Symbol;
            if (namedTypeSymbol.TypeKind != TypeKind.Interface)
            {
                return;
            }
            var contractAttribute = c.Compilation.GetTypeByMetadataName("Ifx.Proxy.ProxyContractAttribute");
            if (contractAttribute is null)
                return;
            var hasAttr = namedTypeSymbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, contractAttribute));
            if (!hasAttr)
            {
                return;
            }

            foreach (var m in namedTypeSymbol.GetMembers().OfType<IMethodSymbol>())
            {
                c.CancellationToken.ThrowIfCancellationRequested();
                if (m.MethodKind != MethodKind.Ordinary)
                {
                    continue;
                }

                var task = c.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
                var genericTask = c.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
                var valueTask = c.Compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
                var genericValueTask = c.Compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");
                var cancellationToken = c.Compilation.GetTypeByMetadataName("System.Threading.CancellationToken");
                var okReturn = SymbolEqualityComparer.Default.Equals(m.ReturnType, task)
                    || SymbolEqualityComparer.Default.Equals(m.ReturnType, valueTask)
                    || (m.ReturnType is INamedTypeSymbol returnType &&
                        (SymbolEqualityComparer.Default.Equals(returnType.OriginalDefinition, genericTask)
                         || SymbolEqualityComparer.Default.Equals(returnType.OriginalDefinition, genericValueTask)));
                var ps = m.Parameters;
                var okParams = ps.Length == 2
                    && !SymbolEqualityComparer.Default.Equals(ps[0].Type, cancellationToken)
                    && ps[0].RefKind == RefKind.None
                    && ps[1].RefKind == RefKind.None
                    && SymbolEqualityComparer.Default.Equals(ps[1].Type, cancellationToken);
                if (!(okReturn && okParams))
                {
                    c.ReportDiagnostic(Diagnostic.Create(Rule, m.Locations.FirstOrDefault(), m.Name));
                }
            }
        }, SymbolKind.NamedType);
    }
}
