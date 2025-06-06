#pragma warning disable CA1708
namespace vc.Ifx.Enums;

/// <summary>
/// Specifies the file overwrite options.
/// This enum is used to determine whether an existing file should be overwritten.
/// </summary>
public enum OverwriteFile
{
    /// <summary>
    /// The default value. No specific action is defined.
    /// </summary>
    Undefined = -1,

    /// <summary>
    /// Do not overwrite the existing file.
    /// </summary>
    No = 0,

    /// <summary>
    /// Overwrite the existing file.
    /// </summary>
    Yes = 1
}
