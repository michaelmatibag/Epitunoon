using Pawtrak.Core.Common;

namespace Pawtrak.Core.Entities;

public class VisitTaskLog : BaseEntity
{
    public Guid VisitReportId { get; set; }
    public Guid RoutineTaskId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }

    public VisitReport VisitReport { get; set; } = null!;
    public RoutineTask RoutineTask { get; set; } = null!;
}
