using System.Linq.Expressions;
using System.Text.Json;
using Ifx.Filtering;
using Ifx.Querying;
using Ifx.Querying.Serialization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ifx.Tests.Querying;

[TestClass]
public sealed class QueryContractCoverageTests
{
    [TestMethod]
    public async Task QuerySpec_ShouldRemainDeferredAndTranslateOnSqlite()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = new RowsContext(new DbContextOptionsBuilder<RowsContext>().UseSqlite(connection).Options);
        await context.Database.EnsureCreatedAsync();
        context.Rows.AddRange(CreateRows());
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
        Func<Task> execute = async () => _ = await query.ToArrayAsync(cancellation.Token);
        await execute.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public void QuerySpec_ShouldRejectInvalidArguments()
    {
        var query = new QuerySpec<Row>();
        Action[] invalid =
        [
            () => query.Where((Expression<Func<Row, bool>>)null!),
            () => query.Where((FilterSpec<Row>)null!),
            () => query.OrderBy<int>(null!),
            () => query.OrderByDescending<int>(null!),
            () => query.ThenBy<int>(null!),
            () => query.ThenByDescending<int>(null!),
            () => query.ThenBy(row => row.Id),
            () => query.ThenByDescending(row => row.Id),
            () => query.Page(-1, 1),
            () => query.Page(0, 1),
            () => query.OrderBy(row => row.Id).Page(0, 0),
            () => query.Select<int>(null!),
            () => query.Apply(null!),
            () => query.Select(row => row.Id).Apply(null!)
        ];

        foreach (Action action in invalid)
        {
            action.Should().Throw<Exception>();
        }

        query.OrderByDescending(row => row.Id).OrderBy(row => row.Id).Apply(CreateRows().AsQueryable()).Select(row => row.Id).Should().Equal(1, 2, 3);
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
    public void PropertyOperators_ShouldRoundTripAndRehydrate(string operation, string value, string expected)
    {
        var node = new PropertyFilter(operation, nameof(Row.Value), value);

        string json = QueryFilterSerializer.Serialize(node);
        FilterNode restored = QueryFilterSerializer.Deserialize(json)!;
        string actual = string.Join(',', CreateRows().AsQueryable().Apply(restored.ToQueryFilter<Row>()).Select(row => row.Id));

        QueryFilterSchemaValidator.Validate(json).Should().BeEmpty();
        restored.Should().Be(node);
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
    public void StringOperators_ShouldRetainCompatibility(string operation, string value, bool ignoreCase, string expected)
    {
        var node = new PropertyFilter(operation, nameof(Row.Name), value, ignoreCase);

        FilterNode restored = QueryFilterSerializer.Deserialize(QueryFilterSerializer.Serialize(node))!;
        string actual = string.Join(',', CreateRows().Where(row => row.Name is not null).AsQueryable().Apply(restored.ToQueryFilter<Row>()).Select(row => row.Id));

        actual.Should().Be(expected);
        if (ignoreCase && operation != "NotEquals")
        {
            restored.ToQueryFilter<Row>().Predicate.Compile()(CreateRows()[2]).Should().BeFalse();
        }
    }

    [TestMethod]
    public void CompositeFilters_ShouldRoundTripAndPreserveNegation()
    {
        var first = new PropertyFilter("Equals", "Id", "1");
        var second = new PropertyFilter("Equals", "Id", "2");
        var either = new CompositeFilter("Or", [first, second]);
        var not = new CompositeFilter("Not", [either]);
        var both = new CompositeFilter("And", [not, new PropertyFilter("Equals", "Value", "2")]);

        string json = QueryFilterSerializer.Serialize(both);
        FilterNode restored = QueryFilterSerializer.Deserialize(json)!;

        CreateRows().AsQueryable().Apply(restored.ToQueryFilter<Row>()).Select(row => row.Id).Should().Equal(3);
        using JsonDocument document = JsonDocument.Parse(json);
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
    public void MalformedPayloads_ShouldFailValidationAndDeserialization(string json)
    {
        QueryFilterSchemaValidator.Validate(json).Should().NotBeEmpty();

        Action deserialize = () => QueryFilterSerializer.Deserialize(json);

        deserialize.Should().Throw<JsonException>();
    }

    [TestMethod]
    public void MalformedNodesAndUnsupportedIgnoreCase_ShouldFailBeforeExecution()
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
            () => new PropertyFilter("In", "Name", "[]", true).ToQueryFilter<Row>(),
            () => new PropertyFilter("Contains", "Name", null, true).ToQueryFilter<Row>()
        ];

        foreach (Action action in invalid)
        {
            action.Should().Throw<Exception>();
        }

        using JsonDocument document = JsonDocument.Parse("{}");
        QueryFilterSchemaValidator.IsValid(document).Should().BeFalse();
        new PropertyFilter("Equals", "Name", null, true).ToQueryFilter<Row>().Predicate.Compile()(CreateRows()[2]).Should().BeTrue();
        new PropertyFilter("NotEquals", "Name", null, true).ToQueryFilter<Row>().Predicate.Compile()(CreateRows()[2]).Should().BeFalse();
    }

    [TestMethod]
    public void SchemaContent_ShouldBeValidAndSavable()
    {
        using JsonDocument schema = JsonDocument.Parse(QueryFilterSchema.Content);
        schema.RootElement.GetProperty("$schema").GetString().Should().Contain("2020-12");
        schema.RootElement.GetProperty("$defs").GetProperty("property").GetProperty("properties")
            .GetProperty("operator").GetProperty("enum").EnumerateArray().Select(value => value.GetString())
            .Should().BeEquivalentTo(QueryFilterOperations.All.Keys);

        Action missing = () => QueryFilterSchema.LoadSchemaFromResource(typeof(QueryContractCoverageTests).Assembly, "missing.json");
        missing.Should().Throw<InvalidOperationException>().WithMessage("*missing.json*");

        string directory = Path.Combine(AppContext.BaseDirectory, "query-schema");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "queryfilter.schema.json");
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        QueryFilterSchema.SaveToFile(path);

        File.ReadAllText(path).Should().Be(QueryFilterSchema.Content);
    }

    private static Row[] CreateRows() =>
    [
        new() { Id = 1, Name = "Alpha", Value = 1 },
        new() { Id = 2, Name = "Beta", Value = 2 },
        new() { Id = 3, Name = null, Value = 2 }
    ];

    private sealed record UnknownNode : FilterNode;

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
}
