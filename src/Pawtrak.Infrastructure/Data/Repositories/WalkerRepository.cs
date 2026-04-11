using System.Data;
using Dapper;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Interfaces.Repositories;

namespace Pawtrak.Infrastructure.Data.Repositories;

public sealed class WalkerRepository : RepositoryBase<Walker>, IWalkerRepository
{
    protected override string TableName => "Walkers";

    public WalkerRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<Walker?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Walker>(
            "SELECT * FROM Walkers WHERE Email = @Email",
            new { Email = email });
    }

    public async Task<IEnumerable<Walker>> GetAvailableByZipCodeAsync(string zipCode, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Walker>(
            "SELECT * FROM Walkers WHERE ZipCode = @ZipCode AND IsAvailable = 1",
            new { ZipCode = zipCode });
    }

    public async Task<Walker?> GetWithReviewsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT w.*, r.*
            FROM Walkers w
            LEFT JOIN Reviews r ON r.WalkerId = w.Id
            WHERE w.Id = @Id
            """;

        Walker? walker = null;
        await conn.QueryAsync<Walker, Review, Walker>(sql, (w, review) =>
        {
            walker ??= w;
            if (review is not null) walker.Reviews.Add(review);
            return walker;
        }, new { Id = id }, splitOn: "Id");

        return walker;
    }

    protected override async Task InsertAsync(IDbConnection conn, Walker entity)
    {
        const string sql = """
            INSERT INTO Walkers
                (Id, FirstName, LastName, Email, PhoneNumber, Bio, City, State, ZipCode,
                 ProfilePhotoUrl, HourlyRate, IsAvailable, IsBackgroundChecked,
                 AverageRating, TotalReviews, StripeAccountId, CreatedAt, UpdatedAt)
            VALUES
                (@Id, @FirstName, @LastName, @Email, @PhoneNumber, @Bio, @City, @State, @ZipCode,
                 @ProfilePhotoUrl, @HourlyRate, @IsAvailable, @IsBackgroundChecked,
                 @AverageRating, @TotalReviews, @StripeAccountId, @CreatedAt, @UpdatedAt)
            """;
        await conn.ExecuteAsync(sql, entity);
    }

    protected override async Task UpdateCoreAsync(IDbConnection conn, Walker entity)
    {
        const string sql = """
            UPDATE Walkers SET
                FirstName = @FirstName, LastName = @LastName, PhoneNumber = @PhoneNumber,
                Bio = @Bio, City = @City, State = @State, ZipCode = @ZipCode,
                ProfilePhotoUrl = @ProfilePhotoUrl, HourlyRate = @HourlyRate,
                IsAvailable = @IsAvailable, IsBackgroundChecked = @IsBackgroundChecked,
                AverageRating = @AverageRating, TotalReviews = @TotalReviews,
                StripeAccountId = @StripeAccountId, UpdatedAt = @UpdatedAt
            WHERE Id = @Id
            """;
        await conn.ExecuteAsync(sql, entity);
    }
}
