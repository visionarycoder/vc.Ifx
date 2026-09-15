using Ifx.Analyzers.Rules;
using Microsoft.CodeAnalysis;

namespace Ifx.Tests.Roslyn.Analyzers;

[TestClass]
public sealed class Ifx1200NoAsyncVoidMethodTests
{
    [TestMethod]
    public async Task ReportsOrdinaryAsyncVoidMethod()
    {
        const string source = "using System.Threading.Tasks; class C { async void Run() { await Task.Yield(); } }";
        var diagnostics = await AnalyzerHarness.RunAsync(source, new Ifx1200NoAsyncVoidMethod());
        Assert.AreEqual(1, diagnostics.Length);
        Diagnostic diagnostic = diagnostics[0];
        Assert.AreEqual("IFX1200", diagnostic.Id);
        Assert.AreEqual(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.AreEqual("Reliability", diagnostic.Descriptor.Category);
        Assert.AreEqual("Method 'Run' is declared 'async void'; use 'async Task' unless this is a recognized event handler", diagnostic.GetMessage());
        StringAssert.EndsWith(diagnostic.Descriptor.HelpLinkUri, "#ifx1200");
        Assert.AreEqual("Run", source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
    }

    [TestMethod]
    [DataRow("using System.Threading.Tasks; class C { async Task Run() { await Task.Yield(); } }")]
    [DataRow("class C { void Run() { } }")]
    public async Task DoesNotReportNonAsyncVoidMethods(string source)
    {
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, new Ifx1200NoAsyncVoidMethod())).Length);
    }

    [TestMethod]
    [DataRow("""
        using System.Threading.Tasks;
        abstract class Base
        {
            public abstract void Run();
        }
        sealed class Derived : Base
        {
            public override async void Run()
            {
                await Task.Yield();
            }
        }
        """)]
    [DataRow("""
        using System.Threading.Tasks;
        interface IHandler
        {
            void Run();
        }
        sealed class Handler : IHandler
        {
            public async void Run()
            {
                await Task.Yield();
            }
        }
        """)]
    [DataRow("""
        using System.Threading.Tasks;
        interface IHandler
        {
            void Run();
        }
        sealed class Handler : IHandler
        {
            async void IHandler.Run()
            {
                await Task.Yield();
            }
        }
        """)]
    public async Task DoesNotReportOverridesOrInterfaceImplementations(string source)
    {
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, new Ifx1200NoAsyncVoidMethod())).Length);
    }

    [TestMethod]
    [DataRow("""
        using System;
        using System.Threading.Tasks;
        class C
        {
            async void OnClick(object sender, EventArgs e)
            {
                await Task.Yield();
            }
        }
        """)]
    [DataRow("""
        using System;
        using System.Threading.Tasks;
        sealed class MyEventArgs : EventArgs
        {
        }
        class C
        {
            async void OnClick(string sender, MyEventArgs e)
            {
                await Task.Yield();
            }
        }
        """)]
    public async Task DoesNotReportRecognizedEventHandlers(string source)
    {
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, new Ifx1200NoAsyncVoidMethod())).Length);
    }

    [TestMethod]
    public async Task ReportsAsyncVoidMethodWithEventArgsWhenSenderIsValueType()
    {
        const string source = """
            using System;
            using System.Threading.Tasks;
            class C
            {
                async void OnClick(int sender, EventArgs e)
                {
                    await Task.Yield();
                }
            }
            """;
        var diagnostics = await AnalyzerHarness.RunAsync(source, new Ifx1200NoAsyncVoidMethod());
        Assert.AreEqual(1, diagnostics.Length);
        Assert.AreEqual("OnClick", source.Substring(diagnostics[0].Location.SourceSpan.Start, diagnostics[0].Location.SourceSpan.Length));
    }

    [TestMethod]
    public async Task ReportsAsyncVoidMethodWhenSecondParameterIsNotEventArgs()
    {
        const string source = """
            using System.Threading.Tasks;
            class C
            {
                async void Run(object sender, string message)
                {
                    await Task.Yield();
                }
            }
            """;
        var diagnostics = await AnalyzerHarness.RunAsync(source, new Ifx1200NoAsyncVoidMethod());
        Assert.AreEqual(1, diagnostics.Length);
        Assert.AreEqual("Run", source.Substring(diagnostics[0].Location.SourceSpan.Start, diagnostics[0].Location.SourceSpan.Length));
    }

    [TestMethod]
    public async Task ReportsAsyncVoidMethodWhenInterfaceContractIsImplementedByAnotherMember()
    {
        const string source = """
            using System.Threading.Tasks;
            interface IHandler
            {
                void Run();
            }
            sealed class Handler : IHandler
            {
                void IHandler.Run()
                {
                }

                public async void Run()
                {
                    await Task.Yield();
                }
            }
            """;
        var diagnostics = await AnalyzerHarness.RunAsync(source, new Ifx1200NoAsyncVoidMethod());
        Assert.AreEqual(1, diagnostics.Length);
        Assert.AreEqual("Run", source.Substring(diagnostics[0].Location.SourceSpan.Start, diagnostics[0].Location.SourceSpan.Length));
    }

    [TestMethod]
    public async Task ReportsAsyncVoidMethodOnTypeWithUnrelatedInterfaceMember()
    {
        const string source = """
            using System.Threading.Tasks;
            interface IHandler
            {
                void Other();
            }
            sealed class Handler : IHandler
            {
                public void Other()
                {
                }

                public async void Run()
                {
                    await Task.Yield();
                }
            }
            """;
        var diagnostics = await AnalyzerHarness.RunAsync(source, new Ifx1200NoAsyncVoidMethod());
        Assert.AreEqual(1, diagnostics.Length);
        Assert.AreEqual("Run", source.Substring(diagnostics[0].Location.SourceSpan.Start, diagnostics[0].Location.SourceSpan.Length));
    }

    [TestMethod]
    public async Task ReportsAsyncVoidMethodWhenInterfaceHasSameNamedNonMethodMember()
    {
        const string source = """
            using System.Threading.Tasks;
            interface IHandler
            {
                int Run { get; }
            }
            sealed class Handler : IHandler
            {
                int IHandler.Run => 0;

                public async void Run()
                {
                    await Task.Yield();
                }
            }
            """;
        var diagnostics = await AnalyzerHarness.RunAsync(source, new Ifx1200NoAsyncVoidMethod(), allowCompilerErrors: true);
        Assert.AreEqual(1, diagnostics.Length);
        Assert.AreEqual("Run", source.Substring(diagnostics[0].Location.SourceSpan.Start, diagnostics[0].Location.SourceSpan.Length));
    }

    [TestMethod]
    public async Task DoesNotReportLocalFunctionsOrLambdas()
    {
        const string source = """
            using System;
            using System.Threading.Tasks;
            class C
            {
                void Run()
                {
                    async void Local()
                    {
                        await Task.Yield();
                    }

                    Action first = async () => await Task.Yield();
                    Action second = async delegate { await Task.Yield(); };
                    first();
                    second();
                    Local();
                }
            }
            """;
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, new Ifx1200NoAsyncVoidMethod())).Length);
    }

    [TestMethod]
    [DataRow("Policy.g.cs", "")]
    [DataRow("Policy.generated.cs", "")]
    [DataRow("Policy.cs", "// <auto-generated/>\n")]
    public async Task GeneratedMethodsAreExcluded(string path, string header)
    {
        var source = header + "using System.Threading.Tasks; class C { async void Run() { await Task.Yield(); } }";
        Assert.AreEqual(0, (await AnalyzerHarness.RunAsync(source, new Ifx1200NoAsyncVoidMethod(), path: path)).Length);
    }

    [TestMethod]
    public async Task HonorsPreCanceledAnalysis()
    {
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            AnalyzerHarness.RunAsync("using System.Threading.Tasks; class C { async void Run() { await Task.Yield(); } }",
                new Ifx1200NoAsyncVoidMethod(), cancellationToken: cancellation.Token));
    }
}
