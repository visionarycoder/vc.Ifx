using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Ifx.Tests.Roslyn.Analyzers;

[TestClass]
public sealed class InternalBranchCoverageTests
{
    [TestMethod]
    public void CacheTypeHelperMatchesKnownCacheTypesAndRejectsUnrelatedInterfaces()
    {
        const string source = """
            namespace Microsoft.Extensions.Caching.Memory { public interface IMemoryCache { } }
            namespace Microsoft.Extensions.Caching.Distributed { public interface IDistributedCache { } }
            namespace Microsoft.Extensions.Caching.Hybrid { public class HybridCache { } }
            interface IUnrelated { }
            sealed class MemoryWrapper : Microsoft.Extensions.Caching.Memory.IMemoryCache { }
            sealed class DistributedWrapper : Microsoft.Extensions.Caching.Distributed.IDistributedCache { }
            sealed class HybridWrapper : Microsoft.Extensions.Caching.Hybrid.HybridCache { }
            sealed class UnrelatedWrapper : IUnrelated { }
            """;
        var compilation = CreateCompilation(source);
        var invoke = GetStaticMethod(typeof(Ifx.Analyzers.Rules.AvoidManualCachingAnalyzer), "IsCacheType");
        var hybrid = compilation.GetTypeByMetadataName("Microsoft.Extensions.Caching.Hybrid.HybridCache")!;
        var memory = compilation.GetTypeByMetadataName("Microsoft.Extensions.Caching.Memory.IMemoryCache")!;
        var distributed = compilation.GetTypeByMetadataName("Microsoft.Extensions.Caching.Distributed.IDistributedCache")!;

        Invoke<bool>(invoke, null, hybrid, memory, distributed).Should().BeFalse();
        Invoke<bool>(invoke, compilation.GetTypeByMetadataName("MemoryWrapper")!, hybrid, memory, distributed).Should().BeTrue();
        Invoke<bool>(invoke, compilation.GetTypeByMetadataName("DistributedWrapper")!, hybrid, memory, distributed).Should().BeTrue();
        Invoke<bool>(invoke, compilation.GetTypeByMetadataName("HybridWrapper")!, hybrid, memory, distributed).Should().BeFalse();
        Invoke<bool>(invoke, compilation.GetTypeByMetadataName("UnrelatedWrapper")!, hybrid, memory, distributed).Should().BeFalse();
    }

    [TestMethod]
    public void RetryHelperRecognizesTaskDelaySymbolsOnly()
    {
        const string source = """
            using System.Threading.Tasks;
            class C
            {
                async Task ReportAsync()
                {
                    while (true)
                    {
                        try { await Task.Yield(); }
                        catch { await Task.Delay(1); }
                    }
                }

                void Ignore()
                {
                    while (true)
                    {
                        try { Delay(1); }
                        catch { }
                    }
                }

                void IgnoreStatic()
                {
                    while (true)
                    {
                        try { TaskHelpers.Delay(1); }
                        catch { }
                    }
                }

                void IgnoreMissingMember()
                {
                    while (true)
                    {
                        try { unknown.Delay(1); }
                        catch { }
                    }
                }

                void IgnoreMissingOverload()
                {
                    while (true)
                    {
                        try { Delay(1, 2); }
                        catch { }
                    }
                }

                void Delay(int milliseconds) { }
            }

            static class TaskHelpers
            {
                public static void Delay(int milliseconds) { }
            }
            """;
        var compilation = CreateCompilation(source);
        var model = compilation.GetSemanticModel(compilation.SyntaxTrees.Single());
        var loops = compilation.SyntaxTrees.Single().GetRoot().DescendantNodes().OfType<WhileStatementSyntax>().ToArray();
        var invoke = GetStaticMethod(typeof(Ifx.Analyzers.Rules.AvoidManualRetryLoopAnalyzer), "ContainsTaskDelay");

        Invoke<bool>(invoke, loops[0], model, CancellationToken.None).Should().BeTrue();
        Invoke<bool>(invoke, loops[1], model, CancellationToken.None).Should().BeFalse();
        Invoke<bool>(invoke, loops[2], model, CancellationToken.None).Should().BeFalse();
        Invoke<bool>(invoke, loops[3], model, CancellationToken.None).Should().BeFalse();
        Invoke<bool>(invoke, loops[4], model, CancellationToken.None).Should().BeFalse();
    }

