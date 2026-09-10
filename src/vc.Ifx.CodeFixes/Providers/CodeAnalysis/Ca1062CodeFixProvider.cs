using System.Composition;

using Microsoft.CodeAnalysis;
using vc.Ifx.CodeFixes.Common;
using Microsoft.CodeAnalysis.CodeFixes;

namespace vc.Ifx.CodeFixes.Providers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Ca1062CodeFixProvider))]
    [Shared]
    public sealed class Ca1062CodeFixProvider : PragmaSuppressCodeFixProviderBase
    {
        protected override string DiagnosticId => "CA1062";

        protected override string Title => "Suppress CA1062 for this file";
    }
}