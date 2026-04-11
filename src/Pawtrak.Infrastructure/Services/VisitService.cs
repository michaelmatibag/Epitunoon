using System.Data;
using Dapper;
using Pawtrak.Application.DTOs;
using Pawtrak.Application.Services;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Enums;
using Pawtrak.Core.Interfaces.Repositories;
using Pawtrak.Core.Interfaces.Services;
using Pawtrak.Infrastructure.Data;

namespace Pawtrak.Infrastructure.Services;

public sealed class VisitService : IVisitService
{
    private readonly IVisitReportRepository _visitReports;
    private readonly IBookingRepository _bookings;
    private readonly IRoutineTaskRepository _routineTasks;
    private readonly IFileStorageService _fileStorage;
    private readonly INotificationService _notifications;
    private readonly IPaymentService _payments;
    private readonly IDbConnectionFactory _db;

    public VisitService(
        IVisitReportRepository visitReports,
        IBookingRepository bookings,
        IRoutineTaskRepository routineTasks,
        IFileStorageService fileStorage,
        INotificationService notifications,
        IPaymentService payments,
        IDbConnectionFactory db)
    {
        _visitReports = visitReports;
        _bookings = bookings;
        _routineTasks = routineTasks;
        _fileStorage = fileStorage;
        _notifications = notifications;
        _payments = payments;
        _db = db;
    }

    public async Task<VisitReportDto> StartVisitAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _bookings.GetWithDetailsAsync(bookingId, cancellationToken)
            ?? throw new InvalidOperationException($"Booking {bookingId} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new InvalidOperationException("Booking must be confirmed before starting a visit.");

        var report = new VisitReport { BookingId = bookingId };
        await _visitReports.AddAsync(report, cancellationToken);

        // Seed one task log per routine task in the dog's ordered checklist
        var tasks = await _routineTasks.GetOrderedByDogIdAsync(booking.DogId, cancellationToken);
        using var conn = _db.CreateConnection();
        foreach (var task in tasks)
        {
            await InsertTaskLogAsync(conn, new VisitTaskLog
            {
                VisitReportId = report.Id,
                RoutineTaskId = task.Id,
                IsCompleted = false
            });
        }

        booking.Status = BookingStatus.InProgress;
        booking.ActualStart = DateTime.UtcNow;
        await _bookings.UpdateAsync(booking, cancellationToken);
        await _notifications.SendVisitStartedAsync(bookingId, cancellationToken);

        return await GetVisitReportAsync(report.Id, cancellationToken)
               ?? throw new InvalidOperationException("Failed to load the created visit report.");
    }

