using Pawtrak.Application.DTOs;
using Pawtrak.Application.Services;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Interfaces.Repositories;
using Pawtrak.Core.Interfaces.Services;

namespace Pawtrak.Infrastructure.Services;

public sealed class WalkerService : IWalkerService
{
    private readonly IWalkerRepository _walkers;
    private readonly IPaymentService _payments;

    public WalkerService(IWalkerRepository walkers, IPaymentService payments)
    {
        _walkers = walkers;
        _payments = payments;
    }

    public async Task<WalkerDto> CreateWalkerAsync(CreateWalkerRequest request, CancellationToken cancellationToken = default)
    {
        var walker = new Walker
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Bio = request.Bio,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode,
            HourlyRate = request.HourlyRate,
            IsAvailable = false,
            IsBackgroundChecked = false
        };
        await _walkers.AddAsync(walker, cancellationToken);
        return ToDto(walker);
    }

    public async Task<WalkerDto?> GetWalkerAsync(Guid walkerId, CancellationToken cancellationToken = default)
    {
        var walker = await _walkers.GetByIdAsync(walkerId, cancellationToken);
        return walker is null ? null : ToDto(walker);
    }

    public async Task<IEnumerable<WalkerDto>> SearchWalkersAsync(WalkerSearchRequest request, CancellationToken cancellationToken = default)
    {
        var walkers = await _walkers.GetAvailableByZipCodeAsync(request.ZipCode, cancellationToken);

        if (request.MaxHourlyRate.HasValue)
            walkers = walkers.Where(w => w.HourlyRate <= request.MaxHourlyRate.Value);

        return walkers.Select(ToDto);
    }

    public async Task<WalkerDto> SetAvailabilityAsync(Guid walkerId, bool isAvailable, CancellationToken cancellationToken = default)
    {
        var walker = await _walkers.GetByIdAsync(walkerId, cancellationToken)
            ?? throw new InvalidOperationException($"Walker {walkerId} not found.");

        walker.IsAvailable = isAvailable;
        await _walkers.UpdateAsync(walker, cancellationToken);
        return ToDto(walker);
    }

    public async Task<string> ConnectStripeAccountAsync(Guid walkerId, CancellationToken cancellationToken = default)
    {
        var walker = await _walkers.GetByIdAsync(walkerId, cancellationToken)
            ?? throw new InvalidOperationException($"Walker {walkerId} not found.");

        var accountId = await _payments.CreateWalkerConnectAccountAsync(walker.Email, cancellationToken);
        walker.StripeAccountId = accountId;
        await _walkers.UpdateAsync(walker, cancellationToken);

        return await _payments.CreateAccountLinkAsync(accountId, cancellationToken);
    }

    internal static WalkerDto ToDto(Walker w) => new(
        w.Id, w.FirstName, w.LastName, w.Email, w.PhoneNumber,
        w.City, w.State, w.ZipCode, w.Bio, w.HourlyRate,
        w.IsAvailable, w.IsBackgroundChecked, w.AverageRating,
        w.TotalReviews, w.ProfilePhotoUrl, w.CreatedAt);
}
