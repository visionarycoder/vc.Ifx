using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ifx.Tests.Roslyn.Analyzers;

[TestClass]
public sealed class IfxFrameworkNamespaceTests
{
    [TestMethod]
    [DataRow("namespace vc.App.VisionaryCoder.Ifx { class C { } }", "vc.App.VisionaryCoder.Ifx.C", true)]
    [DataRow("namespace vc.App.VisionaryCoder.Ifx.Security { class C { } }", "vc.App.VisionaryCoder.Ifx.Security.C", true)]
    [DataRow("namespace vc.App.VisionaryCoder.Ifxness { class C { } }", "vc.App.VisionaryCoder.Ifxness.C", false)]
    [DataRow("namespace Other.Root { class C { } }", "Other.Root.C", false)]
    [DataRow("class C { }", "C", false)]
    public void ContainsMatchesOnlyTheFrameworkRootAndNestedNamespaces(string source, string metadataName, bool expected)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp14));
        var compilation = CSharpCompilation.Create("FrameworkNamespaceTests", [tree], AnalyzerHarness.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var classDeclaration = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
        var symbol = compilation.GetSemanticModel(tree).GetDeclaredSymbol(classDeclaration);

        symbol.Should().NotBeNull();
        symbol!.ToDisplayString().Should().Be(metadataName);

        Ifx.Analyzers.Rules.IfxFrameworkNamespace.Contains(symbol).Should().Be(expected);
    }

    [TestMethod]
    public void ContainsReturnsFalseForNull()
    {
        Ifx.Analyzers.Rules.IfxFrameworkNamespace.Contains(null).Should().BeFalse();
    }
}
