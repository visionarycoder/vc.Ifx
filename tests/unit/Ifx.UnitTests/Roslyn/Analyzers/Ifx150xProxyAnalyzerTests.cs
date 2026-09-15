using Ifx.Analyzers.Abstractions;
using Ifx.Analyzers.Rules;

namespace Ifx.Tests.Roslyn.Analyzers;

[TestClass]
public sealed class Ifx150xProxyAnalyzerTests
{
    [TestMethod]
    [DataRow("""
        namespace Microsoft.Extensions.Caching.Memory
        {
            public interface IMemoryCache
            {
                bool TryGetValue(object key, out object value);
            }
        }

        class Orders
        {
            void Read(Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
            {
                cache.TryGetValue("orders", out _);
            }
        }
        """, "TryGetValue")]
    [DataRow("""
        namespace Microsoft.Extensions.Caching.Distributed
        {
            public interface IDistributedCache
            {
                byte[] Get(string key);
            }
        }

        sealed class CustomCache : Microsoft.Extensions.Caching.Distributed.IDistributedCache
        {
            public byte[] Get(string key) => null;
        }

        class Orders
        {
            byte[] Read(CustomCache cache)
            {
                return cache.Get("orders");
            }
        }
        """, "Get")]
    [DataRow("""
        namespace Microsoft.Extensions.Caching.Hybrid
        {
            public class HybridCache
            {
                public object GetOrCreate(string key) => null;
            }
        }

        class Orders
        {
            object Read(Microsoft.Extensions.Caching.Hybrid.HybridCache cache)
            {
                return cache.GetOrCreate("orders");
            }
        }
        """, "GetOrCreate")]
    public async Task CachingAnalyzerReportsDirectCacheInvocationsOutsideFramework(string source, string methodName)
    {
        var diagnostics = await AnalyzerHarness.RunAsync(source, new AvoidManualCachingAnalyzer(), allowCompilerErrors: true);

        diagnostics.Should().ContainSingle();
        diagnostics[0].Id.Should().Be(DiagnosticIds.Ifx1500AvoidManualCaching);
        diagnostics[0].GetMessage().Should().Contain(methodName);
    }

    [TestMethod]
    [DataRow("""
        namespace Microsoft.Extensions.Caching.Memory
        {
            public interface IMemoryCache
            {
                bool TryGetValue(object key, out object value);
            }
        }

        namespace vc.App.VisionaryCoder.Ifx.Caching
        {
            class Orders
            {
                void Read(Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
                {
                    cache.TryGetValue("orders", out _);
                }
            }
        }
        """)]
    [DataRow("""
        class CacheLike
        {
            public bool TryGetValue(object key, out object value)
            {
                value = null;
                return false;
            }
        }

        class Orders
        {
            void Read(CacheLike cache)
            {
                cache.TryGetValue("orders", out _);
            }
        }
        """)]
    [DataRow("""
        class Orders
        {
            void Read()
            {
                cache.TryGetValue("orders", out _);
            }
        }
        """)]
    [DataRow("""
        class Orders
        {
            void Read()
            {
                bool TryGetValue(object key, out object value)
                {
                    value = null;
                    return false;
                }

                TryGetValue("orders", out _);
            }
        }
        """)]
    [DataRow("class Orders { void Read() { } }")]
    public async Task CachingAnalyzerIgnoresFrameworkNamespaceAndNonCacheTypes(string source)
    {
        var diagnostics = await AnalyzerHarness.RunAsync(source, new AvoidManualCachingAnalyzer(), allowCompilerErrors: true);

        diagnostics.Should().BeEmpty();
    }

    [TestMethod]
    public async Task CachingAnalyzerIgnoresMatchingMethodNamesOnUnrelatedTypesWhenCacheMetadataExists()
    {
        const string source = """
            namespace Microsoft.Extensions.Caching.Memory
            {
                public interface IMemoryCache
                {
                    bool TryGetValue(object key, out object value);
                }
            }

            sealed class CacheLike
            {
                public bool TryGetValue(object key, out object value)
                {
                    value = null;
                    return false;
                }
            }

            class Orders
            {
                void Read(CacheLike cache)
                {
                    cache.TryGetValue("orders", out _);
                }
            }
            """;

        var diagnostics = await AnalyzerHarness.RunAsync(source, new AvoidManualCachingAnalyzer(), allowCompilerErrors: true);

        diagnostics.Should().BeEmpty();
    }

