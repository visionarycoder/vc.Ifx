using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using vc.Ifx.Roslyn;

namespace vc.Ifx.Analyzers.Rules.CodeQuality;

/// <summary>Enforces the versioned ordinary-method control-flow nesting policy.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MethodNestingAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Identifier for excessive control-flow statement nesting.</summary>
    public const string DiagnosticId = "IFX1100";

    /// <summary>Stable interpretation of the emitted metric facts.</summary>
    public const string MetricVersion = "ifx-control-nesting-v1";

    /// <summary>Descriptor for the method nesting policy.</summary>
    public static readonly DiagnosticDescriptor Rule = new(DiagnosticId,
        "Method control-flow nesting exceeds policy",
        "Method '{0}' has control-flow nesting depth {1} (maximum {2})",
        DiagnosticCategories.Maintainability, DiagnosticSeverity.Warning, isEnabledByDefault: true,
        description: "Limit nested control-flow statements in ordinary method bodies, excluding nested functions.",
        helpLinkUri: "https://github.com/visionarycoder/vc.Ifx/blob/main/docs/roslyn/diagnostic-catalog.md#ifx1100");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.MethodDeclaration);
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        MethodDeclarationSyntax method = (MethodDeclarationSyntax)context.Node;
        if (method.Body is null)
        {
            return;
        }

        AnalyzerConfigOptions options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(method.SyntaxTree);
        int limit = 4;
        if (options.TryGetValue("dotnet_code_quality.IFX1100.max_nesting_depth", out string? text)
            && int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int configured)
            && configured >= 0 && configured <= 64)
        {
            limit = configured;
        }

        int maximum = 0;
        Stack<(SyntaxNode Node, int Depth)> pending = new();
        pending.Push((method.Body, 0));
        while (pending.Count > 0)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            (SyntaxNode node, int depth) = pending.Pop();
            if (node is LocalFunctionStatementSyntax or AnonymousFunctionExpressionSyntax)
            {
                continue;
            }

            if (node is IfStatementSyntax or SwitchStatementSyntax or ForStatementSyntax or CommonForEachStatementSyntax
                or WhileStatementSyntax or DoStatementSyntax or TryStatementSyntax)
            {
                depth++;
                if (depth > maximum)
                {
                    maximum = depth;
                }
            }

            foreach (SyntaxNode child in node.ChildNodes())
            {
                pending.Push((child, depth));
            }
        }

        if (maximum > limit)
        {
            ImmutableDictionary<string, string?> properties = ImmutableDictionary<string, string?>.Empty
                .Add("MetricName", "ControlFlowNestingDepth")
                .Add("MetricVersion", MetricVersion)
                .Add("MeasuredValue", maximum.ToString(CultureInfo.InvariantCulture))
                .Add("Threshold", limit.ToString(CultureInfo.InvariantCulture));
            context.ReportDiagnostic(Diagnostic.Create(Rule, method.Identifier.GetLocation(), properties, method.Identifier.ValueText, maximum, limit));
        }
    }
}
