using Azure;
using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace VisionaryCoder.Framework.Data.Azure.Table;

/// <summary>Owns table semantics while the injected Azure SDK owns transport and retries.</summary>
public sealed class AzureTableStorageProvider : ServiceBase<AzureTableStorageProvider>, ITableStorageProvider
{
    private readonly AzureTableStorageOptions options;
    private readonly TableServiceClient tableServiceClient;
    private readonly TableClient tableClient;
    private readonly SemaphoreSlim initialization = new(1, 1);
    private bool initialized;

    /// <summary>Constructs clients without contacting the service.</summary>
    public AzureTableStorageProvider(AzureTableStorageOptions options, ILogger<AzureTableStorageProvider> logger)
        : this(options, logger, CreateServiceClient(options)) { }

    /// <summary>Borrows SDK clients; virtual SDK methods provide the replaceable transport boundary.</summary>
    public AzureTableStorageProvider(AzureTableStorageOptions options, ILogger<AzureTableStorageProvider> logger,
        TableServiceClient tableServiceClient) : base(logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        options.Validate();
        this.tableServiceClient = tableServiceClient ?? throw new ArgumentNullException(nameof(tableServiceClient));
        tableClient = tableServiceClient.GetTableClient(options.TableName)
            ?? throw new ArgumentException("The service client must return a table client.", nameof(tableServiceClient));
        initialized = !options.CreateTableIfNotExists;
    }

    private static TableServiceClient CreateServiceClient(AzureTableStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var clientOptions = options.CreateClientOptions();
        return options.UseManagedIdentity
            ? new TableServiceClient(new Uri(options.StorageAccountUri!), new DefaultAzureCredential(), clientOptions)
            : new TableServiceClient(options.ConnectionString, clientOptions);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        // This sealed provider has no finalizer; callers quiesce operations before disposal.
        initialization.Dispose();
        base.Dispose(disposing);
    }

    /// <inheritdoc />
    public bool TableExists()
    {
        CheckOperation(default);
        return tableServiceClient.Query(filter: TableClient.CreateQueryFilter($"TableName eq {options.TableName}")).Any();
    }

    /// <inheritdoc />
    public async Task<bool> TableExistsAsync(CancellationToken cancellationToken = default)
    {
        CheckOperation(cancellationToken);
        await foreach (var item in tableServiceClient.QueryAsync(
            filter: TableClient.CreateQueryFilter($"TableName eq {options.TableName}"), cancellationToken: cancellationToken)
            .WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            return true;
        }
        cancellationToken.ThrowIfCancellationRequested();
        return false;
    }

    /// <inheritdoc />
    public void AddEntity<T>(T entity) where T : class, ITableEntity
    {
        ValidateEntity(entity);
        Prepare();
        tableClient.AddEntity(entity);
    }

