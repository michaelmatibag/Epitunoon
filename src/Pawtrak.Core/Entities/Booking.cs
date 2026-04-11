using Pawtrak.Core.Common;
using Pawtrak.Core.Enums;

namespace Pawtrak.Core.Entities;

public class Booking : BaseEntity
{
    public Guid OwnerId { get; set; }
    public Guid WalkerId { get; set; }
    public Guid DogId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public DateTime? ActualStart { get; set; }
    public DateTime? ActualEnd { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public decimal BaseAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal WalkerEarnings { get; set; }
    public decimal? TipAmount { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? StripePaymentIntentId { get; set; }

    public Owner Owner { get; set; } = null!;
    public Walker Walker { get; set; } = null!;
    public Dog Dog { get; set; } = null!;
    public VisitReport? VisitReport { get; set; }
    public Payment? Payment { get; set; }
    public Review? Review { get; set; }
}
