using Azure.Storage.Blobs;
using Ifx.Logging;
using Microsoft.Extensions.Logging;
using vc.Ifx.Base;
using vc.Ifx.Data.Contracts;

namespace vc.Ifx.Data;

public class BlobConnector(ILogger<BlobConnector> logger, BlobServiceClient blobServiceClient) : ServiceBase<BlobConnector>(logger), IBlobConnector
{

    private readonly LogInformation logInformation = logger.LogInformation;

    public void UploadFile(string containerName, string blobName, string filePath, OverwriteFile overwriteFile)
    {

        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        containerClient.CreateIfNotExists();
        var blobClient = containerClient.GetBlobClient(blobName);

        using var fileStream = File.OpenRead(filePath);
        blobClient.Upload(fileStream, overwriteFile == OverwriteFile.Yes);

        logInformation($"File {filePath} uploaded to blob {blobName} in container {containerName}.");

    }

    public async Task UploadFileAsync(string containerName, string blobName, string filePath, OverwriteFile overwriteFile)
    {

        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();
        var blobClient = containerClient.GetBlobClient(blobName);

        await using var fileStream = File.OpenRead(filePath);
        await blobClient.UploadAsync(fileStream, overwriteFile == OverwriteFile.Yes);

        logInformation($"File {filePath} uploaded to blob {blobName} in container {containerName}.");

    }

    public void DownloadFile(string containerName, string blobName, string downloadFilePath, OverwriteFile overwriteFile)
    {

        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var response = blobClient.Download();
        using var downloadFileStream = File.OpenWrite(downloadFilePath);
        response.Value.Content.CopyTo(downloadFileStream);
        downloadFileStream.Close();

        logInformation($"Blob {blobName} from container {containerName} downloaded to {downloadFilePath}.");

    }

    public async Task DownloadFileAsync(string containerName, string blobName, string downloadFilePath, OverwriteFile overwriteFile)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (File.Exists(downloadFilePath) && overwriteFile == OverwriteFile.No)
        {
            logInformation($"File {downloadFilePath} already exists and will not be overwritten.");
            return;
        }

        var response = await blobClient.DownloadAsync();
        await using var downloadFileStream = new FileStream(downloadFilePath, overwriteFile == OverwriteFile.Yes ? FileMode.Create : FileMode.CreateNew);
        await response.Value.Content.CopyToAsync(downloadFileStream);
        downloadFileStream.Close();

        logInformation($"Blob {blobName} from container {containerName} downloaded to {downloadFilePath}.");
    }

}
