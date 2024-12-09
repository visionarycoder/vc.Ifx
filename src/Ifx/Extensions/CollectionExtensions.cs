// ReSharper disable ConvertIfStatementToReturnStatement
#pragma warning disable ClassMethodMissingInterface
#pragma warning disable DerivedClasses
namespace vc.Ifx.Extensions;

public static class CollectionExtensions
{

    public static bool IsNullOrEmpty<T>(this ICollection<T>? collection)
    {

        if (collection is null)
            return true;

        if (collection.Count == 0)
            return true;

        return false;

    }

}
