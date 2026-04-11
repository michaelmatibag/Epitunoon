using Pawtrak.Core.Common;

namespace Pawtrak.Core.Entities;

public class GpsPoint : BaseEntity
{
    public Guid VisitReportId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Accuracy { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public VisitReport VisitReport { get; set; } = null!;
}
