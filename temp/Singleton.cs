using System.Reflection;

namespace Utility.AuditLogging;

public class Singleton<T> where T : class
{

    public static T? Instance => typeof(T).InvokeMember(typeof(T).Name, BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic, null, null, null) as T;

}