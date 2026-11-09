namespace Ifx.Querying;

/// <summary>Contains an offset page and the total number of matching items.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount);
