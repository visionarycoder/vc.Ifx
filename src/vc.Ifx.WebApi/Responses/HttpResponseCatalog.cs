using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace VisionaryCoder.Framework.WebApi.Responses;

/// <summary>Immutable standard HTTP status metadata. Reserved entries do not authorize emission.</summary>
public static class HttpResponseCatalog
{
    private static readonly FrozenDictionary<int, HttpResponseDefinition> entries = new HttpResponseDefinition[]
    {
        new(100, "Continue", "The request may continue.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.2.1"),
        new(101, "Switching Protocols", "The connection is switching protocols.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.2.2"),
        new(102, "Processing", "The request is being processed.", "https://www.rfc-editor.org/rfc/rfc2518.html#section-10.1"),
        new(103, "Early Hints", "Preliminary response headers are available.", "https://www.rfc-editor.org/rfc/rfc8297.html#section-2"),
        new(200, "OK", "The request succeeded.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.3.1"),
        new(201, "Created", "The resource was created.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.3.2"),
        new(202, "Accepted", "The request was accepted for processing.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.3.3"),
        new(203, "Non-Authoritative Information", "The response contains transformed information.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.3.4"),
        new(204, "No Content", "The request succeeded without response content.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.3.5"),
        new(205, "Reset Content", "The request succeeded; reset the document view.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.3.6"),
        new(206, "Partial Content", "The response contains the requested range.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.3.7"),
        new(207, "Multi-Status", "The response reports multiple operation results.", "https://www.rfc-editor.org/rfc/rfc4918.html#section-11.1"),
        new(208, "Already Reported", "The resource was already reported.", "https://www.rfc-editor.org/rfc/rfc5842.html#section-7.1"),
        new(226, "IM Used", "The response contains instance manipulations.", "https://www.rfc-editor.org/rfc/rfc3229.html#section-10.4.1"),
        new(300, "Multiple Choices", "Multiple representations are available.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.4.1"),
        new(301, "Moved Permanently", "The resource has a permanent new location.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.4.2"),
        new(302, "Found", "The resource is temporarily at another location.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.4.3"),
        new(303, "See Other", "Retrieve the result from another resource.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.4.4"),
        new(304, "Not Modified", "The stored representation is still valid.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.4.5"),
        new(305, "Use Proxy", "This deprecated status is retained for lookup only.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.4.6"),
        new(306, "(Unused)", "This status is reserved and must not be emitted.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.4.7"),
        new(307, "Temporary Redirect", "Repeat the request at the temporary location.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.4.8"),
        new(308, "Permanent Redirect", "Repeat the request at the permanent location.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.4.9"),
        new(400, "Bad Request", "The request is invalid.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.1"),
        new(401, "Unauthorized", "Authentication is required.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.2"),
        new(402, "Payment Required", "Payment is required.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.3"),
        new(403, "Forbidden", "Access to this resource is forbidden.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.4"),
        new(404, "Not Found", "The requested resource was not found.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.5"),
        new(405, "Method Not Allowed", "The request method is not supported for this resource.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.6"),
        new(406, "Not Acceptable", "No acceptable representation is available.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.7"),
        new(407, "Proxy Authentication Required", "Authentication with the proxy is required.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.8"),
        new(408, "Request Timeout", "The request was not received in time.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.9"),
        new(409, "Conflict", "The request conflicts with the current resource state.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.10"),
        new(410, "Gone", "The requested resource is no longer available.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.11"),
        new(411, "Length Required", "The request requires a content length.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.12"),
        new(412, "Precondition Failed", "A request precondition was not satisfied.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.13"),
        new(413, "Content Too Large", "The request content exceeds the accepted size.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.14"),
        new(414, "URI Too Long", "The request URI exceeds the accepted length.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.15"),
        new(415, "Unsupported Media Type", "The request content format is not supported.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.16"),
        new(416, "Range Not Satisfiable", "The requested range cannot be supplied.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.17"),
        new(417, "Expectation Failed", "A request expectation cannot be satisfied.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.18"),
        new(418, "(Unused)", "This status is reserved and must not be emitted.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.19"),
        new(421, "Misdirected Request", "The request was directed to an unsuitable server.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.20"),
        new(422, "Unprocessable Content", "The request content could not be processed.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.21"),
        new(423, "Locked", "The resource is locked.", "https://www.rfc-editor.org/rfc/rfc4918.html#section-11.3"),
        new(424, "Failed Dependency", "A required operation failed.", "https://www.rfc-editor.org/rfc/rfc4918.html#section-11.4"),
        new(425, "Too Early", "The server declined to risk replaying the request.", "https://www.rfc-editor.org/rfc/rfc8470.html#section-5.2"),
        new(426, "Upgrade Required", "The request requires a different protocol.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.22"),
        new(428, "Precondition Required", "The request requires a precondition.", "https://www.rfc-editor.org/rfc/rfc6585.html#section-3"),
        new(429, "Too Many Requests", "The request rate exceeds the current limit.", "https://www.rfc-editor.org/rfc/rfc6585.html#section-4"),
        new(431, "Request Header Fields Too Large", "The request headers exceed the accepted size.", "https://www.rfc-editor.org/rfc/rfc6585.html#section-5"),
        new(451, "Unavailable For Legal Reasons", "The resource is unavailable for legal reasons.", "https://www.rfc-editor.org/rfc/rfc7725.html#section-3"),
        new(500, "Internal Server Error", "An unexpected error prevented processing the request.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.6.1"),
        new(501, "Not Implemented", "The requested functionality is not implemented.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.6.2"),
        new(502, "Bad Gateway", "An upstream service returned an invalid response.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.6.3"),
        new(503, "Service Unavailable", "The service is currently unavailable.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.6.4"),
        new(504, "Gateway Timeout", "An upstream service did not respond in time.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.6.5"),
        new(505, "HTTP Version Not Supported", "The HTTP version is not supported.", "https://www.rfc-editor.org/rfc/rfc9110.html#section-15.6.6"),
        new(506, "Variant Also Negotiates", "The server could not select a representation.", "https://www.rfc-editor.org/rfc/rfc2295.html#section-8.1"),
        new(507, "Insufficient Storage", "The server cannot store the required representation.", "https://www.rfc-editor.org/rfc/rfc4918.html#section-11.5"),
        new(508, "Loop Detected", "The operation encountered a dependency loop.", "https://www.rfc-editor.org/rfc/rfc5842.html#section-7.2"),
        new(510, "Not Extended", "This historic extension status is retained for lookup only.", "https://www.rfc-editor.org/rfc/rfc2774.html#section-7"),
        new(511, "Network Authentication Required", "Network access requires authentication.", "https://www.rfc-editor.org/rfc/rfc6585.html#section-6"),
    }.ToFrozenDictionary(entry => entry.StatusCode);

