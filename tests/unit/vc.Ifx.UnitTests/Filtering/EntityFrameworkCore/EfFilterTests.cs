using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.Linq.Expressions;
using VisionaryCoder.Framework.Filtering;
using VisionaryCoder.Framework.Filtering.Abstractions;
using VisionaryCoder.Framework.Filtering.EFCore;

namespace VisionaryCoder.Framework.Tests.Filtering.EntityFrameworkCore;

[TestClass]
public sealed class EfFilterTests
{
    public sealed class Customer
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? Age { get; set; }
        public List<Order> Orders { get; set; } = [];
    }

    public sealed class Order
    {
        public int Id { get; set; }
        public int Total { get; set; }
    }

    public sealed class CustomersContext(DbContextOptions<CustomersContext> options) : DbContext(options)
    {
        public DbSet<Customer> Customers => Set<Customer>();
    }

    private static CustomersContext CreateInMemory() => new(new DbContextOptionsBuilder<CustomersContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    public static Customer[] Data() =>
    [
        new() { Id = 1, Name = "Alpha", Age = null },
        new() { Id = 2, Name = "Beta", Age = 20, Orders = [new() { Id = 1, Total = 100 }] },
        new() { Id = 3, Name = null, Age = 30, Orders = [new() { Id = 2, Total = 10 }] }
    ];

    [TestMethod]
    public async Task NestedPredicatesKeepQueryableProviderAndProjection()
    {
        using CustomersContext context = CreateInMemory();
        context.Customers.AddRange(Data());
        await context.SaveChangesAsync();
        var strategy = new EfFilterExecutionStrategy(context);
        FilterNode filter = ExpressionToFilterNode.Translate<Customer>(customer => customer.Orders.Any(order => order.Total > 50));
        IQueryable<Customer> query = strategy.Apply(context.Customers.AsQueryable(), filter);
        query.Provider.Should().BeSameAs(context.Customers.AsQueryable().Provider);
        (await query.OrderBy(customer => customer.Id).Skip(0).Take(1).Select(customer => customer.Name).ToArrayAsync()).Should().Equal("Beta");
        strategy.Apply((IEnumerable<Customer>)Data(), filter).Select(customer => customer.Id).Should().Equal(2);
    }

    [TestMethod]
    public void NullArgumentsAndNullFiltersHaveExplicitBehavior()
    {
        using CustomersContext context = CreateInMemory();
        var strategy = new EfFilterExecutionStrategy(context);
        IEnumerable<Customer> source = Data();
        IQueryable<Customer> query = source.AsQueryable();
        strategy.Apply(source, null).Should().BeSameAs(source);
        strategy.Apply(query, null).Should().BeSameAs(query);
        Action constructor = () => _ = new EfFilterExecutionStrategy(null!);
        Action enumerable = () => strategy.Apply((IEnumerable<Customer>)null!, null);
        Action queryable = () => strategy.Apply((IQueryable<Customer>)null!, null);
        constructor.Should().Throw<ArgumentNullException>();
        enumerable.Should().Throw<ArgumentNullException>();
        queryable.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public async Task InvalidFiltersCannotBroadenDatabaseQueries()
    {
        using CustomersContext context = CreateInMemory();
        context.Customers.AddRange(Data());
        await context.SaveChangesAsync();
        var strategy = new EfFilterExecutionStrategy(context);
        FilterNode[] invalid =
        [
            new FilterCondition("Unknown", FilterOperation.Equals, "1"),
            new FilterCondition("Age", (FilterOperation)99, "1"),
            new FilterCondition("Age", FilterOperation.In, "[\"invalid\"]")
        ];
        foreach (FilterNode node in invalid)
        {
            Action apply = () => strategy.Apply(context.Customers.AsQueryable(), node);
            apply.Should().Throw<Exception>();
        }
        (await strategy.Apply(context.Customers.AsQueryable(), new FilterCondition("Age", FilterOperation.In, "[]")).CountAsync()).Should().Be(0);
        (await strategy.Apply(context.Customers.AsQueryable(), new FilterCondition("Age", FilterOperation.Equals, null)).Select(customer => customer.Id).ToArrayAsync()).Should().Equal(1);
    }

    [TestMethod]
    public async Task RelationalProviderTranslatesFiltersWithoutClientFallback()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = new CustomersContext(new DbContextOptionsBuilder<CustomersContext>().UseSqlite(connection).Options);
        await context.Database.EnsureCreatedAsync();
        Customer[] customers = Data();
        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();
        var strategy = new EfFilterExecutionStrategy(context);
        Expression<Func<Customer, bool>>[] predicates =
        [
            customer => customer.Age == null,
            customer => customer.Age != null && customer.Age >= 20,
            customer => !(customer.Age > 20),
            customer => customer.Name != null && customer.Name.Contains("a"),
            customer => customer.Name != null && customer.Name.StartsWith("Al"),
            customer => customer.Name != null && customer.Name.EndsWith("ta"),
            customer => customer.Orders.Any(order => order.Total > 50),
            customer => customer.Orders.All(order => order.Total > 50),
            customer => new[] { 1, 3 }.Contains(customer.Id),
            customer => false,
            customer => true
        ];
        foreach (var predicate in predicates)
        {
            IQueryable<Customer> query = strategy.Apply(context.Customers.AsNoTracking(), ExpressionToFilterNode.Translate(predicate));
            query.ToQueryString().Should().Contain("SELECT");
            int[] actual = await query.OrderBy(customer => customer.Id).Select(customer => customer.Id).ToArrayAsync();
            actual.Should().Equal(customers.Where(predicate.Compile()).Select(customer => customer.Id));
        }
        IQueryable<Customer> paged = strategy.Apply(context.Customers.AsQueryable(), new FilterConstant(true))
            .OrderBy(customer => customer.Id).Skip(1).Take(1);
        paged.ToQueryString().Should().Contain("LIMIT");
        (await paged.Select(customer => customer.Name).SingleAsync()).Should().Be("Beta");
        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        Func<Task> execute = () => paged.ToArrayAsync(canceled.Token);
        await execute.Should().ThrowAsync<OperationCanceledException>();
    }
}
