using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Shared.Application.Contracts.Notifications
{
    /// <summary>
    /// Describes a notification request without credentials or secret material.
    /// </summary>
    public sealed record NotificationMessage(
        string TemplateCode,
        string Recipient,
        CNotificationChannel Channel,
        IReadOnlyDictionary<string, string> Data);
}
