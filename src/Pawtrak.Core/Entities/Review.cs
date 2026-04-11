using Pawtrak.Core.Common;

namespace Pawtrak.Core.Entities;

public class Review : BaseEntity
{
    public Guid BookingId { get; set; }
    public Guid OwnerId { get; set; }
    public Guid WalkerId { get; set; }
    public int Rating { get; set; } // 1–5
    public string? Comment { get; set; }
    public bool IsPublic { get; set; } = true;

    public Booking Booking { get; set; } = null!;
    public Owner Owner { get; set; } = null!;
    public Walker Walker { get; set; } = null!;
}
