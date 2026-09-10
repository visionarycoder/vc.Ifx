using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using vc.Ifx.CodeFixes.Providers;

namespace VisionaryCoder.Framework.Tests.Roslyn.CodeFixes;

[TestClass]
public sealed class LegacyProviderTests
{
    [TestMethod]
    [DataRow(typeof(Ca1051CodeFixProvider), "CA1051")]
    [DataRow(typeof(Ca1303CodeFixProvider), "CA1303")]
    [DataRow(typeof(Ca1413CodeFixProvider), "CA1413")]
    [DataRow(typeof(Ca1704CodeFixProvider), "CA1704")]
    [DataRow(typeof(Ca1707CodeFixProvider), "CA1707")]
    [DataRow(typeof(Ca1801CodeFixProvider), "CA1801")]
    [DataRow(typeof(Ca1806CodeFixProvider), "CA1806")]
    [DataRow(typeof(Ca1812CodeFixProvider), "CA1812")]
    [DataRow(typeof(Ca1819CodeFixProvider), "CA1819")]
    [DataRow(typeof(Ca1822CodeFixProvider), "CA1822")]
    [DataRow(typeof(Ca1823CodeFixProvider), "CA1823")]
    [DataRow(typeof(Ca1824CodeFixProvider), "CA1824")]
    [DataRow(typeof(Ca1825CodeFixProvider), "CA1825")]
    [DataRow(typeof(Ca2207CodeFixProvider), "CA2207")]
    [DataRow(typeof(MSTest0017CodeFixProvider), "MSTEST0017")]
    public async Task WithdrawnProvidersDoNotRewriteOrSuppressSource(Type type, string id)
    {
        var provider = (CodeFixProvider)Activator.CreateInstance(type)!;
        provider.FixableDiagnosticIds.Should().BeEmpty();
        provider.GetFixAllProvider().Should().BeNull();
        type.GetCustomAttributes(typeof(ExportCodeFixProviderAttribute), false).Should().BeEmpty();
        using var harness = new CodeFixHarness();
        const string source = "public enum Kind { First=1, Second }";
        var document = harness.Document(source);
        var diagnostic = await CodeFixHarness.DiagnosticAsync(document, id, "Kind");
        (await CodeFixHarness.ActionsAsync(provider, document, diagnostic)).Should().BeEmpty();
        (await document.GetTextAsync()).ToString().Should().Be(source);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Func<Task> register = async () => await CodeFixHarness.ActionsAsync(provider, document, diagnostic, cancellation.Token);
        await register.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    [DataRow(typeof(Ca1823CodeFixProvider), "CA1823", "class C { private int value = System.Environment.TickCount; }")]
    [DataRow(typeof(Ca1822CodeFixProvider), "CA1822", "class C { public void M() { } public void Call() { new C().M(); } }")]
    [DataRow(typeof(Ca1825CodeFixProvider), "CA1825", "class C { int[,] values = new int[0, 1]; }")]
    [DataRow(typeof(MSTest0017CodeFixProvider), "MSTEST0017", "class C { static int Next() => 1; static void AreEqual(int expected, int actual) { } void M() { AreEqual(Next(), Next()); } }")]
    [DataRow(typeof(Ca2207CodeFixProvider), "CA2207", "struct C { static int value = System.Environment.TickCount; }")]
    public async Task RetiredTransformationsLeaveBehaviorSensitiveFixturesUntouched(Type type, string id, string source)
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document(source);
        await CodeFixHarness.AssertCompilesAsync(document);
        var provider = (CodeFixProvider)Activator.CreateInstance(type)!;
        (await CodeFixHarness.ActionsAsync(provider, document, await CodeFixHarness.DiagnosticAsync(document, id, "C"))).Should().BeEmpty();
        (await document.GetTextAsync()).ToString().Should().Be(source);
    }

    [TestMethod]
    public void CompatibilityBaseRemainsConstructibleWithoutSuppressionActions()
    {
        var provider = new CompatibilityProvider();
        provider.FixableDiagnosticIds.Should().BeEmpty();
        provider.GetFixAllProvider().Should().BeNull();
    }

    private sealed class CompatibilityProvider : vc.Ifx.CodeFixes.Common.PragmaSuppressCodeFixProviderBase
    {
        protected override string DiagnosticId => "CA1825";
        protected override string Title => "Legacy compatibility";
    }
}
