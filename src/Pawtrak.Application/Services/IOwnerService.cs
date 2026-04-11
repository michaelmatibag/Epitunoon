using Pawtrak.Application.DTOs;

namespace Pawtrak.Application.Services;

public interface IOwnerService
{
    Task<OwnerDto> CreateOwnerAsync(CreateOwnerRequest request, CancellationToken cancellationToken = default);
    Task<OwnerDto?> GetOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<OwnerDto> UpdateOwnerAsync(Guid ownerId, UpdateOwnerRequest request, CancellationToken cancellationToken = default);
    Task<string> UploadProfilePhotoAsync(Guid ownerId, Stream photoStream, string fileName, CancellationToken cancellationToken = default);
}
