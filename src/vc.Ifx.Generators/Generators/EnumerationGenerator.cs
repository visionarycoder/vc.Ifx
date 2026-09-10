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
public sealed class EnumerationGenerator : IIncrementalGenerator
{
    private const string GeneratedAttributeNamespace = "vc.Ifx.Generators";
    private const string AttributeNamespace = "vc.Ifx.Generators.Abstractions.Attributes";
    private const string AttributeName = "GenerateEnumerationAttribute";
    private const string GeneratedAttributeFullName = GeneratedAttributeNamespace + "." + AttributeName;
    private const string AttributeFullName = AttributeNamespace + "." + AttributeName;

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var candidates = context.SyntaxProvider.ForAttributeWithMetadataName(
                AttributeFullName,
                (node, _) => node is EnumDeclarationSyntax,
                (ctx, cancellationToken) => CreateModel(ctx, cancellationToken))
                .Where(model => model != null);

            var generatedAttributeCandidates = context.SyntaxProvider.ForAttributeWithMetadataName(
                GeneratedAttributeFullName,
                (node, _) => node is EnumDeclarationSyntax,
                (ctx, cancellationToken) => CreateModel(ctx, cancellationToken))
                .Where(model => model != null);

            context.RegisterSourceOutput(candidates, (ctx, model) =>
            {
                if (model != null)
                {
                    Emit(ctx, model);
                }
            });

