namespace MKS.Identity.Application.Contracts.Authorization.Interfaces
{
    /// <summary>
    /// Synchronizes code-declared permissions without deleting historical catalog rows.
    /// </summary>
    public interface IAppPermissionCatalogSynchronizer
    {
        /// <summary>Synchronizes catalogs for applications already persisted in the database.</summary>
        Task SynchronizeExistingAsync(CancellationToken cancellationToken = default);

        /// <summary>Synchronizes the catalog declared for one application code, if registered.</summary>
        Task SynchronizeAppAsync(string appCode, CancellationToken cancellationToken = default);
    }
}
