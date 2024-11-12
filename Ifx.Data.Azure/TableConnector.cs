using Azure;
using Azure.Data.Tables;

namespace vc.Ifx.Data.Azure;

public class TableConnector
{
    private readonly TableServiceClient tableServiceClient;
    private readonly ILogger<TableConnector> logger;

    public TableConnector(string connectionString)
    {
        logger = LoggerFactory.Create(builder => builder.AddDebug()).CreateLogger<TableConnector>();
        tableServiceClient = new TableServiceClient(connectionString);
    }

    public TableConnector(ILogger<TableConnector> logger, string connectionString)
    {
        this.logger = logger;
        tableServiceClient = new TableServiceClient(connectionString);
    }

    public async Task AddEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity, new()
    {
        var tableClient = tableServiceClient.GetTableClient(tableName);
        await tableClient.CreateIfNotExistsAsync();

        await tableClient.AddEntityAsync(entity);
        logger.LogInformation($"Entity added to table {tableName}: {entity}");
    }

    public async Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new()
    {
        var tableClient = tableServiceClient.GetTableClient(tableName);

        try
        {
            T entity = await tableClient.GetEntityAsync<T>(partitionKey, rowKey);
            logger.LogInformation($"Entity retrieved from table {tableName} with PartitionKey={partitionKey} and RowKey={rowKey}: {entity}");
            return entity;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            logger.LogInformation($"Entity not found in table {tableName} with PartitionKey={partitionKey} and RowKey={rowKey}");
            return null;
        }
    }
}