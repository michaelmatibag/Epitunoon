using System.Data;
using Pawtrak.Core.Interfaces;
using Pawtrak.Core.Interfaces.Repositories;
using Pawtrak.Infrastructure.Data.Repositories;

namespace Pawtrak.Infrastructure.Data;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnectionFactory _connectionFactory;
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;

    public IOwnerRepository Owners { get; }
    public IWalkerRepository Walkers { get; }
    public IDogRepository Dogs { get; }
    public IBookingRepository Bookings { get; }
    public IRoutineTaskRepository RoutineTasks { get; }
    public IVisitReportRepository VisitReports { get; }
    public IPaymentRepository Payments { get; }
    public IReviewRepository Reviews { get; }

    public UnitOfWork(
        IDbConnectionFactory connectionFactory,
        IOwnerRepository owners,
        IWalkerRepository walkers,
        IDogRepository dogs,
        IBookingRepository bookings,
        IRoutineTaskRepository routineTasks,
        IVisitReportRepository visitReports,
        IPaymentRepository payments,
        IReviewRepository reviews)
    {
        _connectionFactory = connectionFactory;
        Owners = owners;
        Walkers = walkers;
        Dogs = dogs;
        Bookings = bookings;
        RoutineTasks = routineTasks;
        VisitReports = visitReports;
        Payments = payments;
        Reviews = reviews;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // With Dapper each repo executes immediately; SaveChanges is a no-op here
        // unless you wrap operations in an explicit transaction via BeginTransaction().
        return await Task.FromResult(0);
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _connection?.Dispose();
    }
}
