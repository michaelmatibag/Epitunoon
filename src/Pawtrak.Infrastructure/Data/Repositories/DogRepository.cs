using System.Data;
using Dapper;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Interfaces.Repositories;

namespace Pawtrak.Infrastructure.Data.Repositories;

public sealed class DogRepository : RepositoryBase<Dog>, IDogRepository
{
    protected override string TableName => "dogs";

    public DogRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<IEnumerable<Dog>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Dog>(
            "SELECT * FROM dogs WHERE owner_id = @OwnerId",
            new { OwnerId = ownerId });
    }

    public async Task<Dog?> GetWithRoutineTasksAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT d.*, rt.*
            FROM dogs d
            LEFT JOIN routine_tasks rt ON rt.dog_id = d.id
            WHERE d.id = @Id
            ORDER BY rt.sort_order
            """;

        Dog? dog = null;
        await conn.QueryAsync<Dog, RoutineTask, Dog>(sql, (d, task) =>
        {
            dog ??= d;
            if (task is not null) dog.RoutineTasks.Add(task);
            return dog;
        }, new { Id = id }, splitOn: "id");

        return dog;
    }

    protected override async Task InsertAsync(IDbConnection conn, Dog entity)
    {
        const string sql = """
            INSERT INTO dogs
                (id, owner_id, name, breed, age_years, weight_lbs, photo_url,
                 medical_notes, behavior_notes, emergency_contact, vet_name, vet_phone,
                 created_at, updated_at)
            VALUES
                (@Id, @OwnerId, @Name, @Breed, @AgeYears, @WeightLbs, @PhotoUrl,
                 @MedicalNotes, @BehaviorNotes, @EmergencyContact, @VetName, @VetPhone,
                 @CreatedAt, @UpdatedAt)
            """;
        await conn.ExecuteAsync(sql, entity);
    }

    protected override async Task UpdateCoreAsync(IDbConnection conn, Dog entity)
    {
        const string sql = """
            UPDATE dogs SET
                name = @Name, breed = @Breed, age_years = @AgeYears, weight_lbs = @WeightLbs,
                photo_url = @PhotoUrl, medical_notes = @MedicalNotes, behavior_notes = @BehaviorNotes,
                emergency_contact = @EmergencyContact, vet_name = @VetName, vet_phone = @VetPhone,
                updated_at = @UpdatedAt
            WHERE id = @Id
            """;
        await conn.ExecuteAsync(sql, entity);
    }
}
