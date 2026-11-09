using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Ifx.Analyzers;

/// <summary>
/// Contract for a single architectural or quality rule that is registered into a grouped
/// <see cref="Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer"/> at the project root
/// alongside the framework analyzers declared in the <c>Ifx.Analyzers.Rules</c> namespace.
/// </summary>
/// <remarks>
/// Instance members are used instead of static abstract interface members because
/// <c>Ifx.Analyzers</c> targets <c>netstandard2.0</c>, whose runtime does not support static
/// abstract interface members regardless of the project's configured C# language version.
/// Implementing rules must not call <see cref="AnalysisContext.EnableConcurrentExecution"/> or
/// <see cref="AnalysisContext.ConfigureGeneratedCodeAnalysis"/> from <see cref="Initialize"/> -
/// the owning grouped analyzer configures the context once before delegating to every rule.
/// </remarks>
public interface IAnalyzerRule
{
    /// <summary>Gets the diagnostic descriptor reported by this rule.</summary>
    DiagnosticDescriptor Rule { get; }

    /// <summary>Registers this rule's analysis actions on the shared <see cref="AnalysisContext"/>.</summary>
    void Initialize(AnalysisContext context);
}
