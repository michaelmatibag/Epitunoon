using Microsoft.AspNetCore.Mvc;
using Pawtrak.Application.DTOs;
using Pawtrak.Application.Services;

namespace Pawtrak.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OwnersController : ControllerBase
{
    private readonly IOwnerService _ownerService;
    private readonly IDogService _dogService;

    public OwnersController(IOwnerService ownerService, IDogService dogService)
    {
        _ownerService = ownerService;
        _dogService = dogService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OwnerDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateOwnerRequest request, CancellationToken ct)
    {
        var owner = await _ownerService.CreateOwnerAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = owner.Id }, owner);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OwnerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var owner = await _ownerService.GetOwnerAsync(id, ct);
        return owner is null ? NotFound() : Ok(owner);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(OwnerDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOwnerRequest request, CancellationToken ct)
    {
        var owner = await _ownerService.UpdateOwnerAsync(id, request, ct);
        return Ok(owner);
    }

    // ── Dogs ──────────────────────────────────────────────────────────────

    [HttpGet("{id:guid}/dogs")]
    [ProducesResponseType(typeof(IEnumerable<DogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDogs(Guid id, CancellationToken ct)
    {
        var dogs = await _dogService.GetOwnerDogsAsync(id, ct);
        return Ok(dogs);
    }

    [HttpPost("{id:guid}/dogs")]
    [ProducesResponseType(typeof(DogDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddDog(Guid id, [FromBody] CreateDogRequest request, CancellationToken ct)
    {
        var dog = await _dogService.AddDogAsync(id, request, ct);
        return CreatedAtAction(nameof(DogsController.GetById), "Dogs", new { id = dog.Id }, dog);
    }
}
