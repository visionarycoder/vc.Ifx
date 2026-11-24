using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ifx.Roslyn.Reporting;

/// <summary>Inspects resolved component calls against an externally reviewed policy.</summary>
internal static class DesignConformance
{
    internal static string[] Check(JsonElement request)
    {
        string assembly = request.GetProperty("assembly").GetString()!;
        string component = request.GetProperty("component").GetString()!;
        Dictionary<string, string> components = request.GetProperty("assemblies").EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value.GetString()!, StringComparer.Ordinal);
        HashSet<string> contracts = request.GetProperty("allowed_contracts").EnumerateArray()
            .Select(p => p.GetString()!).ToHashSet(StringComparer.Ordinal);
        HashSet<string> dataTypes = request.GetProperty("allowed_data_types").EnumerateArray()
            .Select(p => p.GetString()!).ToHashSet(StringComparer.Ordinal);
        string[] symbols = request.GetProperty("symbols").EnumerateArray().Select(p => p.GetString()!).ToArray();
        if (!LanguageVersionFacts.TryParse(request.GetProperty("language").GetString()!, out LanguageVersion language))
            throw new ArgumentException("Unrecognized design-check language version.");
        CSharpParseOptions options = new(language,
            preprocessorSymbols: symbols);
        SyntaxTree[] trees = request.GetProperty("sources").EnumerateArray()
            .Select(p => p.GetString()!).Distinct(StringComparer.Ordinal)
            .Select(p => CSharpSyntaxTree.ParseText(File.ReadAllText(p), options, p)).ToArray();
        MetadataReference[] references = request.GetProperty("references").EnumerateArray()
            .Select(p => p.GetString()!).Distinct(StringComparer.Ordinal)
            .Select(p => MetadataReference.CreateFromFile(p)).ToArray();
        OutputKind outputKind = request.GetProperty("output_type").GetString() switch
        {
            "Library" => OutputKind.DynamicallyLinkedLibrary,
            "Exe" => OutputKind.ConsoleApplication,
            "WinExe" => OutputKind.WindowsApplication,
            _ => throw new ArgumentException("Unsupported design-check output type.")
        };
        CSharpCompilation compilation = CSharpCompilation.Create(assembly, trees, references,
            new CSharpCompilationOptions(outputKind,
                allowUnsafe: request.GetProperty("allow_unsafe").GetBoolean(),
                checkOverflow: request.GetProperty("check_overflow").GetBoolean()));
        return CheckCompilation(compilation, component, components, contracts, dataTypes);
    }

    internal static string[] CheckCompilation(CSharpCompilation compilation, string component,
        IReadOnlyDictionary<string, string> components, IReadOnlySet<string> contracts,
        IReadOnlySet<string>? dataTypes = null)
    {
        List<string> issues = [];
        if (!components.TryGetValue(compilation.AssemblyName!, out string? owner) || owner != component)
            issues.Add("Compilation has no matching component assignment.");
        foreach (Diagnostic error in compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error))
            issues.Add("Unresolved compilation: " + error);
        foreach (SyntaxTree tree in compilation.SyntaxTrees)
        {
            SemanticModel model = compilation.GetSemanticModel(tree);
            foreach (SyntaxNode node in tree.GetRoot().DescendantNodes())
            {
                // Inspect references as well as invocations: covers method groups,
                // constructors, properties, fields, events and operator symbols.
                if (node is not ExpressionSyntax) continue;
                if (model.GetTypeInfo(node).Type?.TypeKind == TypeKind.Dynamic)
                    issues.Add($"Dynamic dispatch requires separate verification: {tree.FilePath}");
                ISymbol? symbol = model.GetSymbolInfo(node).Symbol;
                if (symbol is IMethodSymbol { ReducedFrom: not null } method) symbol = method.ReducedFrom;
                INamedTypeSymbol? type = symbol is INamedTypeSymbol named ? named : symbol?.ContainingType;
                if (type is null || !components.TryGetValue(type.ContainingAssembly.Name, out string? target)
                    || target == component) continue;
                string identity = type.OriginalDefinition.ToDisplayString();
                if (type.TypeKind == TypeKind.Interface && contracts.Contains(identity)) continue;
                if (dataTypes?.Contains(identity) == true) continue;
                FileLinePositionSpan location = tree.GetLineSpan(node.Span);
                issues.Add($"Forbidden component reference: {component} -> {target} {identity} at {location}");
            }
        }
        return issues.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
    }
}
