using System.Data;
using Dapper;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Interfaces.Repositories;

namespace Pawtrak.Infrastructure.Data.Repositories;

public sealed class OwnerRepository : RepositoryBase<Owner>, IOwnerRepository
{
    protected override string TableName => "Owners";

    public OwnerRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<Owner?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Owner>(
            "SELECT * FROM Owners WHERE Email = @Email",
            new { Email = email });
    }

    public async Task<Owner?> GetWithDogsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT o.*, d.*
            FROM Owners o
            LEFT JOIN Dogs d ON d.OwnerId = o.Id
            WHERE o.Id = @Id
            """;

        Owner? owner = null;
        await conn.QueryAsync<Owner, Dog, Owner>(sql, (o, dog) =>
        {
            owner ??= o;
            if (dog is not null) owner.Dogs.Add(dog);
            return owner;
        }, new { Id = id }, splitOn: "Id");

        return owner;
    }

    public async Task<Owner?> GetWithHouseholdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT o.*, hm.*
            FROM Owners o
            LEFT JOIN HouseholdMembers hm ON hm.OwnerId = o.Id
            WHERE o.Id = @Id
            """;

        Owner? owner = null;
        await conn.QueryAsync<Owner, HouseholdMember, Owner>(sql, (o, member) =>
        {
            owner ??= o;
            if (member is not null) owner.HouseholdMembers.Add(member);
            return owner;
        }, new { Id = id }, splitOn: "Id");

        return owner;
    }

    protected override async Task InsertAsync(IDbConnection conn, Owner entity)
    {
        const string sql = """
            INSERT INTO Owners
                (Id, FirstName, LastName, Email, PhoneNumber, Address, City, State, ZipCode,
                 ProfilePhotoUrl, StripeCustomerId, CreatedAt, UpdatedAt)
            VALUES
                (@Id, @FirstName, @LastName, @Email, @PhoneNumber, @Address, @City, @State, @ZipCode,
                 @ProfilePhotoUrl, @StripeCustomerId, @CreatedAt, @UpdatedAt)
            """;
        await conn.ExecuteAsync(sql, entity);
    }

    protected override async Task UpdateCoreAsync(IDbConnection conn, Owner entity)
    {
        const string sql = """
            UPDATE Owners SET
                FirstName = @FirstName, LastName = @LastName, PhoneNumber = @PhoneNumber,
                Address = @Address, City = @City, State = @State, ZipCode = @ZipCode,
                ProfilePhotoUrl = @ProfilePhotoUrl, StripeCustomerId = @StripeCustomerId,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id
            """;
        await conn.ExecuteAsync(sql, entity);
    }
}
