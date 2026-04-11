using Pawtrak.Application.DTOs;
using Pawtrak.Application.Services;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Enums;
using Pawtrak.Core.Interfaces.Repositories;

namespace Pawtrak.Infrastructure.Services;

public sealed class RoutineTaskService : IRoutineTaskService
{
    private readonly IRoutineTaskRepository _tasks;

    public RoutineTaskService(IRoutineTaskRepository tasks) => _tasks = tasks;

    public async Task<RoutineTaskDto> AddTaskAsync(Guid dogId, CreateRoutineTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = new RoutineTask
        {
            DogId = dogId,
            Title = request.Title,
            Description = request.Description,
            SortOrder = request.SortOrder,
            IsRequired = request.IsRequired,
            Category = Enum.Parse<TaskCategory>(request.Category, ignoreCase: true)
        };
        await _tasks.AddAsync(task, cancellationToken);
        return ToDto(task);
    }

    public async Task<IEnumerable<RoutineTaskDto>> GetDogTasksAsync(Guid dogId, CancellationToken cancellationToken = default)
    {
        var tasks = await _tasks.GetOrderedByDogIdAsync(dogId, cancellationToken);
        return tasks.Select(ToDto);
    }

    public async Task<RoutineTaskDto> UpdateTaskAsync(Guid taskId, CreateRoutineTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await _tasks.GetByIdAsync(taskId, cancellationToken)
            ?? throw new InvalidOperationException($"Task {taskId} not found.");

        task.Title = request.Title;
        task.Description = request.Description;
        task.SortOrder = request.SortOrder;
        task.IsRequired = request.IsRequired;
        task.Category = Enum.Parse<TaskCategory>(request.Category, ignoreCase: true);

        await _tasks.UpdateAsync(task, cancellationToken);
        return ToDto(task);
    }

    public async Task DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await _tasks.GetByIdAsync(taskId, cancellationToken)
            ?? throw new InvalidOperationException($"Task {taskId} not found.");
        await _tasks.DeleteAsync(task, cancellationToken);
    }

    public async Task ReorderTasksAsync(Guid dogId, ReorderTasksRequest request, CancellationToken cancellationToken = default)
    {
        var tasks = (await _tasks.GetByDogIdAsync(dogId, cancellationToken)).ToDictionary(t => t.Id);
        var order = 0;
        foreach (var id in request.TaskIds)
        {
            if (!tasks.TryGetValue(id, out var task)) continue;
            task.SortOrder = order++;
            await _tasks.UpdateAsync(task, cancellationToken);
        }
    }

    internal static RoutineTaskDto ToDto(RoutineTask t) => new(
        t.Id, t.DogId, t.Title, t.Description, t.SortOrder, t.IsRequired, t.Category.ToString());
}
