using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Application.Contracts.Common.DTOs
{
    /// <summary>Filters durable messages by their explicit global, app, or tenant scope.</summary>
    public sealed record OutboxMessageFilter(
        CEntityScope Scope,
        Guid? AppId = null,
        Guid? TenantId = null);
}
