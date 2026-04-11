namespace Pawtrak.Core.Interfaces.Services;

public interface IPaymentService
{
    Task<string> CreatePaymentIntentAsync(Guid bookingId, decimal amount, string currency = "usd", CancellationToken cancellationToken = default);
    Task<bool> CapturePaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    Task<bool> ReleaseToWalkerAsync(string paymentIntentId, string walkerStripeAccountId, decimal amount, CancellationToken cancellationToken = default);
    Task<bool> RefundPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    Task<string> CreateWalkerConnectAccountAsync(string email, CancellationToken cancellationToken = default);
    Task<string> CreateAccountLinkAsync(string stripeAccountId, CancellationToken cancellationToken = default);
}