            context.RegisterSourceOutput(generatedAttributeCandidates, (ctx, model) =>
            {
                if (model != null)
                {
                    Emit(ctx, model);
                }
            });
        }

        private static EnumerationModel? CreateModel(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!(context.TargetSymbol is INamedTypeSymbol enumSymbol))
            {
                return null;
            }

            if (context.Attributes.Length == 0)
            {
                return null;
            }

            var attribute = context.Attributes[0];

            var className = GetConstructorValue(attribute, 0);
            if (string.IsNullOrWhiteSpace(className))
            {
                return null;
            }

            var enumNamespace = enumSymbol.ContainingNamespace.IsGlobalNamespace ? null : enumSymbol.ContainingNamespace.ToDisplayString();

            var outputNamespace = GetNamedValue(attribute, "Namespace");
            if (string.IsNullOrWhiteSpace(outputNamespace))
            {
                outputNamespace = enumNamespace;
            }

            var defaultName = GetConstructorValue(attribute, 1);
            if (string.IsNullOrWhiteSpace(defaultName))
            {
                defaultName = GetNamedValue(attribute, "DefaultName");
            }

            var enumerationNamespace = GetNamedValue(attribute, "EnumerationNamespace");
            var enumerationTypeName = GetNamedValue(attribute, "EnumerationTypeName");

            if (string.IsNullOrWhiteSpace(enumerationNamespace))
            {
                enumerationNamespace = "vc.Ifx.Primitives";
            }

            if (string.IsNullOrWhiteSpace(enumerationTypeName))
            {
                enumerationTypeName = "Enumeration";
            }

            var members = new List<EnumerationMember>();

            foreach (var member in enumSymbol.GetMembers().OfType<IFieldSymbol>())
            {
                if (!member.HasConstantValue)
                {
                    continue;
                }

                if (member.ConstantValue is null)
                {
                    continue;
                }

                var id = Convert.ToInt32(member.ConstantValue);
                members.Add(new EnumerationMember(member.Name, id));
            }

            if (members.Count == 0)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(defaultName))
            {
                defaultName = members[0].Name;
            }

            if (members.All(member => member.Name != defaultName))
            {
                defaultName = members[0].Name;
            }

            return new EnumerationModel(
                outputNamespace ?? string.Empty,
                className ?? string.Empty,
                defaultName ?? string.Empty,
                enumerationNamespace ?? string.Empty,
                enumerationTypeName ?? string.Empty,
                members.ToImmutableArray());
        }

        private static string? GetConstructorValue(AttributeData attribute, int index)
        {
            if (attribute.ConstructorArguments.Length <= index)
            {
                return null;
            }

            return attribute.ConstructorArguments[index].Value as string;
        }

        private static string? GetNamedValue(AttributeData attribute, string name)
        {
            foreach (var argument in attribute.NamedArguments)
            {
                if (argument.Key == name)
                {
                    return argument.Value.Value as string;
                }
            }

            return null;
        }

        private static void Emit(SourceProductionContext context, EnumerationModel model)
        {
            var source = GenerateSource(model);
            var hintName = $"{model.ClassName}.Enumeration.g.cs";

            context.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
        }

        private static string GenerateSource(EnumerationModel model)
        {
            var source = new StringBuilder();

            source.AppendLine("// <auto-generated />");
            source.AppendLine("#nullable enable");
            source.AppendLine();
            source.Append("using ");
            source.Append(model.EnumerationNamespace);
            source.AppendLine(";");
            source.AppendLine();

            if (!string.IsNullOrWhiteSpace(model.Namespace))
            {
                source.Append("namespace ");
                source.Append(model.Namespace);
                source.AppendLine(";");
                source.AppendLine();
            }

            source.Append("public sealed class ");
            source.Append(model.ClassName);
            source.Append(" : ");
            source.Append(model.EnumerationTypeName);
            source.AppendLine();
            source.AppendLine("{");

            foreach (var member in model.Members)
            {
                source.Append("    public static readonly ");
                source.Append(model.ClassName);
                source.Append(' ');
                source.Append(member.Name);
                source.Append(" = new(");
                source.Append(member.Id);
                source.Append(", nameof(");
                source.Append(member.Name);
                source.AppendLine("));");
            }

            source.AppendLine();
            source.Append("    private ");
            source.Append(model.ClassName);
            source.AppendLine("(int value, string name) : base(value, name) { }");
            source.AppendLine();

            source.Append("    public static global::System.Collections.Generic.IReadOnlyCollection<");
            source.Append(model.ClassName);
            source.Append("> All { get; } = new ");
            source.Append(model.ClassName);
            source.AppendLine("[]");
            source.AppendLine("    {");

            foreach (var member in model.Members)
            {
                source.Append("        ");
                source.Append(member.Name);
                source.AppendLine(",");
            }

            source.AppendLine("    };");
            source.AppendLine();

            source.Append("    public static ");
            source.Append(model.ClassName);
            source.AppendLine(" FromValue(int value)");
            source.AppendLine("    {");
            source.AppendLine("        foreach (var item in All)");
            source.AppendLine("        {");
            source.AppendLine("            if (item.Id == value)");
            source.AppendLine("            {");
            source.AppendLine("                return item;");
            source.AppendLine("            }");
            source.AppendLine("        }");
            source.AppendLine();
            source.Append("        return ");
            source.Append(model.DefaultName);
            source.AppendLine(";");
            source.AppendLine("    }");
            source.AppendLine();

            source.Append("    public static ");
            source.Append(model.ClassName);
            source.AppendLine(" FromName(string? name)");
            source.AppendLine("    {");
            source.AppendLine("        if (string.IsNullOrWhiteSpace(name))");
            source.AppendLine("        {");
            source.Append("            return ");
            source.Append(model.DefaultName);
            source.AppendLine(";");
            source.AppendLine("        }");
            source.AppendLine();
            source.AppendLine("        foreach (var item in All)");
            source.AppendLine("        {");
            source.AppendLine("            if (item.Name == name)");
            source.AppendLine("            {");
            source.AppendLine("                return item;");
            source.AppendLine("            }");
            source.AppendLine("        }");
            source.AppendLine();
            source.Append("        return ");
            source.Append(model.DefaultName);
            source.AppendLine(";");
            source.AppendLine("    }");
            source.AppendLine();
            source.AppendLine($"    public static bool operator ==({model.ClassName}? left, {model.ClassName}? right) {{ return left?.Id == right?.Id; }}");
            source.AppendLine();

            source.AppendLine($"    public static bool operator !=({model.ClassName}? left, {model.ClassName}? right) {{ return left?.Id != right?.Id; }}");
            source.AppendLine();

            source.AppendLine($"    public override bool Equals(object? obj) {{ return obj is {model.ClassName} other && Id == other.Id; }}");
            source.AppendLine();
            source.AppendLine("    public override int GetHashCode() { return Id.GetHashCode();} ");
            source.AppendLine();
            source.AppendLine("}");
            
            return source.ToString();
        }

        private sealed class EnumerationModel(string ns, string className, string defaultName, string enumerationNamespace, string enumerationTypeName, ImmutableArray<EnumerationMember> members)
        {
            public string Namespace { get; } = ns;
            public string ClassName { get; } = className;
            public string DefaultName { get; } = defaultName;
            public string EnumerationNamespace { get; } = enumerationNamespace;
            public string EnumerationTypeName { get; } = enumerationTypeName;
            public ImmutableArray<EnumerationMember> Members { get; } = members;
        }

        private sealed class EnumerationMember(string name, int id)
        {
            public string Name { get; } = name;
            public int Id { get; } = id;
        }
}
