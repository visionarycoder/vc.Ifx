namespace VisionaryCoder.Framework.Storage.Ftp;

/// <summary>Connection settings for a single FTP endpoint. Validation does not connect.</summary>
public sealed class FtpStorageOptions
{
    /// <summary>DNS name or IP address, without scheme, path or credentials.</summary>
    public required string Host { get; init; }
    /// <summary>Explicit endpoint port; defaults to 21, including explicit FTPS.</summary>
    public int Port { get; init; } = 21;
    /// <summary>FTP login name.</summary>
    public required string Username { get; init; }
    /// <summary>FTP password. Obtain secrets from an external secret store.</summary>
    public required string Password { get; init; }
    /// <summary>Require explicit TLS with normal certificate validation.</summary>
    public bool UseSsl { get; init; }
    /// <summary>Use passive rather than active data connections.</summary>
    public bool UsePassive { get; init; } = true;
    /// <summary>Positive timeout for each connection and read, not an overall deadline.</summary>
    public int TimeoutMilliseconds { get; init; } = 30000;
    /// <summary>Enable TCP socket keep-alive.</summary>
    public bool KeepAlive { get; init; }
    /// <summary>Legacy transfer mode. Object operations always use binary.</summary>
    public bool UseBinary { get; init; } = true;
    /// <summary>Positive copy buffer and SDK transfer chunk size in bytes.</summary>
    public int BufferSize { get; init; } = 8192;
    /// <summary>Absolute server directory for object keys only; not a security boundary.</summary>
    public string RootPath { get; init; } = "/";
    /// <summary>Credential-free endpoint URI, including IPv6 bracket notation.</summary>
    public string ServerUri => new UriBuilder(UseSsl ? "ftps" : "ftp", Host, Port).Uri.GetLeftPart(UriPartial.Authority);

    /// <summary>Reject invalid endpoint, credentials, limits and object root.</summary>
    public void Validate()
    {
        ValidateText(Host, nameof(Host));
        if (Uri.CheckHostName(Host) == UriHostNameType.Unknown)
            throw new ArgumentException("Host must be a DNS name or IP address.", nameof(Host));
        ValidateText(Username, nameof(Username));
        ValidateText(Password, nameof(Password));
        ArgumentOutOfRangeException.ThrowIfLessThan(Port, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(Port, 65535);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(TimeoutMilliseconds);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(BufferSize);
        ValidateText(RootPath, nameof(RootPath));
        if (!RootPath.StartsWith('/') || RootPath.Contains('\\') || RootPath.Contains(':'))
            throw new ArgumentException("RootPath must be an absolute FTP directory.", nameof(RootPath));
        ValidateSegments(RootPath[1..].TrimEnd('/'));
    }

    internal static void ValidateText(string value, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, name);
        if (value.Any(char.IsControl)) throw new ArgumentException("Control characters are not allowed.", name);
    }

    internal static void ValidateSegments(string path)
    {
        if (path.Length == 0) return;
        if (path.Split('/').Any(segment => segment is "" or "." or ".."))
            throw new ArgumentException("Empty and traversal path segments are not allowed.", nameof(path));
    }
}
