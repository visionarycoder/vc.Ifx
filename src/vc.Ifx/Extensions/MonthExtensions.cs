using VisionaryCoder.Framework.Models;

namespace VisionaryCoder.Framework.Extensions;
public static class MonthExtensions
{
    /// <summary>
    /// Gets the next month after the current month
    /// </summary>
    public static Month Next(this Month month)
    {
        if(month.Ordinal == 0)
            return Month.Unknown;
        if(month.Ordinal == 12)
            return Month.January;
        return new Month((month.Ordinal + 1) % 13);
    }

    /// <summary>
    /// Gets the previous month before the current month
    /// </summary>
    public static Month Previous(this Month month)
    {
        if (month.Ordinal == 0)
            return Month.Unknown;
        if (month.Ordinal == 1)      
            return Month.December;
        return new Month(month.Ordinal - 1);
    }

    /// <summary>
    /// Determines if the month is in a specific quarter
    /// </summary>
    public static bool IsInQuarter(this Month month, int quarter)
    {
        if (quarter is < 1 or > 4)
            throw new ArgumentOutOfRangeException(nameof(quarter), "Quarter must be between 1 and 4");
        if (month.Ordinal == 0) // UNKNOWN month
            return false;
        int monthQuarter = (month.Ordinal - 1) / 3 + 1;
        return monthQuarter == quarter;
    }

    /// <summary>
    /// Gets the quarter (1-4) for this month
    /// </summary>
    public static int GetQuarter(this Month month)
    {
        if (month.Ordinal == 0)
            return 0;
        return (month.Ordinal - 1) / 3 + 1;
    }

    /// <summary>
    /// Checks if the month is a summer month (June, July, August)
    /// </summary>
    public static bool IsSummerMonth(this Month month)
    {
        return month.Ordinal is >= 6 and <= 8;
    }

    /// <summary>
    /// Converts a DateTime to the corresponding Month
    /// </summary>
    public static Month ToMonth(this DateTime date)
    {
        return new Month(date.Month);
    }
}
