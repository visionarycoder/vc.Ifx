namespace vc.Ifx.Extensions;

public static class DateTimeExtensions
{

    public static DateTime GetProceedingWeekday(this DateTime input, DayOfWeek dayOfWeek = DayOfWeek.Sunday)
    {
        var offset = (int)dayOfWeek - (int)input.DayOfWeek;
        var output = input.AddDays(offset).Date;
        return output;
    }

}