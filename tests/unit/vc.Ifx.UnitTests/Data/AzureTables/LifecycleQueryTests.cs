using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Microsoft.Extensions.Logging.Abstractions;
using VisionaryCoder.Framework.Data.Azure.Table;

namespace VisionaryCoder.Framework.Tests.Data.AzureTables;

[TestClass]
public sealed class LifecycleQueryTests
{
    [TestMethod]
    public async Task ExistenceChecksNeverCreateTableAndDisposeAsyncEnumeration()
    {
        var table = new TableFake();
        var service = new ServiceFake(table);
        using var provider = new AzureTableStorageProvider(TableFixture.Options(create: true),
            NullLogger<AzureTableStorageProvider>.Instance, service);
        using var cancellation = new CancellationTokenSource();
        provider.TableExists().Should().BeFalse();
        (await provider.TableExistsAsync(cancellation.Token)).Should().BeFalse();
        service.Pages.Add(TableFixture.Page<TableItem>([], "continue"));
        service.Pages.Add(TableFixture.Page<TableItem>([TableModelFactory.TableItem("Entities")]));
        provider.TableExists().Should().BeTrue();
        (await provider.TableExistsAsync(cancellation.Token)).Should().BeTrue();
        service.Filter.Should().Be("TableName eq 'Entities'");
        service.Token.Should().Be(cancellation.Token);
        service.AsyncPages!.Disposals.Should().Be(1);
        table.Creates.Should().Be(0);
        service.Failure = new RequestFailedException(403, "denied");
        Action sync = () => provider.TableExists();
        Func<Task> asyncCall = () => provider.TableExistsAsync();
        sync.Should().Throw<RequestFailedException>();
        await asyncCall.Should().ThrowAsync<RequestFailedException>();
    }

    [TestMethod]
    public async Task ExistenceHonorsCancellationDuringEmptyAndNonemptyEnumeration()
    {
        foreach (bool hasItem in new[] { false, true })
        {
            using var cancellation = new CancellationTokenSource();
            var service = new ServiceFake(new());
            if (hasItem)
            {
                service.Pages.Add(TableFixture.Page<TableItem>([TableModelFactory.TableItem("Entities")]));
                service.OnYield = cancellation.Cancel;
            }
            else service.OnComplete = cancellation.Cancel;
            using var provider = new AzureTableStorageProvider(TableFixture.Options(), NullLogger<AzureTableStorageProvider>.Instance, service);
            Func<Task> call = () => provider.TableExistsAsync(cancellation.Token);
            await call.Should().ThrowAsync<OperationCanceledException>();
            service.AsyncPages!.Disposals.Should().Be(1);
        }
    }

    [TestMethod]
    public async Task SuccessfulInitializationIsReusedAcrossSyncAndAsyncOperations()
    {
        var table = new TableFake();
        using var provider = TableFixture.Provider(table, TableFixture.Options(create: true));
        provider.AddEntity(TableFixture.Entity());
        provider.AddEntity(TableFixture.Entity());
        await provider.AddEntityAsync(TableFixture.Entity());
        table.Creates.Should().Be(1);

        var asyncTable = new TableFake();
        using var asyncProvider = TableFixture.Provider(asyncTable, TableFixture.Options(create: true));
        await asyncProvider.AddEntityAsync(TableFixture.Entity());
        asyncProvider.AddEntity(TableFixture.Entity());
        await asyncProvider.AddEntityAsync(TableFixture.Entity());
        asyncTable.Creates.Should().Be(1);
    }

    [TestMethod]
    public async Task FailedInitializationReleasesGateAndIsRetriedOnlyOnNextOperation()
    {
        foreach (bool asynchronous in new[] { false, true })
        {
            var table = new TableFake { Failure = new RequestFailedException(503, "unavailable") };
            using var provider = TableFixture.Provider(table, TableFixture.Options(create: true));
            if (asynchronous)
            {
                Func<Task> action = () => provider.AddEntityAsync(TableFixture.Entity());
                await action.Should().ThrowAsync<RequestFailedException>();
            }
            else
            {
                Action action = () => provider.AddEntity(TableFixture.Entity());
                action.Should().Throw<RequestFailedException>();
            }
            table.Creates.Should().Be(1);
            table.Failure = null;
            await provider.AddEntityAsync(TableFixture.Entity());
            table.Creates.Should().Be(2);
            table.Calls.Count(call => call.Operation == "addAsync").Should().Be(1);
        }
    }

