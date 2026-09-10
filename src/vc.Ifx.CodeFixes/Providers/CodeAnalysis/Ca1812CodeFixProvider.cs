using System.Composition;

using Microsoft.CodeAnalysis;
using vc.Ifx.CodeFixes.Common;
using Microsoft.CodeAnalysis.CodeFixes;

namespace vc.Ifx.CodeFixes.Providers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Ca1812CodeFixProvider))]
    [Shared]
    public sealed class Ca1812CodeFixProvider : PragmaSuppressCodeFixProviderBase
    {
        protected override string DiagnosticId => "CA1812";

        protected override string Title => "Suppress CA1812 for this file";
    }
}
