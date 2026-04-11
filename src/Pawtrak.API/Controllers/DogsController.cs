using Microsoft.AspNetCore.Mvc;
using Pawtrak.Application.DTOs;
using Pawtrak.Application.Services;

namespace Pawtrak.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DogsController : ControllerBase
{
    private readonly IDogService _dogService;
    private readonly IRoutineTaskService _routineTaskService;

    public DogsController(IDogService dogService, IRoutineTaskService routineTaskService)
    {
        _dogService = dogService;
        _routineTaskService = routineTaskService;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var dog = await _dogService.GetDogAsync(id, ct);
        return dog is null ? NotFound() : Ok(dog);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DogDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateDogRequest request, CancellationToken ct)
    {
        var dog = await _dogService.UpdateDogAsync(id, request, ct);
        return Ok(dog);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _dogService.DeleteDogAsync(id, ct);
        return NoContent();
    }

    // ── Routine Tasks ─────────────────────────────────────────────────────

    [HttpGet("{id:guid}/tasks")]
    [ProducesResponseType(typeof(IEnumerable<RoutineTaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks(Guid id, CancellationToken ct)
    {
        var tasks = await _routineTaskService.GetDogTasksAsync(id, ct);
        return Ok(tasks);
    }

    [HttpPost("{id:guid}/tasks")]
    [ProducesResponseType(typeof(RoutineTaskDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddTask(Guid id, [FromBody] CreateRoutineTaskRequest request, CancellationToken ct)
    {
        var task = await _routineTaskService.AddTaskAsync(id, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = task.DogId }, task);
    }

    [HttpPut("{id:guid}/tasks/reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReorderTasks(Guid id, [FromBody] ReorderTasksRequest request, CancellationToken ct)
    {
        await _routineTaskService.ReorderTasksAsync(id, request, ct);
        return NoContent();
    }
}
