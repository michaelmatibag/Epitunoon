using Pawtrak.Application.DTOs;

namespace Pawtrak.Application.Services;

public interface IWalkerService
{
    Task<WalkerDto> CreateWalkerAsync(CreateWalkerRequest request, CancellationToken cancellationToken = default);
    Task<WalkerDto?> GetWalkerAsync(Guid walkerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WalkerDto>> SearchWalkersAsync(WalkerSearchRequest request, CancellationToken cancellationToken = default);
    Task<WalkerDto> SetAvailabilityAsync(Guid walkerId, bool isAvailable, CancellationToken cancellationToken = default);
    Task<string> ConnectStripeAccountAsync(Guid walkerId, CancellationToken cancellationToken = default);
}