    public async Task<VisitReportDto> CompleteTaskAsync(Guid visitReportId, Guid routineTaskId, CompleteTaskRequest request, CancellationToken cancellationToken = default)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            """
            UPDATE visit_task_logs SET
                is_completed = true,
                completed_at = @CompletedAt,
                notes = @Notes,
                updated_at = @UpdatedAt
            WHERE visit_report_id = @VisitReportId AND routine_task_id = @RoutineTaskId
            """,
            new
            {
                VisitReportId = visitReportId,
                RoutineTaskId = routineTaskId,
                CompletedAt = DateTime.UtcNow,
                Notes = request.Notes,
                UpdatedAt = DateTime.UtcNow
            });

        return await GetVisitReportAsync(visitReportId, cancellationToken)
               ?? throw new InvalidOperationException($"Visit report {visitReportId} not found.");
    }

    public async Task<VisitReportDto> AddPhotoAsync(Guid visitReportId, Stream photoStream, string fileName, string photoType, CancellationToken cancellationToken = default)
    {
        var url = await _fileStorage.UploadPhotoAsync(photoStream, fileName, "image/jpeg", cancellationToken);
        var type = Enum.Parse<PhotoType>(photoType, ignoreCase: true);

        using var conn = _db.CreateConnection();
        var photo = new VisitPhoto
        {
            VisitReportId = visitReportId,
            PhotoUrl = url,
            Type = type,
            TakenAt = DateTime.UtcNow
        };
        await conn.ExecuteAsync(
            """
            INSERT INTO visit_photos (id, visit_report_id, photo_url, type, caption, taken_at, created_at, updated_at)
            VALUES (@Id, @VisitReportId, @PhotoUrl, @Type, @Caption, @TakenAt, @CreatedAt, @UpdatedAt)
            """, photo);

        return await GetVisitReportAsync(visitReportId, cancellationToken)
               ?? throw new InvalidOperationException($"Visit report {visitReportId} not found.");
    }

    public async Task RecordGpsPointAsync(Guid visitReportId, double latitude, double longitude, double? accuracy = null, CancellationToken cancellationToken = default)
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

    public async Task<VisitReportDto> CompleteVisitAsync(Guid visitReportId, CompleteVisitRequest request, CancellationToken cancellationToken = default)
    {
        var report = await _visitReports.GetByIdAsync(visitReportId, cancellationToken)
            ?? throw new InvalidOperationException($"Visit report {visitReportId} not found.");

        report.Status = VisitReportStatus.Completed;
        report.CompletedAt = DateTime.UtcNow;
        report.Notes = request.Notes;
        await _visitReports.UpdateAsync(report, cancellationToken);

        var booking = await _bookings.GetWithDetailsAsync(report.BookingId, cancellationToken);
        if (booking is not null)
        {
            booking.Status = BookingStatus.Completed;
            booking.ActualEnd = DateTime.UtcNow;
            await _bookings.UpdateAsync(booking, cancellationToken);

            if (booking.StripePaymentIntentId is not null && booking.Walker?.StripeAccountId is not null)
            {
                var payout = booking.WalkerEarnings + (booking.TipAmount ?? 0);
                await _payments.ReleaseToWalkerAsync(
                    booking.StripePaymentIntentId,
                    booking.Walker.StripeAccountId,
                    payout,
                    cancellationToken);
            }

            await _notifications.SendVisitCompletedAsync(booking.Id, cancellationToken);
        }

        return await GetVisitReportAsync(visitReportId, cancellationToken)
               ?? throw new InvalidOperationException($"Visit report {visitReportId} not found.");
    }

    public async Task<VisitReportDto?> GetVisitReportAsync(Guid visitReportId, CancellationToken cancellationToken = default)
    {
        var report = await _visitReports.GetWithFullDetailsAsync(visitReportId, cancellationToken);
        return report is null ? null : ToDto(report);
    }

    private static async Task InsertTaskLogAsync(IDbConnection conn, VisitTaskLog log)
    {
        await conn.ExecuteAsync(
            """
            INSERT INTO visit_task_logs (id, visit_report_id, routine_task_id, is_completed, completed_at, notes, created_at, updated_at)
            VALUES (@Id, @VisitReportId, @RoutineTaskId, @IsCompleted, @CompletedAt, @Notes, @CreatedAt, @UpdatedAt)
            """, log);
    }

    private static VisitReportDto ToDto(VisitReport r) => new(
        r.Id, r.BookingId,
        r.Status.ToString(),
        r.Notes,
        r.CompletedAt,
        r.TaskLogs.Select(tl => new VisitTaskLogDto(
            tl.Id,
            tl.RoutineTaskId,
            tl.RoutineTask?.Title ?? string.Empty,
            tl.RoutineTask?.Category.ToString() ?? string.Empty,
            tl.IsCompleted,
            tl.CompletedAt,
            tl.Notes)).ToList(),
        r.Photos.Select(p => new VisitPhotoDto(
            p.Id, p.PhotoUrl, p.Type.ToString(), p.Caption, p.TakenAt)).ToList(),
        r.GpsPoints.Select(g => new GpsPointDto(
            g.Latitude, g.Longitude, g.Accuracy, g.RecordedAt)).ToList());
}
