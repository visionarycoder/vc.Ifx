namespace VisionaryCoder.Framework.Pipeline.Interceptors.Abstractions;

/// <summary>Application authorization boundary; denial is expressed by an exception.</summary>
public interface IAuthorizationService
{
    /// <summary>Authorizes through the legacy contract.</summary>
    Task AuthorizeAsync(object request);

    /// <summary>Checks cancellation before legacy authorization; override for in-flight cancellation.</summary>
    Task AuthorizeAsync(object request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return AuthorizeAsync(request);
    }
}
