using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace VisionaryCoder.Framework.Tests.Generators.Implementation;

internal static class GeneratorHarness
{
    internal static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.CSharp14);
    private static readonly ImmutableArray<MetadataReference> References = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
        .Split(Path.PathSeparator)
        .Distinct(StringComparer.OrdinalIgnoreCase).Select(path => MetadataReference.CreateFromFile(path)).ToImmutableArray<MetadataReference>();

    internal static CSharpCompilation Compilation(string source, bool hosting = true, string name = "GeneratorFixture", bool includeAttributes = true) => CSharpCompilation.Create(name,
        new[] { CSharpSyntaxTree.ParseText(source, ParseOptions, "Fixture.cs") }.Concat(includeAttributes ? AttributeTrees() : Array.Empty<SyntaxTree>()),
        hosting ? References : References.Where(reference => !Path.GetFileName(reference.Display)!.StartsWith("Microsoft.AspNetCore.", StringComparison.Ordinal)),
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, nullableContextOptions: NullableContextOptions.Enable));

    private static IEnumerable<SyntaxTree> AttributeTrees([CallerFilePath] string sourcePath = "")
    {
        var directory = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourcePath)!, "../../../../../src/vc.Ifx.Generators.Abstractions/Attributes"));
        yield return CSharpSyntaxTree.ParseText("global using System;", ParseOptions, "GlobalUsings.cs");
        foreach (var path in Directory.GetFiles(directory, "*.cs").Order(StringComparer.Ordinal))
        {
            yield return CSharpSyntaxTree.ParseText(File.ReadAllText(path), ParseOptions, Path.GetFileName(path));
        }
    }

    internal static Result Run(string source, IIncrementalGenerator generator, bool hosting = true, bool allowInputErrors = false,
        CancellationToken cancellationToken = default, bool includeAttributes = true)
    {
        var compilation = Compilation(source, hosting, includeAttributes: includeAttributes);
        if (!allowInputErrors) { AssertNoErrors(compilation.GetDiagnostics()); }
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { generator.AsSourceGenerator() }, parseOptions: ParseOptions,
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));
        return Run(compilation, driver, cancellationToken);
    }

    internal static Result Run(CSharpCompilation compilation, GeneratorDriver driver, CancellationToken cancellationToken = default)
    {
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var updated, out var diagnostics, cancellationToken);
        diagnostics.Should().NotContain(diagnostic => diagnostic.Id == "CS8785" || diagnostic.Id == "CS8784", string.Join("\n", diagnostics));
        return new Result((CSharpCompilation)updated, driver, driver.GetRunResult());
    }

    internal static void AssertNoErrors(IEnumerable<Diagnostic> diagnostics) => diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
        .Should().BeEmpty(string.Join("\n", diagnostics));

    internal sealed record Result(CSharpCompilation Compilation, GeneratorDriver Driver, GeneratorDriverRunResult Run)
    {
        internal string Source => string.Join("\n", Run.Results.SelectMany(result => result.GeneratedSources).Select(source => source.SourceText.ToString()));
        internal ImmutableArray<Diagnostic> Diagnostics => Run.Diagnostics;
        internal void AssertCompiles()
        {
            AssertNoErrors(Diagnostics);
            AssertNoErrors(Compilation.GetDiagnostics());
        }
        internal Assembly Emit()
        {
            AssertCompiles();
            using var stream = new MemoryStream();
            var emit = Compilation.WithAssemblyName("GeneratorFixture" + Guid.NewGuid().ToString("N")).Emit(stream);
            AssertNoErrors(emit.Diagnostics);
            stream.Position = 0;
            return AssemblyLoadContext.Default.LoadFromStream(stream);
        }
    }
}
