using Ifx.Filtering.Abstractions;

namespace Ifx.Filtering;

public static class Filter
{
    public static FilterBuilder<T> For<T>() => new();
}
