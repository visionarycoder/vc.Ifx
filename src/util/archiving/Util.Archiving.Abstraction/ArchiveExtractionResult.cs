namespace Util.Archiving;

/// <summary>
/// Represents the result of extracting an archive.
/// </summary>
public sealed record ArchiveExtractionResult(IReadOnlyList<string> ExtractedFiles);
