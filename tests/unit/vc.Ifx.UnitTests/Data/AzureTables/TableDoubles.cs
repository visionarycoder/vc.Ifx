using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Runtime.CompilerServices;
using VisionaryCoder.Framework.Data.Azure.Table;

namespace VisionaryCoder.Framework.Tests.Data.AzureTables;

internal static class TableFixture
{
    public static Response Raw => Mock.Of<Response>();
    public static AzureTableStorageOptions Options(bool create = false, bool concurrency = true, int batch = 100) => new()
    {
        TableName = "Entities", ConnectionString = "UseDevelopmentStorage=true", CreateTableIfNotExists = create,
        EnableOptimisticConcurrency = concurrency, MaxEntitiesPerBatch = batch, MaxEntitiesPerQuery = 7
    };
    public static AzureTableStorageProvider Provider(TableFake table, AzureTableStorageOptions? options = null) =>
        new(options ?? Options(), NullLogger<AzureTableStorageProvider>.Instance, new ServiceFake(table));
    public static TableEntity Entity(string row = "row", string partition = "partition") => new(partition, row) { ETag = new ETag("entity-tag"), ["Value"] = 42 };
    public static Page<T> Page<T>(IReadOnlyList<T> values, string? continuation = null) => global::Azure.Page<T>.FromValues(values, continuation, Raw);
}

internal sealed class ServiceFake(TableFake table) : TableServiceClient
{
    public bool ReturnNull { get; set; }
    public string? TableName { get; private set; }
    public string? Filter { get; private set; }
    public CancellationToken Token { get; private set; }
    public Exception? Failure { get; set; }
    public List<Page<TableItem>> Pages { get; } = [];
    public TestPages<TableItem>? AsyncPages { get; private set; }
    public Action? OnYield { get; set; }
    public Action? OnComplete { get; set; }
    public override TableClient GetTableClient(string tableName) { TableName = tableName; return ReturnNull ? null! : table; }
    public override Pageable<TableItem> Query(string? filter = null, int? maxPerPage = null, CancellationToken cancellationToken = default)
    {
        Filter = filter; Token = cancellationToken;
        if (Failure is not null) throw Failure;
        return Pageable<TableItem>.FromPages(Pages);
    }
    public override AsyncPageable<TableItem> QueryAsync(string? filter = null, int? maxPerPage = null, CancellationToken cancellationToken = default)
    {
        Filter = filter; Token = cancellationToken;
        if (Failure is not null) throw Failure;
        return AsyncPages = new TestPages<TableItem>(Pages) { OnYield = OnYield, OnComplete = OnComplete };
    }
}

internal sealed class TableFake : TableClient
{
    public Action<string>? OnCall { get; set; }
    public List<(string Operation, ITableEntity? Entity, ETag Tag, TableUpdateMode Mode, CancellationToken Token)> Calls { get; } = [];
    public Exception? Failure { get; set; }
    public Func<CancellationToken, Task>? Initialize { get; set; }
    public int Creates { get; private set; }
    public ITableEntity Result { get; set; } = TableFixture.Entity();
    public string? Partition { get; private set; }
    public string? Row { get; private set; }
    public string? Filter { get; private set; }
    public int? PageSize { get; private set; }
    public List<TableTransactionAction>? Batch { get; private set; }
    public List<Page<TableEntity>> Pages { get; } = [];
    public TestPages<TableEntity>? AsyncPages { get; private set; }
    public Action? OnYield { get; set; }
    public Action? OnComplete { get; set; }

