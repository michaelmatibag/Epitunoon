using Pawtrak.Core.Entities;

namespace Pawtrak.Core.Interfaces.Repositories;

public interface IVisitReportRepository : IRepository<VisitReport>
{
    Task<VisitReport?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<VisitReport?> GetWithFullDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}
