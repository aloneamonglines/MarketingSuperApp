namespace MKS.Shared.Domain.MultiTenancy.Entities
{
    public interface IHasTenantEntity
    {
        Guid? TenantId { get; set; }
    }
}