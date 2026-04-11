using Pawtrak.Application.DTOs;

namespace Pawtrak.Application.Services;

public interface IBookingService
{
    Task<BookingDto> CreateBookingAsync(Guid ownerId, CreateBookingRequest request, CancellationToken cancellationToken = default);
    Task<BookingDto?> GetBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BookingDto>> GetOwnerBookingsAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BookingDto>> GetWalkerBookingsAsync(Guid walkerId, CancellationToken cancellationToken = default);
    Task<BookingDto> ConfirmBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<BookingDto> CancelBookingAsync(Guid bookingId, string reason, CancellationToken cancellationToken = default);
    Task<CreatePaymentIntentResponse> CreatePaymentIntentAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<BookingDto> AddTipAsync(AddTipRequest request, CancellationToken cancellationToken = default);
}
