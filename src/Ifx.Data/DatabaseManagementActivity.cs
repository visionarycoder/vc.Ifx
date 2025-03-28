using System.ComponentModel;

namespace vc.Ifx.Data;

/// <summary>
///     Enum for database management activities.
/// </summary>
[Flags]
public enum DatabaseManagementActivity
{
    [Description("No options applied.")] None = 0,

    [Description("Allow overwriting of existing records.")]
    Overwrite = 1 << 0,

    [Description("Force deletion of database entries.")]
    Delete = 1 << 1,

    [Description("Truncate the database table (removes all rows without logging individual row deletions).")]
    Truncate = 1 << 2,

    [Description("Create a backup of the database or table before modification.")]
    CreateBackup = 1 << 3,

    [Description("Log the changes to an audit trail.")]
    LogChanges = 1 << 4,

    [Description("Optimize indexes after the operation.")]
    OptimizeIndexes = 1 << 5,

    [Description("Encrypt the data after operation.")]
    Encrypt = 1 << 6,

    [Description("Decrypt the data before operation.")]
    Decrypt = 1 << 7,

    [Description("Require user confirmation before proceeding.")]
    ConfirmBeforeAction = 1 << 8
}