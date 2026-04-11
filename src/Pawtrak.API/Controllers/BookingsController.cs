using Microsoft.AspNetCore.Mvc;
using Pawtrak.Application.DTOs;
using Pawtrak.Application.Services;

namespace Pawtrak.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IReviewService _reviewService;

    public BookingsController(IBookingService bookingService, IReviewService reviewService)
    {
        _bookingService = bookingService;
        _reviewService = reviewService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromQuery] Guid ownerId,
        [FromBody] CreateBookingRequest request,
        CancellationToken ct)
    {
        var booking = await _bookingService.CreateBookingAsync(ownerId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var booking = await _bookingService.GetBookingAsync(id, ct);
        return booking is null ? NotFound() : Ok(booking);
    }

    [HttpGet("owner/{ownerId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<BookingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOwner(Guid ownerId, CancellationToken ct)
    {
        var bookings = await _bookingService.GetOwnerBookingsAsync(ownerId, ct);
        return Ok(bookings);
    }

    [HttpGet("walker/{walkerId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<BookingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByWalker(Guid walkerId, CancellationToken ct)
    {
        var bookings = await _bookingService.GetWalkerBookingsAsync(walkerId, ct);
        return Ok(bookings);
    }

    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        var booking = await _bookingService.ConfirmBookingAsync(id, ct);
        return Ok(booking);
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancel(Guid id, [FromQuery] string reason, CancellationToken ct)
    {
        var booking = await _bookingService.CancelBookingAsync(id, reason, ct);
        return Ok(booking);
    }

    [HttpPost("{id:guid}/payment-intent")]
    [ProducesResponseType(typeof(CreatePaymentIntentResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreatePaymentIntent(Guid id, CancellationToken ct)
    {
        var result = await _bookingService.CreatePaymentIntentAsync(id, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/tip")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddTip(Guid id, [FromBody] AddTipRequest request, CancellationToken ct)
    {
        var booking = await _bookingService.AddTipAsync(request, ct);
        return Ok(booking);
    }

    // ── Reviews ───────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/review")]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateReview(
        Guid id,
        [FromQuery] Guid ownerId,
        [FromBody] CreateReviewRequest request,
        CancellationToken ct)
    {
        var review = await _reviewService.CreateReviewAsync(ownerId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id }, review);
    }
}
