namespace Pawtrak.Application.DTOs;

public record OwnerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string City,
    string State,
    string ZipCode,
    string? ProfilePhotoUrl,
    DateTime CreatedAt
);

public record CreateOwnerRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Address,
    string City,
    string State,
    string ZipCode
);

public record UpdateOwnerRequest(
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Address,
    string City,
    string State,
    string ZipCode
);
