using Pawtrak.Application.DTOs;
using Pawtrak.Application.Services;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Interfaces.Repositories;
using Pawtrak.Core.Interfaces.Services;

namespace Pawtrak.Infrastructure.Services;

public sealed class DogService : IDogService
{
    private readonly IDogRepository _dogs;
    private readonly IFileStorageService _fileStorage;

    public DogService(IDogRepository dogs, IFileStorageService fileStorage)
    {
        _dogs = dogs;
        _fileStorage = fileStorage;
    }

    public async Task<DogDto> AddDogAsync(Guid ownerId, CreateDogRequest request, CancellationToken cancellationToken = default)
    {
        var dog = new Dog
        {
            OwnerId = ownerId,
            Name = request.Name,
            Breed = request.Breed,
            AgeYears = request.AgeYears,
            WeightLbs = request.WeightLbs,
            MedicalNotes = request.MedicalNotes,
            BehaviorNotes = request.BehaviorNotes,
            EmergencyContact = request.EmergencyContact,
            VetName = request.VetName,
            VetPhone = request.VetPhone
        };
        await _dogs.AddAsync(dog, cancellationToken);
        return ToDto(dog);
    }

    public async Task<DogDto?> GetDogAsync(Guid dogId, CancellationToken cancellationToken = default)
    {
        var dog = await _dogs.GetWithRoutineTasksAsync(dogId, cancellationToken);
        return dog is null ? null : ToDto(dog);
    }

    public async Task<IEnumerable<DogDto>> GetOwnerDogsAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var dogs = await _dogs.GetByOwnerIdAsync(ownerId, cancellationToken);
        return dogs.Select(d => ToDto(d));
    }

    public async Task<DogDto> UpdateDogAsync(Guid dogId, CreateDogRequest request, CancellationToken cancellationToken = default)
    {
        var dog = await _dogs.GetByIdAsync(dogId, cancellationToken)
            ?? throw new InvalidOperationException($"Dog {dogId} not found.");

        dog.Name = request.Name;
        dog.Breed = request.Breed;
        dog.AgeYears = request.AgeYears;
        dog.WeightLbs = request.WeightLbs;
        dog.MedicalNotes = request.MedicalNotes;
        dog.BehaviorNotes = request.BehaviorNotes;
        dog.EmergencyContact = request.EmergencyContact;
        dog.VetName = request.VetName;
        dog.VetPhone = request.VetPhone;

        await _dogs.UpdateAsync(dog, cancellationToken);
        return ToDto(dog);
    }

    public async Task DeleteDogAsync(Guid dogId, CancellationToken cancellationToken = default)
    {
        var dog = await _dogs.GetByIdAsync(dogId, cancellationToken)
            ?? throw new InvalidOperationException($"Dog {dogId} not found.");
        await _dogs.DeleteAsync(dog, cancellationToken);
    }

    public async Task<string> UploadDogPhotoAsync(Guid dogId, Stream photoStream, string fileName, CancellationToken cancellationToken = default)
    {
        var dog = await _dogs.GetByIdAsync(dogId, cancellationToken)
            ?? throw new InvalidOperationException($"Dog {dogId} not found.");

        var url = await _fileStorage.UploadPhotoAsync(photoStream, fileName, "image/jpeg", cancellationToken);
        dog.PhotoUrl = url;
        await _dogs.UpdateAsync(dog, cancellationToken);
        return url;
    }

    private static DogDto ToDto(Dog d) => new(
        d.Id, d.OwnerId, d.Name, d.Breed, d.AgeYears, d.WeightLbs,
        d.PhotoUrl, d.MedicalNotes, d.BehaviorNotes, d.EmergencyContact,
        d.VetName, d.VetPhone,
        d.RoutineTasks.Select(RoutineTaskService.ToDto).ToList());
}
