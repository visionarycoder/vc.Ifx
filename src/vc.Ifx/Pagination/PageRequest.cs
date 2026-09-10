namespace VisionaryCoder.Framework.Pagination;

public sealed class PageRequest(int pageNumber = 1, int pageSize = 50, string? continuationToken = null)
{
    public int PageNumber { get; } = Math.Max(1, pageNumber);
    public int PageSize { get; } = Math.Clamp(pageSize, 1, 1000);
    public string? ContinuationToken { get; } = continuationToken;
    public bool IsTokenPaging => !string.IsNullOrWhiteSpace(ContinuationToken);
}