    [TestMethod]
    public async Task InitializationSerializesAndCanceledWaiterDoesNotBlockOthers()
    {
        var initialized = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var table = new TableFake { Initialize = token => initialized.Task };
        using var provider = TableFixture.Provider(table, TableFixture.Options(create: true));
        Task first = provider.AddEntityAsync(TableFixture.Entity("first"));
        using var cancellation = new CancellationTokenSource();
        Task waiting = provider.AddEntityAsync(TableFixture.Entity("canceled"), cancellation.Token);
        Task last = provider.AddEntityAsync(TableFixture.Entity("last"));
        cancellation.Cancel();
        Func<Task> wait = () => waiting;
        await wait.Should().ThrowAsync<OperationCanceledException>();
        table.Creates.Should().Be(1);
        initialized.SetResult();
        await Task.WhenAll(first, last);
        table.Creates.Should().Be(1);
        table.Calls.Count(call => call.Operation == "addAsync").Should().Be(2);
    }

    [TestMethod]
    public async Task CancellationAfterInitializationAllowsLaterRetry()
    {
        using var cancellation = new CancellationTokenSource();
        var table = new TableFake { Initialize = token => { cancellation.Cancel(); return Task.CompletedTask; } };
        using var provider = TableFixture.Provider(table, TableFixture.Options(create: true));
        Func<Task> call = () => provider.AddEntityAsync(TableFixture.Entity(), cancellation.Token);
        await call.Should().ThrowAsync<OperationCanceledException>();
        table.Initialize = null;
        await provider.AddEntityAsync(TableFixture.Entity());
        table.Creates.Should().Be(2);
    }

    [TestMethod]
    public async Task DisposalIsIdempotentAndPreventsFurtherOperations()
    {
        var table = new TableFake();
        var provider = TableFixture.Provider(table);
        provider.Dispose();
        provider.Dispose();
        Action read = () => provider.GetEntity<TableEntity>("partition", "row");
        Action exists = () => provider.TableExists();
        Func<Task> asyncRead = () => provider.GetEntityAsync<TableEntity>("partition", "row");
        read.Should().Throw<ObjectDisposedException>();
        exists.Should().Throw<ObjectDisposedException>();
        await asyncRead.Should().ThrowAsync<ObjectDisposedException>();
        table.Calls.Should().BeEmpty();
    }

    [TestMethod]
    public async Task QueriesConsumeEveryPageAndPreserveOrderingAndPageHints()
    {
        var table = new TableFake();
        var first = TableFixture.Entity("first");
        var last = TableFixture.Entity("last");
        table.Pages.Add(TableFixture.Page<TableEntity>([first], "page2"));
        table.Pages.Add(TableFixture.Page<TableEntity>([], "page3"));
        table.Pages.Add(TableFixture.Page<TableEntity>([last]));
        using var provider = TableFixture.Provider(table);
        provider.QueryEntities<TableEntity>("Value eq 42").Should().Equal(first, last);
        table.PageSize.Should().Be(7);
        using var cancellation = new CancellationTokenSource();
        (await provider.QueryEntitiesAsync<TableEntity>("Value eq 42", 1, cancellation.Token)).Should().Equal(first, last);
        table.Filter.Should().Be("Value eq 42");
        table.PageSize.Should().Be(1);
        table.AsyncPages!.Token.Should().Be(cancellation.Token);
        table.AsyncPages.Disposals.Should().Be(1);
        provider.QueryEntities<TableEntity>(maxPerPage: 1000).Should().HaveCount(2);
        foreach (int invalid in new[] { 0, 1001 })
        {
            Action query = () => provider.QueryEntities<TableEntity>(maxPerPage: invalid);
            Func<Task> queryAsync = () => provider.QueryEntitiesAsync<TableEntity>(maxPerPage: invalid);
            query.Should().Throw<ArgumentOutOfRangeException>();
            await queryAsync.Should().ThrowAsync<ArgumentOutOfRangeException>();
        }
        table.Pages.Clear();
        provider.QueryEntities<TableEntity>().Should().BeEmpty();
        (await provider.QueryEntitiesAsync<TableEntity>()).Should().BeEmpty();
    }

