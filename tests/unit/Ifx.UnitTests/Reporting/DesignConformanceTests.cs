using Ifx.Roslyn.Reporting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ifx.Generators;
using Ifx.Tests.Generators.Implementation;

namespace Ifx.UnitTests.Reporting;

[TestClass]
public sealed class DesignConformanceTests
{
    private static readonly MetadataReference[] RuntimeReferences =
        ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
        .Select(p => MetadataReference.CreateFromFile(p)).ToArray();

    private static string[] Inspect(string source, string[] allowed)
    {
        CSharpCompilation target = CSharpCompilation.Create("Engine",
            [CSharpSyntaxTree.ParseText("namespace Pricing { public interface IPrice { int Get(); } public sealed class Price : IPrice { public int Get() => 1; public static int Value => 1; } }")],
            RuntimeReferences, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using MemoryStream image = new();
        Assert.IsTrue(target.Emit(image).Success);
        CSharpCompilation caller = CSharpCompilation.Create("Manager",
            [CSharpSyntaxTree.ParseText(source)],
            [.. RuntimeReferences, MetadataReference.CreateFromImage(image.ToArray())],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        return DesignConformance.CheckCompilation(caller, "orders",
            new Dictionary<string, string> { ["Manager"] = "orders", ["Engine"] = "pricing" },
            allowed.ToHashSet(StringComparer.Ordinal));
    }

    [TestMethod]
    public void AllowsExplicitContract()
    {
        Assert.AreEqual(0, Inspect("class M { public int Run(Pricing.IPrice price) => price.Get(); }", ["Pricing.IPrice"]).Length);
    }

    [TestMethod]
    public void RejectsUnapprovedContract()
    {
        Assert.IsTrue(Inspect("class M { public int Run(Pricing.IPrice price) => price.Get(); }", []).Any(i => i.Contains("Forbidden component reference", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void RejectsConcreteConstructionAndAlias()
    {
        Assert.IsTrue(Inspect("using P = Pricing.Price; class M { public int Run() => new P().Get(); }", ["Pricing.IPrice"]).Any(i => i.Contains("Pricing.Price", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void RejectsStaticPropertyBypass()
    {
        Assert.IsTrue(Inspect("class M { public int Run() => Pricing.Price.Value; }", ["Pricing.IPrice"]).Any(i => i.Contains("Forbidden component reference", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void RejectsUnresolvedSymbols()
    {
        Assert.IsTrue(Inspect("class M { public int Run() => Unknown.Get(); }", []).Any(i => i.StartsWith("Unresolved compilation", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void RejectsDynamicDispatch()
    {
        Assert.IsTrue(Inspect("class M { public int Run(dynamic price) => price.Get(); }", []).Any(i => i.StartsWith("Dynamic dispatch", StringComparison.Ordinal)));
        Assert.IsTrue(Inspect("class M { private dynamic Price() => null; public int Run() => Price().Get(); }", []).Any(i => i.StartsWith("Dynamic dispatch", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void ChecksActualGeneratorOutputAgainstApprovedContract()
    {
        CSharpCompilation target = CSharpCompilation.Create("Engine",
            [CSharpSyntaxTree.ParseText("namespace Pricing { public interface IPrice { int Get(); } }")],
            RuntimeReferences, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using MemoryStream image = new();
        Assert.IsTrue(target.Emit(image).Success);
        CSharpCompilation input = GeneratorHarness.Compilation("""
            using Ifx.Generators.Attributes;
            [GenerateInterceptors("Fixture", typeof(Pricing.IPrice))]
            public class Marker { }
            """, name: "Manager").AddReferences(MetadataReference.CreateFromImage(image.ToArray()));
        GeneratorDriver driver = CSharpGeneratorDriver.Create([new InterceptorsGenerator().AsSourceGenerator()],
            parseOptions: GeneratorHarness.ParseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(input, out Compilation output, out var diagnostics);
        Assert.IsFalse(diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
        Assert.IsTrue(driver.GetRunResult().GeneratedTrees.Length > 0);
        Dictionary<string, string> components = new() { ["Manager"] = "orders", ["Engine"] = "pricing" };
        Assert.AreEqual(0, DesignConformance.CheckCompilation((CSharpCompilation)output, "orders", components,
            new HashSet<string>(["Pricing.IPrice"], StringComparer.Ordinal)).Length);
        Assert.IsTrue(DesignConformance.CheckCompilation((CSharpCompilation)output, "orders", components,
            new HashSet<string>(StringComparer.Ordinal)).Any(i => i.Contains("Forbidden component reference", StringComparison.Ordinal)));
    }
}