    [TestMethod]
    public void EventHandlerHelperReturnsFalseWhenEventArgsMetadataIsUnavailable()
    {
        const string source = "using System.Threading.Tasks; class C { async void Run(object sender, EventArgs args) { await Task.Yield(); } }";
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp14));
        var compilation = CSharpCompilation.Create("MissingEventArgs", [tree],
            references: [],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var model = compilation.GetSemanticModel(tree);
        var method = (MethodDeclarationSyntax)tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
        var symbol = model.GetDeclaredSymbol(method)!;
        var invoke = GetStaticMethod(typeof(Ifx.Analyzers.Rules.Ifx1200NoAsyncVoidMethod), "IsRecognizedEventHandler");

        Invoke<bool>(invoke, symbol, compilation, CancellationToken.None).Should().BeFalse();
    }

    [TestMethod]
    public void EventHandlerHelperRejectsRefParametersAndAcceptsOrdinaryEventHandlerShape()
    {
        const string source = """
            using System;
            class C
            {
                void RefSender(ref object sender, EventArgs args) { }
                void RefArgs(object sender, ref EventArgs args) { }
                void Ordinary(object sender, EventArgs args) { }
            }
            """;
        var compilation = CreateCompilation(source);
        var tree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(tree);
        var methods = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().ToArray();
        var invoke = GetStaticMethod(typeof(Ifx.Analyzers.Rules.Ifx1200NoAsyncVoidMethod), "IsRecognizedEventHandler");

        Invoke<bool>(invoke, model.GetDeclaredSymbol(methods[0])!, compilation, CancellationToken.None).Should().BeFalse();
        Invoke<bool>(invoke, model.GetDeclaredSymbol(methods[1])!, compilation, CancellationToken.None).Should().BeFalse();
        Invoke<bool>(invoke, model.GetDeclaredSymbol(methods[2])!, compilation, CancellationToken.None).Should().BeTrue();
    }

    [TestMethod]
    public void TaskLikeHelpersDistinguishExactAndConstructedTaskTypes()
    {
        const string source = "class C<T> { }";
        var compilation = CreateCompilation(source);
        var taskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task")!;
        var genericTaskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1")!;
        var valueTaskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask")!;
        var genericValueTaskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1")!;
        var stringType = compilation.GetSpecialType(SpecialType.System_String);
        var exact = GetStaticMethod(typeof(Ifx.Analyzers.Rules.Ifx1300NoSyncOverAsyncBlocking), "IsExactType");
        var constructed = GetStaticMethod(typeof(Ifx.Analyzers.Rules.Ifx1300NoSyncOverAsyncBlocking), "IsConstructedFrom");
        var taskLike = GetStaticMethod(typeof(Ifx.Analyzers.Rules.Ifx1300NoSyncOverAsyncBlocking), "IsTaskLikeType");

        Invoke<bool>(exact, taskType, taskType).Should().BeTrue();
        Invoke<bool>(exact, stringType, taskType).Should().BeFalse();
        Invoke<bool>(exact, null, taskType).Should().BeFalse();
        var constructedTask = genericTaskType.Construct(stringType);
        var constructedValueTask = genericValueTaskType.Construct(stringType);

        Invoke<bool>(constructed, constructedTask, genericTaskType).Should().BeTrue();
        Invoke<bool>(constructed, taskType, genericTaskType).Should().BeFalse();
        Invoke<bool>(constructed, null, genericTaskType).Should().BeFalse();
        Invoke<bool>(taskLike, valueTaskType, taskType, genericTaskType, valueTaskType, genericValueTaskType).Should().BeTrue();
        Invoke<bool>(taskLike, constructedValueTask, taskType, genericTaskType, valueTaskType, genericValueTaskType).Should().BeTrue();
        Invoke<bool>(taskLike, stringType, taskType, genericTaskType, valueTaskType, genericValueTaskType).Should().BeFalse();
        Invoke<bool>(taskLike, null, null, null, null, null).Should().BeFalse();
    }

    [TestMethod]
    public void UnderscoreAnalyzerLocalVariableHelperMatchesSupportedParentsOnly()
    {
        const string source = """
            unsafe class C
            {
                void Run()
                {
                    int local = 0;
                    for (int counter = 0; counter < 1; counter++) { }
                    using var resource = new Disposable();
                    int[] buffer = new int[1];
                    fixed (int* pointer = buffer) { }
                }
            }

            sealed class Disposable : System.IDisposable
            {
                public void Dispose() { }
            }
            """;
        var root = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp14)).GetRoot();
        var variables = root.DescendantNodes().OfType<VariableDeclaratorSyntax>().ToArray();
        var detached = SyntaxFactory.VariableDeclarator("orphan");
        var invoke = GetStaticMethod(typeof(Ifx.Analyzers.Rules.Ifx1400NoUnderscorePrefixedIdentifier), "IsLocalVariable");

        Invoke<bool>(invoke, variables.Single(variable => variable.Identifier.ValueText == "local")).Should().BeTrue();
        Invoke<bool>(invoke, variables.Single(variable => variable.Identifier.ValueText == "counter")).Should().BeTrue();
        Invoke<bool>(invoke, variables.Single(variable => variable.Identifier.ValueText == "resource")).Should().BeTrue();
        Invoke<bool>(invoke, variables.Single(variable => variable.Identifier.ValueText == "pointer")).Should().BeTrue();
        Invoke<bool>(invoke, detached).Should().BeFalse();
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp14));
        return CSharpCompilation.Create("InternalBranchCoverage", [tree], AnalyzerHarness.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
    }

    private static MethodInfo GetStaticMethod(Type type, string name) =>
        type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;

    private static T Invoke<T>(MethodInfo method, params object?[] args) =>
        (T)method.Invoke(null, args)!;
}
