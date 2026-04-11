namespace Pawtrak.Core.Interfaces.Services;

public interface INotificationService
{
    Task SendBookingConfirmedAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task SendVisitStartedAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task SendVisitCompletedAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task SendPaymentReleasedAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task NotifyHouseholdMembersAsync(Guid ownerId, string subject, string message, CancellationToken cancellationToken = default);
}
