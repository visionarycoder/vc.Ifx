using System.Reflection;
using Microsoft.AspNetCore.Http;
using VisionaryCoder.Framework.WebApi.Responses;

namespace VisionaryCoder.Framework.Tests.WebApi;

[TestClass]
public sealed class HttpResponseCatalogTests
{
    public static IEnumerable<object[]> Entries =>
    [
        [100, "Continue", "The request may continue.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.2.1", false],
        [101, "Switching Protocols", "The connection is switching protocols.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.2.2", false],
        [102, "Processing", "The request is being processed.", "https://www.rfc-editor.org/rfc/rfc2518.html#section-10.1", false],
        [103, "Early Hints", "Preliminary response headers are available.", "https://www.rfc-editor.org/rfc/rfc8297.html#section-2", false],
        [200, "OK", "The request succeeded.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.3.1", false],
        [201, "Created", "The resource was created.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.3.2", false],
        [202, "Accepted", "The request was accepted for processing.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.3.3", false],
        [203, "Non-Authoritative Information", "The response contains transformed information.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.3.4", false],
        [204, "No Content", "The request succeeded without response content.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.3.5", false],
        [205, "Reset Content", "The request succeeded; reset the document view.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.3.6", false],
        [206, "Partial Content", "The response contains the requested range.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.3.7", false],
        [207, "Multi-Status", "The response reports multiple operation results.", "https://www.rfc-editor.org/rfc/rfc4918.html#section-11.1", false],
        [208, "Already Reported", "The resource was already reported.", "https://www.rfc-editor.org/rfc/rfc5842.html#section-7.1", false],
        [226, "IM Used", "The response contains instance manipulations.", "https://www.rfc-editor.org/rfc/rfc3229.html#section-10.4.1", false],
        [300, "Multiple Choices", "Multiple representations are available.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.4.1", false],
        [301, "Moved Permanently", "The resource has a permanent new location.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.4.2", false],
        [302, "Found", "The resource is temporarily at another location.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.4.3", false],
        [303, "See Other", "Retrieve the result from another resource.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.4.4", false],
        [304, "Not Modified", "The stored representation is still valid.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.4.5", false],
        [305, "Use Proxy", "This deprecated status is retained for lookup only.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.4.6", false],
        [306, "(Unused)", "This status is reserved and must not be emitted.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.4.7", false],
        [307, "Temporary Redirect", "Repeat the request at the temporary location.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.4.8", false],
        [308, "Permanent Redirect", "Repeat the request at the permanent location.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.4.9", false],
        [400, "Bad Request", "The request is invalid.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.1", false],
        [401, "Unauthorized", "Authentication is required.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.2", false],
        [402, "Payment Required", "Payment is required.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.3", false],
        [403, "Forbidden", "Access to this resource is forbidden.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.4", false],
        [404, "Not Found", "The requested resource was not found.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.5", false],
        [405, "Method Not Allowed", "The request method is not supported for this resource.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.6", false],
        [406, "Not Acceptable", "No acceptable representation is available.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.7", false],
        [407, "Proxy Authentication Required", "Authentication with the proxy is required.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.8", false],
        [408, "Request Timeout", "The request was not received in time.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.9", true],
        [409, "Conflict", "The request conflicts with the current resource state.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.10", false],
        [410, "Gone", "The requested resource is no longer available.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.11", false],
        [411, "Length Required", "The request requires a content length.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.12", false],
        [412, "Precondition Failed", "A request precondition was not satisfied.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.13", false],
        [413, "Content Too Large", "The request content exceeds the accepted size.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.14", false],
        [414, "URI Too Long", "The request URI exceeds the accepted length.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.15", false],
        [415, "Unsupported Media Type", "The request content format is not supported.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.16", false],
        [416, "Range Not Satisfiable", "The requested range cannot be supplied.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.17", false],
        [417, "Expectation Failed", "A request expectation cannot be satisfied.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.18", false],
        [418, "(Unused)", "This status is reserved and must not be emitted.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.19", false],
        [421, "Misdirected Request", "The request was directed to an unsuitable server.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.20", false],
        [422, "Unprocessable Content", "The request content could not be processed.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.21", false],
        [423, "Locked", "The resource is locked.", "https://www.rfc-editor.org/rfc/rfc4918.html#section-11.3", false],
        [424, "Failed Dependency", "A required operation failed.", "https://www.rfc-editor.org/rfc/rfc4918.html#section-11.4", false],
        [425, "Too Early", "The server declined to risk replaying the request.", "https://www.rfc-editor.org/rfc/rfc8470.html#section-5.2", false],
        [426, "Upgrade Required", "The request requires a different protocol.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.22", false],
        [428, "Precondition Required", "The request requires a precondition.", "https://www.rfc-editor.org/rfc/rfc6585.html#section-3", false],
        [429, "Too Many Requests", "The request rate exceeds the current limit.", "https://www.rfc-editor.org/rfc/rfc6585.html#section-4", true],
        [431, "Request Header Fields Too Large", "The request headers exceed the accepted size.", "https://www.rfc-editor.org/rfc/rfc6585.html#section-5", false],
        [451, "Unavailable For Legal Reasons", "The resource is unavailable for legal reasons.", "https://www.rfc-editor.org/rfc/rfc7725.html#section-3", false],
        [500, "Internal Server Error", "An unexpected error prevented processing the request.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.6.1", false],
        [501, "Not Implemented", "The requested functionality is not implemented.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.6.2", false],
        [502, "Bad Gateway", "An upstream service returned an invalid response.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.6.3", true],
        [503, "Service Unavailable", "The service is currently unavailable.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.6.4", true],
        [504, "Gateway Timeout", "An upstream service did not respond in time.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.6.5", true],
        [505, "HTTP Version Not Supported", "The HTTP version is not supported.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.6.6", false],
        [506, "Variant Also Negotiates", "The server could not select a representation.", "https://www.rfc-editor.org/rfc/rfc2295.html#section-8.1", false],
        [507, "Insufficient Storage", "The server cannot store the required representation.", "https://www.rfc-editor.org/rfc/rfc4918.html#section-11.5", false],
        [508, "Loop Detected", "The operation encountered a dependency loop.", "https://www.rfc-editor.org/rfc/rfc5842.html#section-7.2", false],
        [510, "Not Extended", "This historic extension status is retained for lookup only.", "https://www.rfc-editor.org/rfc/rfc2774.html#section-7", false],
        [511, "Network Authentication Required", "Network access requires authentication.", "https://www.rfc-editor.org/rfc/rfc6585.html#section-6", false],
    ];

