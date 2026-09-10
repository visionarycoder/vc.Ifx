using System.Composition;

using Microsoft.CodeAnalysis;
using vc.Ifx.CodeFixes.Common;
using Microsoft.CodeAnalysis.CodeFixes;

namespace vc.Ifx.CodeFixes.Providers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Ca1707CodeFixProvider))]
    [Shared]
    public sealed class Ca1707CodeFixProvider : PragmaSuppressCodeFixProviderBase
    {
        protected override string DiagnosticId => "CA1707";

        protected override string Title => "Suppress CA1707 for this file";
    }
}
