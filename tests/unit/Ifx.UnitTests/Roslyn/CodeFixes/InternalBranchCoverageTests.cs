using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ifx.Tests.Roslyn.CodeFixes;

[TestClass]
public sealed class InternalBranchCoverageTests
{
    [TestMethod]
    public async Task HelperMethodsHandleSupportedAndUnsupportedSymbols()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("""
            using System;
            unsafe class C
            {
                private int _field;
                public int PublicField;
                public int Auto { get; set; }
                private event Action _handler;

                int Value
                {
                    get
                    {
                        int valueInAccessor = 0;
                        return valueInAccessor;
                    }
                }

                void Run(int value)
                {
                    int local = 0;
                    for (int counter = 0; counter < 1; counter++) { }
                    using var resource = new Disposable();
                    using (var scopedResource = new Disposable()) { }
                    int[] buffer = new int[1];
                    fixed (int* pointer = buffer) { }
                }
            }

            sealed class Disposable : IDisposable
            {
                public void Dispose() { }
            }
            """);
        var root = (await document.GetSyntaxRootAsync())!;
        var model = (await document.GetSemanticModelAsync())!;
        var canRename = GetStaticMethod("CanRename");
        var getDeclaredSymbol = GetStaticMethod("GetDeclaredSymbol");
        var hasCollision = GetStaticMethod("HasCollision");
        var getScope = GetStaticMethod("GetContainingExecutableScope");
        var isLocalVariable = GetStaticMethod("IsLocalVariable");

        var privateField = model.GetDeclaredSymbol(root.DescendantNodes().OfType<VariableDeclaratorSyntax>().First(node => node.Identifier.ValueText == "_field"))!;
        var publicField = model.GetDeclaredSymbol(root.DescendantNodes().OfType<VariableDeclaratorSyntax>().First(node => node.Identifier.ValueText == "PublicField"))!;
        var handler = model.GetDeclaredSymbol(root.DescendantNodes().OfType<VariableDeclaratorSyntax>().First(node => node.Identifier.ValueText == "_handler"))!;
        var implicitField = ((INamedTypeSymbol)model.GetDeclaredSymbol(root.DescendantNodes().OfType<ClassDeclarationSyntax>().First())!)
            .GetMembers().OfType<IFieldSymbol>().First(field => field.IsImplicitlyDeclared);
        var parameter = root.DescendantNodes().OfType<ParameterSyntax>().First();
        var local = root.DescendantNodes().OfType<VariableDeclaratorSyntax>().First(node => node.Identifier.ValueText == "local");
        var counter = root.DescendantNodes().OfType<VariableDeclaratorSyntax>().First(node => node.Identifier.ValueText == "counter");
        var resource = root.DescendantNodes().OfType<VariableDeclaratorSyntax>().First(node => node.Identifier.ValueText == "resource");
        var scopedResource = root.DescendantNodes().OfType<VariableDeclaratorSyntax>().First(node => node.Identifier.ValueText == "scopedResource");
        var pointer = root.DescendantNodes().OfType<VariableDeclaratorSyntax>().First(node => node.Identifier.ValueText == "pointer");
        var accessorLocal = root.DescendantNodes().OfType<VariableDeclaratorSyntax>().First(node => node.Identifier.ValueText == "valueInAccessor");

        Invoke<bool>(canRename, privateField).Should().BeTrue();
        Invoke<bool>(canRename, publicField).Should().BeFalse();
        Invoke<bool>(canRename, implicitField).Should().BeFalse();
        Invoke<bool>(canRename, handler).Should().BeFalse();
        Invoke<bool>(canRename, model.GetDeclaredSymbol(parameter)!).Should().BeTrue();

        Invoke<ISymbol?>(getDeclaredSymbol, parameter, model, CancellationToken.None).Should().NotBeNull();
        Invoke<ISymbol?>(getDeclaredSymbol, local, model, CancellationToken.None).Should().NotBeNull();
        Invoke<ISymbol?>(getDeclaredSymbol, SyntaxFactory.IdentifierName("value"), model, CancellationToken.None).Should().BeNull();

        Invoke<bool>(hasCollision, handler, root, "handler").Should().BeTrue();
        Invoke<SyntaxNode?>(getScope, accessorLocal).Should().BeOfType<AccessorDeclarationSyntax>();

        Invoke<bool>(isLocalVariable, local).Should().BeTrue();
        Invoke<bool>(isLocalVariable, counter).Should().BeTrue();
        Invoke<bool>(isLocalVariable, resource).Should().BeTrue();
        Invoke<bool>(isLocalVariable, scopedResource).Should().BeTrue();
        Invoke<bool>(isLocalVariable, pointer).Should().BeTrue();
        Invoke<bool>(isLocalVariable, root.DescendantNodes().OfType<VariableDeclaratorSyntax>().First(node => node.Identifier.ValueText == "_field")).Should().BeFalse();
        Invoke<bool>(isLocalVariable, SyntaxFactory.VariableDeclarator("orphan")).Should().BeFalse();
    }

    [TestMethod]
    [DataRow("""
        class C
        {
            int Run(int _value)
            {
                int value()
                {
                    return 0;
                }

                return _value + value();
            }
        }
        """, "_value", "value")]
    [DataRow("""
        class C
        {
            int Run(int _value)
            {
                foreach (var value in new[] { 1 })
                {
                    return value;
                }

                return _value;
            }
        }
        """, "_value", "value")]
    [DataRow("""
        class C
        {
            int Run(int _value)
            {
                try
                {
                }
                catch (System.Exception value)
                {
                    return value.Message.Length;
                }

                return _value;
            }
        }
        """, "_value", "value")]
    [DataRow("""
        class C
        {
            int Run(int _value)
            {
                var pair = (1, 2);
                var (value, other) = pair;
                return _value + other;
            }
        }
        """, "_value", "value")]
    public async Task ScopeCollisionHelperDetectsExecutableScopeNameConflicts(string source, string marker, string replacementName)
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document(source);
        var root = (await document.GetSyntaxRootAsync())!;
        var hasScopeCollision = GetStaticMethod("HasScopeCollision");
        var declaration = root.DescendantNodes().OfType<ParameterSyntax>().Single(parameter => parameter.Identifier.ValueText == marker);

        Invoke<bool>(hasScopeCollision, declaration, replacementName).Should().BeTrue();
    }

    [TestMethod]
    public async Task ScopeCollisionHelperAllowsDistinctExecutableScopeNames()
    {
        using var harness = new CodeFixHarness();
        var document = harness.Document("""
            class C
            {
                int Run(int _value)
                {
                    int other()
                    {
                        return 0;
                    }

                    foreach (var item in new[] { 1 })
                    {
                        return item;
                    }

                    try
                    {
                    }
                    catch (System.Exception error)
                    {
                        return error.Message.Length;
                    }

                    var pair = (1, 2);
                    var (first, second) = pair;
                    return _value + first + second + other();
                }
            }
            """);
        var root = (await document.GetSyntaxRootAsync())!;
        var hasScopeCollision = GetStaticMethod("HasScopeCollision");
        var declaration = root.DescendantNodes().OfType<ParameterSyntax>().Single(parameter => parameter.Identifier.ValueText == "_value");

        Invoke<bool>(hasScopeCollision, declaration, "value").Should().BeFalse();
    }

    private static MethodInfo GetStaticMethod(string name) =>
        typeof(Ifx.CodeFixes.Providers.Ifx1400NoUnderscorePrefixedIdentifierCodeFixProvider)
            .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;

    private static T Invoke<T>(MethodInfo method, params object?[] args) =>
        (T)method.Invoke(null, args)!;
}
