using Microsoft.Extensions.Configuration;
using Pawtrak.Core.Interfaces.Services;
using Stripe;

namespace Pawtrak.Infrastructure.Services;

public sealed class StripePaymentService : IPaymentService
{
    public StripePaymentService(IConfiguration configuration)
    {
        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe:SecretKey is not configured.");
    }

    public async Task<string> CreatePaymentIntentAsync(
        Guid bookingId, decimal amount, string currency = "usd",
        CancellationToken cancellationToken = default)
    {
        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100), // Stripe uses cents
            Currency = currency,
            CaptureMethod = "manual",      // authorise now, capture after job completes
            Metadata = new Dictionary<string, string> { ["bookingId"] = bookingId.ToString() }
        }, cancellationToken: cancellationToken);

        return intent.ClientSecret;
    }

    public async Task<bool> CapturePaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        var service = new PaymentIntentService();
        var intent = await service.CaptureAsync(paymentIntentId, cancellationToken: cancellationToken);
        return intent.Status == "succeeded";
    }

    public async Task<bool> ReleaseToWalkerAsync(
        string paymentIntentId, string walkerStripeAccountId, decimal amount,
        CancellationToken cancellationToken = default)
    {
        var piService = new PaymentIntentService();
        var intent = await piService.GetAsync(paymentIntentId, cancellationToken: cancellationToken);

        var transferService = new TransferService();
        var transfer = await transferService.CreateAsync(new TransferCreateOptions
        {
            Amount = (long)(amount * 100),
            Currency = "usd",
            Destination = walkerStripeAccountId,
            SourceTransaction = intent.LatestChargeId,
            Metadata = new Dictionary<string, string> { ["paymentIntentId"] = paymentIntentId }
        }, cancellationToken: cancellationToken);

        return transfer.Id is not null;
    }

    public async Task<bool> RefundPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        var refundService = new RefundService();
        var refund = await refundService.CreateAsync(
            new RefundCreateOptions { PaymentIntent = paymentIntentId },
            cancellationToken: cancellationToken);

        return refund.Status == "succeeded";
    }

    public async Task<string> CreateWalkerConnectAccountAsync(string email, CancellationToken cancellationToken = default)
    {
        var service = new AccountService();
        var account = await service.CreateAsync(new AccountCreateOptions
        {
            Type = "express",
            Email = email,
            Capabilities = new AccountCapabilitiesOptions
            {
                Transfers = new AccountCapabilitiesTransfersOptions { Requested = true }
            }
        }, cancellationToken: cancellationToken);

        return account.Id;
    }

    public async Task<string> CreateAccountLinkAsync(string stripeAccountId, CancellationToken cancellationToken = default)
    {
        var service = new AccountLinkService();
        var link = await service.CreateAsync(new AccountLinkCreateOptions
        {
            Account = stripeAccountId,
            ReturnUrl = "https://pawtrak.app/walker/onboarding-complete",
            RefreshUrl = "https://pawtrak.app/walker/onboarding-refresh",
            Type = "account_onboarding"
        }, cancellationToken: cancellationToken);

        return link.Url;
    }
}
