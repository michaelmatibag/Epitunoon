using System.Data;
using Dapper;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Enums;
using Pawtrak.Core.Interfaces.Repositories;

namespace Pawtrak.Infrastructure.Data.Repositories;

public sealed class BookingRepository : RepositoryBase<Booking>, IBookingRepository
{
    protected override string TableName => "Bookings";

    public BookingRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<IEnumerable<Booking>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Booking>(
            "SELECT * FROM Bookings WHERE OwnerId = @OwnerId ORDER BY ScheduledStart DESC",
            new { OwnerId = ownerId });
    }

    public async Task<IEnumerable<Booking>> GetByWalkerIdAsync(Guid walkerId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Booking>(
            "SELECT * FROM Bookings WHERE WalkerId = @WalkerId ORDER BY ScheduledStart DESC",
            new { WalkerId = walkerId });
    }

    public async Task<IEnumerable<Booking>> GetByStatusAsync(BookingStatus status, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<Booking>(
            "SELECT * FROM Bookings WHERE Status = @Status",
            new { Status = (int)status });
    }

    public async Task<Booking?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT b.*, o.*, w.*, d.*
            FROM Bookings b
            INNER JOIN Owners o ON o.Id = b.OwnerId
            INNER JOIN Walkers w ON w.Id = b.WalkerId
            INNER JOIN Dogs d ON d.Id = b.DogId
            WHERE b.Id = @Id
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
            splitOn: "Id,Id,Id");

        return result.FirstOrDefault();
    }

    protected override async Task InsertAsync(IDbConnection conn, Booking entity)
    {
        const string sql = """
            INSERT INTO Bookings
                (Id, OwnerId, WalkerId, DogId, ScheduledStart, ScheduledEnd, ActualStart, ActualEnd,
                 Status, BaseAmount, PlatformFee, WalkerEarnings, TipAmount, SpecialInstructions,
                 StripePaymentIntentId, CreatedAt, UpdatedAt)
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
            UPDATE Bookings SET
                ActualStart = @ActualStart, ActualEnd = @ActualEnd, Status = @Status,
                TipAmount = @TipAmount, SpecialInstructions = @SpecialInstructions,
                StripePaymentIntentId = @StripePaymentIntentId, UpdatedAt = @UpdatedAt
            WHERE Id = @Id
            """;
        await conn.ExecuteAsync(sql, entity);
    }
}
