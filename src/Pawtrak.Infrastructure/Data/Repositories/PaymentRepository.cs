using System.Data;
using Dapper;
using Pawtrak.Core.Entities;
using Pawtrak.Core.Interfaces.Repositories;

namespace Pawtrak.Infrastructure.Data.Repositories;

public sealed class PaymentRepository : RepositoryBase<Payment>, IPaymentRepository
{
    protected override string TableName => "payments";

    public PaymentRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

    public async Task<Payment?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Payment>(
            "SELECT * FROM payments WHERE booking_id = @BookingId",
            new { BookingId = bookingId });
    }

    public async Task<Payment?> GetByStripePaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        using var conn = _connectionFactory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Payment>(
            "SELECT * FROM payments WHERE stripe_payment_intent_id = @PaymentIntentId",
            new { PaymentIntentId = paymentIntentId });
    }

    protected override async Task InsertAsync(IDbConnection conn, Payment entity)
    {
        const string sql = """
            INSERT INTO payments
                (id, booking_id, amount, platform_fee, walker_payout, tip_amount, status,
                 stripe_payment_intent_id, stripe_charge_id, stripe_transfer_id,
                 authorized_at, released_at, created_at, updated_at)
            VALUES
                (@Id, @BookingId, @Amount, @PlatformFee, @WalkerPayout, @TipAmount, @Status,
                 @StripePaymentIntentId, @StripeChargeId, @StripeTransferId,
                 @AuthorizedAt, @ReleasedAt, @CreatedAt, @UpdatedAt)
            """;
        await conn.ExecuteAsync(sql, entity);
    }

    protected override async Task UpdateCoreAsync(IDbConnection conn, Payment entity)
    {
        const string sql = """
            UPDATE payments SET
                status = @Status, stripe_charge_id = @StripeChargeId,
                stripe_transfer_id = @StripeTransferId, tip_amount = @TipAmount,
                authorized_at = @AuthorizedAt, released_at = @ReleasedAt, updated_at = @UpdatedAt
            WHERE id = @Id
            """;
        await conn.ExecuteAsync(sql, entity);
    }
}
