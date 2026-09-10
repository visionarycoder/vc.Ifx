using System.Composition;

using Microsoft.CodeAnalysis;
using vc.Ifx.CodeFixes.Common;
using Microsoft.CodeAnalysis.CodeFixes;

namespace vc.Ifx.CodeFixes.Providers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Ca1819CodeFixProvider))]
    [Shared]
    public sealed class Ca1819CodeFixProvider : PragmaSuppressCodeFixProviderBase
    {
        protected override string DiagnosticId => "CA1819";

        protected override string Title => "Suppress CA1819 for this file";
    }
}
