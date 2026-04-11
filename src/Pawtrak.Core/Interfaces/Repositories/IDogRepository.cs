using Pawtrak.Core.Entities;

namespace Pawtrak.Core.Interfaces.Repositories;

public interface IDogRepository : IRepository<Dog>
{
    Task<IEnumerable<Dog>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<Dog?> GetWithRoutineTasksAsync(Guid id, CancellationToken cancellationToken = default);
}
