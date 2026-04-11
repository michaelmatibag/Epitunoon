using Pawtrak.Core.Common;
using Pawtrak.Core.Enums;

namespace Pawtrak.Core.Entities;

public class Payment : BaseEntity
{
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal WalkerPayout { get; set; }
    public decimal? TipAmount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string StripePaymentIntentId { get; set; } = string.Empty;
    public string? StripeChargeId { get; set; }
    public string? StripeTransferId { get; set; }
    public DateTime? AuthorizedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }

    public Booking Booking { get; set; } = null!;
}
