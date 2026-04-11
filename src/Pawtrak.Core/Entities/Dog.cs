using Pawtrak.Core.Common;

namespace Pawtrak.Core.Entities;

public class Dog : BaseEntity
{
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public int AgeYears { get; set; }
    public decimal WeightLbs { get; set; }
    public string? PhotoUrl { get; set; }
    public string? MedicalNotes { get; set; }
    public string? BehaviorNotes { get; set; }
    public string? EmergencyContact { get; set; }
    public string? VetName { get; set; }
    public string? VetPhone { get; set; }

    public Owner Owner { get; set; } = null!;
    public ICollection<RoutineTask> RoutineTasks { get; set; } = new List<RoutineTask>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
