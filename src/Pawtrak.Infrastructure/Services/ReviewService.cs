using Pawtrak.Application.DTOs;
using Pawtrak.Application.Services;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Enums;
using Pawtrak.Core.Interfaces.Repositories;

namespace Pawtrak.Infrastructure.Services;

public sealed class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviews;
    private readonly IWalkerRepository _walkers;
    private readonly IBookingRepository _bookings;

    public ReviewService(IReviewRepository reviews, IWalkerRepository walkers, IBookingRepository bookings)
    {
        _reviews = reviews;
        _walkers = walkers;
        _bookings = bookings;
    }

    public async Task<ReviewDto> CreateReviewAsync(Guid ownerId, CreateReviewRequest request, CancellationToken cancellationToken = default)
    {
        var booking = await _bookings.GetWithDetailsAsync(request.BookingId, cancellationToken)
            ?? throw new InvalidOperationException($"Booking {request.BookingId} not found.");

        if (booking.Status != BookingStatus.Completed)
            throw new InvalidOperationException("Reviews can only be submitted for completed bookings.");

        if (request.Rating is < 1 or > 5)
            throw new ArgumentException("Rating must be between 1 and 5.", nameof(request));

        var review = new Review
        {
            BookingId = request.BookingId,
            OwnerId = ownerId,
            WalkerId = booking.WalkerId,
            Rating = request.Rating,
            Comment = request.Comment,
            IsPublic = true,
            Owner = booking.Owner,
            Walker = booking.Walker,
            Booking = booking
        };
        await _reviews.AddAsync(review, cancellationToken);

        // Recalculate walker aggregate rating
        var walker = await _walkers.GetByIdAsync(booking.WalkerId, cancellationToken);
        if (walker is not null)
        {
            walker.AverageRating = await _reviews.GetAverageRatingForWalkerAsync(booking.WalkerId, cancellationToken);
            walker.TotalReviews = (await _reviews.GetByWalkerIdAsync(booking.WalkerId, cancellationToken)).Count();
            await _walkers.UpdateAsync(walker, cancellationToken);
        }

        return ToDto(review);
    }

    public async Task<IEnumerable<ReviewDto>> GetWalkerReviewsAsync(Guid walkerId, CancellationToken cancellationToken = default)
    {
        var reviews = await _reviews.GetByWalkerIdAsync(walkerId, cancellationToken);
        return reviews.Select(ToDto);
    }

    public async Task<ReviewDto?> GetByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var review = await _reviews.GetByBookingIdAsync(bookingId, cancellationToken);
        return review is null ? null : ToDto(review);
    }

    private static ReviewDto ToDto(Review r) => new(
        r.Id, r.BookingId, r.OwnerId,
        r.Owner is not null ? $"{r.Owner.FirstName} {r.Owner.LastName}" : string.Empty,
        r.WalkerId, r.Rating, r.Comment, r.IsPublic, r.CreatedAt);
}
