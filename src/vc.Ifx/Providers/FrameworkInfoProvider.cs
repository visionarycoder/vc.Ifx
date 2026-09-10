using System.Reflection;

namespace VisionaryCoder.Framework.Providers;

/// <summary>
/// Default implementation of <see cref="IFrameworkInfoProvider"/>.
/// </summary>
/// <remarks>
/// Provides build- and assembly-level information about the VisionaryCoder Framework
/// such as the informational version, human-readable name and description, and an
/// approximation of the compilation timestamp. This implementation reads metadata
/// from the executing assembly and falls back to conservative defaults when metadata
/// is not present.
/// </remarks>
public sealed class FrameworkInfoProvider : IFrameworkInfoProvider
{
    /// <inheritdoc />
    /// <remarks>
    /// Attempts to return the assembly <see cref="AssemblyInformationalVersionAttribute"/> value
    /// if available. If not present, falls back to the assembly version and then
    /// to a string literal "0.0.0" as the final fallback.
    /// </remarks>
    public string Version
    {
        get
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                   ?? assembly.GetName().Version?.ToString()
                   ?? "0.0.0";
        }
    }

    /// <summary>
    /// Friendly name of the framework.
    /// </summary>
    public string Name => "VisionaryCoder Framework";

    /// <summary>
    /// Short description of the framework's purpose.
    /// </summary>
    public string Description => "A comprehensive framework for building enterprise-grade applications with proxy interceptor architecture.";
    
    /// <inheritdoc />
    /// <remarks>
    /// The compilation timestamp is derived from the executing assembly file's
    /// creation time. This provides an approximation useful for diagnostics but
    /// is not guaranteed to be the exact build-time in all CI/CD environments.
    /// </remarks>
    public DateTimeOffset CompiledAt { get; } = GetCompilationTime();

    /// <summary>
    /// Attempts to determine an approximate compilation timestamp for the executing assembly.
    /// </summary>
    /// <returns>An approximation of the assembly compilation time based on the assembly file information.</returns>
    private static DateTimeOffset GetCompilationTime()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fileInfo = new FileInfo(assembly.Location);
        return fileInfo.CreationTime;
    }
    
}
