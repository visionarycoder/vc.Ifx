// Copyright (c) VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace VisionaryCoder.Framework.Storage.Sftp;

/// <summary>
/// Configuration options for SFTP storage provider.
/// </summary>
public sealed class SftpStorageOptions
{
    /// <summary>
    /// Gets or sets the SFTP server host address.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SFTP server port. Default is 22.
    /// </summary>
    public int Port { get; set; } = 22;

    /// <summary>
    /// Gets or sets the username for authentication.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for password-based authentication.
    /// Leave empty when using key-based authentication.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the path to the private key file for key-based authentication.
    /// Leave empty when using password-based authentication.
    /// </summary>
    public string? PrivateKeyPath { get; set; }

    /// <summary>
    /// Gets or sets the passphrase for encrypted private key files.
    /// Optional - only needed if the private key is encrypted.
    /// </summary>
    public string? PrivateKeyPassphrase { get; set; }

    /// <summary>
    /// Gets or sets the root directory on the SFTP server. Default is "/".
    /// All file operations will be relative to this directory.
    /// </summary>
    public string RootDirectory { get; set; } = "/";

    /// <summary>
    /// Gets or sets the connection timeout in seconds. Default is 30 seconds.
    /// </summary>
    public int ConnectTimeout { get; set; } = 30;

    /// <summary>
    /// Gets or sets the operation timeout in seconds. Default is 60 seconds.
    /// This applies to individual file operations like upload/download.
    /// </summary>
    public int OperationTimeout { get; set; } = 60;

    /// <summary>
    /// Gets or sets the keep-alive interval in seconds. Default is 10 seconds.
    /// Set to 0 to disable keep-alive packets.
    /// </summary>
    public int KeepAliveInterval { get; set; } = 10;

    /// <summary>
    /// Gets or sets the maximum number of connection retry attempts. Default is 3.
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether to use connection pooling. Default is true.
    /// When enabled, SFTP connections are reused for better performance.
    /// </summary>
    public bool UseConnectionPooling { get; set; } = true;

    /// <summary>
    /// Gets or sets the buffer size for file operations in bytes. Default is 32KB.
    /// </summary>
    public int BufferSize { get; set; } = 32 * 1024;

    /// <summary>
    /// Gets or sets whether file operations should be restricted to the root directory.
    /// When true, prevents path traversal attacks. Default is true.
    /// </summary>
    public bool RestrictToRootDirectory { get; set; } = true;

    /// <summary>
    /// Validates the SFTP storage options.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when options are invalid.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Host))
        {
            throw new ArgumentException("Host cannot be null or empty.", nameof(Host));
        }

        if (Port is < 1 or > 65535)
        {
            throw new ArgumentException("Port must be between 1 and 65535.", nameof(Port));
        }

        if (string.IsNullOrWhiteSpace(Username))
        {
            throw new ArgumentException("Username cannot be null or empty.", nameof(Username));
        }

        // Either password or private key must be provided
        if (string.IsNullOrWhiteSpace(Password) && string.IsNullOrWhiteSpace(PrivateKeyPath))
        {
            throw new ArgumentException("Either Password or PrivateKeyPath must be provided for authentication.", nameof(Password));
        }

        if (ConnectTimeout <= 0)
        {
            throw new ArgumentException("ConnectTimeout must be greater than 0.", nameof(ConnectTimeout));
        }

        if (OperationTimeout <= 0)
        {
            throw new ArgumentException("OperationTimeout must be greater than 0.", nameof(OperationTimeout));
        }

        if (KeepAliveInterval < 0)
        {
            throw new ArgumentException("KeepAliveInterval cannot be negative.", nameof(KeepAliveInterval));
        }

        if (RetryAttempts < 0)
        {
            throw new ArgumentException("RetryAttempts cannot be negative.", nameof(RetryAttempts));
        }

        if (BufferSize <= 0)
        {
            throw new ArgumentException("BufferSize must be greater than 0.", nameof(BufferSize));
        }

        // Normalize root directory
        if (string.IsNullOrWhiteSpace(RootDirectory))
        {
            RootDirectory = "/";
        }
        else
        {
            // Ensure root directory starts with /
            if (!RootDirectory.StartsWith('/'))
            {
                RootDirectory = "/" + RootDirectory;
            }

            // Remove trailing slash unless it's the root
            if (RootDirectory.Length > 1 && RootDirectory.EndsWith('/'))
            {
                RootDirectory = RootDirectory.TrimEnd('/');
            }
        }
    }
}
