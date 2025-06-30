using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using vc.Ifx.Web;

namespace Ifx.Web.UnitTests;

public class OpenApiHelperTests
{
    private const string BaseUrl = "https://api.example.com/";
    private const string JwtToken = "dummy-token";

    private class DummyResponse
    {
        public string? Message { get; set; }
        public int Value { get; set; }
    }

    private static readonly DummyResponse SampleResponse = new() { Message = "ok", Value = 42 };
    private static readonly string SampleResponseJson = JsonConvert.SerializeObject(SampleResponse);

    private static HttpClient CreateMockHttpClient(HttpStatusCode statusCode, string content, Action<HttpRequestMessage>? onRequest = null)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                onRequest?.Invoke(request);
                return new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content, Encoding.UTF8, "application/json")
                };
            });
        return new HttpClient(handlerMock.Object) { BaseAddress = new Uri(BaseUrl) };
    }

    [TestCase("endpoint", HttpStatusCode.OK, "{\"Message\":\"ok\",\"Value\":42}", "ok", 42)]
    [TestCase("endpoint", HttpStatusCode.OK, "{\"Message\":null,\"Value\":0}", null, 0)]
    public async Task GetAsync_ReturnsDeserializedObject_OnSuccess(string endpoint, HttpStatusCode code, string json, string? expectedMsg, int expectedVal)
    {
        var client = CreateMockHttpClient(code, json);
        using var helper = new OpenApiHelper(BaseUrl, JwtToken);
        typeof(OpenApiHelper)
            .GetField("httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(helper, client);

        var result = await helper.GetAsync<DummyResponse>(endpoint);

        Assert.That(result.Message, Is.EqualTo(expectedMsg));
        Assert.That(result.Value, Is.EqualTo(expectedVal));
    }

    [Test]
    public void GetAsync_Throws_OnNonSuccessStatus()
    {
        var client = CreateMockHttpClient(HttpStatusCode.BadRequest, "");
        using var helper = new OpenApiHelper(BaseUrl, JwtToken);
        typeof(OpenApiHelper)
            .GetField("httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(helper, client);

        Assert.ThrowsAsync<HttpRequestException>(async () => await helper.GetAsync<DummyResponse>("endpoint"));
    }

    [Test]
    public void GetAsync_Throws_OnInvalidJson()
    {
        var client = CreateMockHttpClient(HttpStatusCode.OK, "not-json");
        using var helper = new OpenApiHelper(BaseUrl, JwtToken);
        typeof(OpenApiHelper)
            .GetField("httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(helper, client);

        Assert.ThrowsAsync<JsonReaderException>(async () => await helper.GetAsync<DummyResponse>("endpoint"));
    }

    [TestCaseSource(nameof(HttpVerbs))]
    public async Task VerbAsync_ReturnsDeserializedObject_OnSuccess(
        string method,
        Func<OpenApiHelper, string, object?, CancellationToken, Task<DummyResponse>> call)
    {
        var client = CreateMockHttpClient(HttpStatusCode.OK, SampleResponseJson);
        using var helper = new OpenApiHelper(BaseUrl, JwtToken);
        typeof(OpenApiHelper)
            .GetField("httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(helper, client);

        var result = await call(helper, "endpoint", new { foo = "bar" }, default);

        Assert.That(result.Message, Is.EqualTo("ok"));
        Assert.That(result.Value, Is.EqualTo(42));
    }

    [TestCaseSource(nameof(HttpVerbs))]
    public void VerbAsync_Throws_OnNonSuccessStatus(
        string method,
        Func<OpenApiHelper, string, object?, CancellationToken, Task<DummyResponse>> call)
    {
        var client = CreateMockHttpClient(HttpStatusCode.InternalServerError, "");
        using var helper = new OpenApiHelper(BaseUrl, JwtToken);
        typeof(OpenApiHelper)
            .GetField("httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(helper, client);

        Assert.ThrowsAsync<HttpRequestException>(async () => await call(helper, "endpoint", new { foo = "bar" }, default));
    }

    [TestCaseSource(nameof(HttpVerbs))]
    public void VerbAsync_Throws_OnInvalidJson(
        string method,
        Func<OpenApiHelper, string, object?, CancellationToken, Task<DummyResponse>> call)
    {
        var client = CreateMockHttpClient(HttpStatusCode.OK, "not-json");
        using var helper = new OpenApiHelper(BaseUrl, JwtToken);
        typeof(OpenApiHelper)
            .GetField("httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(helper, client);

        Assert.ThrowsAsync<JsonReaderException>(async () => await call(helper, "endpoint", new { foo = "bar" }, default));
    }

    private static IEnumerable<TestCaseData> HttpVerbs()
    {
        yield return new TestCaseData("Post", (Func<OpenApiHelper, string, object?, CancellationToken, Task<DummyResponse>>)
            ((h, e, d, c) => h.PostAsync<DummyResponse>(e, d!, c)));
        yield return new TestCaseData("Put", (Func<OpenApiHelper, string, object?, CancellationToken, Task<DummyResponse>>)
            ((h, e, d, c) => h.PutAsync<DummyResponse>(e, d!, c)));
        yield return new TestCaseData("Delete", (Func<OpenApiHelper, string, object?, CancellationToken, Task<DummyResponse>>)
            ((h, e, d, c) => h.DeleteAsync<DummyResponse>(e, c)));
        yield return new TestCaseData("Patch", (Func<OpenApiHelper, string, object?, CancellationToken, Task<DummyResponse>>)
            ((h, e, d, c) => h.PatchAsync<DummyResponse>(e, d!, c)));
    }

    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var helper = new OpenApiHelper(BaseUrl, JwtToken);
        helper.Dispose();
        Assert.DoesNotThrow(() => helper.Dispose());
    }
}