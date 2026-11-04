namespace MKS.Shared.Application.Contracts.Notifications
{
    /// <summary>
    /// Sends notification requests through a configured provider.
    /// </summary>
    public interface INotificationSender
    {
        /// <summary>Queues a notification for delivery.</summary>
        Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default);
    }
}
