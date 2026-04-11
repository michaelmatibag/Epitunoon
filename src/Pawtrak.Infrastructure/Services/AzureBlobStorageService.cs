using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using Pawtrak.Core.Interfaces.Services;

namespace Pawtrak.Infrastructure.Services;

public sealed class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobContainerClient _container;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"]
            ?? throw new InvalidOperationException("AzureStorage:ConnectionString is not configured.");
        var containerName = configuration["AzureStorage:ContainerName"] ?? "pawtrak-photos";

        _container = new BlobContainerClient(connectionString, containerName);
    }

    public async Task<string> UploadPhotoAsync(
        Stream fileStream, string fileName, string contentType,
        CancellationToken cancellationToken = default)
    {
        var blobName = $"{Guid.NewGuid()}/{fileName}";
        var blob = _container.GetBlobClient(blobName);

        await blob.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);

        return blob.Uri.ToString();
    }

    public async Task DeletePhotoAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        var uri = new Uri(fileUrl);
        var blobName = uri.AbsolutePath.TrimStart('/').Replace($"{_container.Name}/", string.Empty);
        var blob = _container.GetBlobClient(blobName);
        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public Task<string> GetPresignedUrlAsync(string blobName, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var blob = _container.GetBlobClient(blobName);
        var sasUri = blob.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(expiry));
        return Task.FromResult(sasUri.ToString());
    }
}
