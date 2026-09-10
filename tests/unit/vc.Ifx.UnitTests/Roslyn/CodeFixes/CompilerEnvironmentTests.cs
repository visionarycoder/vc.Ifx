using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using vc.Ifx.CodeFixes.Providers;
using vc.Ifx.CodeFixes.Providers.Diagnostics;

namespace VisionaryCoder.Framework.Tests.Roslyn.CodeFixes;

[TestClass]
public sealed class CompilerEnvironmentTests
{
    [TestMethod]
    [DataRow("missing")]
    [DataRow("source")]
    [DataRow("legacy")]
    public async Task NullGuardRequiresFrameworkGuardApi(string environment)
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("public class P { } public class C { public void M(P value) { } }");
        if (environment == "missing")
            document = document.Project.WithMetadataReferences([]).GetDocument(document.Id)!;
        else if (environment == "source")
            document = document.WithText(Microsoft.CodeAnalysis.Text.SourceText.From("namespace System { public class ArgumentNullException { public static void ThrowIfNull(object value) { } } } public class C { public int M(string value) { return value.Length; } }"));
        else
        {
            var packages = Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
            var reference = Path.Combine(packages, "netstandard.library", "2.0.3", "build", "netstandard2.0", "ref", "netstandard.dll");
            File.Exists(reference).Should().BeTrue("the compiler package restores the netstandard2.0 reference assembly");
            document = document.Project.WithMetadataReferences([MetadataReference.CreateFromFile(reference)]).GetDocument(document.Id)!;
        }
        var diagnostic = await CodeFixHarness.DiagnosticAsync(document, "CA1062", "M(");
        (await CodeFixHarness.ActionsAsync(new Ca1062CodeFixProvider(), document, diagnostic)).Should().BeEmpty();
    }

    [TestMethod]
    public async Task UnresolvedBodyInvocationIsNotMistakenForAnExistingGuard()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("public class C { public int M(string value) { Missing(); return value.Length; } }");
        var action = (await CodeFixHarness.ActionsAsync(new Ca1062CodeFixProvider(), document,
            await CodeFixHarness.DiagnosticAsync(document, "CA1062", "M("))).Single();
        (await (await CodeFixHarness.ApplyAsync(action, document)).GetTextAsync()).ToString().Should().Contain("Missing();");
    }

    [TestMethod]
    public async Task DebtDeclinesNonCompilationRoot()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("class C { }").WithSyntaxRoot(SyntaxFactory.IdentifierName("CA1825"));
        var diagnostic = await CodeFixHarness.DiagnosticAsync(document, "IFX1000", "CA1825", "CA1825");
        (await CodeFixHarness.ActionsAsync(new DiagnosticDebtCodeFixProvider(), document, diagnostic)).Should().BeEmpty();
    }

    [TestMethod]
    public async Task ActualSdkDiagnosticIsRemovedByGuard()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "global.json")))
            directory = directory.Parent;
        directory.Should().NotBeNull();
        using var globalJson = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(directory!.FullName, "global.json")));
        var version = globalJson.RootElement.GetProperty("sdk").GetProperty("version").GetString()!;
        var runtimeRoot = Directory.GetParent(typeof(object).Assembly.Location)!.Parent!.Parent!.Parent!.FullName;
        var analyzerDirectory = Path.Combine(runtimeRoot, "sdk", version, "Sdks", "Microsoft.NET.Sdk", "analyzers");
        var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(analyzerDirectory, "Microsoft.CodeAnalysis.NetAnalyzers.dll"));
        var type = assembly.GetTypes().Single(type => !type.IsAbstract && typeof(DiagnosticAnalyzer).IsAssignableFrom(type) && type.Name == "ValidateArgumentsOfPublicMethods");
        var analyzer = (DiagnosticAnalyzer)Activator.CreateInstance(type)!;
        using var harness = new CodeFixHarness();
        var document = harness.Document("public class C { public int M(string value) { return value.Length; } }");
        document = document.Project.WithCompilationOptions(document.Project.CompilationOptions!.WithSpecificDiagnosticOptions(
            ImmutableDictionary<string, ReportDiagnostic>.Empty.Add("CA1062", ReportDiagnostic.Warn))).GetDocument(document.Id)!;
        var compilation = (await document.Project.GetCompilationAsync())!;
        var diagnostic = (await compilation.WithAnalyzers([analyzer]).GetAnalyzerDiagnosticsAsync()).Single(item => item.Id == "CA1062");
        var action = (await CodeFixHarness.ActionsAsync(new Ca1062CodeFixProvider(), document, diagnostic)).Single();
        var changed = await CodeFixHarness.ApplyAsync(action, document);
        await CodeFixHarness.AssertCompilesAsync(changed);
        (await (await changed.Project.GetCompilationAsync())!.WithAnalyzers([analyzer]).GetAnalyzerDiagnosticsAsync()).Should().NotContain(item => item.Id == "CA1062");
    }
}
