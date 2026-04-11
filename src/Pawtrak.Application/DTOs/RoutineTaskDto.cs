namespace Pawtrak.Application.DTOs;

public record RoutineTaskDto(
    Guid Id,
    Guid DogId,
    string Title,
    string? Description,
    int SortOrder,
    bool IsRequired,
    string Category
);

public record CreateRoutineTaskRequest(
    string Title,
    string? Description,
    int SortOrder,
    bool IsRequired,
    string Category
);

public record ReorderTasksRequest(
    List<Guid> TaskIds
);
