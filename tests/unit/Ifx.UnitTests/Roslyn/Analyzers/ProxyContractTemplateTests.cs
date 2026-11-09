using System.Runtime.CompilerServices;
using Ifx.Analyzers.Rules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Ifx.Tests.Roslyn.Analyzers;

[TestClass]
public sealed class ProxyContractTemplateTests
{
    [TestMethod]
    [DataRow("Task<Response>", 0)]
    [DataRow("ValueTask<Response>", 0)]
    [DataRow("void", 1)]
    public async Task Signature_ShippedAttribute_AnalyzesTypedRequest(string returnType, int expected)
    {
        string source = $$"""
            using System.Threading;
            using System.Threading.Tasks;
            public sealed record Request(string Id);
            public sealed record Response(string Value);
            [Ifx.Proxy.ProxyContract]
            public interface IComponent { {{returnType}} ExecuteAsync(Request request, CancellationToken cancellationToken); }
            """;
        var diagnostics = await AnalyzerHarness.RunAsync(source, new Ifx001Signature(),
            additionalReferences: [MetadataReference.CreateFromFile(typeof(Ifx.Proxy.ProxyContractAttribute).Assembly.Location)]);
        diagnostics.Length.Should().Be(expected);
    }

    [TestMethod]
    public void Templates_StartingPoint_CompilesAgainstShippedRuntime()
    {
        DirectoryInfo directory = new(Path.GetDirectoryName(SourcePath())!);
        while (!File.Exists(Path.Combine(directory.FullName, "vc.Ifx.slnx"))) directory = directory.Parent!;
        string folder = Path.Combine(directory.FullName, "templates/component");
        SyntaxTree[] trees = Directory.GetFiles(folder, "*.cs.txt")
            .Select(path => CSharpSyntaxTree.ParseText("global using System; global using System.Threading; global using System.Threading.Tasks;\n" + File.ReadAllText(path))).ToArray();
        var compilation = CSharpCompilation.Create("Template", trees,
            AnalyzerHarness.References.Add(MetadataReference.CreateFromFile(typeof(Ifx.Proxy.ProxyContractAttribute).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
    }

    [TestMethod]
    public void InterceptorPreferences_Defaults_AreOptIn()
    {
        new AvoidManualCachingAnalyzer().SupportedDiagnostics.Single().IsEnabledByDefault.Should().BeFalse();
        new AvoidManualRetryLoopAnalyzer().SupportedDiagnostics.Single().IsEnabledByDefault.Should().BeFalse();
        new AvoidManualJwtValidationAnalyzer().SupportedDiagnostics.Single().IsEnabledByDefault.Should().BeFalse();
    }

    private static string SourcePath([CallerFilePath] string path = "") => path;
}
