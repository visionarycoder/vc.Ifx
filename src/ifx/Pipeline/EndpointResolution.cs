namespace Ifx.Pipeline;

public record EndpointResolution(bool IsLocal, string? ServiceName = null, Uri? Uri = null);
