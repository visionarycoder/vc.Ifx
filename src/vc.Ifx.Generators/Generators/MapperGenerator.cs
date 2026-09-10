using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace vc.Ifx.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class MapperGenerator : IIncrementalGenerator
{

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => s is ClassDeclarationSyntax,
                    transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
                .Where(c => c.Identifier.Text == "Crosswalk");

            var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndClasses, Execute);
        }

        private static void Execute(SourceProductionContext context, (Compilation, ImmutableArray<ClassDeclarationSyntax>) input)
        {

            var (compilation, classDeclarations) = input;
            var namedTypeSymbols = new List<INamedTypeSymbol>();

            foreach (var classDecl in classDeclarations)
            {
                var model = compilation.GetSemanticModel(classDecl.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
                if (symbol != null)
                {
                    namedTypeSymbols.Add(symbol);
                }
            }

            // Group by name (Crosswalk)
            var groups = namedTypeSymbols.GroupBy(t => t.Name);
            foreach (var group in groups)
            {
                var types = group.ToList();

                if (types.Count < 2)
                    continue;

                // Generate pairwise mappings
                for (int i = 0; i < types.Count; i++)
                {
                    for (int j = 0; j < types.Count; j++)
                    {
                        if (i == j) continue;

                        var source = types[i];
                        var target = types[j];

                        GenerateMapper(context, source, target);
                    }
                }
            }
        }

        private static void GenerateMapper(SourceProductionContext context, INamedTypeSymbol source, INamedTypeSymbol target)
        {

            var sourceProps = source.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => !p.IsStatic)
                .ToDictionary(p => p.Name);

            var targetProps = target.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => !p.IsStatic);

            var assignments = new StringBuilder();

            foreach (var targetProp in targetProps)
            {
                if (!sourceProps.TryGetValue(targetProp.Name, out var sourceProp))
                {
                    ReportError(context, $"Missing property '{targetProp.Name}' in source type '{source.Name}'");
                    continue;
                }

                if (!SymbolEqualityComparer.Default.Equals(sourceProp.Type, targetProp.Type))
                {
                    ReportError(context,
                        $"Type mismatch for property '{targetProp.Name}': {sourceProp.Type} -> {targetProp.Type}");
                    continue;
                }

                assignments.AppendLine($"            {targetProp.Name} = source.{sourceProp.Name},");
            }

            var sourceName = source.ToDisplayString();
            var targetName = target.ToDisplayString();

            var className = $"{source.Name}Mapper_{Sanitize(source.ContainingNamespace)}_To_{Sanitize(target.ContainingNamespace)}";

            var code = $@"
namespace Generated.Mappers;

public static class {className}
{{
    public static {targetName} Map({sourceName} source)
    {{
        if (source is null) return null;

        return new {targetName}
        {{
{assignments}
        }};
    }}
}}";

            context.AddSource($"{className}.g.cs", SourceText.From(code, Encoding.UTF8));

        }

        private static void ReportError(SourceProductionContext context, string message)
        {
            var descriptor = new DiagnosticDescriptor(
                id: "GEN001",
                title: "Mapping Error",
                messageFormat: message,
                category: "Mapping",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            context.ReportDiagnostic(Diagnostic.Create(descriptor, Location.None));
        }

        private static string Sanitize(INamespaceSymbol ns) => ns.ToDisplayString().Replace('.', '_');
}
