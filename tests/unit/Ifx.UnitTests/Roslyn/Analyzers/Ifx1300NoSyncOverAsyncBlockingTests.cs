using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;
using Ifx.Analyzers.Rules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ifx.Tests.Roslyn.Analyzers;

[TestClass]
public sealed class Ifx1300NoSyncOverAsyncBlockingTests
{
    [TestMethod]
    public async Task ReportsTaskResultPropertyAccess()
    {
        const string source = "using System.Threading.Tasks; class C { int Run(Task<int> task) { return task.Result; } }";

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source);

        diagnostics.Should().ContainSingle();
        Diagnostic diagnostic = diagnostics[0];
        diagnostic.Id.Should().Be("IFX1300");
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Warning);
        diagnostic.Descriptor.Category.Should().Be("Reliability");
        diagnostic.Descriptor.HelpLinkUri.Should().EndWith("#ifx1300");
        diagnostic.GetMessage().Should().Be("Blocking call 'Result' on an asynchronous operation can cause deadlocks; use 'await' instead");
        GetSpanText(source, diagnostic).Should().Be("Result");
    }

    [TestMethod]
    [DataRow("task.Wait();", "Wait")]
    [DataRow("Task.WaitAll(task);", "WaitAll")]
    [DataRow("Task.WaitAny(task);", "WaitAny")]
    public async Task ReportsTaskWaitPatterns(string body, string expectedMember)
    {
        string source = "using System.Threading.Tasks; class C { void Run(Task task) { " + body + " } }";

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source);

        diagnostics.Should().ContainSingle();
        diagnostics[0].GetMessage().Should().Be(
            "Blocking call '" + expectedMember + "' on an asynchronous operation can cause deadlocks; use 'await' instead");
        GetSpanText(source, diagnostics[0]).Should().Be(expectedMember);
    }

    [TestMethod]
    [DataRow("using System.Threading.Tasks; class C { int Run(Task<int> task) { return task.GetAwaiter().GetResult(); } }")]
    [DataRow("using System.Threading.Tasks; class C { void Run(Task task) { task.GetAwaiter().GetResult(); } }")]
    [DataRow("using System.Threading.Tasks; class C { int Run(ValueTask<int> task) { return task.GetAwaiter().GetResult(); } }")]
    [DataRow("using System.Threading.Tasks; class C { void Run(ValueTask task) { task.GetAwaiter().GetResult(); } }")]
    public async Task ReportsGetAwaiterGetResultOnTaskLikeTypes(string source)
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source);

        diagnostics.Should().ContainSingle();
        diagnostics[0].GetMessage().Should().Be("Blocking call 'GetResult' on an asynchronous operation can cause deadlocks; use 'await' instead");
        GetSpanText(source, diagnostics[0]).Should().Be("GetResult");
    }

    [TestMethod]
    public async Task DoesNotReportAwaitExpressions()
    {
        const string source = "using System.Threading.Tasks; class C { async Task RunAsync(Task task) { await task; } }";

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source);

        diagnostics.Should().BeEmpty();
    }

    [TestMethod]
    public async Task DoesNotReportOtherTaskProperties()
    {
        const string source = "using System.Threading.Tasks; class C { bool Run(Task task) { return task.IsCompleted; } }";

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source);

        diagnostics.Should().BeEmpty();
    }

    [TestMethod]
    public async Task DoesNotReportUnrelatedResultWaitOrAwaiterMembers()
    {
        const string source = """
            class Custom
            {
                public int Result => 42;
                public void Wait() { }
                public CustomAwaiter GetAwaiter() => new CustomAwaiter();
            }

            struct CustomAwaiter
            {
                public int GetResult() => 42;
            }

            class C
            {
                int Run(Custom custom)
                {
                    custom.Wait();
                    return custom.Result + custom.GetAwaiter().GetResult();
                }
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source);

        diagnostics.Should().BeEmpty();
    }

    [TestMethod]
    public async Task DoesNotReportDirectGetResultCallsWithoutGetAwaiterChain()
    {
        const string source = """
            struct CustomAwaiter
            {
                public int GetResult() => 42;
            }

            class C
            {
                int Run(CustomAwaiter awaiter)
                {
                    return awaiter.GetResult();
                }
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source);

        diagnostics.Should().BeEmpty();
    }

    [TestMethod]
    public async Task DoesNotReportGetResultWhenGetAwaiterIsStatic()
    {
        const string source = """
            struct CustomAwaiter
            {
                public void GetResult() { }
            }

            static class AwaiterFactory
            {
                public static CustomAwaiter GetAwaiter() => default;
            }

            class C
            {
                void Run()
                {
                    AwaiterFactory.GetAwaiter().GetResult();
                }
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source);

        diagnostics.Should().BeEmpty();
    }

    [TestMethod]
    public async Task ReportsConditionalAccessWaitAtFallbackLocation()
    {
        const string source = "using System.Threading.Tasks; class C { void Run(Task? task) { task?.Wait(); } }";

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source);

        diagnostics.Should().ContainSingle();
        diagnostics[0].GetMessage().Should().Be("Blocking call 'Wait' on an asynchronous operation can cause deadlocks; use 'await' instead");
        GetSpanText(source, diagnostics[0]).Should().Be(".Wait()");
    }

    [TestMethod]
    public async Task ReportsConditionalAccessResultAtFallbackLocation()
    {
        const string source = "using System.Threading.Tasks; class C { int Run(Task<int>? task) { return task?.Result ?? 0; } }";

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source);

        diagnostics.Should().ContainSingle();
        diagnostics[0].GetMessage().Should().Be("Blocking call 'Result' on an asynchronous operation can cause deadlocks; use 'await' instead");
        GetSpanText(source, diagnostics[0]).Should().Be(".Result");
    }

    [TestMethod]
    [DataRow("Policy.g.cs", "")]
    [DataRow("Policy.generated.cs", "")]
    [DataRow("Policy.cs", "// <auto-generated/>\n")]
    public async Task DoesNotReportGeneratedCode(string path, string header)
    {
        string source = header + "using System.Threading.Tasks; class C { int Run(Task<int> task) { return task.Result; } }";

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source, path: path);

        diagnostics.Should().BeEmpty();
    }

    [TestMethod]
    public async Task HonorsPreCanceledAnalysis()
    {
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Func<Task> act = async () => await AnalyzeAsync(
            "using System.Threading.Tasks; class C { void Run(Task task) { task.Wait(); } }",
            cancellationToken: cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public async Task ReturnsNoDiagnosticsWhenTaskMetadataIsUnavailable()
    {
        const string source = "class C { int Value => 0; }";
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp14), "Policy.cs");
        CSharpCompilation compilation = CSharpCompilation.Create("MissingTaskMetadata", [tree],
            references: ImmutableArray<MetadataReference>.Empty,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        CompilationWithAnalyzers driver = compilation.WithAnalyzers([new Ifx1300NoSyncOverAsyncBlocking()]);

        ImmutableArray<Diagnostic> diagnostics = await driver.GetAnalyzerDiagnosticsAsync();

        diagnostics.Should().BeEmpty();
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source, string path = "Policy.cs",
        CancellationToken cancellationToken = default)
    {
        return await AnalyzerHarness.RunAsync(source, new Ifx1300NoSyncOverAsyncBlocking(), path: path, cancellationToken: cancellationToken);
    }

    private static string GetSpanText(string source, Diagnostic diagnostic)
    {
        return source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length);
    }
}
