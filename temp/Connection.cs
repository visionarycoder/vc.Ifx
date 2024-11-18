using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;

namespace dw.Utilities.Networking
{
	public static class Connection
	{

		private static string[] siteList = { "www.google.com", "www.microsoft.com" };

		private const int timeout = 5000; // milliseconds

		public static bool IsConnectionAvailable()
		{
			return IsConnectionAvailable(siteList);
		}
		public static bool IsConnectionAvailable(string url)
		{
			return IsConnectionAvailable(new string[] { url });
		}
		public static bool IsConnectionAvailable(string[] urlList)
		{

			try
			{
				return CanPingRemoteSite(urlList);
			}
			catch
			{
				// If exception thrown, then no connectivity.
				return false;
			}

		}

		private static bool CanPingRemoteSite(string[] urlList)
		{
			
			Ping ping = new Ping();
			PingReply reply;

			foreach (string url in urlList)
			{

				reply = ping.Send(url, timeout);
				if ((reply != null) && (reply.Status == IPStatus.Success))
					return true;

			}

			return false;

		}

	}

}