    [TestMethod]
    public async Task CachingAnalyzerReportsStaticCacheMethodsWithoutAReceiver()
    {
        const string source = """
            namespace Microsoft.Extensions.Caching.Hybrid
            {
                public class HybridCache
                {
                    public static object GetOrCreate(string key) => null;
                }
            }

            class Orders
            {
                object Read()
                {
                    return Microsoft.Extensions.Caching.Hybrid.HybridCache.GetOrCreate("orders");
                }
            }
            """;

        var diagnostics = await AnalyzerHarness.RunAsync(source, new AvoidManualCachingAnalyzer(), allowCompilerErrors: true);

        diagnostics.Should().ContainSingle();
        diagnostics[0].Id.Should().Be(DiagnosticIds.Ifx1500AvoidManualCaching);
        diagnostics[0].GetMessage().Should().Contain("GetOrCreate");
    }

    [TestMethod]
    [DataRow("""
        using System.Threading.Tasks;

        class Orders
        {
            async Task ReadAsync()
            {
                for (var attempt = 0; attempt < 3; attempt++)
                {
                    try
                    {
                        await CallAsync();
                    }
                    catch
                    {
                        await Task.Delay(10);
                    }
                }
            }

            Task CallAsync() => Task.CompletedTask;
        }
        """)]
    [DataRow("""
        using System.Threading.Tasks;

        class Orders
        {
            async Task ReadAsync()
            {
                while (true)
                {
                    try
                    {
                        await CallAsync();
                    }
                    catch
                    {
                        await Task.Delay(10);
                    }
                }
            }

            Task CallAsync() => Task.CompletedTask;
        }
        """)]
    [DataRow("""
        using System.Threading.Tasks;

        class Orders
        {
            async Task ReadAsync()
            {
                do
                {
                    try
                    {
                        await CallAsync();
                    }
                    catch
                    {
                        await Task.Delay(10);
                    }
                }
                while (true);
            }

            Task CallAsync() => Task.CompletedTask;
        }
        """)]
    public async Task RetryAnalyzerReportsLoopWithTryCatchAndTaskDelay(string source)
    {
        var diagnostics = await AnalyzerHarness.RunAsync(source, new AvoidManualRetryLoopAnalyzer());

        diagnostics.Should().ContainSingle();
        diagnostics[0].Id.Should().Be(DiagnosticIds.Ifx1501AvoidManualRetryLoop);
        diagnostics[0].GetMessage().Should().Contain("Retry");
    }

    [TestMethod]
    [DataRow("""
        using System.Threading.Tasks;
        class Orders
        {
            async Task ReadAsync()
            {
                for (var attempt = 0; attempt < 3; attempt++)
                {
                    await Task.Delay(10);
                }
            }
        }
        """)]
    [DataRow("""
        using System.Threading.Tasks;
        class Orders
        {
            async Task ReadAsync()
            {
                while (true)
                {
                    try
                    {
                        await Task.Yield();
                    }
                    catch
                    {
                        break;
                    }
                }
            }
        }
        """)]
    [DataRow("""
        class Orders
        {
            void Read()
            {
                while (true)
                {
                    try
                    {
                        Delay(10);
                    }
                    catch
                    {
                    }
                }
            }

            void Delay(int milliseconds) { }
        }
        """)]
    [DataRow("""
        using System.Threading.Tasks;
        namespace vc.App.VisionaryCoder.Ifx.Caching
        {
            class Orders
            {
                async Task ReadAsync()
                {
                    while (true)
                    {
                        try
                        {
                            await Task.Yield();
                        }
                        catch
                        {
                            await Task.Delay(10);
                        }
                    }
                }
            }
        }
        """)]
    public async Task RetryAnalyzerIgnoresIncompletePatternsCustomDelayAndFrameworkCode(string source)
    {
        var diagnostics = await AnalyzerHarness.RunAsync(source, new AvoidManualRetryLoopAnalyzer());

        diagnostics.Should().BeEmpty();
    }

