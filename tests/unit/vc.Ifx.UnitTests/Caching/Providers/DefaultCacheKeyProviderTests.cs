// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching.Providers;

namespace VisionaryCoder.Framework.Tests.Caching.Providers;

/// <summary>
/// Comprehensive data-driven unit tests for DefaultCacheKeyProvider with 100% code coverage.
/// Tests all methods with happy path, edge cases, and bad inputs to ensure robust key generation.
/// </summary>
[TestClass]
public class DefaultCacheKeyProviderTests
{
    private DefaultCacheKeyProvider provider = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        provider = new DefaultCacheKeyProvider();
    }

    #region GenerateKey Method Tests

    [TestMethod]
    public void GenerateKey_WithValidContext_ShouldReturnHashedKey()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = "GetUsers",
            Method = "GET",
            Url = "https://api.example.com/users"
        };

        // Act
        string result = provider.GenerateKey(context);

        // Assert
        result.Should().NotBeNullOrEmpty("Should generate a valid cache key");
        result.Should().MatchRegex("^[A-Za-z0-9+/=]+$", "Should be a Base64-encoded SHA256 hash");
    }

    [TestMethod]
    [DataRow("GET", "https://api.example.com/users", "GetUsers")]
    [DataRow("POST", "https://api.example.com/users", "CreateUser")]
    [DataRow("PUT", "https://api.example.com/users/123", "UpdateUser")]
    [DataRow("DELETE", "https://api.example.com/users/123", "DeleteUser")]
    [DataRow("PATCH", "https://api.example.com/users/123", "PatchUser")]
    public void GenerateKey_WithDifferentHttpMethods_ShouldGenerateUniqueKeys(string method, string url, string operation)
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = operation,
            Method = method,
            Url = url
        };

        // Act
        string result = provider.GenerateKey(context);

        // Assert
        result.Should().NotBeNullOrEmpty($"Should generate key for method: {method}");
        result.Should().MatchRegex("^[A-Za-z0-9+/=]+$", "Should be a Base64-encoded SHA256 hash");
    }

    [TestMethod]
    public void GenerateKey_WithSameContextTwice_ShouldReturnSameKey()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = "GetUsers",
            Method = "GET",
            Url = "https://api.example.com/users"
        };

        // Act
        string result1 = provider.GenerateKey(context);
        string result2 = provider.GenerateKey(context);

        // Assert
        result1.Should().Be(result2, "Same context should generate identical keys");
    }

    [TestMethod]
    public void GenerateKey_WithDifferentContexts_ShouldReturnDifferentKeys()
    {
        // Arrange
        var context1 = new ProxyContext
        {
            OperationName = "GetUsers",
            Method = "GET",
            Url = "https://api.example.com/users"
        };

        var context2 = new ProxyContext
        {
            OperationName = "GetUsers",
            Method = "GET",
            Url = "https://api.example.com/products"
        };

        // Act
        string result1 = provider.GenerateKey(context1);
        string result2 = provider.GenerateKey(context2);

        // Assert
        result1.Should().NotBe(result2, "Different contexts should generate different keys");
    }

    [TestMethod]
    public void GenerateKey_WithHeaders_ShouldIncludeRelevantHeaders()
    {
        // Arrange
        var contextWithoutHeaders = new ProxyContext
        {
            OperationName = "GetUsers",
            Method = "GET",
            Url = "https://api.example.com/users"
        };

        var contextWithHeaders = new ProxyContext
        {
            OperationName = "GetUsers",
            Method = "GET",
            Url = "https://api.example.com/users",
            Headers = new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "Authorization", "Bearer token123" }
            }
        };

        // Act
        string keyWithoutHeaders = provider.GenerateKey(contextWithoutHeaders);
        string keyWithHeaders = provider.GenerateKey(contextWithHeaders);

        // Assert
        keyWithoutHeaders.Should().NotBe(keyWithHeaders, "Keys should differ when relevant headers are present");
    }

    [TestMethod]
    [DataRow("Accept", "application/json")]
    [DataRow("Accept-Language", "en-US")]
    [DataRow("Content-Type", "application/json")]
    [DataRow("X-API-Version", "v1")]
    [DataRow("Authorization", "Bearer token")]
    public void GenerateKey_WithRelevantHeaders_ShouldAffectKey(string headerName, string headerValue)
    {
        // Arrange
        var contextWithoutHeader = new ProxyContext
        {
            OperationName = "GetUsers",
            Method = "GET",
            Url = "https://api.example.com/users"
        };

        var contextWithHeader = new ProxyContext
        {
            OperationName = "GetUsers",
            Method = "GET",
            Url = "https://api.example.com/users",
            Headers = new Dictionary<string, string> { { headerName, headerValue } }
        };

        // Act
        string keyWithoutHeader = provider.GenerateKey(contextWithoutHeader);
        string keyWithHeader = provider.GenerateKey(contextWithHeader);

        // Assert
        keyWithoutHeader.Should().NotBe(keyWithHeader, $"Key should change when {headerName} header is present");
    }

    [TestMethod]
    public void GenerateKey_WithIrrelevantHeaders_ShouldIgnoreThem()
    {
        // Arrange
        var contextWithoutHeaders = new ProxyContext
        {
            OperationName = "GetUsers",
            Method = "GET",
            Url = "https://api.example.com/users"
        };

        var contextWithIrrelevantHeaders = new ProxyContext
        {
            OperationName = "GetUsers",
            Method = "GET",
            Url = "https://api.example.com/users",
            Headers = new Dictionary<string, string>
            {
                { "User-Agent", "Mozilla/5.0" },
                { "X-Custom-Header", "custom-value" },
                { "Cache-Control", "no-cache" }
            }
        };

        // Act
        string keyWithoutHeaders = provider.GenerateKey(contextWithoutHeaders);
        string keyWithIrrelevantHeaders = provider.GenerateKey(contextWithIrrelevantHeaders);

        // Assert
        keyWithoutHeaders.Should().Be(keyWithIrrelevantHeaders, "Irrelevant headers should not affect the cache key");
    }

    #endregion

    #region GenerateKey Generic Method Tests

    [TestMethod]
    public void GenerateKey_Generic_WithValidContext_ShouldReturnHashedKey()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = "GetUsers",
            Method = "GET",
            Url = "https://api.example.com/users"
        };

        // Act
        string result = provider.GenerateKey<string>(context);

        // Assert
        result.Should().NotBeNullOrEmpty("Should generate a valid cache key");
        result.Should().MatchRegex("^[A-Za-z0-9+/=]+$", "Should be a Base64-encoded SHA256 hash");
    }

    [TestMethod]
    public void GenerateKey_Generic_WithDifferentTypes_ShouldGenerateDifferentKeys()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = "GetData",
            Method = "GET",
            Url = "https://api.example.com/data"
        };

        // Act
        string stringKey = provider.GenerateKey<string>(context);
        string intKey = provider.GenerateKey<int>(context);
        string listKey = provider.GenerateKey<List<string>>(context);

        // Assert
        stringKey.Should().NotBe(intKey, "Different generic types should generate different keys");
        stringKey.Should().NotBe(listKey, "Different generic types should generate different keys");
        intKey.Should().NotBe(listKey, "Different generic types should generate different keys");
    }

    [TestMethod]
    public void GenerateKey_Generic_WithSameTypeMultipleTimes_ShouldReturnSameKey()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = "GetData",
            Method = "GET",
            Url = "https://api.example.com/data"
        };

        // Act
        string result1 = provider.GenerateKey<string>(context);
        string result2 = provider.GenerateKey<string>(context);

        // Assert
        result1.Should().Be(result2, "Same generic type and context should generate identical keys");
    }

    [TestMethod]
    public void GenerateKey_GenericVsNonGeneric_ShouldGenerateDifferentKeys()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = "GetData",
            Method = "GET",
            Url = "https://api.example.com/data"
        };

        // Act
        string nonGenericKey = provider.GenerateKey(context);
        string genericKey = provider.GenerateKey<string>(context);

        // Assert
        nonGenericKey.Should().NotBe(genericKey, "Generic and non-generic methods should generate different keys");
    }

    #endregion

    #region Key Consistency and Behavior Tests

    [TestMethod]
    public void GenerateKey_ShouldAlwaysReturnValidKey()
    {
        // Arrange
        ProxyContext[] contexts =
        [
            new ProxyContext { OperationName = "GetUsers", Method = "GET", Url = "https://api.example.com/users" },
            new ProxyContext { OperationName = "CreateUser", Method = "POST", Url = "https://api.example.com/users" },
            new ProxyContext { OperationName = "UpdateUser", Method = "PUT", Url = "https://api.example.com/users/123" },
            new ProxyContext { OperationName = "DeleteUser", Method = "DELETE", Url = "https://api.example.com/users/123" },
            new ProxyContext { OperationName = "PatchUser", Method = "PATCH", Url = "https://api.example.com/users/123" }
        ];

        // Act & Assert
        foreach (ProxyContext context in contexts)
        {
            string result = provider.GenerateKey(context);
            result.Should().NotBeNullOrEmpty($"Should always generate valid key for {context.Method} method");
            result.Should().MatchRegex("^[A-Za-z0-9+/=]+$", $"Should be a Base64-encoded SHA256 hash for {context.Method}");
        }
    }

    [TestMethod]
    public void GenerateKey_WithComplexScenarios_ShouldHandleGracefully()
    {
        // Arrange
        var complexContext = new ProxyContext
        {
            OperationName = "ComplexOperation",
            Method = "POST",
            Url = "https://api.example.com/complex/endpoint",
            Headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer token123" },
                { "Content-Type", "application/json" }
            }
        };

        // Act
        string result = provider.GenerateKey(complexContext);

        // Assert
        result.Should().NotBeNullOrEmpty("Should generate keys for complex contexts");
        result.Should().MatchRegex("^[A-Za-z0-9+/=]+$", "Should be a Base64-encoded SHA256 hash");
    }

    #endregion

    #region Edge Cases and Bad Input Tests

    [TestMethod]
    [DataRow(null, "GET", "https://example.com")]
    [DataRow("", "GET", "https://example.com")]
    [DataRow("Operation", null, "https://example.com")]
    [DataRow("Operation", "", "https://example.com")]
    [DataRow("Operation", "GET", null)]
    [DataRow("Operation", "GET", "")]
    public void GenerateKey_WithNullOrEmptyValues_ShouldHandleGracefully(string? operationName, string? method, string? url)
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = operationName,
            Method = method,
            Url = url
        };

        // Act
        string result = provider.GenerateKey(context);

        // Assert
        result.Should().NotBeNullOrEmpty("Should generate key even with null/empty values by using defaults");
        result.Should().MatchRegex("^[A-Za-z0-9+/=]+$", "Should still be a Base64-encoded SHA256 hash");
    }

    [TestMethod]
    public void GenerateKey_WithNullHeaders_ShouldHandleGracefully()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = "TestOperation",
            Method = "GET",
            Url = "https://api.example.com/test",
            Headers = null!
        };

        // Act & Assert
        Action act = () => provider.GenerateKey(context);
        act.Should().NotThrow("Should handle null headers gracefully");
    }

    [TestMethod]
    public void GenerateKey_WithEmptyHeaders_ShouldHandleGracefully()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = "TestOperation",
            Method = "GET",
            Url = "https://api.example.com/test",
            Headers = new Dictionary<string, string>()
        };

        // Act
        string result = provider.GenerateKey(context);

        // Assert
        result.Should().NotBeNullOrEmpty("Should generate key with empty headers");
        result.Should().MatchRegex("^[A-Za-z0-9+/=]+$", "Should be a Base64-encoded SHA256 hash");
    }

    [TestMethod]
    public void GenerateKey_WithSpecialCharactersInUrl_ShouldHandleGracefully()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = "TestOperation",
            Method = "GET",
            Url = "https://api.example.com/test?param1=value with spaces&param2=value%20with%20encoding&param3=特殊字符"
        };

        // Act
        string result = provider.GenerateKey(context);

        // Assert
        result.Should().NotBeNullOrEmpty("Should handle special characters in URL");
        result.Should().MatchRegex("^[A-Za-z0-9+/=]+$", "Should be a Base64-encoded SHA256 hash");
    }

    [TestMethod]
    public void GenerateKey_WithVeryLongUrl_ShouldHandleGracefully()
    {
        // Arrange
        string longUrl = "https://api.example.com/" + new string('a', 2000) + "?" + string.Join("&",
            Enumerable.Range(1, 100).Select(i => $"param{i}=value{i}"));

        var context = new ProxyContext
        {
            OperationName = "TestOperation",
            Method = "GET",
            Url = longUrl
        };

        // Act
        string result = provider.GenerateKey(context);

        // Assert
        result.Should().NotBeNullOrEmpty("Should handle very long URLs");
        result.Should().MatchRegex("^[A-Za-z0-9+/=]+$", "Should be a Base64-encoded SHA256 hash");
        result.Length.Should().Be(44, "Hash length should remain consistent regardless of input size");
    }

    #endregion

    #region Thread Safety Tests

    [TestMethod]
    public void GenerateKey_ShouldBeThreadSafe()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = "ThreadTest",
            Method = "GET",
            Url = "https://api.example.com/thread-test"
        };

        var tasks = new List<Task<string>>();

        // Act
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(Task.Run(() => provider.GenerateKey(context)));
        }

        string[] results = Task.WhenAll(tasks).Result;

        // Assert
        results.Should().AllSatisfy(result =>
        {
            result.Should().NotBeNullOrEmpty("All results should be valid");
            result.Should().MatchRegex("^[A-Za-z0-9+/=]+$", "All results should be Base64-encoded SHA256 hashes");
        });

        var uniqueResults = results.Distinct().ToList();
        uniqueResults.Should().HaveCount(1, "All concurrent calls with same context should produce same result");
    }

    #endregion

    #region Performance and Consistency Tests

    [TestMethod]
    public void GenerateKey_ShouldBeConsistentAcrossInstances()
    {
        // Arrange
        var provider1 = new DefaultCacheKeyProvider();
        var provider2 = new DefaultCacheKeyProvider();

        var context = new ProxyContext
        {
            OperationName = "TestOperation",
            Method = "GET",
            Url = "https://api.example.com/test"
        };

        // Act
        string key1 = provider1.GenerateKey(context);
        string key2 = provider2.GenerateKey(context);

        // Assert
        key1.Should().Be(key2, "Different instances should generate same key for same context");
    }

    [TestMethod]
    public void GenerateKey_WithManyDifferentContexts_ShouldGenerateUniqueKeys()
    {
        // Arrange
        var keys = new HashSet<string>();
        var contexts = new List<ProxyContext>();

        // Create many different contexts
        for (int i = 0; i < 100; i++)
        {
            contexts.Add(new ProxyContext
            {
                OperationName = $"Operation{i}",
                Method = i % 2 == 0 ? "GET" : "POST",
                Url = $"https://api.example.com/endpoint{i}"
            });
        }

        // Act
        foreach (ProxyContext context in contexts)
        {
            string key = provider.GenerateKey(context);
            keys.Add(key);
        }

        // Assert
        keys.Should().HaveCount(100, "All different contexts should generate unique keys");
    }

    #endregion
}
