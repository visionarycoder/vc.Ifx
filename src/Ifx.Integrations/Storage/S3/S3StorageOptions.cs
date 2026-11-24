namespace Ifx.Storage.S3;

/// <summary>Defines the bucket and object-key namespace for an S3 provider.</summary>
public sealed class S3StorageOptions
{
    /// <summary>Gets or sets the bucket name.</summary>
    public required string BucketName { get; init; }

    /// <summary>Gets or sets an optional object-key prefix.</summary>
    public string KeyPrefix { get; init; } = "";

    /// <summary>Validates the bucket and key prefix.</summary>
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(BucketName);
        if (BucketName.Any(char.IsControl))
        {
            throw new ArgumentException("The bucket name cannot contain control characters.", nameof(BucketName));
        }

        if (KeyPrefix.StartsWith('/') || KeyPrefix.Contains('\\') || KeyPrefix.Any(char.IsControl) ||
            KeyPrefix.Split('/', StringSplitOptions.RemoveEmptyEntries).Any(segment => segment is "." or ".."))
        {
            throw new ArgumentException("The key prefix must be a relative S3 object-key prefix.", nameof(KeyPrefix));
        }
    }

    internal string NormalizedPrefix => KeyPrefix.Length == 0 ? "" : KeyPrefix.TrimEnd('/') + "/";
}
