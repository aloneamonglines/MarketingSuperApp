using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Identity.Domain.Auditing.Entities;
using MKS.Identity.Infrastructure.Persistence;

namespace MKS.Identity.Infrastructure.Auditing
{
    /// <summary>
    /// Adds append-only security records to the current identity unit of work.
    /// </summary>
    public sealed class SecurityAuditLogWriter(IdentityDbContext dbContext) : ISecurityAuditLogWriter
    {
        /// <inheritdoc />
        public async Task AppendAsync(
            Guid? actingAsUserId,
            Guid? impersonatorId,
            Guid? breakGlassGrantId,
            string action,
            string entityType,
            string? entityId,
            string? details,
            DateTimeOffset occurredAt,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(action);
            ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
            await dbContext.SecurityAuditLogs.AddAsync(new SecurityAuditLogEntity
            {
                ActingAsUserId = actingAsUserId,
                ImpersonatorId = impersonatorId,
                BreakGlassGrantId = breakGlassGrantId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                OccurredAt = occurredAt
            }, cancellationToken);
        }
    }
}
