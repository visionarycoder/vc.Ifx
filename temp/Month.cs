using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dw.Utilities.DateTime.Months
{
	
	public class Month: IMonth
	{

		#region consts
		public const string Unknown = "???";

		public const string January = "January";
		public const string February = "February";
		public const string March = "March";
		public const string April = "April";
		public const string May = "May";
		public const string June = "June";
		public const string July = "July";
		public const string August = "August";
		public const string September = "September";
		public const string October = "October";
		public const string November = "November";
		public const string December = "December";

		public const string Jan = "Jan";
		public const string Feb = "Feb";
		public const string Mar = "Mar";
		public const string Apr = "Apr";
		//public const string May = "May";  // (Long form == Short form) Duh!
		public const string Jun = "Jun";
		public const string Jul = "Jul";
		public const string Aug = "Aug";
		public const string Sep = "Sep";
		public const string Oct = "Oct";
		public const string Nov = "Nov";
		public const string Dec = "Dec";
		#endregion consts

		private List<string> longMonthNames = new List<string> { Unknown, January, February, March, April, May, June, July, August, September, October, November, December };
		private List<string> shortMonthNames = new List<string> { Unknown, Jan, Feb, Mar, Apr, May, Jun, Jul, Aug, Sep, Oct, Nov, Dec };

		public string Name { get; private set; }
		public string Abbrv { get { return Name.Substring(0, 3); } }
		public int Order { get; private set; }
		public int Index { get { return Order - 1; } }

		public Month(): this(Unknown)
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
				throw new ArgumentOutOfRangeException("order", "Order must be between 0 and " + longMonthNames.Count);
			}

		}

		public Month(string name)
		{

			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name", "Name cannot be empty.");

			if (longMonthNames.Contains(name))
				Order = longMonthNames.IndexOf(name);
			else if (shortMonthNames.Contains(name))
				Order = shortMonthNames.IndexOf(name);
			else
				throw new ArgumentOutOfRangeException("name", "Name is not a valid month name: " + name);

			Name = longMonthNames[Order];

		}

		public override string ToString()
		{
			return Name;
		}

	}

}
