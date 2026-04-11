using Microsoft.AspNetCore.Mvc;
using Pawtrak.Application.DTOs;
using Pawtrak.Application.Services;

namespace Pawtrak.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WalkersController : ControllerBase
{
    private readonly IWalkerService _walkerService;

    public WalkersController(IWalkerService walkerService)
    {
        _walkerService = walkerService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(WalkerDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateWalkerRequest request, CancellationToken ct)
    {
        var walker = await _walkerService.CreateWalkerAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = walker.Id }, walker);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WalkerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var walker = await _walkerService.GetWalkerAsync(id, ct);
        return walker is null ? NotFound() : Ok(walker);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<WalkerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] WalkerSearchRequest request, CancellationToken ct)
    {
        var walkers = await _walkerService.SearchWalkersAsync(request, ct);
        return Ok(walkers);
    }

    [HttpPatch("{id:guid}/availability")]
    [ProducesResponseType(typeof(WalkerDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetAvailability(Guid id, [FromQuery] bool isAvailable, CancellationToken ct)
    {
        var walker = await _walkerService.SetAvailabilityAsync(id, isAvailable, ct);
        return Ok(walker);
    }

    [HttpPost("{id:guid}/stripe-connect")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConnectStripe(Guid id, CancellationToken ct)
    {
        var onboardingUrl = await _walkerService.ConnectStripeAccountAsync(id, ct);
        return Ok(new { onboardingUrl });
    }
}
