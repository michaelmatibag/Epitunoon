using System.Data;
using Dapper;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Interfaces.Repositories;

namespace Pawtrak.Infrastructure.Data.Repositories;

public sealed class ReviewRepository : RepositoryBase<Review>, IReviewRepository
{
    protected override string TableName => "reviews";

    public ReviewRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<IEnumerable<Review>> GetByWalkerIdAsync(Guid walkerId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Review>(
            "SELECT * FROM reviews WHERE walker_id = @WalkerId AND is_public = true ORDER BY created_at DESC",
            new { WalkerId = walkerId });
    }

    public async Task<IEnumerable<Review>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Review>(
            "SELECT * FROM reviews WHERE owner_id = @OwnerId ORDER BY created_at DESC",
            new { OwnerId = ownerId });
    }

    public async Task<Review?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Review>(
            "SELECT * FROM reviews WHERE booking_id = @BookingId",
            new { BookingId = bookingId });
    }

    public async Task<double> GetAverageRatingForWalkerAsync(Guid walkerId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<double>(
            "SELECT COALESCE(AVG(rating::FLOAT), 0) FROM reviews WHERE walker_id = @WalkerId AND is_public = true",
            new { WalkerId = walkerId });
    }

    protected override async Task InsertAsync(IDbConnection conn, Review entity)
    {
        const string sql = """
            INSERT INTO reviews
                (id, booking_id, owner_id, walker_id, rating, comment, is_public, created_at, updated_at)
            VALUES
                (@Id, @BookingId, @OwnerId, @WalkerId, @Rating, @Comment, @IsPublic, @CreatedAt, @UpdatedAt)
            """;
        await conn.ExecuteAsync(sql, entity);
    }

    protected override async Task UpdateCoreAsync(IDbConnection conn, Review entity)
    {
        const string sql = """
            UPDATE reviews SET
                rating = @Rating, comment = @Comment, is_public = @IsPublic, updated_at = @UpdatedAt
            WHERE id = @Id
            """;
        await conn.ExecuteAsync(sql, entity);
    }
}
