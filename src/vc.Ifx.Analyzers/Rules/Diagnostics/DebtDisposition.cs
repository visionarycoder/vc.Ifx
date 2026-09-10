using System;
using System.Text.RegularExpressions;

namespace vc.Ifx.Analyzers.Rules.Diagnostics;

internal static class DebtDisposition
{
    private static readonly Regex Markers = new(@"\b(?<name>Justification|Tracked-by|Fix-by|Intentional):", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    internal static bool HasMarker(string text, bool justificationOnly)
    {
        MatchCollection markers = Markers.Matches(text);
        for (int index = 0; index < markers.Count; index++)
        {
            Match marker = markers[index];
            if (justificationOnly && !string.Equals(marker.Groups["name"].Value, "Justification", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            int start = marker.Index + marker.Length;
            int end = index + 1 < markers.Count ? markers[index + 1].Index : text.Length;
            // Comment terminators and XML markup cannot masquerade as justification text.
            string value = text.Substring(start, end - start).Split('\r', '\n', '<')[0].Trim(' ', '\t', '*', '/');
            if (IsMeaningful(value))
            {
                return true;
            }
        }

        return false;
    }

    internal static bool IsMeaningful(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string trimmed = value!.Trim();
        return !trimmed.StartsWith("TODO", StringComparison.OrdinalIgnoreCase)
            && !trimmed.StartsWith("TBD", StringComparison.OrdinalIgnoreCase)
            && !trimmed.StartsWith("FIXME", StringComparison.OrdinalIgnoreCase);
    }
}
