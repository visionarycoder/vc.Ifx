namespace vc.Ifx.CodeFixes.Common;

/// <summary>Compatibility base; blanket suppression is no longer offered.</summary>
public abstract class PragmaSuppressCodeFixProviderBase : RetiredCodeFixProvider
{
    /// <summary>Retains the legacy derived-provider declaration contract.</summary>
    protected abstract string DiagnosticId { get; }
    /// <summary>Retains the legacy derived-provider title contract.</summary>
    protected abstract string Title { get; }
}
