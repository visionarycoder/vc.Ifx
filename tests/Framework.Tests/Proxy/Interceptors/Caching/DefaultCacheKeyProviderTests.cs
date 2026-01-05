using VisionaryCoder.Framework.Proxy;
using VisionaryCoder.Framework.Proxy.Interceptors.Caching;

namespace VisionaryCoder.Framework.Tests.Proxy.Interceptors.Caching;

[TestClass]
public class DefaultCacheKeyProviderTests
{
    private DefaultCacheKeyProvider provider = null!;

    [TestInitialize]
    public void Setup()
    {
        provider = new DefaultCacheKeyProvider();
    }

    #region Basic Key Generation

    [TestMethod]
    public void GenerateKey_WithMinimalContext_ShouldReturnHashedKey()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1"
        };

        // Act
        string key = provider.GenerateKey<string>(context);

        // Assert
        key.Should().NotBeNullOrWhiteSpace();
        key.Should().MatchRegex(@"^[A-Za-z0-9+/=]+$"); // Base64 pattern
    }

    [TestMethod]
    [DataRow("GET", "https://api.example.com/users", "GetUser")]
    [DataRow("POST", "https://api.example.com/users", "CreateUser")]
    [DataRow("PUT", "https://api.example.com/users/1", "UpdateUser")]
    [DataRow("DELETE", "https://api.example.com/users/1", "DeleteUser")]
    public void GenerateKey_WithDifferentMethods_ShouldGenerateDifferentKeys(string method, string url, string operation)
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = operation,
            Method = method,
            Url = url
        };

        // Act
        string key = provider.GenerateKey<string>(context);

        // Assert
        key.Should().NotBeNullOrWhiteSpace();
    }

    [TestMethod]
    public void GenerateKey_WithSameContextCalledTwice_ShouldReturnSameKey()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1"
        };

        // Act
        string key1 = provider.GenerateKey<string>(context);
        string key2 = provider.GenerateKey<string>(context);

        // Assert
        key1.Should().Be(key2);
    }

    [TestMethod]
    public void GenerateKey_WithDifferentUrls_ShouldGenerateDifferentKeys()
    {
        // Arrange
        var context1 = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1"
        };
        var context2 = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/2"
        };

        // Act
        string key1 = provider.GenerateKey<string>(context1);
        string key2 = provider.GenerateKey<string>(context2);

        // Assert
        key1.Should().NotBe(key2);
    }

    [TestMethod]
    public void GenerateKey_WithDifferentResponseTypes_ShouldGenerateDifferentKeys()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1"
        };

        // Act
        string keyString = provider.GenerateKey<string>(context);
        string keyInt = provider.GenerateKey<int>(context);

        // Assert
        keyString.Should().NotBe(keyInt);
    }

    #endregion

    #region Null/Empty Context Values

    [TestMethod]
    public void GenerateKey_WithNullOperationName_ShouldUseUnknown()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = null,
            Method = "GET",
            Url = "https://api.example.com/users/1"
        };

        // Act
        string key = provider.GenerateKey<string>(context);

        // Assert
        key.Should().NotBeNullOrWhiteSpace();
    }

    [TestMethod]
    public void GenerateKey_WithNullMethod_ShouldUseGET()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = "GetUser",
            Method = null,
            Url = "https://api.example.com/users/1"
        };

        // Act
        string key = provider.GenerateKey<string>(context);

        // Assert
        key.Should().NotBeNullOrWhiteSpace();
    }

    [TestMethod]
    public void GenerateKey_WithNullUrl_ShouldUseEmpty()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = null
        };

        // Act
        string key = provider.GenerateKey<string>(context);

        // Assert
        key.Should().NotBeNullOrWhiteSpace();
    }

    [TestMethod]
    public void GenerateKey_WithAllNullValues_ShouldGenerateKey()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = null,
            Method = null,
            Url = null
        };

        // Act
        string key = provider.GenerateKey<string>(context);

        // Assert
        key.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Headers

    [TestMethod]
    public void GenerateKey_WithRelevantHeader_ShouldIncludeInKey()
    {
        // Arrange
        var contextWithoutHeader = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1"
        };
        var contextWithHeader = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1",
            Headers = new Dictionary<string, string> { { "Accept", "application/json" } }
        };

        // Act
        string keyWithout = provider.GenerateKey<string>(contextWithoutHeader);
        string keyWith = provider.GenerateKey<string>(contextWithHeader);

        // Assert
        keyWithout.Should().NotBe(keyWith);
    }

    [TestMethod]
    [DataRow("Accept")]
    [DataRow("Accept-Language")]
    [DataRow("Content-Type")]
    [DataRow("X-API-Version")]
    public void GenerateKey_WithRelevantHeaders_ShouldIncludeEachInKey(string headerName)
    {
        // Arrange
        var contextWithoutHeader = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1"
        };
        var contextWithHeader = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1",
            Headers = new Dictionary<string, string> { { headerName, "value" } }
        };

        // Act
        string keyWithout = provider.GenerateKey<string>(contextWithoutHeader);
        string keyWith = provider.GenerateKey<string>(contextWithHeader);

        // Assert
        keyWithout.Should().NotBe(keyWith);
    }

    [TestMethod]
    public void GenerateKey_WithIrrelevantHeader_ShouldNotIncludeInKey()
    {
        // Arrange
        var contextWithoutHeader = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1"
        };
        var contextWithHeader = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1",
            Headers = new Dictionary<string, string> { { "X-Custom-Header", "value" } }
        };

        // Act
        string keyWithout = provider.GenerateKey<string>(contextWithoutHeader);
        string keyWith = provider.GenerateKey<string>(contextWithHeader);

        // Assert
        keyWithout.Should().Be(keyWith);
    }

    [TestMethod]
    public void GenerateKey_WithMultipleRelevantHeaders_ShouldOrderHeaders()
    {
        // Arrange
        var context1 = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1",
            Headers = new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "Content-Type", "application/json" }
            }
        };
        var context2 = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1",
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Accept", "application/json" }
            }
        };

        // Act
        string key1 = provider.GenerateKey<string>(context1);
        string key2 = provider.GenerateKey<string>(context2);

        // Assert
        key1.Should().Be(key2); // Order should not matter
    }

    [TestMethod]
    public void GenerateKey_WithMixedRelevantAndIrrelevantHeaders_ShouldOnlyIncludeRelevant()
    {
        // Arrange
        var contextRelevantOnly = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1",
            Headers = new Dictionary<string, string>
            {
                { "Accept", "application/json" }
            }
        };
        var contextMixed = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1",
            Headers = new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "X-Custom-Header", "value" }
            }
        };

        // Act
        string keyRelevant = provider.GenerateKey<string>(contextRelevantOnly);
        string keyMixed = provider.GenerateKey<string>(contextMixed);

        // Assert
        keyRelevant.Should().Be(keyMixed); // Irrelevant headers should be ignored
    }

    [TestMethod]
    public void GenerateKey_WithDifferentCaseHeaderNames_ShouldGenerateDifferentKeys()
    {
        // Arrange - Dictionary keys are case-sensitive, so these are different headers
        var context1 = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1",
            Headers = new Dictionary<string, string> { { "accept", "application/json" } }
        };
        var context2 = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1",
            Headers = new Dictionary<string, string> { { "ACCEPT", "application/json" } }
        };

        // Act
        string key1 = provider.GenerateKey<string>(context1);
        string key2 = provider.GenerateKey<string>(context2);

    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void GenerateKey_WithVeryLongUrl_ShouldGenerateConsistentKey()
    {
        // Arrange
        string longUrl = "https://api.example.com/" + new string('a', 10000);
        var context = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = longUrl
        };

        // Act
        string key = provider.GenerateKey<string>(context);

        // Assert
        key.Should().NotBeNullOrWhiteSpace();
        key.Length.Should().Be(44); // Base64 of SHA256 hash (256 bits / 6 bits per char ≈ 44 chars)
    }

    [TestMethod]
    public void GenerateKey_WithSpecialCharactersInUrl_ShouldHandleGracefully()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users?name=John&age=30&email=test@example.com"
        };

        // Act
        string key = provider.GenerateKey<string>(context);

        // Assert
        key.Should().NotBeNullOrWhiteSpace();
    }

    [TestMethod]
    public void GenerateKey_WithUnicodeInOperationName_ShouldHandleGracefully()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = "获取用户", // "Get User" in Chinese
            Method = "GET",
            Url = "https://api.example.com/users/1"
        };

        // Act
        string key = provider.GenerateKey<string>(context);

        // Assert
        key.Should().NotBeNullOrWhiteSpace();
    }

    [TestMethod]
    public void GenerateKey_WithEmptyHeaders_ShouldNotIncludeHeaders()
    {
        // Arrange
        var contextWithoutHeaders = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1"
        };
        var contextWithEmptyHeaders = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1",
            Headers = new Dictionary<string, string>()
        };

        // Act
        string key1 = provider.GenerateKey<string>(contextWithoutHeaders);
        string key2 = provider.GenerateKey<string>(contextWithEmptyHeaders);

        // Assert
        key1.Should().Be(key2);
    }

    [TestMethod]
    public void GenerateKey_WithOnlyIrrelevantHeaders_ShouldNotIncludeHeaders()
    {
        // Arrange
        var contextWithoutHeaders = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1"
        };
        var contextWithIrrelevantHeaders = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1",
            Headers = new Dictionary<string, string>
            {
                { "X-Custom-1", "value1" },
                { "X-Custom-2", "value2" }
            }
        };

        // Act
        string key1 = provider.GenerateKey<string>(contextWithoutHeaders);
        string key2 = provider.GenerateKey<string>(contextWithIrrelevantHeaders);

        // Assert
        key1.Should().Be(key2);
    }

    [TestMethod]
    public void GenerateKey_WithDifferentHeaderValues_ShouldGenerateDifferentKeys()
    {
        // Arrange
        var context1 = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1",
            Headers = new Dictionary<string, string> { { "Accept", "application/json" } }
        };
        var context2 = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1",
            Headers = new Dictionary<string, string> { { "Accept", "application/xml" } }
        };

        // Act
        string key1 = provider.GenerateKey<string>(context1);
        string key2 = provider.GenerateKey<string>(context2);

        // Assert
        key1.Should().NotBe(key2);
    }

    #endregion

    #region Hash Consistency

    [TestMethod]
    public void GenerateKey_ShouldAlwaysReturnBase64EncodedHash()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1"
        };

        // Act
        string key = provider.GenerateKey<string>(context);

        // Assert
        key.Should().MatchRegex(@"^[A-Za-z0-9+/=]+$");
        key.Length.Should().Be(44); // Base64 of SHA256 hash
    }

    [TestMethod]
    public void GenerateKey_WithIdenticalContexts_ShouldProduceDeterministicHash()
    {
        // Arrange - create 10 identical contexts
        var contexts = Enumerable.Range(0, 10).Select(_ => new ProxyContext
        {
            OperationName = "GetUser",
            Method = "GET",
            Url = "https://api.example.com/users/1",
            Headers = new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "Content-Type", "application/json" }
            }
        }).ToList();

        // Act - generate keys for all contexts
        var keys = contexts.Select(c => provider.GenerateKey<string>(c)).ToList();

        // Assert - all keys should be identical (deterministic hashing)
        keys.Distinct().Should().HaveCount(1);
        keys.Should().AllBe(keys.First());
    }

    #endregion

    #region Complex Scenarios

    [TestMethod]
    public void GenerateKey_WithComplexContext_ShouldIncludeAllRelevantParts()
    {
        // Arrange
        var context = new ProxyContext
        {
            OperationName = "SearchUsers",
            Method = "POST",
            Url = "https://api.example.com/users/search?page=1&limit=10",
            Headers = new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "Content-Type", "application/json" },
                { "X-API-Version", "v2" },
                { "X-Custom-Header", "ignored" }
            }
        };

        // Act
        string key1 = provider.GenerateKey<SearchResult>(context);
        string key2 = provider.GenerateKey<SearchResult>(context);

        // Assert
        key1.Should().Be(key2);
        key1.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    private class SearchResult
    {
        public int TotalCount { get; set; }
        public List<string> Items { get; set; } = [];
    }
}
