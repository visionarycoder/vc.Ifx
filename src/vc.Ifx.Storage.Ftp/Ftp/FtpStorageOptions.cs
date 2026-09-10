namespace VisionaryCoder.Framework.Storage.Ftp;

/// <summary>
/// Configuration options for FTP storage operations.
/// </summary>
public sealed class FtpStorageOptions
{
    /// <summary>
    /// Gets or sets the FTP server host address.
    /// </summary>
    public required string Host { get; init; }
    /// Gets or sets the FTP server port. Default is 21 for FTP, 990 for FTPS.
    public int Port { get; init; } = 21;
    /// Gets or sets the username for FTP authentication.
    public required string Username { get; init; }
    /// Gets or sets the password for FTP authentication.
    public required string Password { get; init; }
    /// Gets or sets whether to use SSL/TLS for secure FTP (FTPS).
    public bool UseSsl { get; init; } = false;
    /// Gets or sets whether to use passive mode for FTP connections.
    public bool UsePassive { get; init; } = true;
    /// Gets or sets the timeout for FTP operations in milliseconds.
    public int TimeoutMilliseconds { get; init; } = 30000;
    /// Gets or sets the keep-alive interval for FTP connections.
    public bool KeepAlive { get; init; } = false;
    /// Gets or sets whether to use binary transfer mode.
    public bool UseBinary { get; init; } = true;
    /// Gets or sets the buffer size for file transfers.
    public int BufferSize { get; init; } = 8192;
    /// Gets the FTP server URI based on the configuration.
    public string ServerUri => UseSsl ? $"ftps://{Host}:{Port}" : $"ftp://{Host}:{Port}";

    /// Validates the configuration and throws exceptions for invalid settings.
    public void Validate()
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(nameof(Host));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(nameof(Username));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(nameof(Password));

        if (Port <= 0 || Port > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(Port), "Port must be between 1 and 65535");
        }

        if (TimeoutMilliseconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(TimeoutMilliseconds), "Timeout must be greater than 0");
        }

        if (BufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(BufferSize), "Buffer size must be greater than 0");
        }
    }
}