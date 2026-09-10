namespace VisionaryCoder.Framework.Models;

public class Month
{
    #region Constants
    public const string UnknownName = "Unknown";

    public const string JanuaryName = "January";
    public const string FebruaryName = "February";
    public const string MarchName = "March";
    public const string AprilName = "April";
    public const string MayName = "May";
    public const string JuneName = "June";
    public const string JulyName = "July";
    public const string AugustName = "August";
    public const string SeptemberName = "September";
    public const string OctoberName = "October";
    public const string NovemberName = "November";
    public const string DecemberName = "December";

    public const string JanAbbreviation = "Jan";
    public const string FebAbbreviation = "Feb";
    public const string MarAbbreviation = "Mar";
    public const string AprAbbreviation = "Apr";
    public const string JunAbbreviation = "Jun";
    public const string JulAbbreviation = "Jul";
    public const string AugAbbreviation = "Aug";
    public const string SepAbbreviation = "Sep";
    public const string OctAbbreviation = "Oct";
    public const string NovAbbreviation = "Nov";
    public const string DecAbbreviation = "Dec";
    #endregion Constants

    #region Static Month Instances
    public static readonly Month Unknown = new(UnknownName);
    public static readonly Month January = new(JanuaryName);
    public static readonly Month February = new(FebruaryName);
    public static readonly Month March = new(MarchName);
    public static readonly Month April = new(AprilName);
    public static readonly Month May = new(MayName);
    public static readonly Month June = new(JuneName);
    public static readonly Month July = new(JulyName);
    public static readonly Month August = new(AugustName);
    public static readonly Month September = new(SeptemberName);
    public static readonly Month October = new(OctoberName);
    public static readonly Month November = new(NovemberName);
    public static readonly Month December = new(DecemberName);

    // Short name static properties for backward compatibility
    public static readonly Month Jan = January;
    public static readonly Month Feb = February;
    public static readonly Month Mar = March;
    public static readonly Month Apr = April;
    public static readonly Month Jun = June;
    public static readonly Month Jul = July;
    public static readonly Month Aug = August;
    public static readonly Month Sep = September;
    public static readonly Month Oct = October;
    public static readonly Month Nov = November;
    public static readonly Month Dec = December;
    #endregion Static Month Instances

    private readonly List<string> longMonthNames = [UnknownName, JanuaryName, FebruaryName, MarchName, AprilName, MayName, JuneName, JulyName, AugustName, SeptemberName, OctoberName, NovemberName, DecemberName];
    private readonly List<string> shortMonthNames = [UnknownName, JanAbbreviation, FebAbbreviation, MarAbbreviation, AprAbbreviation, MayName, JunAbbreviation, JulAbbreviation, AugAbbreviation, SepAbbreviation, OctAbbreviation, NovAbbreviation, DecAbbreviation];

    public string Name { get; private set; }
    public string Abbrv => Name[..3];
    public int Ordinal { get; private set; }
    public int Index => Ordinal - 1;

    public Month()
        : this(UnknownName)
    {
    }

    public Month(int ordinal)
    {
        if (ordinal >= 0 && ordinal < longMonthNames.Count)
        {
            Ordinal = ordinal;
            Name = longMonthNames[ordinal];
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(ordinal), "Ordinal must be between 0 and " + longMonthNames.Count);
        }
    }

    public Month(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (longMonthNames.Contains(name))
        {
            Ordinal = longMonthNames.IndexOf(name);
        }
        else if (shortMonthNames.Contains(name))
        {
            Ordinal = shortMonthNames.IndexOf(name);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(name), $"Name is not a valid month name: {name}");
        }
        Name = longMonthNames[Ordinal];
    }

    public Month(Month other)
    {
        ArgumentNullException.ThrowIfNull(other);
        Name = other.Name;
        Ordinal = other.Ordinal;
    }

    public override string ToString()
    {
        return Name;
    }
}