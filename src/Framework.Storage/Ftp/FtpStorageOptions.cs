using FluentFTP;

namespace VisionaryCoder.Framework.Storage.Ftp;

/// <summary>
/// Configuration options for FTP/FTPS storage provider.
/// </summary>
public sealed class FtpStorageOptions
{
    /// <summary>
    /// Gets or sets the FTP server host address.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the FTP server port (default: 21).
    /// </summary>
    public int Port { get; set; } = 21;

    /// <summary>
    /// Gets or sets the username for authentication.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password for authentication.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the root directory path on the FTP server.
    /// All operations will be relative to this directory.
    /// </summary>
    public string RootDirectory { get; set; } = "/";

    /// <summary>
    /// Gets or sets the FTP encryption mode (None, Implicit, Explicit).
    /// </summary>
    public FtpEncryptionMode EncryptionMode { get; set; } = FtpEncryptionMode.None;

    /// <summary>
    /// Gets or sets the data connection type (AutoPassive, AutoActive, PASV, PORT, etc.).
    /// </summary>
    public FtpDataConnectionType DataConnectionType { get; set; } = FtpDataConnectionType.AutoPassive;

    /// <summary>
    /// Gets or sets whether to validate SSL/TLS certificates (default: true).
    /// Set to false only for development/testing with self-signed certificates.
    /// </summary>
    public bool ValidateCertificate { get; set; } = true;

    /// <summary>
    /// Gets or sets the connection timeout in seconds (default: 30).
    /// </summary>
    public int ConnectTimeout { get; set; } = 30;

    /// <summary>
    /// Gets or sets the data transfer timeout in seconds (default: 60).
    /// </summary>
    public int DataConnectionTimeout { get; set; } = 60;

    /// <summary>
    /// Gets or sets the read timeout in seconds (default: 60).
    /// </summary>
    public int ReadTimeout { get; set; } = 60;

    /// <summary>
    /// Gets or sets the maximum number of connection retries (default: 3).
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether to use connection pooling (default: true).
    /// </summary>
    public bool UseConnectionPooling { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to restrict all operations to the root directory (default: true).
    /// Prevents path traversal attacks by ensuring paths don't escape the root.
    /// </summary>
    public bool RestrictToRootDirectory { get; set; } = true;

    /// <summary>
    /// Validates the FTP storage options.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Host))
        {
            throw new InvalidOperationException("FTP host is required.");
        }

        if (Port <= 0 || Port > 65535)
        {
            throw new InvalidOperationException("FTP port must be between 1 and 65535.");
        }

        if (string.IsNullOrWhiteSpace(RootDirectory))
        {
            throw new InvalidOperationException("Root directory cannot be empty.");
        }

        if (ConnectTimeout <= 0)
        {
            throw new InvalidOperationException("Connect timeout must be greater than 0.");
        }

        if (DataConnectionTimeout <= 0)
        {
            throw new InvalidOperationException("Data connection timeout must be greater than 0.");
        }

        if (ReadTimeout <= 0)
        {
            throw new InvalidOperationException("Read timeout must be greater than 0.");
        }

        if (RetryAttempts < 0)
        {
            throw new InvalidOperationException("Retry attempts cannot be negative.");
        }

        // Normalize root directory
        if (!RootDirectory.StartsWith('/'))
        {
            RootDirectory = "/" + RootDirectory;
        }

        RootDirectory = RootDirectory.TrimEnd('/');
        if (string.IsNullOrEmpty(RootDirectory))
        {
            RootDirectory = "/";
        }
    }
}
