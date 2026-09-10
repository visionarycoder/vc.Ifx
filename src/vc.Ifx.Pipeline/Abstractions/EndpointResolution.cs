namespace VisionaryCoder.Framework.Pipeline.Abstractions;

public record EndpointResolution(bool IsLocal, string? ServiceName = null, Uri? Uri = null);
