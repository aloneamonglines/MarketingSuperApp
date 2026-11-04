namespace MKS.Identity.Application.Contracts.MultiTenancy.DTOs
{
    /// <summary>Defines mutable tenant fields.</summary>
    public sealed record UpdateTenantDto
    {
        public string Name { get; init; } = string.Empty;
        public int MasterStatus { get; init; }
    }
}
