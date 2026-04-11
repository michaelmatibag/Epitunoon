using Pawtrak.Core.Common;
using Pawtrak.Core.Enums;

namespace Pawtrak.Core.Entities;

public class RoutineTask : BaseEntity
{
    public Guid DogId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsRequired { get; set; } = true;
    public TaskCategory Category { get; set; }

    public Dog Dog { get; set; } = null!;
    public ICollection<VisitTaskLog> VisitTaskLogs { get; set; } = new List<VisitTaskLog>();
}
