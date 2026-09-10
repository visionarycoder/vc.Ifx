using System.Text.RegularExpressions;
using vc.Ifx.Roslyn;

namespace VisionaryCoder.Framework.Tests.Roslyn.SharedContracts;

[TestClass]
public sealed class DiagnosticIdPatternTests
{
    [TestMethod]
    [DataRow("CA1822", true)]
    [DataRow("CS0168", true)]
    [DataRow("IFX1000", false)]
    [DataRow("CA\u0661\u0662\u0663\u0664", true)]
    [DataRow("", false)]
    [DataRow("ca1822", false)]
    [DataRow("CS168", false)]
    [DataRow("CA18222", false)]
    [DataRow("prefixCA1822", false)]
    [DataRow("CA1822_suffix", false)]
    public void LegacyMatcherPreservesScope(string text, bool expected)
    {
        Assert.AreEqual(expected, DiagnosticIdPattern.Matcher.IsMatch(text));
        Assert.AreEqual(expected, Regex.IsMatch(text, DiagnosticIdPattern.Pattern));
    }

    [TestMethod]
    [DataRow("CA1822", true)]
    [DataRow("CS0168", true)]
    [DataRow("IFX1000", true)]
    [DataRow("IFX1001", true)]
    [DataRow("IFX9999", true)]
    [DataRow("IFX0000", true)]
    [DataRow("IFX001", true)]
    [DataRow("IFX006", true)]
    [DataRow("IFX000", false)]
    [DataRow("IFX007", false)]
    [DataRow("IFX10000", false)]
    [DataRow("IFX100", false)]
    [DataRow("CA\u0661\u0662\u0663\u0664", false)]
    [DataRow("IFX\uff11\uff10\uff10\uff10", false)]
    [DataRow("ifx1000", false)]
    [DataRow("CS10000", false)]
    [DataRow("CS168", false)]
    [DataRow("prefixIFX1000", false)]
    [DataRow("IFX1000suffix", false)]
    [DataRow("_IFX1000", false)]
    [DataRow("IFX1000_", false)]
    [DataRow("VBD100 SEC001 CQ100 GEN001 IDE0051 MSTest0017", false)]
    [DataRow("", false)]
    public void ReferenceMatcherHasExplicitScopeAndBoundaries(string text, bool expected)
    {
        Assert.AreEqual(expected, DiagnosticIdPattern.ReferenceMatcher.IsMatch(text));
        Assert.AreEqual(expected, Regex.IsMatch(text, DiagnosticIdPattern.ReferencePattern));
    }

    [TestMethod]
    public void ReferenceMatcherPreservesTextOrderRepeatedIdsAndLocations()
    {
        const string text = "// (CA1822), CS0168; IFX1000 / IFX005 / CA1822";
        Match[] matches = DiagnosticIdPattern.ReferenceMatcher.Matches(text).Cast<Match>().ToArray();

        CollectionAssert.AreEqual(new[] { "CA1822", "CS0168", "IFX1000", "IFX005", "CA1822" }, matches.Select(match => match.Value).ToArray());
        foreach (Match match in matches)
        {
            Assert.AreEqual(match.Value, text.Substring(match.Index, match.Length));
        }
        Assert.AreEqual(4, matches[0].Index);
        Assert.IsTrue(matches[4].Index > matches[0].Index);
    }

    [TestMethod]
    public void MatchersUseStableOptionsAndRejectNullInput()
    {
        foreach (Regex matcher in new[] { DiagnosticIdPattern.Matcher, DiagnosticIdPattern.ReferenceMatcher })
        {
            Assert.AreEqual(RegexOptions.Compiled | RegexOptions.CultureInvariant, matcher.Options);
            Assert.ThrowsExactly<ArgumentNullException>(() => matcher.IsMatch(null!));
        }
    }
}
