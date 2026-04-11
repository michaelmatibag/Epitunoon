using Pawtrak.Core.Entities;

namespace Pawtrak.Core.Interfaces.Repositories;

public interface IReviewRepository : IRepository<Review>
{
    Task<IEnumerable<Review>> GetByWalkerIdAsync(Guid walkerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Review>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<Review?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<double> GetAverageRatingForWalkerAsync(Guid walkerId, CancellationToken cancellationToken = default);
}
