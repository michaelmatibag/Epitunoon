using Pawtrak.Application.DTOs;

namespace Pawtrak.Application.Services;

public interface IVisitService
{
    Task<VisitReportDto> StartVisitAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<VisitReportDto> CompleteTaskAsync(Guid visitReportId, Guid routineTaskId, CompleteTaskRequest request, CancellationToken cancellationToken = default);
    Task<VisitReportDto> AddPhotoAsync(Guid visitReportId, Stream photoStream, string fileName, string photoType, CancellationToken cancellationToken = default);
    Task RecordGpsPointAsync(Guid visitReportId, double latitude, double longitude, double? accuracy = null, CancellationToken cancellationToken = default);
    Task<VisitReportDto> CompleteVisitAsync(Guid visitReportId, CompleteVisitRequest request, CancellationToken cancellationToken = default);
    Task<VisitReportDto?> GetVisitReportAsync(Guid visitReportId, CancellationToken cancellationToken = default);
}
