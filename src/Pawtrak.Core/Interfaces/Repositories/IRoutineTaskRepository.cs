using Pawtrak.Core.Entities;

namespace Pawtrak.Core.Interfaces.Repositories;

public interface IRoutineTaskRepository : IRepository<RoutineTask>
{
    Task<IEnumerable<RoutineTask>> GetByDogIdAsync(Guid dogId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RoutineTask>> GetOrderedByDogIdAsync(Guid dogId, CancellationToken cancellationToken = default);
}
