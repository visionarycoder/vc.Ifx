namespace VisionaryCoder.Framework.Proxy.Interceptor.Security;

/// <summary>
/// Represents authenticated user context information including identity, roles, and permissions.
/// </summary>
public class UserContext
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Gets or sets the user roles.
    /// </summary>
    public ICollection<string> Roles { get; set; } = new List<string>();
    
    /// <summary>
    /// Gets or sets the user permissions.
    /// </summary>
    public ICollection<string> Permissions { get; set; } = new List<string>();
    
    /// <summary>
    /// Gets or sets additional claims associated with the user.
    /// </summary>
    public Dictionary<string, object> Claims { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the timestamp when the user was authenticated.
    /// </summary>
    public DateTimeOffset AuthenticatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Gets a value indicating whether the user context is valid.
    /// </summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(UserId) && !string.IsNullOrWhiteSpace(UserName);
}
