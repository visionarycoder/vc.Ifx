using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Ifx.Generators;

/// <summary>
/// Generates reflection-free, AutoMapper-style property mappers for classes decorated with
/// <c>[GenerateMapper(typeof(Target))]</c>. Mapping is opt-in per class, not name-convention based.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class MapperGenerator : IIncrementalGenerator
{
    private const string AttributeNamespace = "Ifx.Generators.Abstractions.Attributes";
    private const string GenerateMapperAttributeFullName = AttributeNamespace + ".GenerateMapperAttribute";
    private const string MapFromAttributeFullName = AttributeNamespace + ".MapFromAttribute";
    private const string MapIgnoreAttributeFullName = AttributeNamespace + ".MapIgnoreAttribute";

    private static readonly DiagnosticDescriptor MappingError = new("GEN001", "Mapping error", "{0}", "Mapping", DiagnosticSeverity.Error, true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var declarations = context.SyntaxProvider.ForAttributeWithMetadataName(
            GenerateMapperAttributeFullName,
            (node, _) => node is ClassDeclarationSyntax,
            (ctx, cancellationToken) => CreateDeclarations(ctx, cancellationToken))
            .SelectMany((declarations, _) => declarations);

        context.RegisterSourceOutput(declarations, Emit);
    }

    private static ImmutableArray<MapperDeclaration> CreateDeclarations(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sourceType = (INamedTypeSymbol)context.TargetSymbol;
        var declarations = ImmutableArray.CreateBuilder<MapperDeclaration>();

        foreach (var attribute in context.Attributes)
        {
            if (attribute.ConstructorArguments.Length == 0 || attribute.ConstructorArguments[0].Value is not INamedTypeSymbol targetType)
            {
                continue;
            }

            declarations.Add(new MapperDeclaration(sourceType, targetType, IsBidirectional(attribute), GetAttributeLocation(attribute, cancellationToken)));
        }

        return declarations.ToImmutable();
    }

    private static void Emit(SourceProductionContext context, MapperDeclaration declaration)
    {
        var forward = BuildDirection(context, declaration.Source, declaration.Target, declaration.Location);
        AddSource(context, declaration.Source, declaration.Target, forward);

        if (declaration.Bidirectional)
        {
            var reverse = BuildDirection(context, declaration.Target, declaration.Source, declaration.Location);
            AddSource(context, declaration.Target, declaration.Source, reverse);
        }
    }

    private static string BuildDirection(SourceProductionContext context, INamedTypeSymbol source, INamedTypeSymbol target, Location location)
    {
        var sourceProperties = ReadableProperties(source);
        var assignments = new StringBuilder();

        foreach (var targetProperty in WritableProperties(target))
        {
            var sourceName = GetMapFromSourceName(targetProperty);
            sourceName ??= targetProperty.Name;

            if (!sourceProperties.TryGetValue(sourceName, out var sourceProperty))
            {
                if (HasAttribute(targetProperty, MapIgnoreAttributeFullName)) { continue; }
                context.ReportDiagnostic(Diagnostic.Create(MappingError, GetDiagnosticLocation(targetProperty, location),
                    $"'{target.ToDisplayString()}.{targetProperty.Name}' has no matching member '{sourceName}' on '{source.ToDisplayString()}'. Add [MapFrom] to redirect it or [MapIgnore] to exclude it."));
                continue;
            }

            if (!SymbolEqualityComparer.Default.Equals(sourceProperty.Type, targetProperty.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(MappingError, GetDiagnosticLocation(targetProperty, location),
                    $"Type mismatch mapping '{source.ToDisplayString()}.{sourceProperty.Name}' ({sourceProperty.Type}) to '{target.ToDisplayString()}.{targetProperty.Name}' ({targetProperty.Type})."));
                continue;
            }

            assignments.Append("            ").Append(targetProperty.Name).Append(" = source.").Append(sourceProperty.Name).AppendLine(",");
        }

        return assignments.ToString();
    }

    private static void AddSource(SourceProductionContext context, INamedTypeSymbol source, INamedTypeSymbol target, string assignments)
    {
        var sourceName = source.ToDisplayString();
        var targetName = target.ToDisplayString();
        var className = $"{Sanitize(source)}To{Sanitize(target)}Mapper";
        var isSourceReferenceType = source.IsReferenceType;

        var code = new StringBuilder();
        code.AppendLine("// <auto-generated />");
        code.AppendLine("#nullable enable");
        code.AppendLine();
        code.AppendLine("namespace Generated.Mappers;");
        code.AppendLine();
        code.Append("public static class ").AppendLine(className);
        code.AppendLine("{");
        code.Append("    public static ").Append(targetName).Append(" Map(").Append(sourceName).AppendLine(isSourceReferenceType ? "? source)" : " source)");
        code.AppendLine("    {");
        if (isSourceReferenceType)
        {
            code.AppendLine("        if (source is null) { return default!; }");
            code.AppendLine();
        }
        code.Append("        return new ").AppendLine(targetName);
        code.AppendLine("        {");
        code.Append(assignments);
        code.AppendLine("        };");
        code.AppendLine("    }");
        code.AppendLine("}");

        context.AddSource($"{className}.g.cs", SourceText.From(code.ToString(), Encoding.UTF8));
    }

    private static Dictionary<string, IPropertySymbol> ReadableProperties(INamedTypeSymbol type) =>
        AllProperties(type).Where(p => !p.IsStatic && p.GetMethod is not null && p.DeclaredAccessibility == Accessibility.Public)
            .GroupBy(p => p.Name).ToDictionary(g => g.Key, g => g.First());

    private static IEnumerable<IPropertySymbol> WritableProperties(INamedTypeSymbol type) =>
        AllProperties(type).Where(p => !p.IsStatic && p.SetMethod is not null && p.DeclaredAccessibility == Accessibility.Public)
            .GroupBy(p => p.Name).Select(g => g.First());

    private static IEnumerable<IPropertySymbol> AllProperties(INamedTypeSymbol type)
    {
        for (var current = type; current is not null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
        {
            foreach (var member in current.GetMembers().OfType<IPropertySymbol>())
            {
                yield return member;
            }
        }
    }

    private static bool HasAttribute(ISymbol symbol, string attributeFullName) =>
        symbol.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == attributeFullName);

    private static bool IsBidirectional(AttributeData attribute) =>
        attribute.NamedArguments.Any(pair => pair.Key == "Bidirectional" && pair.Value.Value is true);

    private static Location GetAttributeLocation(AttributeData attribute, CancellationToken cancellationToken) =>
        attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation() ?? Location.None;

    private static Location GetDiagnosticLocation(ISymbol symbol, Location fallback) =>
        symbol.Locations.FirstOrDefault() ?? fallback;

    private static string? GetMapFromSourceName(IPropertySymbol targetProperty)
    {
        var mapFrom = targetProperty.GetAttributes().FirstOrDefault(attribute => attribute.AttributeClass?.ToDisplayString() == MapFromAttributeFullName);
        return mapFrom is not null && mapFrom.ConstructorArguments.Length > 0 ? mapFrom.ConstructorArguments[0].Value as string : null;
    }

    private static string Sanitize(INamedTypeSymbol type) =>
        type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", string.Empty).Replace('.', '_');

    private sealed class MapperDeclaration(INamedTypeSymbol source, INamedTypeSymbol target, bool bidirectional, Location location)
    {
        public INamedTypeSymbol Source { get; } = source;
        public INamedTypeSymbol Target { get; } = target;
        public bool Bidirectional { get; } = bidirectional;
        public Location Location { get; } = location;
    }
}
