#pragma warning disable ClassMethodMissingInterface
#pragma warning disable DerivedClasses
using vc.Ifx.Options;

namespace vc.Ifx.Extensions;

/// <summary>
/// Provides extension methods for <see cref="DateTime"/>.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Gets the proceeding weekday from the specified date.
    /// </summary>
    /// <param name="input">The original date and time.</param>
    /// <param name="dayOfWeek">The day of the week to get. Default is <see cref="DayOfWeek.Sunday"/>.</param>
    /// <returns>The date of the proceeding weekday.</returns>
    public static DateTime GetProceedingWeekday(this DateTime input, DayOfWeek dayOfWeek = DayOfWeek.Sunday)
    {
        var offset = (int)dayOfWeek - (int)input.DayOfWeek;
        var output = input.AddDays(offset).Date;
        return output;
    }

    /// <summary>
    /// Gets the date part of the <see cref="DateTime"/> after applying the specified offset.
    /// </summary>
    /// <param name="dateTime">The original date and time.</param>
    /// <param name="offset">The time span to offset the date and time.</param>
    /// <param name="dateIsOption">Specifies whether the offset should be added or subtracted. Default is <see cref="DateIsOption.InThePast"/>.</param>
    /// <returns>The date part of the <see cref="DateTime"/> after applying the offset.</returns>
    public static DateTime GetDateOnly(this DateTime dateTime, TimeSpan offset, DateIsOption dateIsOption = DateIsOption.InThePast)
    {
        var offsetDateTime = dateIsOption == DateIsOption.InThePast
            ? dateTime.Subtract(offset)
            : dateTime.Add(offset);
        var dateOnly = offsetDateTime.Date;
        return dateOnly;
    }
}
