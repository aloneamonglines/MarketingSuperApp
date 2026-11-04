namespace MKS.Identity.Application.Contracts.MultiTenancy.DTOs
{
    /// <summary>
    /// Describes a tenant without exposing administrator credentials.
    /// </summary>
    public sealed record TenantDto
    {
        public Guid Id { get; init; }
        public Guid AppId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
        public int MasterStatus { get; init; }
    }
}
