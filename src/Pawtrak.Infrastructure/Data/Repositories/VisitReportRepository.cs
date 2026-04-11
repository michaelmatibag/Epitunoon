using System.Data;
using Dapper;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Interfaces.Repositories;

namespace Pawtrak.Infrastructure.Data.Repositories;

public sealed class VisitReportRepository : RepositoryBase<VisitReport>, IVisitReportRepository
{
    protected override string TableName => "visit_reports";

    public VisitReportRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<VisitReport?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<VisitReport>(
            "SELECT * FROM visit_reports WHERE booking_id = @BookingId",
            new { BookingId = bookingId });
    }

    public async Task<VisitReport?> GetWithFullDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();

        var report = await conn.QuerySingleOrDefaultAsync<VisitReport>(
            "SELECT * FROM visit_reports WHERE id = @Id", new { Id = id });

        if (report is null) return null;

        // Task logs joined with routine task name + category
        const string taskLogSql = """
            SELECT vtl.*, rt.*
            FROM visit_task_logs vtl
            JOIN routine_tasks rt ON rt.id = vtl.routine_task_id
            WHERE vtl.visit_report_id = @Id
            ORDER BY rt.sort_order
            """;

        report.TaskLogs = (await conn.QueryAsync<VisitTaskLog, RoutineTask, VisitTaskLog>(
            taskLogSql,
            (log, task) => { log.RoutineTask = task; return log; },
            new { Id = id },
            splitOn: "id")).ToList();

        report.Photos = (await conn.QueryAsync<VisitPhoto>(
            "SELECT * FROM visit_photos WHERE visit_report_id = @Id ORDER BY taken_at",
            new { Id = id })).ToList();

        report.GpsPoints = (await conn.QueryAsync<GpsPoint>(
            "SELECT * FROM gps_points WHERE visit_report_id = @Id ORDER BY recorded_at",
            new { Id = id })).ToList();

        return report;
    }

    protected override async Task InsertAsync(IDbConnection conn, VisitReport entity)
    {
        const string sql = """
            INSERT INTO visit_reports
                (id, booking_id, notes, status, completed_at, created_at, updated_at)
            VALUES
                (@Id, @BookingId, @Notes, @Status, @CompletedAt, @CreatedAt, @UpdatedAt)
            """;
        await conn.ExecuteAsync(sql, entity);
    }

    protected override async Task UpdateCoreAsync(IDbConnection conn, VisitReport entity)
    {
        const string sql = """
            UPDATE visit_reports SET
                notes = @Notes, status = @Status, completed_at = @CompletedAt, updated_at = @UpdatedAt
            WHERE id = @Id
            """;
        await conn.ExecuteAsync(sql, entity);
    }
}
