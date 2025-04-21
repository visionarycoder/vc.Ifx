using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;

using vc.Ifx.Base;

namespace vc.Ifx.Data;

public class TableConnector(ILogger<TableConnector> logger, TableServiceClient tableServiceClient) : ServiceBase<TableConnector>(logger), ITableConnector
{

    private readonly LogWarning logWarning = logger.LogWarning;

    public Response AddEntity<T>(string tableName, T entity) where T : class, ITableEntity, new()
    {
        var tableClient = tableServiceClient.GetTableClient(tableName);
        tableClient.CreateIfNotExists();
        var response = tableClient.AddEntity(entity);
        logInformation($"Entity added to table {tableName}: {entity}");
        return response;
    }

    public Response<T> GetEntity<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new()
    {
        var tableClient = tableServiceClient.GetTableClient(tableName);
        var response = tableClient.GetEntity<T>(partitionKey, rowKey);
        logInformation($"Entity retrieved from table {tableName} with PartitionKey={partitionKey} and RowKey={rowKey}: {response.Value}");
        return response;
    }

    public async Task<Response> AddEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity, new()
    {
        var tableClient = tableServiceClient.GetTableClient(tableName);
        await tableClient.CreateIfNotExistsAsync();

        var response = await tableClient.AddEntityAsync(entity);
        logInformation($"Entity added to table {tableName}: {entity}");
        return response;
    }

    public async Task<Response<T>> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new()
    {

        var tableClient = tableServiceClient.GetTableClient(tableName);
        var response = await tableClient.GetEntityAsync<T>(partitionKey, rowKey);
        logInformation($"Entity retrieved from table {tableName} with PartitionKey={partitionKey} and RowKey={rowKey}: {response.Value}");
        return response;
    }

}