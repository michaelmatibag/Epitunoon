using Pawtrak.Core.Entities;

namespace Pawtrak.Core.Interfaces.Repositories;

public interface IWalkerRepository : IRepository<Walker>
{
    Task<Walker?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Walker>> GetAvailableByZipCodeAsync(string zipCode, CancellationToken cancellationToken = default);
    Task<Walker?> GetWithReviewsAsync(Guid id, CancellationToken cancellationToken = default);
}