    private Response Record(string operation, CancellationToken token, ITableEntity? entity = null, ETag tag = default, TableUpdateMode mode = TableUpdateMode.Replace)
    {
        lock (Calls) Calls.Add((operation, entity, tag, mode, token));
        OnCall?.Invoke(operation);
        if (Failure is not null) throw Failure;
        return TableFixture.Raw;
    }
    public override Response<TableItem> CreateIfNotExists(CancellationToken cancellationToken = default)
    {
        Creates++;
        Record("create", cancellationToken);
        return Response.FromValue(TableModelFactory.TableItem("Entities"), TableFixture.Raw);
    }
    public override async Task<Response<TableItem>> CreateIfNotExistsAsync(CancellationToken cancellationToken = default)
    {
        Creates++;
        Record("createAsync", cancellationToken);
        if (Initialize is not null) await Initialize(cancellationToken);
        return Response.FromValue(TableModelFactory.TableItem("Entities"), TableFixture.Raw);
    }
    public override Response AddEntity<T>(T entity, CancellationToken cancellationToken = default) => Record("add", cancellationToken, entity);
    public override Task<Response> AddEntityAsync<T>(T entity, CancellationToken cancellationToken = default) => Task.FromResult(Record("addAsync", cancellationToken, entity));
    public override Response UpdateEntity<T>(T entity, ETag ifMatch, TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default) => Record("update", cancellationToken, entity, ifMatch, mode);
    public override Task<Response> UpdateEntityAsync<T>(T entity, ETag ifMatch, TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default) => Task.FromResult(Record("updateAsync", cancellationToken, entity, ifMatch, mode));
    public override Response UpsertEntity<T>(T entity, TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default) => Record("upsert", cancellationToken, entity, mode: mode);
    public override Task<Response> UpsertEntityAsync<T>(T entity, TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default) => Task.FromResult(Record("upsertAsync", cancellationToken, entity, mode: mode));
    public override Response DeleteEntity(string partitionKey, string rowKey, ETag ifMatch = default, CancellationToken cancellationToken = default)
    {
        Partition = partitionKey; Row = rowKey;
        return Record("delete", cancellationToken, tag: ifMatch);
    }
    public override Task<Response> DeleteEntityAsync(string partitionKey, string rowKey, ETag ifMatch = default, CancellationToken cancellationToken = default)
    {
        Partition = partitionKey; Row = rowKey;
        return Task.FromResult(Record("deleteAsync", cancellationToken, tag: ifMatch));
    }
    public override Response<T> GetEntity<T>(string partitionKey, string rowKey, IEnumerable<string>? select = null, CancellationToken cancellationToken = default)
    {
        Partition = partitionKey; Row = rowKey;
        Record("get", cancellationToken);
        return Response.FromValue((T)Result, TableFixture.Raw);
    }
    public override Task<Response<T>> GetEntityAsync<T>(string partitionKey, string rowKey, IEnumerable<string>? select = null, CancellationToken cancellationToken = default)
    {
        Partition = partitionKey; Row = rowKey;
        Record("getAsync", cancellationToken);
        return Task.FromResult(Response.FromValue((T)Result, TableFixture.Raw));
    }
    public override Pageable<T> Query<T>(string? filter = null, int? maxPerPage = null, IEnumerable<string>? select = null, CancellationToken cancellationToken = default)
    {
        Filter = filter; PageSize = maxPerPage;
        Record("query", cancellationToken);
        return (Pageable<T>)(object)Pageable<TableEntity>.FromPages(Pages);
    }
    public override AsyncPageable<T> QueryAsync<T>(string? filter = null, int? maxPerPage = null, IEnumerable<string>? select = null, CancellationToken cancellationToken = default)
    {
        Filter = filter; PageSize = maxPerPage;
        Record("queryAsync", cancellationToken);
        AsyncPages = new TestPages<TableEntity>(Pages) { OnYield = OnYield, OnComplete = OnComplete };
        return (AsyncPageable<T>)(object)AsyncPages;
    }
    public override Response<IReadOnlyList<Response>> SubmitTransaction(IEnumerable<TableTransactionAction> transactionActions, CancellationToken cancellationToken = default)
    {
        Batch = transactionActions.ToList();
        Record("batch", cancellationToken);
        return Response.FromValue<IReadOnlyList<Response>>([], TableFixture.Raw);
    }
    public override Task<Response<IReadOnlyList<Response>>> SubmitTransactionAsync(IEnumerable<TableTransactionAction> transactionActions, CancellationToken cancellationToken = default)
    {
        Batch = transactionActions.ToList();
        Record("batchAsync", cancellationToken);
        return Task.FromResult(Response.FromValue<IReadOnlyList<Response>>([], TableFixture.Raw));
    }
}

internal sealed class TestPages<T>(IEnumerable<Page<T>> pages) : AsyncPageable<T> where T : notnull
{
    public int Disposals { get; private set; }
    public CancellationToken Token { get; private set; }
    public Action? OnYield { get; init; }
    public Action? OnComplete { get; init; }
    public override async IAsyncEnumerable<Page<T>> AsPages(string? continuationToken = null, int? pageSizeHint = null)
    {
        foreach (var page in pages) yield return page;
        await Task.CompletedTask;
    }
    public override async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        Token = cancellationToken;
        try
        {
            await foreach (var page in AsPages())
                foreach (var item in page.Values)
                {
                    OnYield?.Invoke();
                    yield return item;
                }
            OnComplete?.Invoke();
        }
        finally { Disposals++; }
    }
}
