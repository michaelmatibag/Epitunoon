using Pawtrak.Application.DTOs;
using Pawtrak.Application.Services;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Interfaces.Repositories;
using Pawtrak.Core.Interfaces.Services;

namespace Pawtrak.Infrastructure.Services;

public sealed class OwnerService : IOwnerService
{
    private readonly IOwnerRepository _owners;
    private readonly IFileStorageService _fileStorage;

    public OwnerService(IOwnerRepository owners, IFileStorageService fileStorage)
    {
        _owners = owners;
        _fileStorage = fileStorage;
    }

    public async Task<OwnerDto> CreateOwnerAsync(CreateOwnerRequest request, CancellationToken cancellationToken = default)
    {
        var owner = new Owner
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode
        };
        await _owners.AddAsync(owner, cancellationToken);
        return ToDto(owner);
    }

    public async Task<OwnerDto?> GetOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var owner = await _owners.GetByIdAsync(ownerId, cancellationToken);
        return owner is null ? null : ToDto(owner);
    }

    public async Task<OwnerDto> UpdateOwnerAsync(Guid ownerId, UpdateOwnerRequest request, CancellationToken cancellationToken = default)
    {
        var owner = await _owners.GetByIdAsync(ownerId, cancellationToken)
            ?? throw new InvalidOperationException($"Owner {ownerId} not found.");

        owner.FirstName = request.FirstName;
        owner.LastName = request.LastName;
        owner.PhoneNumber = request.PhoneNumber;
        owner.Address = request.Address;
        owner.City = request.City;
        owner.State = request.State;
        owner.ZipCode = request.ZipCode;

        await _owners.UpdateAsync(owner, cancellationToken);
        return ToDto(owner);
    }

    public async Task<string> UploadProfilePhotoAsync(Guid ownerId, Stream photoStream, string fileName, CancellationToken cancellationToken = default)
    {
        var owner = await _owners.GetByIdAsync(ownerId, cancellationToken)
            ?? throw new InvalidOperationException($"Owner {ownerId} not found.");

        var url = await _fileStorage.UploadPhotoAsync(photoStream, fileName, "image/jpeg", cancellationToken);
        owner.ProfilePhotoUrl = url;
        await _owners.UpdateAsync(owner, cancellationToken);
        return url;
    }

    internal static OwnerDto ToDto(Owner o) => new(
        o.Id, o.FirstName, o.LastName, o.Email, o.PhoneNumber,
        o.City, o.State, o.ZipCode, o.ProfilePhotoUrl, o.CreatedAt);
}