    [TestMethod]
    public async Task PartitionFiltersEscapeApostrophesInsteadOfConcatenatingOData()
    {
        var table = new TableFake();
        using var provider = TableFixture.Provider(table);
        provider.GetEntitiesByPartitionKey<TableEntity>("a' or RowKey eq 'x");
        table.Filter.Should().Be("PartitionKey eq 'a'' or RowKey eq ''x'");
        await provider.GetEntitiesByPartitionKeyAsync<TableEntity>("a'b");
        table.Filter.Should().Be("PartitionKey eq 'a''b'");
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task StreamingLinksIndependentMethodAndEnumeratorTokens(bool cancel)
    {
        var table = new TableFake();
        table.Pages.Add(TableFixture.Page<TableEntity>([TableFixture.Entity()]));
        using var provider = TableFixture.Provider(table);
        using var methodCancellation = new CancellationTokenSource();
        using var enumerationCancellation = new CancellationTokenSource();
        if (cancel) table.OnYield = enumerationCancellation.Cancel;
        await using IAsyncEnumerator<TableEntity> enumerator = provider
            .EnumerateEntitiesAsync<TableEntity>(cancellationToken: methodCancellation.Token)
            .GetAsyncEnumerator(enumerationCancellation.Token);

        if (cancel)
        {
            Func<Task> next = () => enumerator.MoveNextAsync().AsTask();
            await next.Should().ThrowAsync<OperationCanceledException>();
        }
        else
        {
            (await enumerator.MoveNextAsync()).Should().BeTrue();
            (await enumerator.MoveNextAsync()).Should().BeFalse();
        }

        table.AsyncPages!.Token.Should().NotBe(methodCancellation.Token);
        table.AsyncPages.Token.Should().NotBe(enumerationCancellation.Token);
        table.AsyncPages.Disposals.Should().Be(1);
    }

    [TestMethod]
    public async Task StreamingDisposesOnEarlyExitFailureAndCancellation()
    {
        var table = new TableFake();
        table.Pages.Add(TableFixture.Page<TableEntity>([TableFixture.Entity(), TableFixture.Entity("second")]));
        using var provider = TableFixture.Provider(table);
        await foreach (var item in provider.EnumerateEntitiesAsync<TableEntity>()) break;
        table.AsyncPages!.Disposals.Should().Be(1);
        var failure = new InvalidOperationException("enumeration failure");
        table.OnYield = () => throw failure;
        Func<Task> failed = () => provider.QueryEntitiesAsync<TableEntity>();
        (await failed.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(failure);
        table.AsyncPages!.Disposals.Should().Be(1);

        using var cancellation = new CancellationTokenSource();
        table.OnYield = cancellation.Cancel;
        Func<Task> canceled = () => provider.QueryEntitiesAsync<TableEntity>(cancellationToken: cancellation.Token);
        await canceled.Should().ThrowAsync<OperationCanceledException>();
        table.AsyncPages!.Disposals.Should().Be(1);

        table.Pages.Clear();
        table.OnYield = null;
        using var emptyCancellation = new CancellationTokenSource();
        table.OnComplete = emptyCancellation.Cancel;
        Func<Task> emptyCanceled = () => provider.QueryEntitiesAsync<TableEntity>(cancellationToken: emptyCancellation.Token);
        await emptyCanceled.Should().ThrowAsync<OperationCanceledException>();
        table.AsyncPages!.Disposals.Should().Be(1);
    }
}
