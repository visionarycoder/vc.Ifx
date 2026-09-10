using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using vc.Ifx.Analyzers.Models;

namespace vc.Ifx.Analyzers.Rules.Framework
{

    internal static class Ifx005ControllerAttributeOrder
    {
        public static readonly DiagnosticDescriptor Rule = new(
            DiagnosticIds.Ifx005ControllerAttributeOrder,
            "Controller attributes should be in preferred order",
            "Controller attribute '{0}' should appear before '{1}'",
            "Design",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Ensures consistent controller attribute ordering and prefers role-based Authorize usage over separate RequiredScope attribute.",
            helpLinkUri: "Docs/Framework/ifx005.md");

        public static void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
        }

        private static void AnalyzeType(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol typeSymbol)
            {
                return;
            }

            if (!InheritsControllerBase(typeSymbol))
            {
                return;
            }

            var syntax = typeSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(context.CancellationToken) as ClassDeclarationSyntax;
            if (syntax is null || syntax.AttributeLists.Count == 0)
            {
                return;
            }

            var attributes = syntax.AttributeLists
                .SelectMany(list => list.Attributes)
                .Select(attribute => new
                {
                    Attribute = attribute,
                    Name = GetAttributeName(attribute)
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .ToList();

            if (attributes.Count == 0)
            {
                return;
            }

            var hasRoleAuthorize = attributes.Any(a => a.Name == "Authorize" && a.Attribute.ArgumentList?.Arguments.Any(arg => arg.NameEquals?.Name.Identifier.Text == "Roles") == true);
            var requiredScope = attributes.FirstOrDefault(a => a.Name == "RequiredScope");
            if (hasRoleAuthorize && requiredScope != null)
            {
                var diagnostic = Diagnostic.Create(Rule, requiredScope.Attribute.GetLocation(), "RequiredScope", "Authorize(Roles=...) when role-based access is used");
                context.ReportDiagnostic(diagnostic);
                return;
            }

            var preferredOrder = new[]
            {
            "Authorize",
            "ApiVersion",
            "Route",
            "ApiController",
            "FeatureGate",
            "EnableRateLimiting",
            "RequiredScope"
        };

            var expectedRank = -1;
            string? previousName = null;

            foreach (var item in attributes)
            {
                var currentRank = System.Array.IndexOf(preferredOrder, item.Name);
                if (currentRank < 0)
                {
                    continue;
                }

                if (currentRank < expectedRank)
                {
                    var diagnostic = Diagnostic.Create(Rule, item.Attribute.GetLocation(), item.Name, previousName ?? "previous attribute");
                    context.ReportDiagnostic(diagnostic);
                    return;
                }

                expectedRank = currentRank;
                previousName = item.Name;
            }
        }

        private static bool InheritsControllerBase(INamedTypeSymbol typeSymbol)
        {
            var current = typeSymbol;
            while (current != null)
            {
                if (current.BaseType?.ToDisplayString() == "Microsoft.AspNetCore.Mvc.ControllerBase")
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        private static string GetAttributeName(AttributeSyntax attribute)
        {
            var name = attribute.Name.ToString();

            if (name.EndsWith("Attribute"))
            {
                name = name.Substring(0, name.Length - 9);
            }

            var lastDot = name.LastIndexOf('.');
            if (lastDot >= 0)
            {
                name = name.Substring(lastDot + 1);
            }

            return name;
        }
    }
}
