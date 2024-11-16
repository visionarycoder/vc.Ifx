namespace vc.Helper;

public static class CollectionHelper
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
