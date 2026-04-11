using System.Data;
using Dapper;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Interfaces.Repositories;

namespace Pawtrak.Infrastructure.Data.Repositories;

public sealed class RoutineTaskRepository : RepositoryBase<RoutineTask>, IRoutineTaskRepository
{
    protected override string TableName => "routine_tasks";

    public RoutineTaskRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<IEnumerable<RoutineTask>> GetByDogIdAsync(Guid dogId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<RoutineTask>(
            "SELECT * FROM routine_tasks WHERE dog_id = @DogId",
            new { DogId = dogId });
    }

    public async Task<IEnumerable<RoutineTask>> GetOrderedByDogIdAsync(Guid dogId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<RoutineTask>(
            "SELECT * FROM routine_tasks WHERE dog_id = @DogId ORDER BY sort_order ASC",
            new { DogId = dogId });
    }

    protected override async Task InsertAsync(IDbConnection conn, RoutineTask entity)
    {
        const string sql = """
            INSERT INTO routine_tasks
                (id, dog_id, title, description, sort_order, is_required, category, created_at, updated_at)
            VALUES
                (@Id, @DogId, @Title, @Description, @SortOrder, @IsRequired, @Category, @CreatedAt, @UpdatedAt)
            """;
        await conn.ExecuteAsync(sql, entity);
    }

    protected override async Task UpdateCoreAsync(IDbConnection conn, RoutineTask entity)
    {
        const string sql = """
            UPDATE routine_tasks SET
                title = @Title, description = @Description, sort_order = @SortOrder,
                is_required = @IsRequired, category = @Category, updated_at = @UpdatedAt
            WHERE id = @Id
            """;
        await conn.ExecuteAsync(sql, entity);
    }
}
