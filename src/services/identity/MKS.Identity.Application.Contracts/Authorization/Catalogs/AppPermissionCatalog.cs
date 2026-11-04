namespace MKS.Identity.Application.Contracts.Authorization.Catalogs
{
    /// <summary>
    /// Describes the code-owned authorization catalog for one application.
    /// </summary>
    public sealed record AppPermissionCatalog(
        string AppCode,
        IReadOnlyCollection<AppFunctionCatalogEntry> Functions,
        IReadOnlyCollection<AppPermissionCatalogEntry> Permissions);

    /// <summary>Describes a function used to group permissions.</summary>
    public sealed record AppFunctionCatalogEntry(string Code, string NameKey);

    /// <summary>Describes an individually assignable application permission.</summary>
    public sealed record AppPermissionCatalogEntry(string Code, string? FunctionCode, string NameKey);
}
