using vc.Ifx.Generators;

namespace VisionaryCoder.Framework.Tests.Generators.Implementation;

[TestClass]
public sealed class LegacyGeneratorTests
{
    private const string Header = "using System; using System.Threading.Tasks; using vc.Ifx.Generators.Abstractions.Attributes; ";
    private const string EnumerationBase = "namespace vc.Ifx.Primitives { public abstract class Enumeration(int id, string name) { public int Id { get; } = id; public string Name { get; } = name; } }";

    [TestMethod]
    public void EnumerationDefaultAndExplicitOptionsCompileAndRun()
    {
        var result = GeneratorHarness.Run(Header + "namespace Example { [GenerateEnumeration(\"Status\", \"Busy\")] public enum StatusCode { Ready=1, Busy=2 } }" + EnumerationBase, new EnumerationGenerator());
        var assembly = result.Emit();
        var type = assembly.GetType("Example.Status")!;
        var ready = type.GetField("Ready")!.GetValue(null);
        var busy = type.GetField("Busy")!.GetValue(null);
        type.GetMethod("FromValue")!.Invoke(null, new object[] { 1 }).Should().BeSameAs(ready);
        type.GetMethod("FromValue")!.Invoke(null, new object[] { 99 }).Should().BeSameAs(busy);
        type.GetMethod("FromName")!.Invoke(null, new object?[] { null }).Should().BeSameAs(busy);
        type.GetMethod("FromName")!.Invoke(null, new object[] { "Ready" }).Should().BeSameAs(ready);
        type.GetMethod("FromName")!.Invoke(null, new object[] { "Unknown" }).Should().BeSameAs(busy);
        ready!.Equals(busy).Should().BeFalse();
        ready.Equals(ready).Should().BeTrue();
    }

    [TestMethod]
    [DataRow("[GenerateEnumeration(\"Status\")]")]
    [DataRow("[GenerateEnumeration(\"Status\", \"Missing\")]")]
    [DataRow("[GenerateEnumeration(\"Status\", DefaultName=\"Busy\", Namespace=\"Output\", EnumerationNamespace=\"vc.Ifx.Primitives\", EnumerationTypeName=\"Enumeration\")]")]
    public void EnumerationFallbackAndNamespaceOptionsCompile(string attribute) => GeneratorHarness.Run(
        Header + attribute + " public enum Code { Ready, Busy }" + EnumerationBase, new EnumerationGenerator()).AssertCompiles();

    [TestMethod]
    [DataRow("[GenerateEnumeration(null)] public enum Code { Ready }")]
    [DataRow("[GenerateEnumeration(\"\")] public enum Code { Ready }")]
    [DataRow("[GenerateEnumeration(\"Status\")] public enum Code { }")]
    [DataRow("public enum Code { Ready }")]
    public void EnumerationNoGenerationCasesStayQuiet(string source)
    {
        var result = GeneratorHarness.Run(Header + source, new EnumerationGenerator());
        result.Diagnostics.Should().BeEmpty();
        result.Source.Should().BeEmpty();
    }

    [TestMethod]
    public void LegacyEnumerationAttributeNamespaceStillWorks() => GeneratorHarness.Run("""
        namespace vc.Ifx.Generators { public sealed class GenerateEnumerationAttribute(string name) : System.Attribute { } }
        [vc.Ifx.Generators.GenerateEnumeration("Status")] public enum Code { Ready }
        """ + EnumerationBase, new EnumerationGenerator()).AssertCompiles();

    [TestMethod]
    public void EnumerationEscapesKeywordFieldsAndSeparatesNamespaceHintNames() => GeneratorHarness.Run(Header + """
        namespace First { [GenerateEnumeration("class", "default")] public enum Code { @default, Other } }
        namespace Second { [GenerateEnumeration("class")] public enum Code { @default } }
        """ + EnumerationBase, new EnumerationGenerator()).AssertCompiles();

    [TestMethod]
    public void MalformedLegacyDeclarationsDoNotCrash()
    {
        GeneratorHarness.Run(Header + "[GenerateEnumeration(\"Status\")] public enum Code { Invalid=Unknown }", new EnumerationGenerator(), allowInputErrors: true).Source.Should().BeEmpty();
        GeneratorHarness.Run(Header + "[GenerateInterceptors] public class Marker { }", new InterceptorsGenerator(), allowInputErrors: true).Source.Should().BeEmpty();
        GeneratorHarness.Run(Header + "[GenerateInterceptors(\"source\", new Type[]{null})] public class Marker { }", new InterceptorsGenerator()).Source.Should().BeEmpty();
    }

    [TestMethod]
    public void InterceptorIgnoresIncompatibleServiceArrayMetadata() => GeneratorHarness.Run(Header + """
        namespace vc.Ifx.Generators.Abstractions.Attributes { public sealed class GenerateInterceptorsAttribute(string source, string services) : Attribute { } }
        [GenerateInterceptors("source", "not an array")] public class Marker { }
        """, new InterceptorsGenerator(), includeAttributes: false).Source.Should().BeEmpty();

