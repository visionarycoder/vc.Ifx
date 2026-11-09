using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ifx.Querying;
using Ifx.Querying.EntityFrameworkCore;

namespace Ifx.Tests.Filtering.EntityFrameworkCore;

[TestClass]
public sealed class QueryPagingTests
{
    [TestMethod]
    public async Task ToPagedResultAsync_DelayedPage_AwaitsBeforeCounting()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var interceptor = new Gate();
        var options = new DbContextOptionsBuilder<Context>().UseSqlite(connection).AddInterceptors(interceptor).Options;
        await using var context = new Context(options);
        await context.Database.EnsureCreatedAsync();
        context.Rows.AddRange(new Row { Id = 1 }, new Row { Id = 2 }, new Row { Id = 3 });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        interceptor.Armed = true;
        Task<PagedResult<Row>> pending = QuerySpec<Row>.For(context.Rows).Where(row => row.Id > 1)
            .OrderBy(row => row.Id).TakeItems(1).ToPagedResultAsync();
        try
        {
            await interceptor.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally
        {
            interceptor.Release.TrySetResult();
        }
        PagedResult<Row> page = await pending.WaitAsync(TimeSpan.FromSeconds(10));
        page.Items.Select(row => row.Id).Should().Equal(2);
        page.TotalCount.Should().Be(2);
        interceptor.Reads.Should().Be(2);
        context.ChangeTracker.Entries().Should().BeEmpty();
    }

    private sealed class Row { public int Id { get; set; } }
    private sealed class Context(DbContextOptions<Context> options) : DbContext(options)
    {
        public DbSet<Row> Rows => Set<Row>();
    }

    private sealed class Gate : DbCommandInterceptor
    {
        public bool Armed { get; set; }
        public int Reads { get; private set; }
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            if (Armed && ++Reads == 1)
            {
                Entered.TrySetResult();
                await Release.Task.WaitAsync(cancellationToken);
            }
            return result;
        }
    }
}
