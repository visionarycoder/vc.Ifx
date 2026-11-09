namespace Ifx.Querying;

public sealed record SortClause(string Path, SortDirection Direction = SortDirection.Ascending);
