namespace vc.Ifx;

public class Month
{

    #region consts
    public const string UNKNOWN = "???";

    public const string JANUARY = "January";
    public const string FEBRUARY = "February";
    public const string MARCH = "March";
    public const string APRIL = "April";
    public const string MAY = "May";
    public const string JUNE = "June";
    public const string JULY = "July";
    public const string AUGUST = "August";
    public const string SEPTEMBER = "September";
    public const string OCTOBER = "October";
    public const string NOVEMBER = "November";
    public const string DECEMBER = "December";

    public const string JAN = "Jan";
    public const string FEB = "Feb";
    public const string MAR = "Mar";
    public const string APR = "Apr";
    public const string JUN = "Jun";
    public const string JUL = "Jul";
    public const string AUG = "Aug";
    public const string SEP = "Sep";
    public const string OCT = "Oct";
    public const string NOV = "Nov";
    public const string DEC = "Dec";
    #endregion consts

    private readonly List<string> longMonthNames = [UNKNOWN, JANUARY, FEBRUARY, MARCH, APRIL, MAY, JUNE, JULY, AUGUST, SEPTEMBER, OCTOBER, NOVEMBER, DECEMBER];
    private readonly List<string> shortMonthNames = [UNKNOWN, JAN, FEB, MAR, APR, MAY, JUN, JUL, AUG, SEP, OCT, NOV, DEC];

    public string Name { get; private set; }
    public string Abbrv => Name[..3];
    public int Order { get; private set; }
    public int Index => Order - 1;

    public Month()
        : this(UNKNOWN)
    {
    }

    public Month(int order)
    {

        if (order >= 0 && order < longMonthNames.Count)
        {
            Order = order;
            Name = longMonthNames[order];
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(order), "Order must be between 0 and " + longMonthNames.Count);
        }

    }

    public Month(string name)
    {

        ArgumentNullException.ThrowIfNull(name, nameof(name));

        if (longMonthNames.Contains(name))
            Order = longMonthNames.IndexOf(name);
        else if (shortMonthNames.Contains(name))
            Order = shortMonthNames.IndexOf(name);
        else
            throw new ArgumentOutOfRangeException(nameof(name), $"Name is not a valid month name: {name}");

        Name = longMonthNames[Order];

    }

    public override string ToString()
    {
        return Name;
    }

}