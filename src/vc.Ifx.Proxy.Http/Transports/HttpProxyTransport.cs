using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VisionaryCoder.Framework.Proxy.Exceptions;

namespace VisionaryCoder.Framework.Proxy.Transports;

/// <summary>One HTTP attempt per invocation, with caller-owned client and payload sources.</summary>
public sealed class HttpProxyTransport : IProxyTransport
{
    /// <summary>Context.Properties key for a detached IReadOnlyDictionary&lt;string, string[]&gt; response-header snapshot.</summary>
    public const string ResponseHeadersKey = "HttpProxy.ResponseHeaders";
    private readonly HttpClient httpClient;
    private readonly HttpProxyTransportOptions options;
    private readonly JsonSerializerOptions jsonOptions;

    /// <summary>Uses default HTTP transport options without changing or owning the client.</summary>
    public HttpProxyTransport(HttpClient httpClient) : this(httpClient, new HttpProxyTransportOptions()) { }

    /// <summary>Uses explicit transport options. Configure the client's handler pipeline without another retry layer.</summary>
    public HttpProxyTransport(HttpClient httpClient, HttpProxyTransportOptions options)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        options.Validate();
        jsonOptions = new JsonSerializerOptions(options.JsonOptions);
        jsonOptions.MakeReadOnly(populateMissingResolver: true);
    }

    /// <inheritdoc />
    public async Task<ProxyResponse<T>> SendCoreAsync<T>(ProxyContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.Headers);
        ArgumentNullException.ThrowIfNull(context.Properties);
        context.Properties.Remove(ResponseHeadersKey);
        cancellationToken.ThrowIfCancellationRequested();
        context.CancellationToken.ThrowIfCancellationRequested();
        var method = new HttpMethod(context.Method ?? "GET");
        Uri uri = ResolveUri(context.Url);
        bool replaySafe = options.ClassifyRetryableFailures && (method == HttpMethod.Get || method == HttpMethod.Head) && context.Body is null;
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, context.CancellationToken);
        TimeSpan timeout = options.RequestTimeout ?? httpClient.Timeout;
        if (timeout != Timeout.InfiniteTimeSpan) deadline.CancelAfter(timeout);
        CancellationToken token = deadline.Token;
        try
        {
            using var request = new HttpRequestMessage(method, uri);
            request.Content = await CreateContentAsync(context.Body, token).ConfigureAwait(false);
            AddHeaders(request, context.Headers);
            using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            if (!response.IsSuccessStatusCode)
            {
                CaptureHeaders(context, response);
                string message = $"HTTP request failed with status code {(int)response.StatusCode}.";
                if (replaySafe && IsTransient(response.StatusCode))
                    throw new RetryableTransportException(message, new HttpRequestException(message, null, response.StatusCode));
                return new ProxyResponse<T> { IsSuccess = false, StatusCode = (int)response.StatusCode, ErrorMessage = message };
            }
            T? data = default;
            if (method != HttpMethod.Head && response.StatusCode is not (HttpStatusCode.NoContent or HttpStatusCode.ResetContent))
            {
                Stream stream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
                byte[] bytes = await ReadBoundedAsync(stream, options.MaxResponseBodyBytes, token).ConfigureAwait(false);
                if (bytes.Length > 0) data = await DecodeAsync<T>(bytes, response.Content.Headers.ContentType, token).ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            CaptureHeaders(context, response);
            return ProxyResponse<T>.Success(data!, (int)response.StatusCode);
        }
        catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested)
        { throw new OperationCanceledException("HTTP invocation was canceled.", exception, cancellationToken); }
        catch (OperationCanceledException exception) when (context.CancellationToken.IsCancellationRequested)
        { throw new OperationCanceledException("HTTP invocation was canceled.", exception, context.CancellationToken); }
        catch (OperationCanceledException exception) when (deadline.IsCancellationRequested || exception.InnerException is TimeoutException)
        {
            var failure = new TimeoutException("HTTP invocation timed out.", exception);
            if (replaySafe) throw new RetryableTransportException(failure.Message, failure);
            throw failure;
        }
        catch (HttpRequestException exception) when (replaySafe && exception.HttpRequestError is
            HttpRequestError.ConnectionError or HttpRequestError.NameResolutionError or HttpRequestError.ResponseEnded)
        { throw new RetryableTransportException("A replay-safe HTTP invocation failed transiently.", exception); }
        catch (HttpIOException exception) when (replaySafe && exception.HttpRequestError == HttpRequestError.ResponseEnded)
        { throw new RetryableTransportException("A replay-safe HTTP response ended prematurely.", exception); }
    }

    private Uri ResolveUri(string? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (!Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out Uri? uri)) throw new ArgumentException("Invalid HTTP URL.", nameof(value));
        if (!uri.IsAbsoluteUri)
        {
            if (httpClient.BaseAddress is null) throw new ArgumentException("Relative URLs require HttpClient.BaseAddress.", nameof(value));
            uri = new Uri(httpClient.BaseAddress, uri);
        }
        if (uri.Scheme is not ("http" or "https") || uri.UserInfo.Length > 0 || uri.Fragment.Length > 0)
            throw new ArgumentException("Use an HTTP(S) URL without userinfo or fragment.", nameof(value));
        return uri;
    }

    private async Task<HttpContent?> CreateContentAsync(object? body, CancellationToken token)
    {
        if (body is null) return null;
        byte[] bytes;
        string mediaType = "application/json";
        switch (body)
        {
            case HttpContent content:
                bytes = await ReadBoundedAsync(await content.ReadAsStreamAsync(token).ConfigureAwait(false), options.MaxRequestBodyBytes, token).ConfigureAwait(false);
                var headers = content.Headers.Where(header => !header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)).ToArray();
                if (headers.SelectMany(header => header.Value).Any(value => value.Contains('\r') || value.Contains('\n')))
                    throw new ArgumentException("Content headers cannot contain line breaks.", nameof(body));
                var copy = new ByteArrayContent(bytes);
                foreach (var header in headers) copy.Headers.TryAddWithoutValidation(header.Key, header.Value);
                return copy;
            case Stream stream:
                bytes = await ReadBoundedAsync(stream, options.MaxRequestBodyBytes, token).ConfigureAwait(false);
                mediaType = "application/octet-stream";
                break;
            case byte[] data:
                bytes = data.ToArray();
                mediaType = "application/octet-stream";
                break;
            case string text:
                bytes = Encoding.UTF8.GetBytes(text);
                break;
            default:
                bytes = JsonSerializer.SerializeToUtf8Bytes(body, body.GetType(), jsonOptions);
                break;
        }
        if (bytes.Length > options.MaxRequestBodyBytes) throw new InvalidDataException("HTTP request body exceeds the configured limit.");
        var result = new ByteArrayContent(bytes);
        result.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        return result;
    }

    private static void AddHeaders(HttpRequestMessage request, Dictionary<string, string> headers)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in headers)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(header.Key);
            ArgumentNullException.ThrowIfNull(header.Value);
            if (!names.Add(header.Key) || header.Value.Contains('\r') || header.Value.Contains('\n') ||
                header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) || header.Key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Duplicate, injected or manually framed HTTP headers are not supported.", nameof(headers));
            if (request.Headers.TryAddWithoutValidation(header.Key, header.Value)) continue;
            if (request.Content is null) throw new ArgumentException("Content headers require a request body.", nameof(headers));
            if (request.Content.Headers.Any(existing => existing.Key.Equals(header.Key, StringComparison.OrdinalIgnoreCase))) request.Content.Headers.Remove(header.Key);
            request.Content.Headers.Add(header.Key, header.Value);
        }
    }

    private static async Task<byte[]> ReadBoundedAsync(Stream source, int limit, CancellationToken token)
    {
        using var buffer = new MemoryStream();
        byte[] chunk = new byte[Math.Min(limit, 81920)];
        int read;
        while ((read = await source.ReadAsync(chunk, token).ConfigureAwait(false)) > 0)
        {
            if (buffer.Length + read > limit) throw new InvalidDataException("HTTP body exceeds the configured limit.");
            buffer.Write(chunk, 0, read);
        }
        token.ThrowIfCancellationRequested();
        return buffer.ToArray();
    }

    private async Task<T?> DecodeAsync<T>(byte[] bytes, MediaTypeHeaderValue? contentType, CancellationToken token)
    {
        if (typeof(T) == typeof(byte[])) return (T)(object)bytes;
        using var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = contentType;
        string text = await content.ReadAsStringAsync(token).ConfigureAwait(false);
        if (typeof(T) == typeof(string)) return (T)(object)text;
        return JsonSerializer.Deserialize<T>(text, jsonOptions);
    }

    private static void CaptureHeaders(ProxyContext context, HttpResponseMessage response)
    {
        Dictionary<string, string[]> headers = response.Headers.Concat(response.Content.Headers).Concat(response.TrailingHeaders)
            .GroupBy(header => header.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.SelectMany(header => header.Value).ToArray(), StringComparer.OrdinalIgnoreCase);
        context.Properties[ResponseHeadersKey] = new ReadOnlyDictionary<string, string[]>(headers);
    }

    private static bool IsTransient(HttpStatusCode status) => (int)status is 408 or 429 or 500 or 502 or 503 or 504;
}
