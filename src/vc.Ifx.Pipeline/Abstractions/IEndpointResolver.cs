namespace VisionaryCoder.Framework.Pipeline.Abstractions;

public interface IEndpointResolver
{
    // Decide local vs. remote routing for a given request type
    EndpointResolution Resolve(Type requestType);
}