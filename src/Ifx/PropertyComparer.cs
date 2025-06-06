using System.ComponentModel;
using System.Diagnostics;

namespace vc.Ifx;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class PropertyComparer<T>(PropertyDescriptor property, ListSortDirection direction) : IComparer<T>
{

    // The following code contains code implemented by Rockford Lhotka:
    // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnadvnet/html/vbnet01272004.asp

    private PropertyDescriptor Property { get; } = property;
    private ListSortDirection Direction { get; } = direction;

    #region IComparer<T>
    public int Compare(T? left, T? right)
    {
        // Handle nulls for xWord and yWord
        if (left is null && right is null)
            return 0;
        if (left is null)
            return -1;
        if (right is null)
            return 1;

        // Get property values
        var leftObject = GetPropertyValue(left, Property.Name);
        var rightObject = GetPropertyValue(right, Property.Name);

        // Determine sort order
        return Direction == ListSortDirection.Ascending
            ? CompareAscending(leftObject, rightObject)
            : CompareDescending(leftObject, rightObject);
    }

    public bool Equals(T left, T right) => left!.Equals(right);

    public int GetHashCode(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));
        return obj.GetHashCode();
    }

    #endregion

    // Compare two property values of any type
    private static int CompareAscending(object? left, object? right)
    {

        int result;
        switch (left)
        {
            case null when (right == null): result = 0; break;
            case null: result = -1; break;
            default:
            {
                if (right == null)
                {
                    result = 1;
                }
                // If values implement IComparer
                else if (left is IComparable comparable)
                {
                    result = comparable.CompareTo(right);
                }
                // If values don't implement IComparer but are equivalent
                else if (left.Equals(right))
                {
                    result = 0;
                }
                // Values don't implement IComparer and are not equivalent, so compare as string values
                else
                {
                    result = string.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal);
                }
                break;
            }
        }

        // Return result
        return result;
    }

    private static int CompareDescending(object? left, object? right)
    {
        // Return result adjusted for ascending or descending sort order ie
        // multiplied by 1 for ascending or -1 for descending
        return CompareAscending(left, right) * -1;
    }

    private static object? GetPropertyValue(T value, string property)
    {
        var propertyInfo = value?.GetType().GetProperty(property);
        if (propertyInfo != null)
            return propertyInfo.GetValue(value, null) ?? default(T);
        return null;
    }

    private string GetDebuggerDisplay()
    {
        return ToString() ?? string.Empty;
    }

}