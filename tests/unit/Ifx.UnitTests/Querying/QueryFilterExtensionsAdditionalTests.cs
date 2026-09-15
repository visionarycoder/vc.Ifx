using Ifx.Querying;

namespace Ifx.Tests.Querying;

[TestClass]
public sealed class QueryFilterExtensionsAdditionalTests
{
    [TestMethod]
    public void Join_ShouldTreatNullEntriesAsAlwaysTrueFilters()
    {
        QueryFilter<Row>[] filters =
        [
            null!,
            new(row => row.Value > 1),
            null!
        ];

        IQueryable<Row> result = CreateRows().AsQueryable().Apply(filters.Join());

        result.Select(row => row.Id).Should().Equal(2, 3);
    }

    [TestMethod]
    public void IgnoreCaseHelpers_WithWhitespaceOrNullValue_ShouldReturnAlwaysTrue()
    {
        QueryFilter<Row> startsWith = QueryFilterExtensions.StartsWithIgnoreCase<Row>(row => row.Name!, " ");
        QueryFilter<Row> endsWith = QueryFilterExtensions.EndsWithIgnoreCase<Row>(row => row.Name!, null);

        startsWith.Predicate.Compile()(CreateRows()[2]).Should().BeTrue();
        endsWith.Predicate.Compile()(CreateRows()[2]).Should().BeTrue();
    }

    [TestMethod]
    public void And_WithNestedExpression_ShouldReplaceOnlyTargetParameter()
    {
        QueryFilter<Row> left = new(row => row.Id > 0);
        QueryFilter<Row> nested = new(row => new[] { 1, 2 }.Any(value => value == row.Id));

        IQueryable<Row> result = CreateRows().AsQueryable().Apply(left.And(nested));

        result.Select(row => row.Id).Should().Equal(1, 2);
    }

    private static Row[] CreateRows() =>
    [
        new() { Id = 1, Name = "Alpha", Value = 1 },
        new() { Id = 2, Name = "Beta", Value = 2 },
        new() { Id = 3, Name = null, Value = 2 }
    ];

    private sealed class Row
    {
        public int Id { get; init; }

        public string? Name { get; init; }

        public int Value { get; init; }
    }
}
