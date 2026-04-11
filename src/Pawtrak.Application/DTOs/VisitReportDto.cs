namespace Pawtrak.Application.DTOs;

public record VisitReportDto(
    Guid Id,
    Guid BookingId,
    string Status,
    string? Notes,
    DateTime? CompletedAt,
    List<VisitTaskLogDto> TaskLogs,
    List<VisitPhotoDto> Photos,
    List<GpsPointDto> GpsPoints
);

public record VisitTaskLogDto(
    Guid Id,
    Guid RoutineTaskId,
    string TaskTitle,
    string TaskCategory,
    bool IsCompleted,
    DateTime? CompletedAt,
    string? Notes
);

public record VisitPhotoDto(
    Guid Id,
    string PhotoUrl,
    string Type,
    string? Caption,
    DateTime TakenAt
);

public record GpsPointDto(
    double Latitude,
    double Longitude,
    double? Accuracy,
    DateTime RecordedAt
);

public record CompleteVisitRequest(
    string? Notes
);

public record CompleteTaskRequest(
    string? Notes
);
