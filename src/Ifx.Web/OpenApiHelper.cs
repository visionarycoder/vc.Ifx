using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;


namespace vc.Ifx.Web;

public class OpenApiHelper : IDisposable
{

    private readonly HttpClient httpClient;
    private bool disposed;

    public OpenApiHelper(string baseUrl, string jwtToken)
    {
        httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
    }

    public async Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(endpoint, cancellationToken);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<T>(responseBody) ?? throw new InvalidOperationException();
    }

    public async Task<T> PostAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default)
    {
        var json = JsonConvert.SerializeObject(data);
        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await httpClient.PostAsync(endpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<T>(responseBody) ?? throw new InvalidOperationException();
    }

    public async Task<T> PutAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default)
    {
        var json = JsonConvert.SerializeObject(data);
        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await httpClient.PutAsync(endpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<T>(responseBody) ?? throw new InvalidOperationException();
    }

    public async Task<T> DeleteAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync(endpoint, cancellationToken);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<T>(responseBody) ?? throw new InvalidOperationException();
    }

    public async Task<T> PatchAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default)
    {
        var json = JsonConvert.SerializeObject(data);
        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint)
        {
            Content = content
        };

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<T>(responseBody) ?? throw new InvalidOperationException();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                httpClient.Dispose();
            }

            // Note: No unmanaged resources to release

            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~OpenApiHelper()
    {
        Dispose(false);
    }
}
