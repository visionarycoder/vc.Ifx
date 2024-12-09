using vc.Ifx.Options;

namespace vc.Ifx.Data.Contracts;

public interface IBlobConnector
{

    Guid Instance { get; }
    DateTime CreatedAt { get; }

    void UploadFile(string containerName, string blobName, string filePath, FileOverwriteOption fileOverwriteOption);
    void DownloadFile(string containerName, string blobName, string downloadFilePath, FileOverwriteOption fileOverwriteOption);

    Task UploadFileAsync(string containerName, string blobName, string filePath, FileOverwriteOption fileOverwriteOption);
    Task DownloadFileAsync(string containerName, string blobName, string downloadFilePath, FileOverwriteOption fileOverwriteOption);

}