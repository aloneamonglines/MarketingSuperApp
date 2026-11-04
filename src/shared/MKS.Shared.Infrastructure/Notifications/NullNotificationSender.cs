using Microsoft.Extensions.Logging;
using MKS.Shared.Application.Contracts.Notifications;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Shared.Infrastructure.Notifications
{
    /// <summary>
    /// Logs notification metadata without sending or logging recipient data.
    /// </summary>
    public sealed partial class NullNotificationSender(ILogger<NullNotificationSender> logger) : INotificationSender
    {
        /// <inheritdoc />
        public Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);
            cancellationToken.ThrowIfCancellationRequested();
            LogNotificationSkipped(logger, message.TemplateCode, message.Channel);
            return Task.CompletedTask;
        }

        [LoggerMessage(
            EventId = 1001,
            Level = LogLevel.Information,
            Message = "Notification with template {TemplateCode} on channel {Channel} was not sent because no provider is configured.")]
        private static partial void LogNotificationSkipped(
            ILogger logger,
            string templateCode,
            CNotificationChannel channel);
    }
}