    [TestMethod]
    [DynamicData(nameof(Entries))]
    public void EveryEntryMatchesThePublishedContract(int code, string title, string detail, string type, bool transient)
    {
        var entry = HttpResponseCatalog.Get(code);
        Assert.AreEqual(code, entry.StatusCode);
        Assert.AreEqual(title, entry.ReasonPhrase);
        Assert.AreEqual(title, entry.DefaultTitle);
        Assert.AreEqual(detail, entry.SafeDetail);
        Assert.AreEqual(type, entry.Type);
        Assert.IsTrue(entry.IsDetailSafeForClients);
        Assert.AreEqual(transient ? HttpRetryability.PotentiallyTransient : HttpRetryability.NeverByDefault, entry.Retryability);
        Assert.AreEqual(code >= 200 && code is not (204 or 205 or 304), entry.AllowsBody);
        Assert.IsTrue(HttpResponseCatalog.TryGet(code, out var found));
        Assert.AreSame(entry, found);
    }

    [TestMethod]
    public void CatalogIsCompleteOrderedUniqueAndImmutable()
    {
        int[] expected = Entries.Select(row => (int)row[0]).ToArray();
        CollectionAssert.AreEqual(expected, HttpResponseCatalog.All.Select(entry => entry.StatusCode).ToArray());
        var aspNetStatuses = typeof(StatusCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(field => (int)field.GetRawConstantValue()!).Distinct().Except([419, 499]);
        foreach (int code in aspNetStatuses)
            Assert.IsTrue(HttpResponseCatalog.TryGet(code, out var found), $"Missing ASP.NET status {code}");
        Assert.AreEqual(expected.Length, expected.Distinct().Count());
        Assert.IsFalse(typeof(HttpResponseDefinition).GetProperties().Any(property => property.SetMethod is not null));
        var list = (IList<HttpResponseDefinition>)HttpResponseCatalog.All;
        Assert.ThrowsExactly<NotSupportedException>(() => list.Add(HttpResponseCatalog.NotFound));
        Assert.ThrowsExactly<NotSupportedException>(() => list[0] = HttpResponseCatalog.NotFound);
        Assert.ThrowsExactly<NotSupportedException>(() => list.Clear());
    }

    [TestMethod]
    public void CommonHelpersReturnTheCanonicalInstances()
    {
        HttpResponseDefinition[] helpers =
        [
            HttpResponseCatalog.Unauthorized, HttpResponseCatalog.Forbidden, HttpResponseCatalog.NotFound,
            HttpResponseCatalog.Conflict, HttpResponseCatalog.UnprocessableContent, HttpResponseCatalog.TooManyRequests,
            HttpResponseCatalog.InternalServerError, HttpResponseCatalog.BadGateway,
            HttpResponseCatalog.ServiceUnavailable, HttpResponseCatalog.GatewayTimeout
        ];
        CollectionAssert.AreEqual(new[] {401,403,404,409,422,429,500,502,503,504}, helpers.Select(x => x.StatusCode).ToArray());
        foreach (var entry in helpers) Assert.AreSame(entry, HttpResponseCatalog.Get(entry.StatusCode));
    }

    [TestMethod]
    public void EveryUnregisteredCodeIsRejected()
    {
        for (int code = -1; code <= 1000; code++)
        {
            if (Entries.Any(entry => (int)entry[0] == code)) continue;
            Assert.IsFalse(HttpResponseCatalog.TryGet(code, out var entry));
            Assert.IsNull(entry);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => HttpResponseCatalog.Get(code));
        }
    }

    [TestMethod]
    public void BodyRulesCoverEveryStatusAndRequestMethod()
    {
        foreach (var entry in HttpResponseCatalog.All)
        foreach (string method in new[] {"GET", "POST", "HEAD", "head", "CONNECT", "connect"})
        {
            bool expected = entry.AllowsBody && !method.Equals("HEAD", StringComparison.OrdinalIgnoreCase)
                && !(method.Equals("CONNECT", StringComparison.OrdinalIgnoreCase) && entry.StatusCode is >= 200 and < 300);
            Assert.AreEqual(expected, HttpResponseCatalog.CanWriteBody(entry.StatusCode, method));
        }
        Assert.ThrowsExactly<ArgumentNullException>(() => HttpResponseCatalog.CanWriteBody(200, null!));
        Assert.ThrowsExactly<ArgumentException>(() => HttpResponseCatalog.CanWriteBody(200, " "));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => HttpResponseCatalog.CanWriteBody(999, "GET"));
    }
}
