namespace VisionaryCoder.Framework;

/// <summary>Typed request marker retaining the non-generic request base.</summary>
/// <typeparam name="T">Application-selected marker type; does not add a payload member.</typeparam>
public class ServiceRequest<T> : ServiceRequest
{

}
