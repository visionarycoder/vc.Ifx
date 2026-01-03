namespace VisionaryCoder.Framework.Proxy.Interceptors.Security;

/// <summary>
/// Represents user context information.
/// </summary>
public class UserContext
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    /// Gets or sets the user name.
    public string UserName { get; set; } = string.Empty;
    /// Gets or sets the user roles.
    public ICollection<string> Roles { get; set; } = new List<string>();
    /// Gets or sets the user permissions.
    public ICollection<string> Permissions { get; set; } = new List<string>();
}
