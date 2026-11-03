namespace MKS.Shared.Application.Contracts.MultiTenancy
{
    public interface IHasTenantDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the tenant associated with this entity.
        /// </summary>
        Guid? TenantId { get; set; }
        /// <summary>
        /// Gets or sets the name of the tenant associated with this entity.
        /// </summary>
        string? TenantName { get; set; }
        /// <summary>
        /// Gets or sets the code of the tenant associated with this entity.
        /// </summary>
        string? TenantCode { get; set; }
    }
}