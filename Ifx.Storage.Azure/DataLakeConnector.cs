using Azure.Storage.Files.DataLake;

using Microsoft.Extensions.Logging;

namespace vc.Ifx.Storage.Azure;

public class DataLakeConnector
{
    private readonly DataLakeServiceClient dataLakeServiceClient;
    private readonly ILogger<DataLakeConnector> logger;

    public DataLakeConnector(string connectionString)
    {
        logger = LoggerFactory.Create(builder => builder.AddDebug()).CreateLogger<DataLakeConnector>();
        dataLakeServiceClient = new DataLakeServiceClient(connectionString);
    }

    public DataLakeConnector(ILogger<DataLakeConnector> logger, string connectionString)
    {
        this.logger = logger;
        dataLakeServiceClient = new DataLakeServiceClient(connectionString);
    }

    public async Task UploadFileAsync(string fileSystemName, string directoryName, string fileName, string filePath)
    {
        var fileSystemClient = dataLakeServiceClient.GetFileSystemClient(fileSystemName);
        await fileSystemClient.CreateIfNotExistsAsync();

        var directoryClient = fileSystemClient.GetDirectoryClient(directoryName);
        await directoryClient.CreateIfNotExistsAsync();

        var fileClient = directoryClient.GetFileClient(fileName);
        await using var fileStream = File.OpenRead(filePath);
        await fileClient.UploadAsync(fileStream, true);

        logger.LogInformation($"File {filePath} uploaded to data lake {fileSystemName} in directory {directoryName} with file name {fileName}.");
    }

    public async Task DownloadFileAsync(string fileSystemName, string directoryName, string fileName, string downloadFilePath)
    {
        var fileSystemClient = dataLakeServiceClient.GetFileSystemClient(fileSystemName);
        var directoryClient = fileSystemClient.GetDirectoryClient(directoryName);
        var fileClient = directoryClient.GetFileClient(fileName);

        var response = await fileClient.ReadAsync();
        await using var downloadFileStream = File.OpenWrite(downloadFilePath);
        await response.Value.Content.CopyToAsync(downloadFileStream);
        downloadFileStream.Close();

        logger.LogInformation($"File {fileName} from data lake {fileSystemName} in directory {directoryName} downloaded to {downloadFilePath}.");
    }
}