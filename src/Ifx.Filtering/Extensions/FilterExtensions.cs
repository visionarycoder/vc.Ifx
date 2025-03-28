using System.ComponentModel.DataAnnotations;

namespace Ifx.Filtering.Extensions;

/// <summary>
///     Extensions for configuring and applying filters.
/// </summary>
public static class FilterExtensions
{

    /// <summary>
    ///     Configures pagination for the filter using page number and size.
    /// </summary>
    /// <typeparam name="T">The type of the entity being filtered.</typeparam>
    /// <param name="filter">The filter to configure.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    public static void SetPagination<T>(this Filter<T> filter, int? pageNumber, int? pageSize) where T : class
    {
        if (pageNumber is < 1 || pageSize is < 1)
            // Return early if the input is not valid
            return;

        filter.Skip = (pageNumber - 1) * pageSize;
        filter.Take = pageSize;
    }

    /// <summary>
    ///     Configures offset and limit for the filter.
    /// </summary>
    /// <typeparam name="T">The type of the entity being filtered.</typeparam>
    /// <param name="filter">The filter to configure.</param>
    /// <param name="offset">The number of items to skip.</param>
    /// <param name="limit">The maximum number of items to take.</param>
    public static void SetOffsetAndLimit<T>(this Filter<T> filter, int? offset, int? limit) where T : class
    {
        if (offset is < 0 || limit is <= 0)
            // Return early if the input is not valid
            return;

        filter.Skip = offset;
        filter.Take = limit;
    }

    /// <summary>
    ///     Validates the filter by checking if the property names in the criteria exist in the entity type.
    /// </summary>
    /// <typeparam name="T">The type of the entity being filtered.</typeparam>
    /// <param name="filter">The filter to validate.</param>
    /// <returns>A ValidationResult object indicating whether the filter is valid or not.</returns>
    public static ValidationResult ValidateFilter<T>(this Filter<T> filter) where T : class
    {
        // Get the set of property names for the entity type T
        var properties = typeof(T).GetProperties().Select(p => p.Name).ToHashSet();

        // Find criteria with invalid property names
        var invalidProperties = filter.Criteria.Values
            .Where(criterion => !properties.Contains(criterion.PropertyName))
            .Select(criterion => criterion.PropertyName)
            .ToList();

        // Return a ValidationResult indicating success or failure
        return invalidProperties.Any()
            ? new ValidationResult("Filter validation failed.", invalidProperties)
            : ValidationResult.Success!;
    }

    /// <summary>
    ///     Converts a filter of one type to a filter of another type.
    /// </summary>
    /// <typeparam name="TSource">The source type of the filter.</typeparam>
    /// <typeparam name="TDestination">The destination type of the filter.</typeparam>
    /// <param name="source">The source filter to convert.</param>
    /// <returns>A new filter of the destination type.</returns>
    public static Filter<TDestination> Convert<TSource, TDestination>(this Filter<TSource> source) where TSource : class where TDestination : class
    {
        var target = new Filter<TDestination>(source.Criteria.Values.ToArray())
        {
            Skip = source.Skip,
            Take = source.Take,
            OrderBy = source.OrderBy,
            OrderByDirection = source.OrderByDirection,
            ThenBy = source.ThenBy,
            ThenByDirection = source.ThenByDirection
        };
        return target;
    }
}