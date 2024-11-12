using Microsoft.Extensions.Logging;

namespace vc.Ifx.Storage.Azure;

public class BlobConnector
{

    private readonly BlobServiceClient blobServiceClient;
    private readonly ILogger<BlobConnector> logger;

    public BlobConnector(string connectionString)
    {

        logger = LoggerFactory.Create(builder => builder.AddDebug()).CreateLogger<BlobConnector>();
        blobServiceClient = new BlobServiceClient(connectionString);

    }

    public BlobConnector(ILogger<BlobConnector> logger, string connectionString)
    {

        this.logger = logger;
        blobServiceClient = new BlobServiceClient(connectionString);

    }

    public async Task UploadFileAsync(string containerName, string blobName, string filePath)
    {

        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();
        var blobClient = containerClient.GetBlobClient(blobName);

        await using var fileStream = File.OpenRead(filePath);
        await blobClient.UploadAsync(fileStream, true);

        logger.LogInformation($"File {filePath} uploaded to blob {blobName} in container {containerName}.");

    }

    public async Task DownloadFileAsync(string containerName, string blobName, string downloadFilePath)
    {

        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var response = await blobClient.DownloadAsync();
        await using var downloadFileStream = File.OpenWrite(downloadFilePath);
        await response.Value.Content.CopyToAsync(downloadFileStream);
        downloadFileStream.Close();

        logger.LogInformation($"Blob {blobName} from container {containerName} downloaded to {downloadFilePath}.");

    }
}