    /// <inheritdoc />
    public async Task AddEntityAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class, ITableEntity
    {
        ValidateEntity(entity);
        await PrepareAsync(cancellationToken).ConfigureAwait(false);
        await tableClient.AddEntityAsync(entity, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
    }

    /// <inheritdoc />
    public void UpdateEntity<T>(T entity, ETag etag = default, TableUpdateMode mode = TableUpdateMode.Replace) where T : class, ITableEntity
    {
        ValidateEntity(entity);
        ValidateMode(mode);
        ETag condition = ResolveETag(etag, entity.ETag);
        Prepare();
        tableClient.UpdateEntity(entity, condition, mode);
    }

    /// <inheritdoc />
    public async Task UpdateEntityAsync<T>(T entity, ETag etag = default, TableUpdateMode mode = TableUpdateMode.Replace,
        CancellationToken cancellationToken = default) where T : class, ITableEntity
    {
        ValidateEntity(entity);
        ValidateMode(mode);
        ETag condition = ResolveETag(etag, entity.ETag);
        await PrepareAsync(cancellationToken).ConfigureAwait(false);
        await tableClient.UpdateEntityAsync(entity, condition, mode, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
    }

    /// <inheritdoc />
    public void UpsertEntity<T>(T entity, TableUpdateMode mode = TableUpdateMode.Replace) where T : class, ITableEntity
    {
        ValidateEntity(entity);
        ValidateMode(mode);
        Prepare();
        tableClient.UpsertEntity(entity, mode);
    }

    /// <inheritdoc />
    public async Task UpsertEntityAsync<T>(T entity, TableUpdateMode mode = TableUpdateMode.Replace,
        CancellationToken cancellationToken = default) where T : class, ITableEntity
    {
        ValidateEntity(entity);
        ValidateMode(mode);
        await PrepareAsync(cancellationToken).ConfigureAwait(false);
        await tableClient.UpsertEntityAsync(entity, mode, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
    }

    /// <inheritdoc />
    public void DeleteEntity(string partitionKey, string rowKey, ETag etag = default)
    {
        ValidateKeys(partitionKey, rowKey);
        ETag condition = ResolveETag(etag, default);
        Prepare();
        tableClient.DeleteEntity(partitionKey, rowKey, condition);
    }

    /// <inheritdoc />
    public async Task DeleteEntityAsync(string partitionKey, string rowKey, ETag etag = default, CancellationToken cancellationToken = default)
    {
        ValidateKeys(partitionKey, rowKey);
        ETag condition = ResolveETag(etag, default);
        await PrepareAsync(cancellationToken).ConfigureAwait(false);
        await tableClient.DeleteEntityAsync(partitionKey, rowKey, condition, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
    }

    /// <inheritdoc />
    public T? GetEntity<T>(string partitionKey, string rowKey) where T : class, ITableEntity
    {
        ValidateKeys(partitionKey, rowKey);
        Prepare();
        try { return tableClient.GetEntity<T>(partitionKey, rowKey).Value; }
        catch (RequestFailedException exception) when (exception.Status == 404) { return null; }
    }

    /// <inheritdoc />
    public async Task<T?> GetEntityAsync<T>(string partitionKey, string rowKey, CancellationToken cancellationToken = default)
        where T : class, ITableEntity
    {
        ValidateKeys(partitionKey, rowKey);
        await PrepareAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            Response<T> response = await tableClient.GetEntityAsync<T>(partitionKey, rowKey, cancellationToken: cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return response.Value;
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return null;
        }
    }

    /// <inheritdoc />
    public List<T> QueryEntities<T>(string? filter = null, int? maxPerPage = null) where T : class, ITableEntity
    {
        int pageSize = PageSize(maxPerPage);
        Prepare();
        return tableClient.Query<T>(filter, pageSize).ToList();
    }

    /// <inheritdoc />
    public async Task<List<T>> QueryEntitiesAsync<T>(string? filter = null, int? maxPerPage = null, CancellationToken cancellationToken = default)
        where T : class, ITableEntity
    {
        var results = new List<T>();
        await foreach (var entity in EnumerateEntitiesAsync<T>(filter, maxPerPage, cancellationToken).ConfigureAwait(false))
            results.Add(entity);
        return results;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<T> EnumerateEntitiesAsync<T>(string? filter = null, int? maxPerPage = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : class, ITableEntity
    {
        int pageSize = PageSize(maxPerPage);
        await PrepareAsync(cancellationToken).ConfigureAwait(false);
        await foreach (var entity in tableClient.QueryAsync<T>(filter, pageSize, cancellationToken: cancellationToken)
            .WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return entity;
        }
        cancellationToken.ThrowIfCancellationRequested();
    }

    /// <inheritdoc />
    public void SubmitBatch(IEnumerable<TableTransactionAction> actions)
    {
        var batch = ValidateBatch(actions, default);
        Prepare();
        tableClient.SubmitTransaction(batch);
    }

    /// <inheritdoc />
    public async Task SubmitBatchAsync(IEnumerable<TableTransactionAction> actions, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var batch = ValidateBatch(actions, cancellationToken);
        await PrepareAsync(cancellationToken).ConfigureAwait(false);
        await tableClient.SubmitTransactionAsync(batch, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
    }

    /// <inheritdoc />
    public List<T> GetEntitiesByPartitionKey<T>(string partitionKey) where T : class, ITableEntity
    {
        ValidateKey(partitionKey, nameof(partitionKey));
        return QueryEntities<T>(TableClient.CreateQueryFilter($"PartitionKey eq {partitionKey}"));
    }

    /// <inheritdoc />
    public Task<List<T>> GetEntitiesByPartitionKeyAsync<T>(string partitionKey, CancellationToken cancellationToken = default)
        where T : class, ITableEntity
    {
        ValidateKey(partitionKey, nameof(partitionKey));
        return QueryEntitiesAsync<T>(TableClient.CreateQueryFilter($"PartitionKey eq {partitionKey}"), cancellationToken: cancellationToken);
    }

    private void CheckOperation(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        Logger.LogDebug("Table operation for {TableName}", options.TableName);
    }

    private void Prepare()
    {
        CheckOperation(default);
        initialization.Wait();
        try
        {
            if (initialized) return;
            tableClient.CreateIfNotExists();
            initialized = true;
        }
        finally { initialization.Release(); }
    }

    private async Task PrepareAsync(CancellationToken cancellationToken)
    {
        CheckOperation(cancellationToken);
        await initialization.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (initialized) return;
            await tableClient.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            initialized = true;
        }
        finally { initialization.Release(); }
    }

    private int PageSize(int? maxPerPage)
    {
        int size = maxPerPage ?? options.MaxEntitiesPerQuery;
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1, nameof(maxPerPage));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(size, 1000, nameof(maxPerPage));
        return size;
    }

    private ETag ResolveETag(ETag explicitTag, ETag entityTag)
    {
        if (!string.IsNullOrEmpty(explicitTag.ToString())) return explicitTag;
        if (!options.EnableOptimisticConcurrency) return ETag.All;
        if (string.IsNullOrEmpty(entityTag.ToString()) || entityTag == ETag.All)
            throw new ArgumentException("A conditional ETag is required; pass ETag.All explicitly for an unconditional operation.", nameof(explicitTag));
        return entityTag;
    }

    private static void ValidateEntity(ITableEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ValidateKeys(entity.PartitionKey, entity.RowKey);
    }

    private static void ValidateKeys(string partitionKey, string rowKey)
    {
        ValidateKey(partitionKey, nameof(partitionKey));
        ValidateKey(rowKey, nameof(rowKey));
    }

    private static void ValidateKey(string key, string name)
    {
        ArgumentNullException.ThrowIfNull(key, name);
        if (key.Length > 1024 || key.Any(character => character is '/' or '\\' or '#' or '?' or <= '\u001f' or >= '\u007f' and <= '\u009f'))
            throw new ArgumentException("Invalid Azure Table key.", name);
    }

    private static void ValidateMode(TableUpdateMode mode)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
    }

    private List<TableTransactionAction> ValidateBatch(IEnumerable<TableTransactionAction> actions, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actions);
        var batch = new List<TableTransactionAction>();
        var rows = new HashSet<string>(StringComparer.Ordinal);
        string? partition = null;
        foreach (var action in actions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(action);
            if (batch.Count == options.MaxEntitiesPerBatch)
                throw new InvalidOperationException($"Batch operation cannot contain more than {options.MaxEntitiesPerBatch} actions.");
            ValidateEntity(action.Entity);
            if (!Enum.IsDefined(action.ActionType)) throw new ArgumentOutOfRangeException(nameof(actions));
            partition ??= action.Entity.PartitionKey;
            if (partition != action.Entity.PartitionKey || !rows.Add(action.Entity.RowKey))
                throw new ArgumentException("Batch entities must share a partition and have unique row keys.", nameof(actions));
            ETag condition = action.ActionType is TableTransactionActionType.UpdateMerge or TableTransactionActionType.UpdateReplace or TableTransactionActionType.Delete
                ? ResolveETag(action.ETag, action.Entity.ETag) : action.ETag;
            batch.Add(new TableTransactionAction(action.ActionType, action.Entity, condition));
        }
        if (batch.Count == 0) throw new ArgumentException("A batch requires at least one action.", nameof(actions));
        return batch;
    }
}
