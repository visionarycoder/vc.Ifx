namespace vc.Ifx.Extensions;

public static class TimeSpanExtensions
{

    public static DateTime GetDate(this TimeSpan timeSpan, DateIs dateIs = DateIs.Historical)
    {
        var targetDateTime = dateIs == DateIs.Historical
            ? DateTime.Now.Subtract(timeSpan)
            : DateTime.Now.Add(timeSpan);
        var targetDate = targetDateTime.Date;
        return targetDate;
    }

}