    [TestMethod]
    [DataRow("ulong", "18446744073709551615UL")]
    [DataRow("long", "-2147483649L")]
    public void EnumerationOverflowReportsAtMemberWithoutGeneratorCrash(string type, string value)
    {
        var result = GeneratorHarness.Run(Header + $"[GenerateEnumeration(\"Status\")] public enum Code : {type} {{ Huge={value} }}", new EnumerationGenerator());
        result.Diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == "GEN002" && diagnostic.Location.IsInSource);
        result.Source.Should().BeEmpty();
    }

    [TestMethod]
    public void MapperGeneratesBidirectionalCompiledMappings()
    {
        var result = GeneratorHarness.Run("namespace First { public class Crosswalk { public int Id { get; set; } } } namespace Second { public class Crosswalk { public int Id { get; set; } } }", new MapperGenerator());
        result.AssertCompiles();
        result.Run.Results[0].GeneratedSources.Should().HaveCount(2);
        result.Source.Should().Contain("Id = source.Id");
    }

    [TestMethod]
    [DataRow("namespace First { public class Crosswalk { public int Id { get; set; } } } namespace Second { public class Crosswalk { public string Id { get; set; } = \"\"; } }")]
    [DataRow("namespace First { public class Crosswalk { } } namespace Second { public class Crosswalk { public int Id { get; set; } } }")]
    public void MapperReportsIncompatibleProperties(string source) => GeneratorHarness.Run(source, new MapperGenerator()).Diagnostics.Should().Contain(diagnostic => diagnostic.Id == "GEN001");

    [TestMethod]
    public void MapperWithoutPairProducesNoOutput() => GeneratorHarness.Run("public class Crosswalk { } public class Other { }", new MapperGenerator()).Source.Should().BeEmpty();

    [TestMethod]
    public void MapperPartialDeclarationsDoNotDuplicateMappings()
    {
        var result = GeneratorHarness.Run("namespace First { public partial class Crosswalk { } public partial class Crosswalk { } } namespace Second { public class Crosswalk { } }", new MapperGenerator());
        result.AssertCompiles();
        result.Run.Results[0].GeneratedSources.Should().HaveCount(2);
    }

    [TestMethod]
    public void InterceptorGeneratesAllOrdinaryMemberShapesAndAsyncReturns()
    {
        var result = GeneratorHarness.Run(Header + """
            namespace Example
            {
                public interface IBase { int Value { get; set; } event Action Changed; string Get(); }
                public interface IService : IBase
                {
                    event Action OwnEvent;
                    string ReadOnly { get; }
                    string WriteOnly { set; }
                    int this[int index] { get; set; }
                    int @class(ref int x, out int y, in int z);
                    void Run(params string[] values);
                    Task RunAsync(); Task<int> NumberAsync();
                    ValueTask RunValueAsync(); ValueTask<int> ValueAsync();
                    T Identity<T>(T input);
                    T Create<T>() where T : class, new();
                    T StructValue<T>() where T : struct;
                    T UnmanagedValue<T>() where T : unmanaged;
                    T Constrained<T>(T item) where T : IDisposable;
                }
                [GenerateInterceptors("Example", typeof(IService))] public class Marker { }
            }
            """, new InterceptorsGenerator());
        result.AssertCompiles();
        result.Source.Should().Contain("async global::System.Threading.Tasks.Task<int>").And.Contain("public event").And.Contain("public int this[");
    }

    [TestMethod]
    [DataRow("null", "")]
    [DataRow("\"\"", "InterceptorSuffix=\"Trace\"")]
    [DataRow("\"source\\\"\\\\\\r\\n\\t\"", "DecoratorSuffix=\"Decorator\"")]
    [DataRow("\"Activity\"", "InterceptorSuffix=\"\", DecoratorSuffix=\"Alias\"")]
    public void InterceptorFallbackAliasesAndStringEscapingCompile(string activity, string options)
    {
        var suffix = string.IsNullOrEmpty(options) ? "" : ", " + options;
        GeneratorHarness.Run(Header + $"public interface IService {{ string Read(); }} [GenerateInterceptors({activity}, typeof(IService){suffix})] public class Marker {{ }}", new InterceptorsGenerator()).AssertCompiles();
    }

    [TestMethod]
    public void InterceptorClosedGenericContractsAndRequestMetadataCompile() => GeneratorHarness.Run(Header + """
        namespace VisionaryCoder.Framework { public class ServiceRequest { public string CorrelationId => "correlation"; public string MessageId => "message"; } }
        namespace Example
        {
            public class Request : VisionaryCoder.Framework.ServiceRequest { }
            public interface IService<T> { T Read(Request request); }
            [GenerateInterceptors("Example", typeof(IService<int>), typeof(IService<int>))] public class Marker { }
        }
        """, new InterceptorsGenerator()).AssertCompiles();

    [TestMethod]
    public void InterceptorPreservesHiddenBasePropertyWithoutMemberCollision() => GeneratorHarness.Run(Header + """
        public interface IBase { int Value { get; set; } }
        public interface IService : IBase { new T Value<T>() where T : struct; }
        [GenerateInterceptors("source", typeof(IService))] public class Marker { }
        """, new InterceptorsGenerator()).AssertCompiles();

    [TestMethod]
    public void InterceptorMarkersInDifferentNamespacesHaveUniqueHintNames() => GeneratorHarness.Run(Header + """
        namespace First { public interface IService { } [GenerateInterceptors("first", typeof(IService))] public class Marker { } }
        namespace Second { public interface IService { } [GenerateInterceptors("second", typeof(IService))] public class Marker { } }
        """, new InterceptorsGenerator()).AssertCompiles();

    [TestMethod]
    [DataRow("[GenerateInterceptors(\"Activity\")] public class Marker { }")]
    [DataRow("[GenerateInterceptors(\"Activity\", typeof(string))] public class Marker { }")]
    [DataRow("[GenerateInterceptors(\"Activity\", null)] public class Marker { }")]
    [DataRow("public class Marker { }")]
    public void InterceptorWithoutServiceContractsProducesNoOutput(string source) => GeneratorHarness.Run(Header + source, new InterceptorsGenerator()).Source.Should().BeEmpty();
}
