using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ColumbiaAi.Backend.Configuration;

namespace ColumbiaAi.Backend.Services;

public interface IBlobStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<Stream> DownloadFileAsync(string fileName);
    Task<bool> DeleteFileAsync(string fileName);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    public BlobStorageService(BlobStorageConfig config)
    {
        var blobServiceClient = new BlobServiceClient(config.ConnectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(config.ContainerName);
        _containerClient.CreateIfNotExists();
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var blobName = $"{Guid.NewGuid()}_{fileName}";
        var blobClient = _containerClient.GetBlobClient(blobName);
        
        var blobHttpHeaders = new BlobHttpHeaders
        {
            ContentType = contentType
        };

        await blobClient.UploadAsync(fileStream, new BlobUploadOptions
        {
            HttpHeaders = blobHttpHeaders
        });

        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadFileAsync(string fileName)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);
        var response = await blobClient.DownloadAsync();
        return response.Value.Content;
    }

    public async Task<bool> DeleteFileAsync(string fileName)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);
        return await blobClient.DeleteIfExistsAsync();
    }
}
