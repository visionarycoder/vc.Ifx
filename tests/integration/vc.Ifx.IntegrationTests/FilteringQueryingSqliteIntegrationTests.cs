using System.Data.Common;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VisionaryCoder.Framework.Filtering;
using VisionaryCoder.Framework.Filtering.EFCore;
using VisionaryCoder.Framework.Querying;
using VisionaryCoder.Framework.Querying.Serialization;
using PortableNode = VisionaryCoder.Framework.Filtering.Abstractions.FilterNode;

namespace vc.Ifx.IntegrationTests;

[TestClass]
public sealed class FilteringQueryingSqliteIntegrationTests
{
    [TestMethod]
    public async Task PortableFilterRoundtripComposesWithRelationalPagingAndProjection()
    {
        await using var database = await Database.CreateAsync();
        int[] buckets = [1];
        var filter = new FilterSpec<Row>(row => row.Name != null && row.Name.Contains("Al") && row.Score >= 10)
            .And(new FilterSpec<Row>(row => buckets.Contains(row.Bucket)));
        PortableNode restored = Roundtrip(filter);
        var adapter = new EfFilterExecutionStrategy(database.Context);
        var shape = new QuerySpec<Row>().OrderByDescending(row => row.Score).ThenBy(row => row.Id)
            .Page(1, 2).Select(row => new Result(row.Id, row.Name, row.Score));
        IQueryable<Result> query = shape.Apply(adapter.Apply(database.Context.Rows.AsNoTracking(), restored));

        Assert.AreEqual(0, database.Commands.Reads);
        string sql = query.ToQueryString();
        StringAssert.Contains(sql, "WHERE");
        StringAssert.Contains(sql, "ORDER BY");
        StringAssert.Contains(sql, "LIMIT");
        Assert.AreEqual(0, database.Commands.Reads, "SQL inspection must not enumerate the query.");
        Result[] actual = await query.ToArrayAsync();
        Result[] expected = shape.Apply(filter.Apply(SeedRows().AsQueryable())).ToArray();

        CollectionAssert.AreEqual(new[] { 2, 3 }, actual.Select(row => row.Id).ToArray());
        CollectionAssert.AreEqual(expected, actual);
        Assert.AreEqual(1, database.Commands.Reads);
        Assert.AreEqual(0, database.Context.ChangeTracker.Entries().Count());
        CollectionAssert.AreEqual(new[] { 2, 3, 5 }, adapter.Apply((IEnumerable<Row>)SeedRows(), restored).Select(row => row.Id).ToArray());
    }

    [TestMethod]
    public async Task LegacyQuerySchemaRehydratesThroughFilteringIntoSqlite()
    {
        await using var database = await Database.CreateAsync();
        var submitted = new CompositeFilter("And",
        [
            new PropertyFilter("NotEquals", nameof(Row.Name), null),
            new PropertyFilter("Contains", nameof(Row.Name), "Al"),
            new PropertyFilter("GreaterThanOrEqual", nameof(Row.Score), "10"),
            new PropertyFilter("NotIn", nameof(Row.Id), "[\"3\"]")
        ]);
        string json = QueryFilterSerializer.Serialize(submitted);
        Assert.AreEqual(0, QueryFilterSchemaValidator.Validate(json).Count);
        QueryFilter<Row> restored = QueryFilterSerializer.Deserialize(json)!.ToQueryFilter<Row>();
        var filter = new FilterSpec<Row>(restored.Predicate);
        var adapter = new EfFilterExecutionStrategy(database.Context);
        var shape = new QuerySpec<Row>().OrderByDescending(row => row.Score).ThenBy(row => row.Id)
            .Page(0, 10).Select(row => row.Id);
        IQueryable<int> query = shape.Apply(adapter.Apply(database.Context.Rows, Roundtrip(filter)));

        Assert.AreEqual(0, database.Commands.Reads);
        int[] actual = await query.ToArrayAsync();

        CollectionAssert.AreEqual(new[] { 5, 2 }, actual);
        CollectionAssert.AreEqual(shape.Apply(SeedRows().AsQueryable().Apply(restored)).ToArray(), actual);
        Assert.AreEqual(1, database.Commands.Reads);
    }

