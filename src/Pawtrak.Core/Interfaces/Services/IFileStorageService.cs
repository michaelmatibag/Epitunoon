namespace Pawtrak.Core.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> UploadPhotoAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task DeletePhotoAsync(string fileUrl, CancellationToken cancellationToken = default);
    Task<string> GetPresignedUrlAsync(string blobName, TimeSpan expiry, CancellationToken cancellationToken = default);
}
