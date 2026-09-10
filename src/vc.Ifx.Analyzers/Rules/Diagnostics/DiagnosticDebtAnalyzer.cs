using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using vc.Ifx.Roslyn;

namespace vc.Ifx.Analyzers.Rules.Diagnostics;

/// <summary>
/// Finds references to C# compiler diagnostics and .NET code-analysis diagnostics in source.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DiagnosticDebtAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Diagnostic raised when a source comment mentions a compiler or code-analysis diagnostic without a disposition marker.
    /// </summary>
    public static readonly DiagnosticDescriptor DiagnosticReferenceRule = new(
        DiagnosticIdentifiers.DiagnosticReferenceRequiresDisposition,
        "Diagnostic reference requires disposition",
        "Diagnostic '{0}' is referenced without an explicit disposition",
        "Maintainability",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "References to CA#### and CS#### diagnostics should state whether the issue will be fixed, is intentionally suppressed, or is tracked elsewhere.");

    /// <summary>
    /// Diagnostic raised when pragma warning suppressions lack an explicit justification comment.
    /// </summary>
    public static readonly DiagnosticDescriptor DiagnosticSuppressionRule = new(
        DiagnosticIdentifiers.DiagnosticSuppressionRequiresJustification,
        "Diagnostic suppression requires justification",
        "Suppression for diagnostic '{0}' requires a justification",
        "Maintainability",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Suppressions for CA#### and CS#### diagnostics should include an explicit justification so diagnostic debt can be reviewed.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticReferenceRule, DiagnosticSuppressionRule);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
    }

    private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        SyntaxNode root = context.Tree.GetRoot(context.CancellationToken);

        foreach (SyntaxTrivia trivia in root.DescendantTrivia(descendIntoTrivia: true))
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (trivia.IsKind(SyntaxKind.PragmaWarningDirectiveTrivia))
            {
                AnalyzePragmaWarningDirective(context, trivia);
                continue;
            }

            if (IsComment(trivia))
            {
                AnalyzeComment(context, trivia);
            }
        }
    }

    private static void AnalyzePragmaWarningDirective(SyntaxTreeAnalysisContext context, SyntaxTrivia trivia)
    {
        if (trivia.GetStructure() is not PragmaWarningDirectiveTriviaSyntax directive || !directive.DisableOrRestoreKeyword.IsKind(SyntaxKind.DisableKeyword))
        {
            return;
        }

        string text = trivia.ToFullString();
        if (HasDisposition(text))
        {
            return;
        }

        foreach (System.Text.RegularExpressions.Match match in DiagnosticIdPattern.Matcher.Matches(text))
        {
            Report(context, DiagnosticSuppressionRule, trivia.SpanStart + match.Index, match.Length, match.Value);
        }
    }

    private static void AnalyzeComment(SyntaxTreeAnalysisContext context, SyntaxTrivia trivia)
    {
        string text = trivia.ToFullString();
        if (HasDisposition(text))
        {
            return;
        }

        foreach (System.Text.RegularExpressions.Match match in DiagnosticIdPattern.Matcher.Matches(text))
        {
            Report(context, DiagnosticReferenceRule, trivia.SpanStart + match.Index, match.Length, match.Value);
        }
    }

    private static void Report(SyntaxTreeAnalysisContext context, DiagnosticDescriptor rule, int start, int length, string diagnosticId)
    {
        Location location = Location.Create(context.Tree, new TextSpan(start, length));
        Diagnostic diagnostic = Diagnostic.Create(rule, location, ImmutableDictionary<string, string?>.Empty.Add("DiagnosticId", diagnosticId), diagnosticId);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsComment(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
            || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
            || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
            || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
    }

    private static bool HasDisposition(string text)
    {
        return text.IndexOf("Justification:", System.StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("Tracked-by:", System.StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("Fix-by:", System.StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("Intentional:", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
