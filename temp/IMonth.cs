using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dw.Utilities.DateTime.Months
{
	
	interface IMonth
	{

		string Name { get; }
		string Abbrv { get; }
		int Order { get; }

	}

}
