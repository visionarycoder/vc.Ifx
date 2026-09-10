using System.Composition;

using Microsoft.CodeAnalysis;
using vc.Ifx.CodeFixes.Common;
using Microsoft.CodeAnalysis.CodeFixes;

namespace vc.Ifx.CodeFixes.Providers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Ca1824CodeFixProvider))]
    [Shared]
    public sealed class Ca1824CodeFixProvider : PragmaSuppressCodeFixProviderBase
    {
        protected override string DiagnosticId => "CA1824";

        protected override string Title => "Suppress CA1824 for this file";
    }
}