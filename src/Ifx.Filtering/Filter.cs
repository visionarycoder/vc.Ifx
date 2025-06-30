namespace Ifx.Filtering;

public interface IPagination
{
    public int? Skip { get; set; }
    public int? Take { get; set; }
}

public class Filter : IPagination
{

    public enum ComparisonOperatorOption
    {
        Undefined = 0,
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        Contains
    }

    public enum IgnoreCaseOption
    {
        Undefined = 0,
        No = Undefined,
        Yes = 1
    }

    public enum SortDirectionOption
    {
        Undefined = 0,
        Ascending = Undefined,
        Descending
    }

    public static Filter Empty => new();

    private readonly Dictionary<string, Criterion> criteria = new();
    public IReadOnlyDictionary<string, Criterion> Criteria => criteria;

    private int? skip;
    public int? Skip
    {
        get => skip;
        set => skip = value < 0 ? null : value;
    }

    private int? take;
    public int? Take
    {
        get => take;
        set => take = value < 0 ? null : value;
    }

    private Dictionary<string,OrderByClause> orderByClauses = new();
    public IReadOnlyList<OrderByClause> OrderByClauses => [.. orderByClauses.Select(i => i.Value)];

    public Filter()
    {
    }

    public Filter(string propertyName, object? propertyValue, ComparisonOperatorOption comparisonOperator = ComparisonOperatorOption.Equals, IgnoreCaseOption ignoreCase = IgnoreCaseOption.Undefined)
        : this(new Criterion(propertyName, propertyValue, comparisonOperator, ignoreCase))
    {
    }

    public Filter(Criterion criterion)
    {
        AddCriterion(criterion);
    }

    public Filter(params (string propertyName, object? propertyValue)[] criteria)
        : this(criteria.Select(c => new Criterion(c.propertyName, c.propertyValue)).ToArray())
    {
    }

    public Filter(params (string propertyName, object? propertyValue, ComparisonOperatorOption comparisonOperator)[] criteria)
    : this(criteria.Select(c => new Criterion(c.propertyName, c.propertyValue, c.comparisonOperator)).ToArray())
    {
    }

    public Filter(params (string propertyName, object? propertyValue, ComparisonOperatorOption comparisonOperator, IgnoreCaseOption ignoreOption)[] criteria)
        : this(criteria.Select(c => new Criterion(c.propertyName, c.propertyValue, c.comparisonOperator, c.ignoreOption)).ToArray())
    {
    }

    public Filter(params Criterion[] criteria)
    {
        AddCriteria(criteria);
    }

    public void AddCriterion(string propertyName, object? propertyValue, IgnoreCaseOption ignoreCase = IgnoreCaseOption.Yes, ComparisonOperatorOption comparisonOperator = ComparisonOperatorOption.Equals)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("Property name cannot be null or whitespace.");
        var criteria = new Criterion(propertyName, propertyValue, comparisonOperator, ignoreCase);
        AddCriterion(criteria);
    }

    public void AddCriterion(Criterion criterion)
    {
        if (criterion == null || string.IsNullOrWhiteSpace(criterion.PropertyName))
            throw new ArgumentException("Invalid criterion.");
        criteria[criterion.PropertyName] = criterion;
    }

    public void AddCriteria(params (string propertyName, object? propertyValue)[] additionalCriteria)
    {
        if (additionalCriteria == null || additionalCriteria.Length == 0)
            return;
        additionalCriteria.Where(c => !string.IsNullOrWhiteSpace(c.propertyName)).Select(c => new Criterion(c.propertyName, c.propertyValue)).ToList().ForEach(c => AddCriterion(c));
    }

    public void AddCriteria(params (string propertyName, object? propertyValue, ComparisonOperatorOption comparisonOperator)[] additionalCriteria)
    {
        if (additionalCriteria == null || additionalCriteria.Length == 0)
            return;
        additionalCriteria.Where(c => !string.IsNullOrWhiteSpace(c.propertyName)).Select(c => new Criterion(c.propertyName, c.propertyValue, c.comparisonOperator)).ToList().ForEach(c => AddCriterion(c));
    }

    public void AddCriteria(params (string propertyName, object? propertyValue, ComparisonOperatorOption comparisonOperator, IgnoreCaseOption ignoreCase)[] additionalCriteria)
    {
        if (additionalCriteria == null || additionalCriteria.Length == 0)
            return;
        additionalCriteria.Where(c => !string.IsNullOrWhiteSpace(c.propertyName)).Select(c => new Criterion(c.propertyName, c.propertyValue, c.comparisonOperator, c.ignoreCase)).ToList().ForEach(c => AddCriterion(c));
    }

    public void AddCriteria(params Criterion[] additionalCriteria)
    {
        if (additionalCriteria == null || additionalCriteria.Length == 0)
            return;
        additionalCriteria.Where(c => c != null).ToList().ForEach(AddCriterion);
    }

    public void AddOrderByClause(string propertyName, SortDirectionOption sortDirection = SortDirectionOption.Ascending)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("Property name cannot be null or whitespace.");
        orderByClauses[propertyName] = new OrderByClause(propertyName, sortDirection);
    }

    public void AddOrderByClause(OrderByClause orderByClause)
    {
        if (orderByClause == null || string.IsNullOrWhiteSpace(orderByClause.PropertyName))
            throw new ArgumentException("Invalid order by clause.");
        orderByClauses[orderByClause.PropertyName] = orderByClause;
    }

    public void AddOrderByClauses(params (string propertyName, SortDirectionOption sortDirection)[] orderByClauses)
    {
        if (orderByClauses == null || orderByClauses.Length == 0)
            return;
        orderByClauses.Where(c => !string.IsNullOrWhiteSpace(c.propertyName)).Select(c => new OrderByClause(c.propertyName, c.sortDirection)).ToList().ForEach(AddOrderByClause);
    }

    public void AddOrderByClauses(params OrderByClause[] orderByClauses)
    {
        if (orderByClauses == null || orderByClauses.Length == 0)
            return;
        orderByClauses.Where(c => c != null).ToList().ForEach(AddOrderByClause);
    }

    public record Criterion(string PropertyName, object? PropertyValue, ComparisonOperatorOption ComparisonOperator = ComparisonOperatorOption.Equals, IgnoreCaseOption IgnoreCase = IgnoreCaseOption.Yes);
    public record OrderByClause(string PropertyName, SortDirectionOption SortDirection = SortDirectionOption.Ascending);

}

/// <summary>
/// Represents a generic filter for querying data.
/// Includes criteria for filtering, pagination, and ordering.
/// </summary>
public class Filter<T> : Filter where T : class { }