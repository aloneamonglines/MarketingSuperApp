namespace MKS.Identity.Application.Contracts.MultiTenancy.DTOs
{
    /// <summary>Defines a tenant update in an atomic bulk request.</summary>
    public sealed record UpdateTenantItemDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public int MasterStatus { get; init; }
    }
}
