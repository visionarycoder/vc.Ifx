namespace VisionaryCoder.Framework.Storage.Local;

/// <summary>Configures the root used by object storage operations.</summary>
/// <remarks>The legacy file/directory API does not use this root. This is not a security sandbox.</remarks>
public class LocalStorageOptions
{
    /// <summary>Gets or sets an absolute root, defaulting to the current directory when options are created.</summary>
    public string RootPath { get; set; } = Directory.GetCurrentDirectory();

    /// <summary>Validates root syntax without accessing or creating the directory.</summary>
    /// <exception cref="ArgumentNullException">The root is null.</exception>
    /// <exception cref="ArgumentException">The root is blank, invalid, or not fully qualified.</exception>
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(RootPath);
        if (!Path.IsPathFullyQualified(RootPath))
        {
            throw new ArgumentException("The object storage root must be an absolute path.", nameof(RootPath));
        }

        _ = Path.GetFullPath(RootPath);
    }
}
