using Pawtrak.Core.Common;

namespace Pawtrak.Core.Entities;

public class WalkerAvailability : BaseEntity
{
    public Guid WalkerId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public Walker Walker { get; set; } = null!;
}