    /// <summary>Gets all entries in ascending status order.</summary>
    public static IReadOnlyList<HttpResponseDefinition> All { get; } =
        Array.AsReadOnly(entries.Values.OrderBy(entry => entry.StatusCode).ToArray());

    /// <summary>Gets a known status or throws for unregistered/excluded codes.</summary>
    public static HttpResponseDefinition Get(int statusCode) =>
        TryGet(statusCode, out var entry) ? entry : throw new ArgumentOutOfRangeException(nameof(statusCode), statusCode, "Unknown catalog status.");

    /// <summary>Looks up a known status without exposing mutable storage.</summary>
    public static bool TryGet(int statusCode, [NotNullWhen(true)] out HttpResponseDefinition? entry) =>
        entries.TryGetValue(statusCode, out entry);

    /// <summary>Determines content permission for a known status and request method.</summary>
    public static bool CanWriteBody(int statusCode, string method)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(method);
        return Get(statusCode).AllowsBody && !HttpMethods.IsHead(method)
            && !(HttpMethods.IsConnect(method) && statusCode < 300);
    }

    internal static HttpResponseDefinition GetError(int statusCode)
    {
        var entry = Get(statusCode);
        if (statusCode < 400 || statusCode is 418 or 510)
        {
            throw new ArgumentOutOfRangeException(nameof(statusCode), "Problem details require an active error status.");
        }

        return entry;
    }

    /// <summary>Gets standard 401 metadata.</summary>
    public static HttpResponseDefinition Unauthorized => Get(401);
    /// <summary>Gets standard 403 metadata.</summary>
    public static HttpResponseDefinition Forbidden => Get(403);
    /// <summary>Gets standard 404 metadata.</summary>
    public static HttpResponseDefinition NotFound => Get(404);
    /// <summary>Gets standard 409 metadata.</summary>
    public static HttpResponseDefinition Conflict => Get(409);
    /// <summary>Gets standard 422 metadata.</summary>
    public static HttpResponseDefinition UnprocessableContent => Get(422);
    /// <summary>Gets standard 429 metadata.</summary>
    public static HttpResponseDefinition TooManyRequests => Get(429);
    /// <summary>Gets standard 500 metadata.</summary>
    public static HttpResponseDefinition InternalServerError => Get(500);
    /// <summary>Gets standard 502 metadata.</summary>
    public static HttpResponseDefinition BadGateway => Get(502);
    /// <summary>Gets standard 503 metadata.</summary>
    public static HttpResponseDefinition ServiceUnavailable => Get(503);
    /// <summary>Gets standard 504 metadata.</summary>
    public static HttpResponseDefinition GatewayTimeout => Get(504);
}
