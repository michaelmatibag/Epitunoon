using System.Data;
using Dapper;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Interfaces.Repositories;

namespace Pawtrak.Infrastructure.Data.Repositories;

public sealed class WalkerRepository : RepositoryBase<Walker>, IWalkerRepository
{
    protected override string TableName => "walkers";

    public WalkerRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<Walker?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Walker>(
            "SELECT * FROM walkers WHERE email = @Email",
            new { Email = email });
    }

    public async Task<IEnumerable<Walker>> GetAvailableByZipCodeAsync(string zipCode, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Walker>(
            "SELECT * FROM walkers WHERE zip_code = @ZipCode AND is_available = true",
            new { ZipCode = zipCode });
    }

    public async Task<Walker?> GetWithReviewsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT w.*, r.*
            FROM walkers w
            LEFT JOIN reviews r ON r.walker_id = w.id
            WHERE w.id = @Id
            """;

        Walker? walker = null;
        await conn.QueryAsync<Walker, Review, Walker>(sql, (w, review) =>
        {
            walker ??= w;
            if (review is not null) walker.Reviews.Add(review);
            return walker;
        }, new { Id = id }, splitOn: "id");

        return walker;
    }

    protected override async Task InsertAsync(IDbConnection conn, Walker entity)
    {
        const string sql = """
            INSERT INTO walkers
                (id, first_name, last_name, email, phone_number, bio, city, state, zip_code,
                 profile_photo_url, hourly_rate, is_available, is_background_checked,
                 average_rating, total_reviews, stripe_account_id, created_at, updated_at)
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
            UPDATE walkers SET
                first_name = @FirstName, last_name = @LastName, phone_number = @PhoneNumber,
                bio = @Bio, city = @City, state = @State, zip_code = @ZipCode,
                profile_photo_url = @ProfilePhotoUrl, hourly_rate = @HourlyRate,
                is_available = @IsAvailable, is_background_checked = @IsBackgroundChecked,
                average_rating = @AverageRating, total_reviews = @TotalReviews,
                stripe_account_id = @StripeAccountId, updated_at = @UpdatedAt
            WHERE id = @Id
            """;
        await conn.ExecuteAsync(sql, entity);
    }
}
