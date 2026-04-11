using Dapper;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Interfaces.Services;
using Pawtrak.Infrastructure.Data;

namespace Pawtrak.Infrastructure.Services;

public sealed class GpsTrackingService : IGpsTrackingService
{
    private readonly IDbConnectionFactory _db;

    public GpsTrackingService(IDbConnectionFactory db) => _db = db;

    public async Task RecordPointAsync(Guid visitReportId, double latitude, double longitude, double? accuracy = null, CancellationToken cancellationToken = default)
    {
        using var conn = _db.CreateConnection();
        var point = new GpsPoint
        {
            VisitReportId = visitReportId,
            Latitude = latitude,
            Longitude = longitude,
            Accuracy = accuracy,
            RecordedAt = DateTime.UtcNow
        };
        await conn.ExecuteAsync(
            """
            INSERT INTO gps_points (id, visit_report_id, latitude, longitude, accuracy, recorded_at, created_at, updated_at)
            VALUES (@Id, @VisitReportId, @Latitude, @Longitude, @Accuracy, @RecordedAt, @CreatedAt, @UpdatedAt)
            """, point);
    }

    public async Task<IEnumerable<GpsPoint>> GetTrackAsync(Guid visitReportId, CancellationToken cancellationToken = default)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<GpsPoint>(
            "SELECT * FROM gps_points WHERE visit_report_id = @VisitReportId ORDER BY recorded_at",
            new { VisitReportId = visitReportId });
    }
}
