using Azure;
using Azure.Data.Tables;

namespace vc.Ifx.Data.Contracts;

public interface ITableConnector
{

    Response AddEntity<T>(string tableName, T entity) where T : class, ITableEntity, new();
    Response<T> GetEntity<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new();

    Task<Response> AddEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity, new();
    Task<Response<T>> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new();

}