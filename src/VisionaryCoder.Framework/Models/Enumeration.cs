using System.Reflection;

namespace VisionaryCoder.Framework.Models;

public abstract class Enumeration(int id, string name) : IComparable
{

    public string Name { get; } = name;

    public int Id { get; } = id;

    public override string ToString() => Name;

    public static IEnumerable<T> GetAll<T>() where T : Enumeration => typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Select(f => f.GetValue(null)).Cast<T>();

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
        {
            return false;
        }
        bool typeMatches = GetType() == obj.GetType();
        bool valueMatches = Id.Equals(otherValue.Id);
        return typeMatches && valueMatches;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static int AbsoluteDifference(Enumeration firstValue, Enumeration secondValue)
    {
        int absoluteDifference = Math.Abs(firstValue.Id - secondValue.Id);
        return absoluteDifference;
    }

    public static T FromValue<T>(int value) where T : Enumeration
    {
        T matchingItem = Parse<T, int>(value, "value", item => item.Id == value);
        return matchingItem;
    }

    public static T FromDisplayName<T>(string displayName) where T : Enumeration
    {
        T matchingItem = Parse<T, string>(displayName, "display name", item => item.Name == displayName);
        return matchingItem;
    }

    private static T Parse<T, K>(K value, string description, Func<T, bool> predicate) where T : Enumeration
    {
        T? matchingItem = GetAll<T>().FirstOrDefault(predicate);
        if (matchingItem == null)
        {
            throw new InvalidOperationException($"'{value}' is not a valid {description} in {typeof(T)}");
        }
        return matchingItem;
    }

    public int CompareTo(object? other) => Id.CompareTo((((Enumeration)other!)!).Id);
}