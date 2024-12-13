using System.Reflection;

namespace dw.Utilities.Architecture
{

	public class Singleton<T> where T : class
	{

		public static T Instance
		{

			get
			{
				return typeof(T)
					.InvokeMember(typeof(T).Name, BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic, null, null, null) as T;
			}

		}

	}

}
