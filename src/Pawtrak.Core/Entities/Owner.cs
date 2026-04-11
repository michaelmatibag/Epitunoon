using Pawtrak.Core.Common;

namespace Pawtrak.Core.Entities;

public class Owner : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string? ProfilePhotoUrl { get; set; }
    public string? StripeCustomerId { get; set; }

    public ICollection<Dog> Dogs { get; set; } = new List<Dog>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<HouseholdMember> HouseholdMembers { get; set; } = new List<HouseholdMember>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
