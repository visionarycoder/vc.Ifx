namespace VisionaryCoder.Framework.Extensions;

/// <summary>
/// Provides extension methods for <see cref="DateTime"/>.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Gets the next occurrence of the specified weekday from the given date.
    /// </summary>
    /// <param name="input">The original date and time.</param>
    /// <param name="dayOfWeek">The day of the week to get. Default is <see cref="DayOfWeek.Sunday"/>.</param>
    /// <returns>The date of the next occurrence of the specified weekday.</returns>
    public static DateTime GetProceedingWeekday(this DateTime input, DayOfWeek dayOfWeek = DayOfWeek.Sunday)
    {
        int offset = ((int)dayOfWeek - (int)input.DayOfWeek + 7) % 7;
        offset = offset == 0 ? 7 : offset; // Ensure it always returns a future date
        return input.AddDays(offset).Date;
    }

    /// <summary>
    /// Gets the previous occurrence of the specified weekday from the given date.
    /// </summary>
    /// <param name="input">The original date and time.</param>
    /// <param name="dayOfWeek">The day of the week to get.</param>
    /// <returns>The date of the previous occurrence of the specified weekday.</returns>
    public static DateTime GetPreviousWeekday(this DateTime input, DayOfWeek dayOfWeek)
    {
        int offset = ((int)input.DayOfWeek - (int)dayOfWeek + 7) % 7;
        offset = offset == 0 ? 7 : offset; // Ensure it always returns a past date
        return input.AddDays(-offset).Date;
    }

    /// <summary>
    /// Gets the date part of the <see cref="DateTime"/> after applying the specified offset.
    /// </summary>
    /// <param name="dateTime">The original date and time.</param>
    /// <param name="offset">The time span to offset the date and time.</param>
    /// <param name="shiftDate">Specifies whether the offset should be added or subtracted. Default is <see cref="ShiftDate.ToPast"/>.</param>
    /// <returns>The date part of the <see cref="DateTime"/> after applying the offset.</returns>
    public static DateTime GetDateOnly(this DateTime dateTime, TimeSpan offset, ShiftDate shiftDate = ShiftDate.ToPast)
    {
        DateTime offsetDateTime = shiftDate == ShiftDate.ToPast
            ? dateTime.Subtract(offset)
            : dateTime.Add(offset);
        return offsetDateTime.Date;
    }

    /// <summary>
    /// Determines whether the given date is a weekend.
    /// </summary>
    /// <param name="dateTime">The date to check.</param>
    /// <returns>True if the date is a weekend; otherwise, false.</returns>
    public static bool IsWeekend(this DateTime dateTime)
    {
        return dateTime.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
    }

    /// <summary>
    /// Determines whether the given date is a weekday.
    /// </summary>
    /// <param name="dateTime">The date to check.</param>
    /// <returns>True if the date is a weekday; otherwise, false.</returns>
    public static bool IsWeekday(this DateTime dateTime)
    {
        return !dateTime.IsWeekend();
    }

    /// <summary>
    /// Gets the start of the week for the given date.
    /// </summary>
    /// <param name="dateTime">The date to calculate from.</param>
    /// <param name="startOfWeek">The day considered as the start of the week. Default is <see cref="DayOfWeek.Monday"/>.</param>
    /// <returns>The date of the start of the week.</returns>
    public static DateTime GetStartOfWeek(this DateTime dateTime, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        int diff = (7 + (dateTime.DayOfWeek - startOfWeek)) % 7;
        return dateTime.AddDays(-diff).Date;
    }

    /// <summary>
    /// Specifies whether a date is in the past or in the future.
    /// </summary>
    public enum ShiftDate
    {
        /// <summary>
        /// Indicates that the date is in the future.
        /// </summary>
        ToFuture,
        /// <summary>
        /// Indicates that the date is in the past.
        /// </summary>
        ToPast
    }
}
