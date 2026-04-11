using System.Data;
using Dapper;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Interfaces.Repositories;

namespace Pawtrak.Infrastructure.Data.Repositories;

public sealed class VisitReportRepository : RepositoryBase<VisitReport>, IVisitReportRepository
{
    protected override string TableName => "VisitReports";

    public VisitReportRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<VisitReport?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<VisitReport>(
            "SELECT * FROM VisitReports WHERE BookingId = @BookingId",
            new { BookingId = bookingId });
    }

    public async Task<VisitReport?> GetWithFullDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();

        // Load the report
        var report = await conn.QuerySingleOrDefaultAsync<VisitReport>(
            "SELECT * FROM VisitReports WHERE Id = @Id", new { Id = id });

        if (report is null) return null;

        // Load task logs with task titles
        report.TaskLogs = (await conn.QueryAsync<VisitTaskLog>(
            "SELECT * FROM VisitTaskLogs WHERE VisitReportId = @Id", new { Id = id })).ToList();

        // Load photos
        report.Photos = (await conn.QueryAsync<VisitPhoto>(
            "SELECT * FROM VisitPhotos WHERE VisitReportId = @Id ORDER BY TakenAt", new { Id = id })).ToList();

        // Load GPS track
        report.GpsPoints = (await conn.QueryAsync<GpsPoint>(
            "SELECT * FROM GpsPoints WHERE VisitReportId = @Id ORDER BY RecordedAt", new { Id = id })).ToList();

        return report;
    }

    protected override async Task InsertAsync(IDbConnection conn, VisitReport entity)
    {
        const string sql = """
            INSERT INTO VisitReports
                (Id, BookingId, Notes, Status, CompletedAt, CreatedAt, UpdatedAt)
            VALUES
                (@Id, @BookingId, @Notes, @Status, @CompletedAt, @CreatedAt, @UpdatedAt)
            """;
        await conn.ExecuteAsync(sql, entity);
    }

    protected override async Task UpdateCoreAsync(IDbConnection conn, VisitReport entity)
    {
        const string sql = """
            UPDATE VisitReports SET
                Notes = @Notes, Status = @Status, CompletedAt = @CompletedAt, UpdatedAt = @UpdatedAt
            WHERE Id = @Id
            """;
        await conn.ExecuteAsync(sql, entity);
    }
}
