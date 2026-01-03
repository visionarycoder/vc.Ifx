using Azure;
using Azure.Data.Tables;

namespace VisionaryCoder.Framework.Data.Azure.Table;

/// <summary>
/// Defines NoSQL table-oriented storage operations for CRUD, queries and batch operations.
/// This interface separates table semantics from file/directory storage concerns.
/// </summary>
public interface ITableStorageProvider
{
    bool TableExists();
    Task<bool> TableExistsAsync(CancellationToken cancellationToken = default);

    void AddEntity<T>(T entity) where T : class, ITableEntity;
    Task AddEntityAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class, ITableEntity;

    void UpdateEntity<T>(T entity, ETag etag = default, TableUpdateMode mode = TableUpdateMode.Replace) where T : class, ITableEntity;
    Task UpdateEntityAsync<T>(T entity, ETag etag = default, TableUpdateMode mode = TableUpdateMode.Replace, CancellationToken cancellationToken = default) where T : class, ITableEntity;

    void UpsertEntity<T>(T entity, TableUpdateMode mode = TableUpdateMode.Replace) where T : class, ITableEntity;
    Task UpsertEntityAsync<T>(T entity, TableUpdateMode mode = TableUpdateMode.Replace, CancellationToken cancellationToken = default) where T : class, ITableEntity;

    void DeleteEntity(string partitionKey, string rowKey, ETag etag = default);
    Task DeleteEntityAsync(string partitionKey, string rowKey, ETag etag = default, CancellationToken cancellationToken = default);

    T? GetEntity<T>(string partitionKey, string rowKey) where T : class, ITableEntity;
    Task<T?> GetEntityAsync<T>(string partitionKey, string rowKey, CancellationToken cancellationToken = default) where T : class, ITableEntity;

    List<T> QueryEntities<T>(string? filter = null, int? maxPerPage = null) where T : class, ITableEntity;
    Task<List<T>> QueryEntitiesAsync<T>(string? filter = null, int? maxPerPage = null, CancellationToken cancellationToken = default) where T : class, ITableEntity;

    IAsyncEnumerable<T> EnumerateEntitiesAsync<T>(string? filter = null, int? maxPerPage = null, CancellationToken cancellationToken = default) where T : class, ITableEntity;

    void SubmitBatch(IEnumerable<TableTransactionAction> actions);
    Task SubmitBatchAsync(IEnumerable<TableTransactionAction> actions, CancellationToken cancellationToken = default);

    List<T> GetEntitiesByPartitionKey<T>(string partitionKey) where T : class, ITableEntity;
    Task<List<T>> GetEntitiesByPartitionKeyAsync<T>(string partitionKey, CancellationToken cancellationToken = default) where T : class, ITableEntity;
}
