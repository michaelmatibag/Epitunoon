using Pawtrak.Core.Interfaces.Repositories;

namespace Pawtrak.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IOwnerRepository Owners { get; }
    IWalkerRepository Walkers { get; }
    IDogRepository Dogs { get; }
    IBookingRepository Bookings { get; }
    IRoutineTaskRepository RoutineTasks { get; }
    IVisitReportRepository VisitReports { get; }
    IPaymentRepository Payments { get; }
    IReviewRepository Reviews { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
