namespace VisionaryCoder.Framework.Pipeline.Interceptors.Abstractions;

public interface IAuthorizationService
{
    Task AuthorizeAsync(object request);
}
