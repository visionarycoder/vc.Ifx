using System.Linq.Expressions;
using Ifx.Filtering.Abstractions;

namespace Ifx.Filtering;

public sealed class FilterBuilder<T>
{
    private readonly List<FilterNode> roots = [];

    public FilterBuilder<T> Where(Expression<Func<T, bool>> predicate)
    {
        FilterNode node = ExpressionToFilterNode.Translate(predicate);
        roots.Add(node);
        return this;
    }

    public FilterNode Build()
    {
        return roots.Count switch
        {
            0 => new FilterGroup(FilterCombination.And, new List<FilterNode>()),
            1 => roots[0],
            _ => new FilterGroup(FilterCombination.And, roots.ToArray())
        };
    }
}
