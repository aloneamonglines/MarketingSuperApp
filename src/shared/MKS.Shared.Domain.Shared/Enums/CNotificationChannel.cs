namespace MKS.Shared.Domain.Shared.Enums
{
    /// <summary>
    /// Identifies a supported notification delivery channel.
    /// </summary>
    public enum CNotificationChannel
    {
        /// <summary>Email notification.</summary>
        Email = 1,
        /// <summary>SMS notification.</summary>
        Sms = 2,
        /// <summary>In-application notification.</summary>
        InApp = 3
    }
}
