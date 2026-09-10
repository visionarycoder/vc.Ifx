using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace vc.Ifx.CodeFixes.Common
{
    internal static class CaCodeFixUtilities
    {
        public static async Task<Document> AddFilePragmaSuppressionAsync(Document document, string diagnosticId, CancellationToken cancellationToken)
        {
            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var content = text.ToString();
            var disable = "#pragma warning disable " + diagnosticId;
            var restore = "#pragma warning restore " + diagnosticId;

            if (content.IndexOf(disable, StringComparison.Ordinal) >= 0)
            {
                return document;
            }

            var lineEnding = text.Lines.Any() ? text.Lines[0].ToString().EndsWith("\r") ? "\r\n" : Environment.NewLine : Environment.NewLine;
            var updated = disable + lineEnding + content + lineEnding + restore + lineEnding;
            return document.WithText(SourceText.From(updated, text.Encoding));
        }
    }
}