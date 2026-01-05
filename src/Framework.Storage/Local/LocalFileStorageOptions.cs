namespace VisionaryCoder.Framework.Storage.Local;

/// <summary>
/// Configuration options for local file system storage operations.
/// </summary>
public sealed class LocalFileStorageOptions
{
    /// <summary>
    /// Gets or sets the root directory for all storage operations.
    /// All paths will be resolved relative to this directory.
    /// </summary>
    public required string RootDirectory { get; init; }

    /// <summary>
    /// Gets or sets whether to create the root directory if it doesn't exist.
    /// </summary>
    public bool CreateRootIfNotExists { get; init; } = true;

    /// <summary>
    /// Gets or sets whether to restrict operations to the root directory.
    /// When true, prevents path traversal attacks by blocking operations outside the root.
    /// </summary>
    public bool RestrictToRootDirectory { get; init; } = true;

    /// <summary>
    /// Gets or sets the buffer size for large file operations.
    /// </summary>
    public int BufferSize { get; init; } = 81920; // 80KB default buffer

    /// <summary>
    /// Gets or sets the file options for file stream operations.
    /// </summary>
    public FileOptions FileOptions { get; init; } = FileOptions.Asynchronous;

    /// <summary>
    /// Validates the configuration and throws exceptions for invalid settings.
    /// </summary>
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(RootDirectory);

        if (BufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(BufferSize), "Buffer size must be greater than 0");
        }

        // Validate root directory path
        if (!Path.IsPathRooted(RootDirectory))
        {
            throw new ArgumentException("Root directory must be an absolute path", nameof(RootDirectory));
        }

        // Check if root exists when CreateRootIfNotExists is false
        if (!CreateRootIfNotExists && !Directory.Exists(RootDirectory))
        {
            throw new DirectoryNotFoundException($"Root directory '{RootDirectory}' does not exist");
        }
    }
}