    [TestMethod]
    public async Task NullableNegationRetainsClrSemanticsAcrossSerializationAndDatabase()
    {
        await using var database = await Database.CreateAsync();
        var filter = new FilterSpec<Row>(row => row.Score >= 10).Not();
        var adapter = new EfFilterExecutionStrategy(database.Context);
        var shape = new QuerySpec<Row>().Where(new FilterSpec<Row>(row => row.Id > 0))
            .OrderBy(row => row.Id).Select(row => row.Id);
        int[] actual = await shape.Apply(adapter.Apply(database.Context.Rows, Roundtrip(filter))).ToArrayAsync();

        CollectionAssert.AreEqual(new[] { 1, 6 }, actual, "Null scores must survive negated lifted comparison.");
        CollectionAssert.AreEqual(shape.Apply(filter.Apply(SeedRows().AsQueryable())).ToArray(), actual);
        Assert.AreEqual(1, database.Commands.Reads);
    }

    [TestMethod]
    public async Task CanceledRelationalEnumerationLeavesBorrowedContextUsable()
    {
        await using var database = await Database.CreateAsync();
        var filter = new FilterSpec<Row>(row => row.Id > 1);
        var query = new QuerySpec<Row>().Where(filter).OrderBy(row => row.Id).Page(0, 2)
            .Apply(new EfFilterExecutionStrategy(database.Context).Apply(database.Context.Rows, Roundtrip(filter)));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => query.ToArrayAsync(cancellation.Token));

        CollectionAssert.AreEqual(new[] { 2, 3 }, await query.Select(row => row.Id).ToArrayAsync());
        Assert.AreEqual(6, await database.Context.Rows.CountAsync());
        Assert.AreEqual(System.Data.ConnectionState.Open, database.Context.Database.GetDbConnection().State);
    }

    [TestMethod]
    public async Task InvalidSerializedFilterFailsClosedBeforeRelationalExecution()
    {
        await using var database = await Database.CreateAsync();
        const string malformed = "{\"operator\":\"And\",\"children\":[{\"operator\":\"Equals\",\"property\":\"Id\",\"value\":\"2\"},{\"operator\":\"Unknown\",\"property\":\"Id\"}]}";

        await Assert.ThrowsExactlyAsync<JsonException>(async () =>
        {
            var restored = QueryFilterSerializer.Deserialize(malformed)!.ToQueryFilter<Row>();
            await new QuerySpec<Row>().Where(restored.Predicate).Apply(database.Context.Rows).ToArrayAsync();
        });

        Assert.AreEqual(0, database.Commands.Reads, "An invalid child cannot silently become an unrestricted query.");
    }

    private static PortableNode Roundtrip(FilterSpec<Row> filter) =>
        JsonSerializer.Deserialize<PortableNode>(JsonSerializer.Serialize(filter.ToFilterNode()))!;

    private static Row[] SeedRows() =>
    [
        new() { Id = 1, Name = null, Score = null, Bucket = 1 },
        new() { Id = 2, Name = "Alpha", Score = 10, Bucket = 1 },
        new() { Id = 3, Name = "Alpine", Score = 10, Bucket = 1 },
        new() { Id = 4, Name = "Beta", Score = 30, Bucket = 2 },
        new() { Id = 5, Name = "Alpha", Score = 20, Bucket = 1 },
        new() { Id = 6, Name = "Alpha", Score = 9, Bucket = 2 }
    ];

    public sealed class Row
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? Score { get; set; }
        public int Bucket { get; set; }
    }

    public sealed record Result(int Id, string? Name, int? Score);

    public sealed class RowsContext(DbContextOptions<RowsContext> options) : DbContext(options)
    {
        public DbSet<Row> Rows => Set<Row>();
    }

    private sealed class CommandCapture : DbCommandInterceptor
    {
        public int Reads { get; set; }
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Reads++;
            return ValueTask.FromResult(result);
        }
    }

    private sealed class Database(SqliteConnection connection, RowsContext context, CommandCapture commands) : IAsyncDisposable
    {
        public RowsContext Context { get; } = context;
        public CommandCapture Commands { get; } = commands;

        public static async Task<Database> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            var commands = new CommandCapture();
            var context = new RowsContext(new DbContextOptionsBuilder<RowsContext>().UseSqlite(connection).AddInterceptors(commands).Options);
            try
            {
                await connection.OpenAsync();
                await context.Database.EnsureCreatedAsync();
                context.Rows.AddRange(SeedRows());
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
                commands.Reads = 0;
                return new Database(connection, context, commands);
            }
            catch
            {
                await context.DisposeAsync();
                await connection.DisposeAsync();
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
