using Microsoft.Extensions.Logging;
using Pawtrak.Core.Interfaces.Services;

namespace Pawtrak.Infrastructure.Services;

// Stub implementation — replace with SendGrid / SMTP / etc.
public sealed class EmailNotificationService : INotificationService
{
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(ILogger<EmailNotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendBookingConfirmedAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Booking confirmed notification queued for BookingId={BookingId}", bookingId);
        return Task.CompletedTask;
    }

    public Task SendVisitStartedAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Visit started notification queued for BookingId={BookingId}", bookingId);
        return Task.CompletedTask;
    }

    public Task SendVisitCompletedAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Visit completed notification queued for BookingId={BookingId}", bookingId);
        return Task.CompletedTask;
    }

    public Task SendPaymentReleasedAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Payment released notification queued for BookingId={BookingId}", bookingId);
        return Task.CompletedTask;
    }

    public Task NotifyHouseholdMembersAsync(Guid ownerId, string subject, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Household notification queued for OwnerId={OwnerId} Subject={Subject}", ownerId, subject);
        return Task.CompletedTask;
    }
}
