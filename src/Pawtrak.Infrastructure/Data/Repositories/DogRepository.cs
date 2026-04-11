using System.Data;
using Dapper;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Interfaces.Repositories;

namespace Pawtrak.Infrastructure.Data.Repositories;

public sealed class DogRepository : RepositoryBase<Dog>, IDogRepository
{
    protected override string TableName => "Dogs";

    public DogRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<IEnumerable<Dog>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Dog>(
            "SELECT * FROM Dogs WHERE OwnerId = @OwnerId",
            new { OwnerId = ownerId });
    }

    public async Task<Dog?> GetWithRoutineTasksAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT d.*, rt.*
            FROM Dogs d
            LEFT JOIN RoutineTasks rt ON rt.DogId = d.Id
            WHERE d.Id = @Id
            ORDER BY rt.SortOrder
            """;

        Dog? dog = null;
        await conn.QueryAsync<Dog, RoutineTask, Dog>(sql, (d, task) =>
        {
            dog ??= d;
            if (task is not null) dog.RoutineTasks.Add(task);
            return dog;
        }, new { Id = id }, splitOn: "Id");

        return dog;
    }

    protected override async Task InsertAsync(IDbConnection conn, Dog entity)
    {
        const string sql = """
            INSERT INTO Dogs
                (Id, OwnerId, Name, Breed, AgeYears, WeightLbs, PhotoUrl,
                 MedicalNotes, BehaviorNotes, EmergencyContact, VetName, VetPhone, CreatedAt, UpdatedAt)
            VALUES
                (@Id, @OwnerId, @Name, @Breed, @AgeYears, @WeightLbs, @PhotoUrl,
                 @MedicalNotes, @BehaviorNotes, @EmergencyContact, @VetName, @VetPhone, @CreatedAt, @UpdatedAt)
            """;
        await conn.ExecuteAsync(sql, entity);
    }

    protected override async Task UpdateCoreAsync(IDbConnection conn, Dog entity)
    {
        const string sql = """
            UPDATE Dogs SET
                Name = @Name, Breed = @Breed, AgeYears = @AgeYears, WeightLbs = @WeightLbs,
                PhotoUrl = @PhotoUrl, MedicalNotes = @MedicalNotes, BehaviorNotes = @BehaviorNotes,
                EmergencyContact = @EmergencyContact, VetName = @VetName, VetPhone = @VetPhone,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id
            """;
        await conn.ExecuteAsync(sql, entity);
    }
}
