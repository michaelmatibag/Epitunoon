using Pawtrak.Core.Common;
using Pawtrak.Core.Enums;

namespace Pawtrak.Core.Entities;

public class VisitReport : BaseEntity
{
    public Guid BookingId { get; set; }
    public string? Notes { get; set; }
    public VisitReportStatus Status { get; set; } = VisitReportStatus.InProgress;
    public DateTime? CompletedAt { get; set; }

    public Booking Booking { get; set; } = null!;
    public ICollection<VisitPhoto> Photos { get; set; } = new List<VisitPhoto>();
    public ICollection<VisitTaskLog> TaskLogs { get; set; } = new List<VisitTaskLog>();
    public ICollection<GpsPoint> GpsPoints { get; set; } = new List<GpsPoint>();
}
