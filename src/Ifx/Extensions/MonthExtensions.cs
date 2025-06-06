namespace vc.Ifx.Extensions;

public static class MonthExtensions
{

    /// <summary>
    /// Gets the next month after the current month
    /// </summary>
    public static Month Next(this Month month)
    {
        return new Month((month.Order + 1) % 13);
    }

    /// <summary>
    /// Gets the previous month before the current month
    /// </summary>
    public static Month Previous(this Month month)
    {
        return new Month(month.Order == 0 ? 12 : month.Order - 1);
    }

    /// <summary>
    /// Determines if the month is in a specific quarter
    /// </summary>
    public static bool IsInQuarter(this Month month, int quarter)
    {
        if (quarter < 1 || quarter > 4)
            throw new ArgumentOutOfRangeException(nameof(quarter), "Quarter must be between 1 and 4");

        if (month.Order == 0) // UNKNOWN month
            return false;

        var monthQuarter = (month.Order - 1) / 3 + 1;
        return monthQuarter == quarter;
    }

    /// <summary>
    /// Gets the quarter (1-4) for this month
    /// </summary>
    public static int GetQuarter(this Month month)
    {
        if (month.Order == 0) // UNKNOWN month
            return 0;

        return (month.Order - 1) / 3 + 1;
    }

    /// <summary>
    /// Checks if the month is a summer month (June, July, August)
    /// </summary>
    public static bool IsSummerMonth(this Month month)
    {
        return month.Order >= 6 && month.Order <= 8;
    }

    /// <summary>
    /// Converts a DateTime to the corresponding Month
    /// </summary>
    public static Month ToMonth(this DateTime date)
    {
        return new Month(date.Month);
    }

}