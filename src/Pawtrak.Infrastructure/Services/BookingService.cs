using Pawtrak.Application.DTOs;
using Pawtrak.Application.Services;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Enums;
using Pawtrak.Core.Interfaces.Repositories;
using Pawtrak.Core.Interfaces.Services;

namespace Pawtrak.Infrastructure.Services;

public sealed class BookingService : IBookingService
{
    private const decimal PlatformFeeRate = 0.15m;
    private const decimal WalkerEarningsRate = 1m - PlatformFeeRate;

    private readonly IBookingRepository _bookings;
    private readonly IWalkerRepository _walkers;
    private readonly IOwnerRepository _owners;
    private readonly IDogRepository _dogs;
    private readonly IPaymentService _paymentService;
    private readonly INotificationService _notifications;

    public BookingService(
        IBookingRepository bookings,
        IWalkerRepository walkers,
        IOwnerRepository owners,
        IDogRepository dogs,
        IPaymentService paymentService,
        INotificationService notifications)
    {
        _bookings = bookings;
        _walkers = walkers;
        _owners = owners;
        _dogs = dogs;
        _paymentService = paymentService;
        _notifications = notifications;
    }

    public async Task<BookingDto> CreateBookingAsync(Guid ownerId, CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        var walker = await _walkers.GetByIdAsync(request.WalkerId, cancellationToken)
            ?? throw new InvalidOperationException($"Walker {request.WalkerId} not found.");
        var owner = await _owners.GetByIdAsync(ownerId, cancellationToken)
            ?? throw new InvalidOperationException($"Owner {ownerId} not found.");
        var dog = await _dogs.GetByIdAsync(request.DogId, cancellationToken)
            ?? throw new InvalidOperationException($"Dog {request.DogId} not found.");

        var hours = (decimal)(request.ScheduledEnd - request.ScheduledStart).TotalHours;
        var baseAmount = Math.Round(hours * walker.HourlyRate, 2);
        var platformFee = Math.Round(baseAmount * PlatformFeeRate, 2);
        var walkerEarnings = Math.Round(baseAmount * WalkerEarningsRate, 2);

        var booking = new Booking
        {
            OwnerId = ownerId,
            WalkerId = request.WalkerId,
            DogId = request.DogId,
            ScheduledStart = request.ScheduledStart,
            ScheduledEnd = request.ScheduledEnd,
            Status = BookingStatus.Pending,
            BaseAmount = baseAmount,
            PlatformFee = platformFee,
            WalkerEarnings = walkerEarnings,
            SpecialInstructions = request.SpecialInstructions,
            Owner = owner,
            Walker = walker,
            Dog = dog
        };

        await _bookings.AddAsync(booking, cancellationToken);
        return ToDto(booking);
    }

    public async Task<BookingDto?> GetBookingAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _bookings.GetWithDetailsAsync(bookingId, cancellationToken);
        return booking is null ? null : ToDto(booking);
    }

    public async Task<IEnumerable<BookingDto>> GetOwnerBookingsAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var ids = await _bookings.GetByOwnerIdAsync(ownerId, cancellationToken);
        var result = new List<BookingDto>();
        foreach (var b in ids)
        {
            var full = await _bookings.GetWithDetailsAsync(b.Id, cancellationToken);
            if (full is not null) result.Add(ToDto(full));
        }
        return result;
    }

    public async Task<IEnumerable<BookingDto>> GetWalkerBookingsAsync(Guid walkerId, CancellationToken cancellationToken = default)
    {
        var ids = await _bookings.GetByWalkerIdAsync(walkerId, cancellationToken);
        var result = new List<BookingDto>();
        foreach (var b in ids)
        {
            var full = await _bookings.GetWithDetailsAsync(b.Id, cancellationToken);
            if (full is not null) result.Add(ToDto(full));
        }
        return result;
    }

    public async Task<BookingDto> ConfirmBookingAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _bookings.GetWithDetailsAsync(bookingId, cancellationToken)
            ?? throw new InvalidOperationException($"Booking {bookingId} not found.");

        booking.Status = BookingStatus.Confirmed;
        await _bookings.UpdateAsync(booking, cancellationToken);
        await _notifications.SendBookingConfirmedAsync(bookingId, cancellationToken);
        return ToDto(booking);
    }

    public async Task<BookingDto> CancelBookingAsync(Guid bookingId, string reason, CancellationToken cancellationToken = default)
    {
        var booking = await _bookings.GetWithDetailsAsync(bookingId, cancellationToken)
            ?? throw new InvalidOperationException($"Booking {bookingId} not found.");

        booking.Status = BookingStatus.Cancelled;
        await _bookings.UpdateAsync(booking, cancellationToken);

        if (booking.StripePaymentIntentId is not null)
            await _paymentService.RefundPaymentAsync(booking.StripePaymentIntentId, cancellationToken);

        return ToDto(booking);
    }

    public async Task<CreatePaymentIntentResponse> CreatePaymentIntentAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _bookings.GetByIdAsync(bookingId, cancellationToken)
            ?? throw new InvalidOperationException($"Booking {bookingId} not found.");

        var clientSecret = await _paymentService.CreatePaymentIntentAsync(
            bookingId, booking.BaseAmount, cancellationToken: cancellationToken);

        // Client secret format: pi_XXXX_secret_YYYY — extract the payment intent ID
        var parts = clientSecret.Split('_');
        var paymentIntentId = $"{parts[0]}_{parts[1]}";

        booking.StripePaymentIntentId = paymentIntentId;
        await _bookings.UpdateAsync(booking, cancellationToken);

        return new CreatePaymentIntentResponse(clientSecret, paymentIntentId);
    }

    public async Task<BookingDto> AddTipAsync(AddTipRequest request, CancellationToken cancellationToken = default)
    {
        var booking = await _bookings.GetWithDetailsAsync(request.BookingId, cancellationToken)
            ?? throw new InvalidOperationException($"Booking {request.BookingId} not found.");

        booking.TipAmount = request.TipAmount;
        await _bookings.UpdateAsync(booking, cancellationToken);
        return ToDto(booking);
    }

    private static BookingDto ToDto(Booking b) => new(
        b.Id,
        b.OwnerId, b.Owner is not null ? $"{b.Owner.FirstName} {b.Owner.LastName}" : string.Empty,
        b.WalkerId, b.Walker is not null ? $"{b.Walker.FirstName} {b.Walker.LastName}" : string.Empty,
        b.DogId, b.Dog?.Name ?? string.Empty,
        b.ScheduledStart, b.ScheduledEnd, b.ActualStart, b.ActualEnd,
        b.Status.ToString(),
        b.BaseAmount, b.PlatformFee, b.WalkerEarnings, b.TipAmount,
        b.SpecialInstructions, b.CreatedAt);
}
