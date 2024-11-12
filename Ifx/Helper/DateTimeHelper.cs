namespace vc.Helper;

public static class DateTimeHelper
{

    public static DateTime GetProceedingWeekday(DateTime input, DayOfWeek dayOfWeek)
    {
        var offset = (int)dayOfWeek - (int)input.DayOfWeek;
        var output = input.AddDays(offset).Date;
        return output;
    }


    public static DateTime GetDate(TimeSpan timeSpan, DateIs dateIs = DateIs.Historical)
    {
        var targetDateTime = dateIs == DateIs.Historical 
            ? DateTime.Now.Subtract(timeSpan) 
            : DateTime.Now.Add(timeSpan);
        var targetDate = targetDateTime.Date;
        return targetDate;
    }

    public enum DateIs
    {
        Historical,
        InTheFuture
    }
}