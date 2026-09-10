using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace VisionaryCoder.Framework.Data.Azure.Table;

/// <summary>
/// Provides Azure Table Storage-based NoSQL table operations implementation.
/// This service wraps Azure Table Storage operations with logging, error handling, and async support.
/// Supports both connection string and managed identity authentication.
/// </summary>
public sealed class AzureTableStorageProvider : ServiceBase<AzureTableStorageProvider>, ITableStorageProvider
{
    private readonly AzureTableStorageOptions options;
    private readonly TableServiceClient tableServiceClient;
    private readonly TableClient tableClient;

    public AzureTableStorageProvider(AzureTableStorageOptions options, ILogger<AzureTableStorageProvider> logger)
        : base(logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.options.Validate();

        try
        {
            // Create table service client based on authentication method
            if (options.UseManagedIdentity)
            {
                tableServiceClient = new TableServiceClient(new Uri(options.StorageAccountUri!), new DefaultAzureCredential());
            }
            else
            {
                tableServiceClient = new TableServiceClient(options.ConnectionString);
            }

            tableClient = tableServiceClient.GetTableClient(options.TableName);

            // Create table if it doesn't exist and option is enabled
            if (options.CreateTableIfNotExists)
            {
                tableClient.CreateIfNotExists();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to initialize Azure Table Storage client");
            throw;
        }
    }

    /// <summary>
    /// Checks if the table exists.
    /// </summary>
    public bool TableExists()
    {
        try
        {
            Logger.LogTrace("Table existence check for '{TableName}'", options.TableName);

            TableItem? item = tableServiceClient.Query(filter: $"TableName eq '{options.TableName}'").FirstOrDefault();
            bool exists = item is not null;

            Logger.LogTrace("Table existence check for '{TableName}': {Exists}", options.TableName, exists);
            return exists;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking table existence for '{TableName}'", options.TableName);
            throw;
        }
    }

    /// <summary>
    /// Checks if the table exists asynchronously.
    /// </summary>
    public async Task<bool> TableExistsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogTrace("Table existence check async for '{TableName}'", options.TableName);

            bool exists = false;
            await foreach (TableItem? item in tableServiceClient.QueryAsync(filter: $"TableName eq '{options.TableName}'", cancellationToken: cancellationToken))
            {
                exists = true;
                break;
            }

            Logger.LogTrace("Table existence check async for '{TableName}': {Exists}", options.TableName, exists);
            return exists;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking table existence async for '{TableName}'", options.TableName);
            throw;
        }
    }

    /// <summary>
    /// Adds a new entity to the table.
    /// </summary>
    public void AddEntity<T>(T entity) where T : class, ITableEntity
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            Logger.LogDebug("Adding entity to table '{TableName}' with PartitionKey '{PartitionKey}' and RowKey '{RowKey}'",
                options.TableName, entity.PartitionKey, entity.RowKey);

            tableClient.AddEntity(entity);
            Logger.LogTrace("Successfully added entity to table '{TableName}'", options.TableName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error adding entity to table '{TableName}'", options.TableName);
            throw;
        }
    }

    /// <summary>
    /// Adds a new entity to the table asynchronously.
    /// </summary>
    public async Task AddEntityAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class, ITableEntity
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            Logger.LogDebug("Adding entity async to table '{TableName}' with PartitionKey '{PartitionKey}' and RowKey '{RowKey}'",
                options.TableName, entity.PartitionKey, entity.RowKey);

            await tableClient.AddEntityAsync(entity, cancellationToken);
            Logger.LogTrace("Successfully added entity async to table '{TableName}'", options.TableName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error adding entity async to table '{TableName}'", options.TableName);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing entity in the table.
    /// </summary>
    public void UpdateEntity<T>(T entity, ETag etag = default, TableUpdateMode mode = TableUpdateMode.Replace) where T : class, ITableEntity
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            Logger.LogDebug("Updating entity in table '{TableName}' with PartitionKey '{PartitionKey}' and RowKey '{RowKey}'",
                options.TableName, entity.PartitionKey, entity.RowKey);

            tableClient.UpdateEntity(entity, etag == default ? ETag.All : etag, mode);
            Logger.LogTrace("Successfully updated entity in table '{TableName}'", options.TableName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating entity in table '{TableName}'", options.TableName);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing entity in the table asynchronously.
    /// </summary>
    public async Task UpdateEntityAsync<T>(T entity, ETag etag = default, TableUpdateMode mode = TableUpdateMode.Replace, CancellationToken cancellationToken = default) where T : class, ITableEntity
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            Logger.LogDebug("Updating entity async in table '{TableName}' with PartitionKey '{PartitionKey}' and RowKey '{RowKey}'",
                options.TableName, entity.PartitionKey, entity.RowKey);

            await tableClient.UpdateEntityAsync(entity, etag == default ? ETag.All : etag, mode, cancellationToken);
            Logger.LogTrace("Successfully updated entity async in table '{TableName}'", options.TableName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating entity async in table '{TableName}'", options.TableName);
            throw;
        }
    }

    /// <summary>
    /// Upserts an entity (insert or replace) in the table.
    /// </summary>
    public void UpsertEntity<T>(T entity, TableUpdateMode mode = TableUpdateMode.Replace) where T : class, ITableEntity
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            Logger.LogDebug("Upserting entity in table '{TableName}' with PartitionKey '{PartitionKey}' and RowKey '{RowKey}'",
                options.TableName, entity.PartitionKey, entity.RowKey);

            tableClient.UpsertEntity(entity, mode);
            Logger.LogTrace("Successfully upserted entity in table '{TableName}'", options.TableName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error upserting entity in table '{TableName}'", options.TableName);
            throw;
        }
    }

    /// <summary>
    /// Upserts an entity (insert or replace) in the table asynchronously.
    /// </summary>
    public async Task UpsertEntityAsync<T>(T entity, TableUpdateMode mode = TableUpdateMode.Replace, CancellationToken cancellationToken = default) where T : class, ITableEntity
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            Logger.LogDebug("Upserting entity async in table '{TableName}' with PartitionKey '{PartitionKey}' and RowKey '{RowKey}'",
                options.TableName, entity.PartitionKey, entity.RowKey);

            await tableClient.UpsertEntityAsync(entity, mode, cancellationToken);
            Logger.LogTrace("Successfully upserted entity async in table '{TableName}'", options.TableName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error upserting entity async in table '{TableName}'", options.TableName);
            throw;
        }
    }

    /// <summary>
    /// Deletes an entity from the table.
    /// </summary>
    public void DeleteEntity(string partitionKey, string rowKey, ETag etag = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(rowKey);

        try
        {
            Logger.LogDebug("Deleting entity from table '{TableName}' with PartitionKey '{PartitionKey}' and RowKey '{RowKey}'",
                options.TableName, partitionKey, rowKey);

            tableClient.DeleteEntity(partitionKey, rowKey, etag == default ? ETag.All : etag);
            Logger.LogTrace("Successfully deleted entity from table '{TableName}'", options.TableName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting entity from table '{TableName}'", options.TableName);
            throw;
        }
    }

    /// <summary>
    /// Deletes an entity from the table asynchronously.
    /// </summary>
    public async Task DeleteEntityAsync(string partitionKey, string rowKey, ETag etag = default, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(rowKey);

        try
        {
            Logger.LogDebug("Deleting entity async from table '{TableName}' with PartitionKey '{PartitionKey}' and RowKey '{RowKey}'",
                options.TableName, partitionKey, rowKey);

            await tableClient.DeleteEntityAsync(partitionKey, rowKey, etag == default ? ETag.All : etag, cancellationToken);
            Logger.LogTrace("Successfully deleted entity async from table '{TableName}'", options.TableName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting entity async from table '{TableName}'", options.TableName);
            throw;
        }
    }

    /// <summary>
    /// Gets an entity by partition key and row key.
    /// </summary>
    public T? GetEntity<T>(string partitionKey, string rowKey)
        where T : class, ITableEntity
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(rowKey);

        try
        {
            Logger.LogDebug("Getting entity from table '{TableName}' with PartitionKey '{PartitionKey}' and RowKey '{RowKey}'",
                options.TableName, partitionKey, rowKey);

            Response<T>? response = tableClient.GetEntity<T>(partitionKey, rowKey);
            Logger.LogTrace("Successfully retrieved entity from table '{TableName}'", options.TableName);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            Logger.LogDebug("Entity not found in table '{TableName}' with PartitionKey '{PartitionKey}' and RowKey '{RowKey}'",
                options.TableName, partitionKey, rowKey);
            return null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting entity from table '{TableName}'", options.TableName);
            throw;
        }
    }

    /// <summary>
    /// Gets an entity by partition key and row key asynchronously.
    /// </summary>
    public async Task<T?> GetEntityAsync<T>(string partitionKey, string rowKey, CancellationToken cancellationToken = default)
        where T : class, ITableEntity
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(rowKey);

        try
        {
            Logger.LogDebug("Getting entity async from table '{TableName}' with PartitionKey '{PartitionKey}' and RowKey '{RowKey}'",
                options.TableName, partitionKey, rowKey);

            Response<T>? response = await tableClient.GetEntityAsync<T>(partitionKey, rowKey, cancellationToken: cancellationToken);
            Logger.LogTrace("Successfully retrieved entity async from table '{TableName}'", options.TableName);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            Logger.LogDebug("Entity not found async in table '{TableName}' with PartitionKey '{PartitionKey}' and RowKey '{RowKey}'",
                options.TableName, partitionKey, rowKey);
            return null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting entity async from table '{TableName}'", options.TableName);
            throw;
        }
    }

    /// <summary>
    /// Queries entities from the table with an optional filter.
    /// </summary>
    public List<T> QueryEntities<T>(string? filter = null, int? maxPerPage = null)
        where T : class, ITableEntity
    {
        try
        {
            Logger.LogDebug("Querying entities from table '{TableName}' with filter '{Filter}'", options.TableName, filter ?? "none");

            int pageSize = maxPerPage ?? options.MaxEntitiesPerQuery;
            Pageable<T>? query = tableClient.Query<T>(filter, pageSize);

            var results = query.ToList();
            Logger.LogTrace("Successfully queried {Count} entities from table '{TableName}'", results.Count, options.TableName);
            return results;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error querying entities from table '{TableName}'", options.TableName);
            throw;
        }
    }

    /// <summary>
    /// Queries entities from the table with an optional filter asynchronously.
    /// </summary>
    public async Task<List<T>> QueryEntitiesAsync<T>(string? filter = null, int? maxPerPage = null, CancellationToken cancellationToken = default)
        where T : class, ITableEntity
    {
        try
        {
            Logger.LogDebug("Querying entities async from table '{TableName}' with filter '{Filter}'", options.TableName, filter ?? "none");

            int pageSize = maxPerPage ?? options.MaxEntitiesPerQuery;
            AsyncPageable<T>? query = tableClient.QueryAsync<T>(filter, pageSize, cancellationToken: cancellationToken);

            var results = new List<T>();
            await foreach (T? entity in query)
            {
                results.Add(entity);
            }

            Logger.LogTrace("Successfully queried {Count} entities async from table '{TableName}'", results.Count, options.TableName);
            return results;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error querying entities async from table '{TableName}'", options.TableName);
            throw;
        }
    }

    /// <summary>
    /// Enumerates entities from the table with an optional filter asynchronously.
    /// </summary>
    public async IAsyncEnumerable<T> EnumerateEntitiesAsync<T>(string? filter = null, int? maxPerPage = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where T : class, ITableEntity
    {
        Logger.LogDebug("Enumerating entities async from table '{TableName}' with filter '{Filter}'", options.TableName, filter ?? "none");

        int pageSize = maxPerPage ?? options.MaxEntitiesPerQuery;
        AsyncPageable<T>? query = tableClient.QueryAsync<T>(filter, pageSize, cancellationToken: cancellationToken);

        await foreach (T? entity in query.WithCancellation(cancellationToken))
        {
            yield return entity;
        }
    }

    /// <summary>
    /// Submits a batch transaction operation.
    /// </summary>
    public void SubmitBatch(IEnumerable<TableTransactionAction> actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        try
        {
            var actionList = actions.ToList();
            Logger.LogDebug("Submitting batch transaction to table '{TableName}' with {Count} actions",
                options.TableName, actionList.Count);

            if (actionList.Count > options.MaxEntitiesPerBatch)
            {
                throw new InvalidOperationException($"Batch operation cannot contain more than {options.MaxEntitiesPerBatch} actions");
            }

            tableClient.SubmitTransaction(actionList);
            Logger.LogTrace("Successfully submitted batch transaction to table '{TableName}'", options.TableName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error submitting batch transaction to table '{TableName}'", options.TableName);
            throw;
        }
    }

    /// <summary>
    /// Submits a batch transaction operation asynchronously.
    /// </summary>
    public async Task SubmitBatchAsync(IEnumerable<TableTransactionAction> actions, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actions);

        try
        {
            var actionList = actions.ToList();
            Logger.LogDebug("Submitting batch transaction async to table '{TableName}' with {Count} actions",
                options.TableName, actionList.Count);

            if (actionList.Count > options.MaxEntitiesPerBatch)
            {
                throw new InvalidOperationException($"Batch operation cannot contain more than {options.MaxEntitiesPerBatch} actions");
            }

            await tableClient.SubmitTransactionAsync(actionList, cancellationToken);
            Logger.LogTrace("Successfully submitted batch transaction async to table '{TableName}'", options.TableName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error submitting batch transaction async to table '{TableName}'", options.TableName);
            throw;
        }
    }

    /// <summary>
    /// Gets entities by partition key.
    /// </summary>
    public List<T> GetEntitiesByPartitionKey<T>(string partitionKey)
        where T : class, ITableEntity
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);

        string filter = $"PartitionKey eq '{partitionKey}'";
        return QueryEntities<T>(filter);
    }

    /// <summary>
    /// Gets entities by partition key asynchronously.
    /// </summary>
    public async Task<List<T>> GetEntitiesByPartitionKeyAsync<T>(string partitionKey, CancellationToken cancellationToken = default)
        where T : class, ITableEntity
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partitionKey);

        string filter = $"PartitionKey eq '{partitionKey}'";
        return await QueryEntitiesAsync<T>(filter, cancellationToken: cancellationToken);
    }

}
