using Microsoft.AspNetCore.Mvc;
using Pawtrak.Application.DTOs;
using Pawtrak.Application.Services;

namespace Pawtrak.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class VisitsController : ControllerBase
{
    private readonly IVisitService _visitService;

    public VisitsController(IVisitService visitService)
    {
        _visitService = visitService;
    }

    [HttpPost("start/{bookingId:guid}")]
    [ProducesResponseType(typeof(VisitReportDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Start(Guid bookingId, CancellationToken ct)
    {
        var report = await _visitService.StartVisitAsync(bookingId, ct);
        return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VisitReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var report = await _visitService.GetVisitReportAsync(id, ct);
        return report is null ? NotFound() : Ok(report);
    }

    [HttpPost("{id:guid}/tasks/{taskId:guid}/complete")]
    [ProducesResponseType(typeof(VisitReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CompleteTask(
        Guid id,
        Guid taskId,
        [FromBody] CompleteTaskRequest request,
        CancellationToken ct)
    {
        var report = await _visitService.CompleteTaskAsync(id, taskId, request, ct);
        return Ok(report);
    }

    [HttpPost("{id:guid}/photos")]
    [ProducesResponseType(typeof(VisitReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadPhoto(
        Guid id,
        IFormFile photo,
        [FromQuery] string photoType,
        CancellationToken ct)
    {
        using var stream = photo.OpenReadStream();
        var report = await _visitService.AddPhotoAsync(id, stream, photo.FileName, photoType, ct);
        return Ok(report);
    }

    [HttpPost("{id:guid}/gps")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RecordGps(
        Guid id,
        [FromBody] GpsPointDto point,
        CancellationToken ct)
    {
        await _visitService.RecordGpsPointAsync(id, point.Latitude, point.Longitude, point.Accuracy, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(VisitReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Complete(
        Guid id,
        [FromBody] CompleteVisitRequest request,
        CancellationToken ct)
    {
        var report = await _visitService.CompleteVisitAsync(id, request, ct);
        return Ok(report);
    }
}
