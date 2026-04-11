using System.Data;
using Dapper;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Interfaces.Repositories;

namespace Pawtrak.Infrastructure.Data.Repositories;

public sealed class OwnerRepository : RepositoryBase<Owner>, IOwnerRepository
{
    protected override string TableName => "owners";

    public OwnerRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<Owner?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Owner>(
            "SELECT * FROM owners WHERE email = @Email",
            new { Email = email });
    }

    public async Task<Owner?> GetWithDogsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT o.*, d.*
            FROM owners o
            LEFT JOIN dogs d ON d.owner_id = o.id
            WHERE o.id = @Id
            """;

        Owner? owner = null;
        await conn.QueryAsync<Owner, Dog, Owner>(sql, (o, dog) =>
        {
            owner ??= o;
            if (dog is not null) owner.Dogs.Add(dog);
            return owner;
        }, new { Id = id }, splitOn: "id");

        return owner;
    }

    public async Task<Owner?> GetWithHouseholdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT o.*, hm.*
            FROM owners o
            LEFT JOIN household_members hm ON hm.owner_id = o.id
            WHERE o.id = @Id
            """;

        Owner? owner = null;
        await conn.QueryAsync<Owner, HouseholdMember, Owner>(sql, (o, member) =>
        {
            owner ??= o;
            if (member is not null) owner.HouseholdMembers.Add(member);
            return owner;
        }, new { Id = id }, splitOn: "id");

        return owner;
    }

    protected override async Task InsertAsync(IDbConnection conn, Owner entity)
    {
        const string sql = """
            INSERT INTO owners
                (id, first_name, last_name, email, phone_number, address, city, state, zip_code,
                 profile_photo_url, stripe_customer_id, created_at, updated_at)
            VALUES
                (@Id, @FirstName, @LastName, @Email, @PhoneNumber, @Address, @City, @State, @ZipCode,
                 @ProfilePhotoUrl, @StripeCustomerId, @CreatedAt, @UpdatedAt)
            """;
        await conn.ExecuteAsync(sql, entity);
    }

    protected override async Task UpdateCoreAsync(IDbConnection conn, Owner entity)
    {
        const string sql = """
            UPDATE owners SET
                first_name = @FirstName, last_name = @LastName, phone_number = @PhoneNumber,
                address = @Address, city = @City, state = @State, zip_code = @ZipCode,
                profile_photo_url = @ProfilePhotoUrl, stripe_customer_id = @StripeCustomerId,
                updated_at = @UpdatedAt
            WHERE id = @Id
            """;
        await conn.ExecuteAsync(sql, entity);
    }
}
