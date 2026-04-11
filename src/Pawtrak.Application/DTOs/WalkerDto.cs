namespace Pawtrak.Application.DTOs;

public record WalkerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string City,
    string State,
    string ZipCode,
    string Bio,
    decimal HourlyRate,
    bool IsAvailable,
    bool IsBackgroundChecked,
    double AverageRating,
    int TotalReviews,
    string? ProfilePhotoUrl,
    DateTime CreatedAt
);

public record CreateWalkerRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string City,
    string State,
    string ZipCode,
    string Bio,
    decimal HourlyRate
);

public record WalkerSearchRequest(
    string ZipCode,
    DateTime? AvailableFrom = null,
    DateTime? AvailableTo = null,
    decimal? MaxHourlyRate = null
);
