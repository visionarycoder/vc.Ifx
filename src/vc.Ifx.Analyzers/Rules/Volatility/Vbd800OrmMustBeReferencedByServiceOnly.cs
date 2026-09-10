using System.Threading;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Helpers;
using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Volatility;

/// <summary>
/// VBD800: ORM project should only be referenced by its corresponding Service project
/// Volatility-Based Decomposition: ORM implementation details should be encapsulated
/// </summary>
public static class Vbd800OrmMustBeReferencedByServiceOnly
{
    public static readonly DiagnosticDescriptor Rule = new(id: DiagnosticIds.Vbd800OrmMustBeReferencedByServiceOnly, title: "ORM project should only be referenced by its corresponding Service project", messageFormat: "ORM project '{1}' should only be referenced by its Service project. Project '{0}' should not directly reference ORM implementations.", category: "Architecture.Volatility", defaultSeverity: DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "ORM projects contain database-specific implementations and should be encapsulated within their Service layer. This prevents database concerns from leaking into higher layers.", helpLinkUri: "https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#legacy-vbd-policy");

    public static void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var namedType = (INamedTypeSymbol)context.Symbol;

        var assemblyName = context.Compilation.Assembly.Name;

        var currentProjectType = ProjectAnalyzer.GetProjectType(assemblyName!);
        if (currentProjectType is ProjectType.Test or ProjectType.Unknown)
        {
            return;
        }

        var violation = FindInvalidOrmReference(namedType, assemblyName!, currentProjectType, context.CancellationToken);
        if (violation == null)
        {
            return;
        }

        var (sourceMember, referencedAssembly) = violation.Value;
        var location = sourceMember.Locations[0];
        var diagnostic = Diagnostic.Create(Rule, location, assemblyName!, referencedAssembly);
        context.ReportDiagnostic(diagnostic);
    }

    private static (ISymbol Source, string ReferencedAssembly)? FindInvalidOrmReference(INamedTypeSymbol type, string assemblyName, ProjectType currentProjectType, CancellationToken cancellationToken)
    {
        foreach (var (source, referencedType) in TypeDependencies.Enumerate(type, cancellationToken))
        {
            var referencedAssembly = referencedType.ContainingAssembly?.Name;
            if (string.IsNullOrWhiteSpace(referencedAssembly))
            {
                continue;
            }

            var referencedProjectType = ProjectAnalyzer.GetProjectType(referencedAssembly!);
            if (referencedProjectType != ProjectType.Orm)
            {
                continue;
            }

            if (IsAllowedReference(currentProjectType, assemblyName, referencedAssembly!))
            {
                continue;
            }

            return (source, referencedAssembly!);
        }

        return null;
    }

    private static bool IsAllowedReference(ProjectType currentProjectType, string referencingAssembly, string referencedAssembly)
    {
        if (currentProjectType is ProjectType.Orm or ProjectType.Infrastructure)
        {
            return true;
        }

        if (currentProjectType == ProjectType.Service)
        {
            var referencingBase = referencingAssembly.Substring(0, referencingAssembly.Length - ".Service".Length);

            var referencedBase = RemoveSuffixes(referencedAssembly, [".Orm", ".Administration", ".Financial"]);

            return referencingBase.Equals(referencedBase, System.StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static string RemoveSuffixes(string value, string[] suffixes)
    {
        // Strip repeatedly until no suffix matches. ORM assemblies are named with a
        // compound suffix (e.g. 'Access.Advantage.Orm.Financial'), so a single ordered
        // pass would leave a trailing '.Orm' and wrongly flag the owning Service's
        // references to its own ORM as a violation.
        var result = value;
        bool removedAny;
        do
        {
            removedAny = false;
            foreach (var suffix in suffixes)
            {
                if (result.EndsWith(suffix, System.StringComparison.OrdinalIgnoreCase))
                {
                    result = result.Substring(0, result.Length - suffix.Length);
                    removedAny = true;
                }
            }
        }
        while (removedAny);

        return result;
    }

}
