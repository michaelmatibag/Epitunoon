using Pawtrak.Core.Common;

namespace Pawtrak.Core.Entities;

public class Walker : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string? ProfilePhotoUrl { get; set; }
    public decimal HourlyRate { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsBackgroundChecked { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public string? StripeAccountId { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<WalkerAvailability> Availabilities { get; set; } = new List<WalkerAvailability>();
}
