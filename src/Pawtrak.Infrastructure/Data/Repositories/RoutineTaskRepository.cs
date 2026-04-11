using System.Data;
using Dapper;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Interfaces.Repositories;

namespace Pawtrak.Infrastructure.Data.Repositories;

public sealed class RoutineTaskRepository : RepositoryBase<RoutineTask>, IRoutineTaskRepository
{
    protected override string TableName => "RoutineTasks";

    public RoutineTaskRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<IEnumerable<RoutineTask>> GetByDogIdAsync(Guid dogId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<RoutineTask>(
            "SELECT * FROM RoutineTasks WHERE DogId = @DogId",
            new { DogId = dogId });
    }

    public async Task<IEnumerable<RoutineTask>> GetOrderedByDogIdAsync(Guid dogId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<RoutineTask>(
            "SELECT * FROM RoutineTasks WHERE DogId = @DogId ORDER BY SortOrder ASC",
            new { DogId = dogId });
    }

    protected override async Task InsertAsync(IDbConnection conn, RoutineTask entity)
    {
        const string sql = """
            INSERT INTO RoutineTasks
                (Id, DogId, Title, Description, SortOrder, IsRequired, Category, CreatedAt, UpdatedAt)
            VALUES
                (@Id, @DogId, @Title, @Description, @SortOrder, @IsRequired, @Category, @CreatedAt, @UpdatedAt)
            """;
        await conn.ExecuteAsync(sql, entity);
    }

    protected override async Task UpdateCoreAsync(IDbConnection conn, RoutineTask entity)
    {
        const string sql = """
            UPDATE RoutineTasks SET
                Title = @Title, Description = @Description, SortOrder = @SortOrder,
                IsRequired = @IsRequired, Category = @Category, UpdatedAt = @UpdatedAt
            WHERE Id = @Id
            """;
        await conn.ExecuteAsync(sql, entity);
    }
}