    [TestMethod]
    public async Task RetryAnalyzerIgnoresMalformedLoopWithoutContainingSymbol()
    {
        const string source = """
            using System.Threading.Tasks;

            class Orders
            {
                while (true)
                {
                    try
                    {
                    }
                    catch
                    {
                        Task.Delay(10);
                    }
                }
            }
            """;

        var diagnostics = await AnalyzerHarness.RunAsync(source, new AvoidManualRetryLoopAnalyzer(), allowCompilerErrors: true);

        diagnostics.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("""
        namespace System.IdentityModel.Tokens.Jwt
        {
            public class JwtSecurityTokenHandler
            {
                public object ValidateToken(string token) => null;
            }
        }

        class Orders
        {
            object Read(System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler handler)
            {
                return handler.ValidateToken("jwt");
            }
        }
        """, "ValidateToken")]
    [DataRow("""
        using System.Threading.Tasks;

        namespace System.IdentityModel.Tokens.Jwt
        {
            public class JwtSecurityTokenHandler
            {
                public Task<object> ValidateTokenAsync(string token) => Task.FromResult<object>(null);
            }
        }

        class Orders
        {
            Task<object> ReadAsync(System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler handler)
            {
                return handler.ValidateTokenAsync("jwt");
            }
        }
        """, "ValidateTokenAsync")]
    public async Task JwtAnalyzerReportsManualValidationOutsideFramework(string source, string methodName)
    {
        var diagnostics = await AnalyzerHarness.RunAsync(source, new AvoidManualJwtValidationAnalyzer(), allowCompilerErrors: true);

        diagnostics.Should().ContainSingle();
        diagnostics[0].Id.Should().Be(DiagnosticIds.Ifx1502AvoidManualJwtValidation);
        diagnostics[0].GetMessage().Should().Contain(methodName);
    }

    [TestMethod]
    [DataRow("""
        namespace System.IdentityModel.Tokens.Jwt
        {
            public class JwtSecurityTokenHandler
            {
                public object ReadToken(string token) => null;
            }
        }

        class Orders
        {
            object Read(System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler handler)
            {
                return handler.ReadToken("jwt");
            }
        }
        """)]
    [DataRow("""
        class JwtSecurityTokenHandler
        {
            public object ValidateToken(string token) => null;
        }

        class Orders
        {
            object Read(JwtSecurityTokenHandler handler)
            {
                return handler.ValidateToken("jwt");
            }
        }
        """)]
    [DataRow("""
        namespace System.IdentityModel.Tokens.Jwt
        {
            public class JwtSecurityTokenHandler
            {
                public object ValidateToken(string token) => null;
            }
        }

        namespace vc.App.VisionaryCoder.Ifx.Security
        {
            class Orders
            {
                object Read(System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler handler)
                {
                    return handler.ValidateToken("jwt");
                }
            }
        }
        """)]
    [DataRow("""
        class Orders
        {
            object Read()
            {
                return handler.ValidateToken("jwt");
            }
        }
        """)]
    [DataRow("""
        class Orders
        {
            object Read()
            {
                object ValidateToken(string token)
                {
                    return null;
                }

                return ValidateToken("jwt");
            }
        }
        """)]
    [DataRow("class Orders { void Read() { } }")]
    public async Task JwtAnalyzerIgnoresOtherMethodsLookalikesFrameworkCodeAndMissingMetadata(string source)
    {
        var diagnostics = await AnalyzerHarness.RunAsync(source, new AvoidManualJwtValidationAnalyzer(), allowCompilerErrors: true);

        diagnostics.Should().BeEmpty();
    }

    [TestMethod]
    public async Task JwtAnalyzerIgnoresMatchingMethodNamesOnUnrelatedTypesWhenJwtMetadataExists()
    {
        const string source = """
            namespace System.IdentityModel.Tokens.Jwt
            {
                public class JwtSecurityTokenHandler
                {
                    public object ValidateToken(string token) => null;
                }
            }

            class CustomHandler
            {
                public object ValidateToken(string token) => null;
            }

            class Orders
            {
                object Read(CustomHandler handler)
                {
                    return handler.ValidateToken("jwt");
                }
            }
            """;

        var diagnostics = await AnalyzerHarness.RunAsync(source, new AvoidManualJwtValidationAnalyzer(), allowCompilerErrors: true);

        diagnostics.Should().BeEmpty();
    }

    [TestMethod]
    public async Task JwtAnalyzerReportsStaticValidationWithoutAReceiver()
    {
        const string source = """
            namespace System.IdentityModel.Tokens.Jwt
            {
                public class JwtSecurityTokenHandler
                {
                    public static object ValidateToken(string token) => null;
                }
            }

            class Orders
            {
                object Read()
                {
                    return System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.ValidateToken("jwt");
                }
            }
            """;

        var diagnostics = await AnalyzerHarness.RunAsync(source, new AvoidManualJwtValidationAnalyzer(), allowCompilerErrors: true);

        diagnostics.Should().ContainSingle();
        diagnostics[0].Id.Should().Be(DiagnosticIds.Ifx1502AvoidManualJwtValidation);
        diagnostics[0].GetMessage().Should().Contain("ValidateToken");
    }
}
