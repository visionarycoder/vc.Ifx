namespace vc.Ifx.Extensions;

public static class CollectionExtensions
{

    public static bool IsNullOrEmpty<T>(this ICollection<T>? collection)
    {

        return collection switch
        {
            null => true,
            _ => collection.Count == 0
        };

    }

}
