using Pawtrak.Core.Entities;

namespace Pawtrak.Core.Interfaces.Repositories;

public interface IOwnerRepository : IRepository<Owner>
{
    Task<Owner?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Owner?> GetWithDogsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Owner?> GetWithHouseholdAsync(Guid id, CancellationToken cancellationToken = default);
}
