// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework.Proxy.Interceptors.Authentication;

/// <summary>
/// Represents tenant context information for multi-tenant applications.
/// Contains tenant-specific metadata and configuration details.
/// </summary>
public class TenantContext
{
    /// <summary>
    /// Gets or sets the unique tenant identifier.
    /// This should be consistent across all tenant operations.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant's display name.
    /// </summary>
    public string TenantName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant's domain name if applicable.
    /// </summary>
    public string? Domain { get; set; }

    /// <summary>
    /// Gets or sets the tenant's subscription tier or plan.
    /// </summary>
    public string? SubscriptionTier { get; set; }

    /// <summary>
    /// Gets or sets additional tenant-specific configuration settings.
    /// </summary>
    public Dictionary<string, object> Settings { get; set; } = new();

    /// <summary>
    /// Gets or sets the timestamp when the tenant was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets whether the tenant is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets tenant-specific features that are enabled.
    /// </summary>
    public ICollection<string> EnabledFeatures { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets tenant-specific resource limits or quotas.
    /// </summary>
    public Dictionary<string, int> ResourceLimits { get; set; } = new();

    /// <summary>
    /// Gets a value indicating whether the tenant context is valid and active.
    /// </summary>
    public bool IsValid => !string.IsNullOrEmpty(TenantId) && IsActive;

    /// <summary>
    /// Determines if the tenant has a specific feature enabled.
    /// </summary>
    /// <param name="featureName">The feature name to check.</param>
    /// <returns>True if the tenant has the specified feature enabled.</returns>
    public bool HasFeature(string featureName) =>
        EnabledFeatures.Any(f => f.Equals(featureName, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Gets a resource limit value for the tenant.
    /// </summary>
    /// <param name="resourceName">The resource name to check.</param>
    /// <returns>The resource limit, or null if not specified.</returns>
    public int? GetResourceLimit(string resourceName) =>
        ResourceLimits.TryGetValue(resourceName, out int limit) ? limit : null;

    /// <summary>
    /// Gets a tenant setting value.
    /// </summary>
    /// <typeparam name="T">The type of the setting value.</typeparam>
    /// <param name="settingName">The setting name.</param>
    /// <returns>The setting value, or default if not found.</returns>
    public T? GetSetting<T>(string settingName)
    {
        if (string.IsNullOrWhiteSpace(settingName))
        {
            return default;
        }

        if (!Settings.TryGetValue(settingName, out object? value))
        {
            return default;
        }

        if (value is null)
        {
            return default;
        }

        return value is T typedValue
            ? typedValue
            : throw new InvalidCastException($"Setting '{settingName}' is not of type {typeof(T).Name}.");
    }
}
