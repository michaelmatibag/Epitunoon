namespace Pawtrak.Application.DTOs;

public record DogDto(
    Guid Id,
    Guid OwnerId,
    string Name,
    string Breed,
    int AgeYears,
    decimal WeightLbs,
    string? PhotoUrl,
    string? MedicalNotes,
    string? BehaviorNotes,
    string? EmergencyContact,
    string? VetName,
    string? VetPhone,
    List<RoutineTaskDto> RoutineTasks
);

public record CreateDogRequest(
    string Name,
    string Breed,
    int AgeYears,
    decimal WeightLbs,
    string? MedicalNotes,
    string? BehaviorNotes,
    string? EmergencyContact,
    string? VetName,
    string? VetPhone
);
