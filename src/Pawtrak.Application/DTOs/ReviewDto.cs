namespace Pawtrak.Application.DTOs;

public record ReviewDto(
    Guid Id,
    Guid BookingId,
    Guid OwnerId,
    string OwnerName,
    Guid WalkerId,
    int Rating,
    string? Comment,
    bool IsPublic,
    DateTime CreatedAt
);

public record CreateReviewRequest(
    Guid BookingId,
    int Rating,
    string? Comment
);
