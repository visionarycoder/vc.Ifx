namespace vc.Ifx.Data.Contracts;

public interface IBlobConnector
{

    Guid Instance { get; }
    DateTime CreatedAt { get; }

    void UploadFile(string containerName, string blobName, string filePath, OverwriteFile overwriteFile);
    void DownloadFile(string containerName, string blobName, string downloadFilePath, OverwriteFile overwriteFile);

    Task UploadFileAsync(string containerName, string blobName, string filePath, OverwriteFile overwriteFile);
    Task DownloadFileAsync(string containerName, string blobName, string downloadFilePath, OverwriteFile overwriteFile);

}