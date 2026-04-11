using System.Data;
using Dapper;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Interfaces.Repositories;

namespace Pawtrak.Infrastructure.Data.Repositories;

public sealed class PaymentRepository : RepositoryBase<Payment>, IPaymentRepository
{
    protected override string TableName => "Payments";

    public PaymentRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<Payment?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Payment>(
            "SELECT * FROM Payments WHERE BookingId = @BookingId",
            new { BookingId = bookingId });
    }

    public async Task<Payment?> GetByStripePaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Payment>(
            "SELECT * FROM Payments WHERE StripePaymentIntentId = @PaymentIntentId",
            new { PaymentIntentId = paymentIntentId });
    }

    protected override async Task InsertAsync(IDbConnection conn, Payment entity)
    {
        const string sql = """
            INSERT INTO Payments
                (Id, BookingId, Amount, PlatformFee, WalkerPayout, TipAmount, Status,
                 StripePaymentIntentId, StripeChargeId, StripeTransferId, AuthorizedAt, ReleasedAt,
                 CreatedAt, UpdatedAt)
            VALUES
                (@Id, @BookingId, @Amount, @PlatformFee, @WalkerPayout, @TipAmount, @Status,
                 @StripePaymentIntentId, @StripeChargeId, @StripeTransferId, @AuthorizedAt, @ReleasedAt,
                 @CreatedAt, @UpdatedAt)
            """;
        await conn.ExecuteAsync(sql, entity);
    }

    protected override async Task UpdateCoreAsync(IDbConnection conn, Payment entity)
    {
        const string sql = """
            UPDATE Payments SET
                Status = @Status, StripeChargeId = @StripeChargeId, StripeTransferId = @StripeTransferId,
                TipAmount = @TipAmount, AuthorizedAt = @AuthorizedAt, ReleasedAt = @ReleasedAt,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id
            """;
        await conn.ExecuteAsync(sql, entity);
    }
}
