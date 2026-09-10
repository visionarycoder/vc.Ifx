using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.Json;
using VisionaryCoder.Framework.Filtering;
using VisionaryCoder.Framework.Querying;
using VisionaryCoder.Framework.Querying.Serialization;

namespace VisionaryCoder.Framework.Tests.Querying;

[TestClass]
public sealed class QueryContractTests
{
    public sealed class Row
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Value { get; set; }
    }

    public sealed class RowsContext(DbContextOptions<RowsContext> options) : DbContext(options)
    {
        public DbSet<Row> Rows => Set<Row>();
    }

    private static Row[] Rows() =>
    [new() { Id = 1, Name = "Alpha", Value = 1 }, new() { Id = 2, Name = "Beta", Value = 2 }, new() { Id = 3, Name = null, Value = 2 }];

    [TestMethod]
    public async Task SpecificationsRemainDeferredAndTranslateOnSqlite()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = new RowsContext(new DbContextOptionsBuilder<RowsContext>().UseSqlite(connection).Options);
        await context.Database.EnsureCreatedAsync();
        context.Rows.AddRange(Rows());
        await context.SaveChangesAsync();
        var all = new QuerySpec<Row>();
        QuerySpec<Row> ordered = all.Where(row => row.Value > 0).OrderByDescending(row => row.Value).ThenBy(row => row.Id);
        QuerySpec<Row, int> page = ordered.Page(1, 1).Select(row => row.Id);
        IQueryable<int> query = page.Apply(context.Rows);
        query.ToQueryString().Should().Contain("ORDER BY").And.Contain("LIMIT");
        (await query.ToArrayAsync()).Should().Equal(3);
        (await all.Apply(context.Rows).CountAsync()).Should().Be(3);
        (await ordered.Apply(context.Rows).CountAsync()).Should().Be(3);
        (await ordered.Page(1, 1).Page(0, 1).Apply(context.Rows).Select(row => row.Id).ToArrayAsync()).Should().Equal(2);
        var descending = all.Where(new FilterSpec<Row>(row => row.Value == 2)).OrderBy(row => row.Value).ThenByDescending(row => row.Id);
        (await descending.Apply(context.Rows).Select(row => row.Id).ToArrayAsync()).Should().Equal(3, 2);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Func<Task> execute = () => query.ToArrayAsync(cancellation.Token);
        await execute.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public void InvalidSpecificationArgumentsAreRejected()
    {
        var query = new QuerySpec<Row>();
        Action[] invalid =
        [
            () => query.Where((Expression<Func<Row, bool>>)null!),
            () => query.Where((FilterSpec<Row>)null!),
            () => query.OrderBy<int>(null!), () => query.OrderByDescending<int>(null!),
            () => query.ThenBy<int>(null!), () => query.ThenByDescending<int>(null!),
            () => query.ThenBy(row => row.Id), () => query.ThenByDescending(row => row.Id),
            () => query.Page(0, 1), () => query.Page(-1, 1), () => query.Page(0, 0),
            () => query.Select<int>(null!), () => query.Apply(null!),
            () => query.Select(row => row.Id).Apply(null!)
        ];
        foreach (Action action in invalid) action.Should().Throw<Exception>();
        query.OrderByDescending(row => row.Id).OrderBy(row => row.Id).Apply(Rows().AsQueryable()).Select(row => row.Id).Should().Equal(1, 2, 3);
    }

    [TestMethod]
    [DataRow("Equals", "2", "2,3")]
    [DataRow("NotEquals", "2", "1")]
    [DataRow("GreaterThan", "1", "2,3")]
    [DataRow("GreaterThanOrEqual", "2", "2,3")]
    [DataRow("LessThan", "2", "1")]
    [DataRow("LessThanOrEqual", "1", "1")]
    [DataRow("In", "[\"2\"]", "2,3")]
    [DataRow("NotIn", "[\"2\"]", "1")]
    public void PropertyOperatorsRoundtripAndRehydrate(string operation, string value, string expected)
    {
        var node = new PropertyFilter(operation, nameof(Row.Value), value);
        string json = QueryFilterSerializer.Serialize(node);
        QueryFilterSchemaValidator.Validate(json).Should().BeEmpty();
        FilterNode restored = QueryFilterSerializer.Deserialize(json)!;
        restored.Should().Be(node);
        string actual = string.Join(',', Rows().AsQueryable().Apply(restored.ToQueryFilter<Row>()).Select(row => row.Id));
        actual.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("Contains", "ph", false, "1")]
    [DataRow("StartsWith", "Al", false, "1")]
    [DataRow("EndsWith", "ta", false, "2")]
    [DataRow("Contains", "PH", true, "1")]
    [DataRow("StartsWith", "AL", true, "1")]
    [DataRow("EndsWith", "TA", true, "2")]
    [DataRow("Equals", "ALPHA", true, "1")]
    [DataRow("NotEquals", "ALPHA", true, "2")]
    public void StringOperatorsRetainCompatibility(string operation, string value, bool ignoreCase, string expected)
    {
        var node = new PropertyFilter(operation, nameof(Row.Name), value, ignoreCase);
        FilterNode restored = QueryFilterSerializer.Deserialize(QueryFilterSerializer.Serialize(node))!;
        string actual = string.Join(',', Rows().Where(row => row.Name != null).AsQueryable().Apply(restored.ToQueryFilter<Row>()).Select(row => row.Id));
        actual.Should().Be(expected);
        if (ignoreCase && operation != "NotEquals")
            restored.ToQueryFilter<Row>().Predicate.Compile()(Rows()[2]).Should().BeFalse();
    }

    [TestMethod]
    public void CompositeRoundtripPreservesAllChildrenAndNegation()
    {
        var first = new PropertyFilter("Equals", "Id", "1");
        var second = new PropertyFilter("Equals", "Id", "2");
        var either = new CompositeFilter("Or", [first, second]);
        var not = new CompositeFilter("Not", [either]);
        var both = new CompositeFilter("And", [not, new PropertyFilter("Equals", "Value", "2")]);
        FilterNode restored = QueryFilterSerializer.Deserialize(QueryFilterSerializer.Serialize(both))!;
        Rows().AsQueryable().Apply(restored.ToQueryFilter<Row>()).Select(row => row.Id).Should().Equal(3);
        using JsonDocument document = JsonDocument.Parse(QueryFilterSerializer.Serialize(both));
        QueryFilterSchemaValidator.IsValid(document).Should().BeTrue();
        QueryFilterSerializer.Deserialize("{\"operator\":\"Equals\",\"property\":\"Name\"}")
            .Should().Be(new PropertyFilter("Equals", "Name", null));
        QueryFilterSerializer.Deserialize("{\"operator\":\"Equals\",\"property\":\"Name\",\"value\":null}")
            .Should().Be(new PropertyFilter("Equals", "Name", null));
    }

    [TestMethod]
    [DataRow("null")]
    [DataRow("[]")]
    [DataRow("{")]
    [DataRow("{}")]
    [DataRow("{\"operator\":1}")]
    [DataRow("{\"operator\":\" \"}")]
    [DataRow("{\"operator\":\"Unknown\",\"property\":\"Id\"}")]
    [DataRow("{\"operator\":\"Equals\",\"property\":1}")]
    [DataRow("{\"operator\":\"Equals\",\"property\":\"Id\",\"value\":1}")]
    [DataRow("{\"operator\":\"Equals\",\"property\":\"Id\",\"ignoreCase\":1}")]
    [DataRow("{\"operator\":\"Equals\",\"property\":\"Id\",\"unknown\":true}")]
    [DataRow("{\"operator\":\"Equals\",\"operator\":\"NotEquals\",\"property\":\"Id\"}")]
    [DataRow("{\"operator\":\"And\",\"children\":[]}")]
    [DataRow("{\"operator\":\"And\",\"children\":null}")]
    [DataRow("{\"operator\":\"And\",\"children\":[null]}")]
    [DataRow("{\"operator\":\"And\",\"children\":[{}],\"property\":\"Id\"}")]
    [DataRow("{\"operator\":\"Bad\",\"children\":[{}]}")]
    [DataRow("{\"operator\":\"Not\",\"children\":[{},{}]}")]
    public void MalformedPayloadsFailValidationAndDeserialization(string json)
    {
        QueryFilterSchemaValidator.Validate(json).Should().NotBeEmpty();
        Action deserialize = () => QueryFilterSerializer.Deserialize(json);
        deserialize.Should().Throw<JsonException>();
    }

    private sealed record UnknownNode : FilterNode;

    [TestMethod]
    public void MalformedNodesAndUnsupportedIgnoreCaseFailBeforeExecution()
    {
        Action[] invalid =
        [
            () => QueryFilterSerializer.Serialize(null!),
            () => QueryFilterSerializer.Deserialize(null!),
            () => QueryFilterSchemaValidator.Validate(null!),
            () => QueryFilterSchemaValidator.IsValid(null!),
            () => QueryFilterRehydrator.ToQueryFilter<Row>(null!),
            () => QueryFilterSerializer.Serialize(new UnknownNode()),
            () => new CompositeFilter("And", []).ToQueryFilter<Row>(),
            () => new PropertyFilter("Equals", "Value", "1", true).ToQueryFilter<Row>(),
            () => new PropertyFilter("In", "Name", "[]", true).ToQueryFilter<Row>()
        ];
        foreach (Action action in invalid) action.Should().Throw<Exception>();
        using JsonDocument document = JsonDocument.Parse("{}");
        QueryFilterSchemaValidator.IsValid(document).Should().BeFalse();
        Action nullString = () => new PropertyFilter("Contains", "Name", null, true).ToQueryFilter<Row>();
        nullString.Should().Throw<ArgumentException>();
        new PropertyFilter("Equals", "Name", null, true).ToQueryFilter<Row>().Predicate.Compile()(Rows()[2]).Should().BeTrue();
    }

    [TestMethod]
    public void EmbeddedSchemaIsARealSchemaAndCanBeSaved()
    {
        using JsonDocument schema = JsonDocument.Parse(QueryFilterSchema.Content);
        schema.RootElement.GetProperty("$schema").GetString().Should().Contain("2020-12");
        schema.RootElement.GetProperty("$defs").GetProperty("property").GetProperty("properties")
            .GetProperty("operator").GetProperty("enum").EnumerateArray().Select(value => value.GetString())
            .Should().BeEquivalentTo(QueryFilterOperations.All.Keys);
        Action missing = () => QueryFilterSchema.LoadSchemaFromResource(typeof(QueryContractTests).Assembly, "missing.json");
        missing.Should().Throw<InvalidOperationException>().WithMessage("*missing.json*");
        string path = Path.Combine(Path.GetTempPath(), $"ifx-schema-{Guid.NewGuid():N}.json");
        try
        {
            QueryFilterSchema.SaveToFile(path);
            File.ReadAllText(path).Should().Be(QueryFilterSchema.Content);
        }
        finally { File.Delete(path); }
    }

    [TestMethod]
    public void CompatibilityCompositionHandlesNullEntriesAndNestedParameters()
    {
        QueryFilter<Row>[] nullable = [null!, new(row => row.Value > 1), null!];
        Rows().AsQueryable().Apply(nullable.Join()).Should().HaveCount(2);
        QueryFilterExtensions.StartsWithIgnoreCase<Row>(row => row.Name!, " ").Predicate.Compile()(Rows()[2]).Should().BeTrue();
        QueryFilterExtensions.EndsWithIgnoreCase<Row>(row => row.Name!, null).Predicate.Compile()(Rows()[2]).Should().BeTrue();
        QueryFilter<Row> first = new(row => row.Id > 0);
        QueryFilter<Row> nested = new(row => new[] { 1, 2 }.Any(value => value == row.Id));
        Rows().AsQueryable().Apply(first.And(nested)).Select(row => row.Id).Should().Equal(1, 2);
    }
}
