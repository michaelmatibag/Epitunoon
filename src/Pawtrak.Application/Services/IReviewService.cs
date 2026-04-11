using Pawtrak.Application.DTOs;

namespace Pawtrak.Application.Services;

public interface IReviewService
{
    Task<ReviewDto> CreateReviewAsync(Guid ownerId, CreateReviewRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReviewDto>> GetWalkerReviewsAsync(Guid walkerId, CancellationToken cancellationToken = default);
    Task<ReviewDto?> GetByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
