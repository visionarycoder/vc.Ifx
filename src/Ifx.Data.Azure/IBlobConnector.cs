namespace vc.Ifx.Data;

public interface IBlobConnector
{

    void UploadFile(string containerName, string blobName, string filePath, OverwriteFile overwriteFile);
    void DownloadFile(string containerName, string blobName, string downloadFilePath, OverwriteFile overwriteFile);

    Task UploadFileAsync(string containerName, string blobName, string filePath, OverwriteFile overwriteFile);
    Task DownloadFileAsync(string containerName, string blobName, string downloadFilePath, OverwriteFile overwriteFile);

}