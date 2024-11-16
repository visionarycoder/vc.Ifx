using Microsoft.Extensions.Logging;

namespace vc.Ifx.Storage.Azure;

public class FileConnector
{
    private readonly ShareServiceClient shareServiceClient;
    private readonly ILogger<FileConnector> logger;

    public FileConnector(string connectionString)
    {
        logger = LoggerFactory.Create(builder => builder.AddDebug()).CreateLogger<FileConnector>();
        shareServiceClient = new ShareServiceClient(connectionString);
    }

    public FileConnector(ILogger<FileConnector> logger, string connectionString)
    {
        this.logger = logger;
        shareServiceClient = new ShareServiceClient(connectionString);
    }

    public async Task UploadFileAsync(string shareName, string directoryName, string fileName, string filePath)
    {
        var shareClient = shareServiceClient.GetShareClient(shareName);
        await shareClient.CreateIfNotExistsAsync();

        var directoryClient = shareClient.GetDirectoryClient(directoryName);
        await directoryClient.CreateIfNotExistsAsync();

        var fileClient = directoryClient.GetFileClient(fileName);
        await using var fileStream = File.OpenRead(filePath);
        await fileClient.CreateAsync(fileStream.Length);
        await fileClient.UploadAsync(fileStream);

        logger.LogInformation($"File {filePath} uploaded to file share {shareName} in directory {directoryName} with file name {fileName}.");
    }

    public async Task DownloadFileAsync(string shareName, string directoryName, string fileName, string downloadFilePath)
    {
        var shareClient = shareServiceClient.GetShareClient(shareName);
        var directoryClient = shareClient.GetDirectoryClient(directoryName);
        var fileClient = directoryClient.GetFileClient(fileName);

        var response = await fileClient.DownloadAsync();
        await using var downloadFileStream = File.OpenWrite(downloadFilePath);
        await response.Value.Content.CopyToAsync(downloadFileStream);
        downloadFileStream.Close();

        logger.LogInformation($"File {fileName} from file share {shareName} in directory {directoryName} downloaded to {downloadFilePath}.");
    }
}