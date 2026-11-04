using MKS.Identity.Application.Contracts.Authorization.Catalogs;

namespace MKS.Identity.Application.Contracts.Authorization.Interfaces
{
    /// <summary>
    /// Supplies authorization entries declared by application code.
    /// </summary>
    public interface IAppPermissionCatalogProvider
    {
        /// <summary>Returns catalogs owned by the registering application module.</summary>
        IReadOnlyCollection<AppPermissionCatalog> GetCatalogs();
    }
}
