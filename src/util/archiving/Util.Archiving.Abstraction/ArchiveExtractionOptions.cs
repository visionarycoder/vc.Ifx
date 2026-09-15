namespace Util.Archiving;

/// <summary>
/// Configures archive extraction behavior.
/// </summary>
public sealed record ArchiveExtractionOptions
{
    /// <summary>
    /// Gets the default extraction options.
    /// </summary>
    public static ArchiveExtractionOptions Default { get; } = new();

    /// <summary>
    /// Gets a value indicating whether existing files may be overwritten.
    /// </summary>
    public bool OverwriteFiles { get; init; }
}
