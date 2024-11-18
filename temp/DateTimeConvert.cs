using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dw.Utilities.DateTime
{

	public static class Convert
	{

		public static bool IsValidDateTimeString(string possibleDateTimeString)
		{
			System.DateTime date;
			return System.DateTime.TryParse(possibleDateTimeString, out date);
		}

		public static System.DateTime? ConvertStringToDateTime(string possibleDateTimeString)
		{
		
			System.DateTime date;
			if (System.DateTime.TryParse(possibleDateTimeString, out date))
				return date;
			return null;

		}

		#region Unused
		//public static string WorkWeekToMonth(string yearWorkweek)
		//{

		//  string year = yearWorkweek.Substring(0, 4);
		//  string workweek = yearWorkweek.Replace(year, String.Empty).Split('.')[0];
		//  int weeks = int.Parse(workweek);
		//  if (weeks > 52)
		//    weeks = 52;

		//  System.DateTime date = System.DateTime.Parse("1/1/" + year).AddDays(weeks * 7);
		//  string month = year + "M" + date.Month.ToString().PadLeft(2, '0');

		//  return month;
		//}

		//public static int WeekToMonth(int week)
		//{
		//  return WeekToMonth(System.DateTime.Now.Year, week);
		//}

		//public static int WeekToMonth(int year, int week)
		//{

		//  int month = -1;

		//  try
		//  {
		//    System.DateTime dateTime = new System.DateTime(year, 1, 1).AddDays((week * 7) - 1);
		//    month = dateTime.Month;
		//  }
		//  catch
		//  {
		//    // ignore
		//  }

		//  return month;
		//}

		//public static string MonthToQuarter(string yearMonth)
		//{

		//  string year = yearMonth.Substring(0, 4);
		//  string month = yearMonth.Replace(year + "M", string.Empty);

		//  int mon = int.Parse(month);
		//  int qtr = ((mon - 1) / 3) + 1;
		//  string quarter = year + "Q" + qtr;

		//  return quarter;

		//}
		#endregion Unused

	}

}
