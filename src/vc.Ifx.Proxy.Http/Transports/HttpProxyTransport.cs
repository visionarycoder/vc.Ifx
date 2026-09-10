using System.Text.Json;

namespace VisionaryCoder.Framework.Proxy.Transports;
/// <summary>
/// Example HTTP transport implementation.
/// </summary>
/// <param name="httpClient">The HTTP client to use for transport.</param>
internal sealed class HttpProxyTransport(HttpClient httpClient) : IProxyTransport
{
    private readonly HttpClient httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    /// <summary>
    /// Sends an HTTP request and returns a typed response.
    /// </summary>
    /// <typeparam name="T">The expected response type.</typeparam>
    /// <param name="context">The proxy context.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task representing the HTTP response.</returns>
    public async Task<ProxyResponse<T>> SendCoreAsync<T>(ProxyContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(new HttpMethod(context.Method ?? "GET"), context.Url);
            
            // Add headers from context
            foreach (KeyValuePair<string, string> header in context.Headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                T? data = JsonSerializer.Deserialize<T>(content);
                return ProxyResponse<T>.Success(data!, (int)response.StatusCode);
            }
            else
            {
                return ProxyResponse<T>.Failure($"HTTP {response.StatusCode}: {content}");
            }
        }
        catch (Exception ex)
        {
            return ProxyResponse<T>.Failure($"Transport error: {ex.Message}");
        }
    }
}
