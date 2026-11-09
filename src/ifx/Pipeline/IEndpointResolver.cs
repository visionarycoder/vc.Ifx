namespace Ifx.Pipeline;

public interface IEndpointResolver
{
    // Decide local vs. remote routing for a given request type
    EndpointResolution Resolve(Type requestType);
}