using Pawtrak.Application.DTOs;

namespace Pawtrak.Application.Services;

public interface IRoutineTaskService
{
    Task<RoutineTaskDto> AddTaskAsync(Guid dogId, CreateRoutineTaskRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<RoutineTaskDto>> GetDogTasksAsync(Guid dogId, CancellationToken cancellationToken = default);
    Task<RoutineTaskDto> UpdateTaskAsync(Guid taskId, CreateRoutineTaskRequest request, CancellationToken cancellationToken = default);
    Task DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task ReorderTasksAsync(Guid dogId, ReorderTasksRequest request, CancellationToken cancellationToken = default);
}
