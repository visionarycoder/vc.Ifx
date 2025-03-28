namespace Ifx.Filtering;

public class Filter
{
    public enum ComparisonOperatorOption
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        Contains
    }

    public enum IgnoreCaseOption
    {
        No = 0,
        Yes = 1
    }

    public enum SortDirectionOption
    {
        Ascending,
        Descending
    }

    private readonly Dictionary<string, Criterion> criteria = new();

    private int? skip;

    private int? take;

    public Filter()
    {
    }

    public Filter(string propertyName, object? propertyValue, IgnoreCaseOption ignoreCase = IgnoreCaseOption.Yes, ComparisonOperatorOption comparisonOperator = ComparisonOperatorOption.Equals)
    {
        AddCriterion(new Criterion(propertyName, propertyValue, comparisonOperator, ignoreCase));
    }

    public Filter(params Criterion[] criteria)
    {
        AddCriteria(criteria);
    }

    public static Filter Empty => new();
    public IReadOnlyDictionary<string, Criterion> Criteria => criteria;

    public int? Skip
    {
        get => skip;
        set => skip = value < 0 ? null : value;
    }

    public int? Take
    {
        get => take;
        set => take = value < 0 ? null : value;
    }

    public string? OrderBy { get; set; }
    public SortDirectionOption OrderByDirection { get; set; } = SortDirectionOption.Ascending;

    public string? ThenBy { get; set; }
    public SortDirectionOption ThenByDirection { get; set; } = SortDirectionOption.Ascending;

    public void AddCriterion(string propertyName, object? propertyValue, IgnoreCaseOption ignoreCase = IgnoreCaseOption.Yes, ComparisonOperatorOption comparisonOperator = ComparisonOperatorOption.Equals)
    {
        AddCriterion(new Criterion(propertyName, propertyValue, comparisonOperator, ignoreCase));
    }

    public void AddCriterion(Criterion criterion)
    {
        if (criterion == null || string.IsNullOrWhiteSpace(criterion.PropertyName))
            throw new ArgumentException("Invalid criterion.");
        criteria[criterion.PropertyName] = criterion;
    }

    public void AddCriteria(params Criterion[] additionalCriteria)
    {
        foreach (var criterion in additionalCriteria) AddCriterion(criterion);
    }

    public class Criterion(string propertyName, object? propertyValue, ComparisonOperatorOption comparisonOperator = ComparisonOperatorOption.Equals, IgnoreCaseOption ignoreCase = IgnoreCaseOption.Yes)
    {
        public string PropertyName { get; set; } = propertyName;
        public object? PropertyValue { get; set; } = propertyValue;
        public ComparisonOperatorOption ComparisonOperator { get; set; } = comparisonOperator;
        public IgnoreCaseOption IgnoreCase { get; set; } = ignoreCase;
    }
}

/// <summary>
///     Represents a generic filter for querying data.
///     Includes criteria for filtering, pagination, and ordering.
/// </summary>
public class Filter<T> : Filter where T : class
{
    public Filter()
    {
    }

    public Filter(string propertyName, object? propertyValue, IgnoreCaseOption ignoreCase = IgnoreCaseOption.Yes, ComparisonOperatorOption comparisonOperator = ComparisonOperatorOption.Equals)
    {
        AddCriterion(new Criterion(propertyName, propertyValue, comparisonOperator, ignoreCase));
    }

    public Filter(params Criterion[] criteria)
    {
        AddCriteria(criteria);
    }

    public new static Filter<T> Empty => new();
}