using Pawtrak.Core.Entities;

namespace Pawtrak.Core.Interfaces.Services;

public interface IGpsTrackingService
{
    Task RecordPointAsync(Guid visitReportId, double latitude, double longitude, double? accuracy = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<GpsPoint>> GetTrackAsync(Guid visitReportId, CancellationToken cancellationToken = default);
}
