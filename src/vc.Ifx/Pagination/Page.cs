namespace VisionaryCoder.Framework.Pagination;

public sealed class Page<T>(IReadOnlyList<T> items, int count, int pageNumber, int pageSize, string? nextToken = null)
{
    public IReadOnlyList<T> Items { get; } = items;
    public int TotalCount { get; } = count;            // Optional for token paging
    public int PageNumber { get; } = pageNumber;       // Meaningful for offset paging
    public int PageSize { get; } = pageSize;
    public string? NextToken { get; } = nextToken;     // For token/continuation paging
}
