namespace MKS.Identity.Application.Contracts.Common.Interfaces
{
    /// <summary>
    /// Appends security events to the identity audit log.
    /// </summary>
    public interface ISecurityAuditLogWriter
    {
        /// <summary>Adds an audit entry to the current unit of work.</summary>
        Task AppendAsync(
            Guid? actingAsUserId,
            Guid? impersonatorId,
            Guid? breakGlassGrantId,
            string action,
            string entityType,
            string? entityId,
            string? details,
            DateTimeOffset occurredAt,
            CancellationToken cancellationToken = default);
    }
}
