// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Messaging.Azure.Queue;

/// <summary>
/// Configuration options for Azure Queue Storage provider.
/// </summary>
public sealed class AzureQueueStorageOptions
{
    /// <summary>
    /// Gets or sets the Azure Storage account connection string.
    /// Required if UseManagedIdentity is false.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the storage account URI.
    /// Required if UseManagedIdentity is true.
    /// </summary>
    public string? StorageAccountUri { get; set; }

    /// <summary>
    /// Gets or sets whether to use managed identity for authentication.
    /// Default is false (uses connection string).
    /// </summary>
    public bool UseManagedIdentity { get; set; }

    /// <summary>
    /// Gets or sets the queue name.
    /// </summary>
    public string QueueName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to create the queue if it doesn't exist.
    /// Default is true.
    /// </summary>
    public bool CreateQueueIfNotExists { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to encode messages as Base64.
    /// Default is true for compatibility.
    /// </summary>
    public bool EncodeMessages { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of messages to retrieve in a single operation.
    /// Default is 32 (Azure Queue Storage maximum).
    /// </summary>
    public int MaxMessagesToRetrieve { get; set; } = 32;

    /// <summary>
    /// Gets or sets the visibility timeout in seconds for received messages.
    /// Default is 30 seconds.
    /// </summary>
    public int VisibilityTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the message time-to-live in seconds.
    /// Use -1 for unlimited (default).
    /// </summary>
    public int MessageTimeToLiveSeconds { get; set; } = -1;

    /// <summary>
    /// Validates the configuration options.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(QueueName))
        {
            throw new InvalidOperationException("QueueName is required.");
        }

        if (UseManagedIdentity)
        {
            if (string.IsNullOrWhiteSpace(StorageAccountUri))
            {
                throw new InvalidOperationException("StorageAccountUri is required when UseManagedIdentity is true.");
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new InvalidOperationException("ConnectionString is required when UseManagedIdentity is false.");
            }
        }
    }
}
