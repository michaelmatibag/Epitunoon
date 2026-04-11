using System.Data;
using Dapper;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Interfaces.Repositories;

namespace Pawtrak.Infrastructure.Data.Repositories;

public sealed class ReviewRepository : RepositoryBase<Review>, IReviewRepository
{
    protected override string TableName => "Reviews";

    public ReviewRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<IEnumerable<Review>> GetByWalkerIdAsync(Guid walkerId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Review>(
            "SELECT * FROM Reviews WHERE WalkerId = @WalkerId AND IsPublic = 1 ORDER BY CreatedAt DESC",
            new { WalkerId = walkerId });
    }

    public async Task<IEnumerable<Review>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Review>(
            "SELECT * FROM Reviews WHERE OwnerId = @OwnerId ORDER BY CreatedAt DESC",
            new { OwnerId = ownerId });
    }

    public async Task<Review?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Review>(
            "SELECT * FROM Reviews WHERE BookingId = @BookingId",
            new { BookingId = bookingId });
    }

    public async Task<double> GetAverageRatingForWalkerAsync(Guid walkerId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<double>(
            "SELECT COALESCE(AVG(\"Rating\"::FLOAT), 0) FROM \"Reviews\" WHERE \"WalkerId\" = @WalkerId AND \"IsPublic\" = TRUE",
            new { WalkerId = walkerId });
    }

    protected override async Task InsertAsync(IDbConnection conn, Review entity)
    {
        const string sql = """
            INSERT INTO Reviews
                (Id, BookingId, OwnerId, WalkerId, Rating, Comment, IsPublic, CreatedAt, UpdatedAt)
            VALUES
                (@Id, @BookingId, @OwnerId, @WalkerId, @Rating, @Comment, @IsPublic, @CreatedAt, @UpdatedAt)
            """;
        await conn.ExecuteAsync(sql, entity);
    }

    protected override async Task UpdateCoreAsync(IDbConnection conn, Review entity)
    {
        const string sql = """
            UPDATE Reviews SET
                Rating = @Rating, Comment = @Comment, IsPublic = @IsPublic, UpdatedAt = @UpdatedAt
            WHERE Id = @Id
            """;
        await conn.ExecuteAsync(sql, entity);
    }
}
