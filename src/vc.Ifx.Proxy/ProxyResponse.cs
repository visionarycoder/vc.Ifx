namespace VisionaryCoder.Framework.Proxy;

/// <summary>
/// Represents a response from a proxy operation.
/// </summary>
/// <typeparam name="T">The type of the response data.</typeparam>
public class ProxyResponse<T>
{
    /// <summary>
    /// Gets or sets the response data.
    /// </summary>
    public T? Data { get; set; }
    /// Gets or sets a value indicating whether the operation was successful.
    public bool IsSuccess { get; set; }
    /// Gets or sets the error message if the operation failed.
    public string? ErrorMessage { get; set; }
    /// Gets or sets the status code.
    public int? StatusCode { get; set; }

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    /// <param name="data">The response data.</param>
    /// <returns>A successful response.</returns>
    public static ProxyResponse<T> Success(T data)
    {
        return new ProxyResponse<T> { Data = data, IsSuccess = true };
    }

    /// <summary>
    /// Creates a successful response with status code.
    /// </summary>
    /// <param name="data">The response data.</param>
    /// <param name="statusCode">The status code.</param>
    /// <returns>A successful response.</returns>
    public static ProxyResponse<T> Success(T data, int statusCode)
    {
        return new ProxyResponse<T> { Data = data, IsSuccess = true, StatusCode = statusCode };
    }

    /// <summary>
    /// Creates a failed response.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>A failed response.</returns>
    public static ProxyResponse<T> Failure(string errorMessage)
    {
        return new ProxyResponse<T> { IsSuccess = false, ErrorMessage = errorMessage };
    }
}
