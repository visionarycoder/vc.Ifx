namespace VisionaryCoder.Framework.WebApi.Responses;

/// <summary>Describes status transience without asserting that an operation is safe to replay.</summary>
public enum HttpRetryability
{
    /// <summary>No generic retry is recommended.</summary>
    NeverByDefault,
    /// <summary>A transient failure is possible; replay still requires application policy.</summary>
    PotentiallyTransient
}

/// <summary>Immutable metadata for a registered HTTP status.</summary>
public sealed class HttpResponseDefinition
{
    internal HttpResponseDefinition(int statusCode, string reasonPhrase, string safeDetail, string type)
    {
        StatusCode = statusCode;
        ReasonPhrase = reasonPhrase;
        SafeDetail = safeDetail;
        Type = type;
    }

    /// <summary>Gets the numeric status.</summary>
    public int StatusCode { get; }
    /// <summary>Gets the registered reason phrase.</summary>
    public string ReasonPhrase { get; }
    /// <summary>Gets the default problem title.</summary>
    public string DefaultTitle => ReasonPhrase;
    /// <summary>Gets a client-safe description without diagnostic data.</summary>
    public string SafeDetail { get; }
    /// <summary>Gets the absolute RFC reference URI.</summary>
    public string Type { get; }
    /// <summary>Gets the default transience classification, never a replay-safety guarantee.</summary>
    public HttpRetryability Retryability => StatusCode is 408 or 429 or 502 or 503 or 504
        ? HttpRetryability.PotentiallyTransient : HttpRetryability.NeverByDefault;
    /// <summary>Gets whether the catalog detail is safe to expose.</summary>
    public bool IsDetailSafeForClients => true;
    /// <summary>Gets whether this status permits content, independent of request method.</summary>
    public bool AllowsBody => StatusCode >= 200 && StatusCode is not (204 or 205 or 304);
}
