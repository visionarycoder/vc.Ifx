using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using vc.Ifx.Roslyn;

namespace vc.Ifx.Analyzers.Rules.Diagnostics;

/// <summary>Enforces meaningful disposition of diagnostic references and suppressions.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DiagnosticDebtAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Shared comment-disposition rule.</summary>
    public static readonly DiagnosticDescriptor DiagnosticReferenceRule = DiagnosticDescriptors.DiagnosticReferenceRequiresDisposition;

    /// <summary>Shared suppression-justification rule.</summary>
    public static readonly DiagnosticDescriptor DiagnosticSuppressionRule = DiagnosticDescriptors.DiagnosticSuppressionRequiresJustification;

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => DiagnosticDescriptors.All;

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
        context.RegisterSyntaxNodeAction(AnalyzeSuppressionAttribute, SyntaxKind.Attribute);
    }

    private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        AnalyzerConfigOptions options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Tree);
        bool comments = IsEnabled(options, "dotnet_code_quality.IFX1000.analyze_comments");
        bool suppressions = IsEnabled(options, "dotnet_code_quality.IFX1001.analyze_suppressions");
        if (!comments && !suppressions)
        {
            return;
        }

        foreach (SyntaxTrivia trivia in context.Tree.GetRoot(context.CancellationToken).DescendantTrivia(descendIntoTrivia: true))
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            if (trivia.GetStructure() is PragmaWarningDirectiveTriviaSyntax directive)
            {
                if (suppressions && directive.IsActive && directive.DisableOrRestoreKeyword.IsKind(SyntaxKind.DisableKeyword))
                {
                    AnalyzePragma(context, directive);
                }
            }
            else if (comments && IsComment(trivia) &&
                !trivia.Token.Parent!.AncestorsAndSelf().OfType<PragmaWarningDirectiveTriviaSyntax>().Any())
            {
                string text = trivia.ToFullString();
                if (DebtDisposition.HasMarker(text, justificationOnly: false))
                {
                    continue;
                }

                foreach (Match match in DiagnosticIdPattern.ReferenceMatcher.Matches(text))
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                    context.ReportDiagnostic(CreateDiagnostic(DiagnosticReferenceRule,
                        Location.Create(context.Tree, new TextSpan(trivia.FullSpan.Start + match.Index, match.Length)), match.Value));
                }
            }
        }
    }

    private static void AnalyzePragma(SyntaxTreeAnalysisContext context, PragmaWarningDirectiveTriviaSyntax directive)
    {
        if (directive.DescendantTrivia().Where(IsComment).Any(trivia => DebtDisposition.HasMarker(trivia.ToFullString(), justificationOnly: true)))
        {
            return;
        }

        foreach (ExpressionSyntax code in directive.ErrorCodes)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            string id = code.ToString();
            if (int.TryParse(id, NumberStyles.None, CultureInfo.InvariantCulture, out int numeric) && numeric > 0 && numeric <= 9999)
            {
                id = "CS" + numeric.ToString("D4", CultureInfo.InvariantCulture);
            }

            if (IsSupportedId(id))
            {
                context.ReportDiagnostic(CreateDiagnostic(DiagnosticSuppressionRule, code.GetLocation(), id));
            }
        }
    }

    private static void AnalyzeSuppressionAttribute(SyntaxNodeAnalysisContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        if (!IsEnabled(context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree), "dotnet_code_quality.IFX1001.analyze_suppressions"))
        {
            return;
        }

        AttributeSyntax attribute = (AttributeSyntax)context.Node;
        if (context.SemanticModel.GetSymbolInfo(attribute, context.CancellationToken).Symbol is not IMethodSymbol constructor ||
            constructor.ContainingType.ToDisplayString() is not ("System.Diagnostics.CodeAnalysis.SuppressMessageAttribute" or "System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessageAttribute"))
        {
            return;
        }

        ExpressionSyntax? checkId = null;
        string? justification = null;
        for (int index = 0; index < attribute.ArgumentList!.Arguments.Count; index++)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            AttributeArgumentSyntax argument = attribute.ArgumentList.Arguments[index];
            if (argument.NameEquals?.Name.Identifier.ValueText == "Justification")
            {
                justification = context.SemanticModel.GetConstantValue(argument.Expression, context.CancellationToken).Value as string;
            }
            else if (argument.NameEquals is null &&
                (argument.NameColon?.Name.Identifier.ValueText == "checkId" || (argument.NameColon is null && index == 1)))
            {
                checkId = argument.Expression;
            }
        }

        if (checkId is null || DebtDisposition.IsMeaningful(justification) ||
            context.SemanticModel.GetConstantValue(checkId, context.CancellationToken).Value is not string value)
        {
            return;
        }

        string id = value.Split(':')[0].Trim();
        if (IsSupportedId(id))
        {
            context.ReportDiagnostic(CreateDiagnostic(DiagnosticSuppressionRule, checkId.GetLocation(), id));
        }
    }

    private static bool IsSupportedId(string id)
    {
        Match match = DiagnosticIdPattern.ReferenceMatcher.Match(id);
        return match.Success && match.Length == id.Length;
    }

    private static bool IsEnabled(AnalyzerConfigOptions options, string key) =>
        !options.TryGetValue(key, out string? text) || !bool.TryParse(text, out bool enabled) || enabled;

    private static bool IsComment(SyntaxTrivia trivia) => trivia.Kind() is SyntaxKind.SingleLineCommentTrivia
        or SyntaxKind.MultiLineCommentTrivia or SyntaxKind.SingleLineDocumentationCommentTrivia or SyntaxKind.MultiLineDocumentationCommentTrivia;

    private static Diagnostic CreateDiagnostic(DiagnosticDescriptor descriptor, Location location, string id) =>
        Diagnostic.Create(descriptor, location, ImmutableDictionary<string, string?>.Empty.Add(DiagnosticPropertyNames.DiagnosticId, id), id);
}
