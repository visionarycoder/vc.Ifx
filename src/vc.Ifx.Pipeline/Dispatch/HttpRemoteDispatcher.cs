using System.Diagnostics;
using System.Text;
using VisionaryCoder.Framework.Pipeline.Abstractions;
using VisionaryCoder.Framework.Pipeline.Dispatch.Abstractions;

namespace VisionaryCoder.Framework.Pipeline.Dispatch;

public sealed class HttpRemoteDispatcher(HttpClient http, ISerializer serializer) : IRemoteDispatcher
{
    public async Task<TResponse> DispatchAsync<TRequest, TResponse>(
        TRequest request, EndpointResolution endpoint)
        where TRequest : IRequest<TResponse>
    {
        string payload = serializer.Serialize(request);
        using var msg = new HttpRequestMessage(HttpMethod.Post, endpoint.Uri)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        // Propagate current Activity context
        Activity? activity = Activity.Current;
        if (activity is not null)
        {
            // System.Net.Http instrumentation will also do this, but explicit is ok
            msg.Headers.TryAddWithoutValidation("traceparent", activity.Id);
            foreach ((string key, string? value) in activity.Baggage)
                msg.Headers.TryAddWithoutValidation($"baggage-{key}", value);
        }

        using HttpResponseMessage resp = await http.SendAsync(msg);
        resp.EnsureSuccessStatusCode();
        string json = await resp.Content.ReadAsStringAsync();
        return serializer.Deserialize<TResponse>(json);
    }
}

// Example generic gRPC client stub
