using Pawtrak.Application.DTOs;

namespace Pawtrak.Application.Services;

public interface IDogService
{
    Task<DogDto> AddDogAsync(Guid ownerId, CreateDogRequest request, CancellationToken cancellationToken = default);
    Task<DogDto?> GetDogAsync(Guid dogId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DogDto>> GetOwnerDogsAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<DogDto> UpdateDogAsync(Guid dogId, CreateDogRequest request, CancellationToken cancellationToken = default);
    Task DeleteDogAsync(Guid dogId, CancellationToken cancellationToken = default);
    Task<string> UploadDogPhotoAsync(Guid dogId, Stream photoStream, string fileName, CancellationToken cancellationToken = default);
}
