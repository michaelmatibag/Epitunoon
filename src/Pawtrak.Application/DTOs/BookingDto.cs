namespace Pawtrak.Application.DTOs;

public record BookingDto(
    Guid Id,
    Guid OwnerId,
    string OwnerName,
    Guid WalkerId,
    string WalkerName,
    Guid DogId,
    string DogName,
    DateTime ScheduledStart,
    DateTime ScheduledEnd,
    DateTime? ActualStart,
    DateTime? ActualEnd,
    string Status,
    decimal BaseAmount,
    decimal PlatformFee,
    decimal WalkerEarnings,
    decimal? TipAmount,
    string? SpecialInstructions,
    DateTime CreatedAt
);

public record CreateBookingRequest(
    Guid WalkerId,
    Guid DogId,
    DateTime ScheduledStart,
    DateTime ScheduledEnd,
    string? SpecialInstructions
);

public record AddTipRequest(
    Guid BookingId,
    decimal TipAmount
);
