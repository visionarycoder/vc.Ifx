namespace vc.Ifx.Analyzers.Models;

/// <summary>
/// Defines the volatility level of different project types
/// Higher numbers = more volatile (changes more frequently)
/// </summary>
public enum VolatilityLevel
{
    /// <summary>Very stable - rarely changes (e.g., Ifx infrastructure)</summary>
    VeryStable = 0,

    /// <summary>Stable - changes infrequently (e.g., Contracts)</summary>
    Stable = 1,

    /// <summary>Moderate - changes occasionally (e.g., ORM models)</summary>
    Moderate = 2,

    /// <summary>Volatile - changes frequently (e.g., Services)</summary>
    Volatile = 3,

    /// <summary>Very volatile - changes very frequently (e.g., Web APIs, UI)</summary>
    VeryVolatile = 4
}
