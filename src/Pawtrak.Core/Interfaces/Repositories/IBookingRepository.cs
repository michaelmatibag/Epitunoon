using Pawtrak.Core.Entities;
using Pawtrak.Core.Enums;

namespace Pawtrak.Core.Interfaces.Repositories;

public interface IBookingRepository : IRepository<Booking>
{
    Task<IEnumerable<Booking>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetByWalkerIdAsync(Guid walkerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetByStatusAsync(BookingStatus status, CancellationToken cancellationToken = default);
    Task<Booking?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}
