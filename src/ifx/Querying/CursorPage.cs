namespace Ifx.Querying;

/// <summary>
/// A page of results produced by cursor (keyset) pagination: the items themselves, plus an opaque
/// <see cref="NextCursor"/> token to request the following page. A <c>null</c> <see cref="NextCursor"/>
/// means there are no more results.
/// </summary>
/// <remarks>
/// Unlike an offset page, there is no total count — computing one would require a
/// separate full-table scan/count query, which defeats the throughput purpose of keyset pagination.
/// If a total count is required, use offset paging instead.
/// </remarks>
/// <typeparam name="T">The item type contained in the page.</typeparam>
/// <param name="Items">The materialized items for the current page.</param>
/// <param name="NextCursor">The opaque token used to request the next page, or <c>null</c> when no further page exists.</param>
public sealed record CursorPage<T>(IReadOnlyList<T> Items, string? NextCursor);
