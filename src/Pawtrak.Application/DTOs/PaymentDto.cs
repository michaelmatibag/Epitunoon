namespace Pawtrak.Application.DTOs;

public record PaymentDto(
    Guid Id,
    Guid BookingId,
    decimal Amount,
    decimal PlatformFee,
    decimal WalkerPayout,
    decimal? TipAmount,
    string Status,
    string StripePaymentIntentId,
    DateTime? AuthorizedAt,
    DateTime? ReleasedAt
);

public record CreatePaymentIntentResponse(
    string ClientSecret,
    string PaymentIntentId
);
