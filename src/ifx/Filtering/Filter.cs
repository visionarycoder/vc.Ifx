using Ifx.Filtering;

namespace Ifx.Filtering;

public static class Filter
{
    public static FilterBuilder<T> For<T>() => new();
}
