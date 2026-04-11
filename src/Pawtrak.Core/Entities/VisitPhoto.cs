using Pawtrak.Core.Common;
using Pawtrak.Core.Enums;

namespace Pawtrak.Core.Entities;

public class VisitPhoto : BaseEntity
{
    public Guid VisitReportId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public PhotoType Type { get; set; }
    public string? Caption { get; set; }
    public DateTime TakenAt { get; set; } = DateTime.UtcNow;

    public VisitReport VisitReport { get; set; } = null!;
}
