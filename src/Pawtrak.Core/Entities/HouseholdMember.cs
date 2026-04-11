using Pawtrak.Core.Common;

namespace Pawtrak.Core.Entities;

public class HouseholdMember : BaseEntity
{
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool NotifyOnBooking { get; set; } = true;
    public bool NotifyOnVisitStart { get; set; } = true;
    public bool NotifyOnVisitEnd { get; set; } = true;

    public Owner Owner { get; set; } = null!;
}
