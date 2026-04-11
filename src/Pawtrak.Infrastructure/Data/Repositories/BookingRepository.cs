using System.Data;
using Dapper;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Enums;
using Pawtrak.Core.Interfaces.Repositories;

namespace Pawtrak.Infrastructure.Data.Repositories;

public sealed class BookingRepository : RepositoryBase<Booking>, IBookingRepository
{
    protected override string TableName => "bookings";

    public BookingRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<IEnumerable<Booking>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Booking>(
            "SELECT * FROM bookings WHERE owner_id = @OwnerId ORDER BY scheduled_start DESC",
            new { OwnerId = ownerId });
    }

    public async Task<IEnumerable<Booking>> GetByWalkerIdAsync(Guid walkerId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Booking>(
            "SELECT * FROM bookings WHERE walker_id = @WalkerId ORDER BY scheduled_start DESC",
            new { WalkerId = walkerId });
    }

    public async Task<IEnumerable<Booking>> GetByStatusAsync(BookingStatus status, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Booking>(
            "SELECT * FROM bookings WHERE status = @Status",
            new { Status = (int)status });
    }

    public async Task<Booking?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT b.*, o.*, w.*, d.*
            FROM bookings b
            INNER JOIN owners o ON o.id = b.owner_id
            INNER JOIN walkers w ON w.id = b.walker_id
            INNER JOIN dogs d ON d.id = b.dog_id
            WHERE b.id = @Id
            """;

        var result = await conn.QueryAsync<Booking, Owner, Walker, Dog, Booking>(
            sql,
            (booking, owner, walker, dog) =>
            {
                booking.Owner = owner;
                booking.Walker = walker;
                booking.Dog = dog;
                return booking;
            },
            new { Id = id },
            splitOn: "id,id,id");

        return result.FirstOrDefault();
    }

    protected override async Task InsertAsync(IDbConnection conn, Booking entity)
    {
        const string sql = """
            INSERT INTO bookings
                (id, owner_id, walker_id, dog_id, scheduled_start, scheduled_end, actual_start, actual_end,
                 status, base_amount, platform_fee, walker_earnings, tip_amount, special_instructions,
                 stripe_payment_intent_id, created_at, updated_at)
            VALUES
                (@Id, @OwnerId, @WalkerId, @DogId, @ScheduledStart, @ScheduledEnd, @ActualStart, @ActualEnd,
                 @Status, @BaseAmount, @PlatformFee, @WalkerEarnings, @TipAmount, @SpecialInstructions,
                 @StripePaymentIntentId, @CreatedAt, @UpdatedAt)
            """;
        await conn.ExecuteAsync(sql, entity);
    }

    protected override async Task UpdateCoreAsync(IDbConnection conn, Booking entity)
    {
        const string sql = """
            UPDATE bookings SET
                actual_start = @ActualStart, actual_end = @ActualEnd, status = @Status,
                tip_amount = @TipAmount, special_instructions = @SpecialInstructions,
                stripe_payment_intent_id = @StripePaymentIntentId, updated_at = @UpdatedAt
            WHERE id = @Id
            """;
        await conn.ExecuteAsync(sql, entity);
    }
}
