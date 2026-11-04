namespace MKS.Shared.Domain.MultiTenancy.Entities
{
    public interface IHasTenantEntity
    {
        Guid TenantId { get; set; }
    }

    public interface IMayHaveTenantEntity
    {
        Guid? TenantId { get; set; }
    }
}