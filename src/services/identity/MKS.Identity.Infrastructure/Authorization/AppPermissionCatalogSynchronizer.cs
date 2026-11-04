using MKS.Identity.Application.Contracts.Authorization.Catalogs;
using MKS.Identity.Application.Contracts.Authorization.Interfaces;
using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Identity.Domain.Authorization.Entities;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MKS.Identity.Infrastructure.Authorization
{
    /// <summary>
    /// Upserts active catalog entries and deprecates entries removed from a registered module.
    /// </summary>
    public sealed class AppPermissionCatalogSynchronizer(
        IdentityDbContext dbContext,
        IIdentityUnitOfWork unitOfWork,
        IEnumerable<IAppPermissionCatalogProvider> providers) : IAppPermissionCatalogSynchronizer
    {
        /// <inheritdoc />
        public async Task SynchronizeExistingAsync(CancellationToken cancellationToken = default)
        {
            var appCodes = await dbContext.Apps
                .Select(app => app.Code)
                .ToListAsync(cancellationToken);
            foreach (var appCode in appCodes)
            {
                await SynchronizeAppAsync(appCode, cancellationToken);
            }
        }

        /// <inheritdoc />
        public async Task SynchronizeAppAsync(
            string appCode,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(appCode);
            var catalogs = LoadCatalogs();
            var normalizedCode = appCode.Trim().ToLowerInvariant();
            var matchingCatalogs = catalogs
                .Where(catalog => string.Equals(
                    catalog.AppCode.Trim(),
                    normalizedCode,
                    StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (matchingCatalogs.Length == 0)
            {
                return;
            }

            if (matchingCatalogs.Length > 1)
            {
                throw new InvalidOperationException($"More than one permission catalog is registered for app '{normalizedCode}'.");
            }

            var catalog = Validate(matchingCatalogs[0]);
            await unitOfWork.ExecuteAsync(async token =>
            {
                var app = await dbContext.Apps.SingleOrDefaultAsync(
                    item => item.Code == normalizedCode,
                    token)
                    ?? throw new InvalidOperationException(
                        $"A permission catalog is registered for unknown app '{normalizedCode}'.");
                var existingFunctions = await dbContext.AppFunctions
                    .Where(function => function.AppId == app.Id)
                    .ToListAsync(token);
                var existingPermissions = await dbContext.AppPermissions
                    .Where(permission => permission.AppId == app.Id)
                    .ToListAsync(token);
                var functionCodes = catalog.Functions
                    .Select(function => function.Code)
                    .ToHashSet(StringComparer.Ordinal);
                var permissionCodes = catalog.Permissions
                    .Select(permission => permission.Code)
                    .ToHashSet(StringComparer.Ordinal);

                foreach (var entry in catalog.Functions)
                {
                    var existing = existingFunctions.SingleOrDefault(function => function.Code == entry.Code);
                    if (existing is null)
                    {
                        await dbContext.AppFunctions.AddAsync(new AppFunctionEntity
                        {
                            AppId = app.Id,
                            Code = entry.Code,
                            NameKey = entry.NameKey,
                            Deprecated = false
                        }, token);
                    }
                    else
                    {
                        existing.NameKey = entry.NameKey;
                        existing.Deprecated = false;
                    }
                }

                foreach (var entry in catalog.Permissions)
                {
                    var existing = existingPermissions.SingleOrDefault(permission => permission.Code == entry.Code);
                    if (existing is null)
                    {
                        await dbContext.AppPermissions.AddAsync(new AppPermissionEntity
                        {
                            AppId = app.Id,
                            Code = entry.Code,
                            FunctionCode = entry.FunctionCode,
                            NameKey = entry.NameKey,
                            Deprecated = false
                        }, token);
                    }
                    else
                    {
                        existing.FunctionCode = entry.FunctionCode;
                        existing.NameKey = entry.NameKey;
                        existing.Deprecated = false;
                    }
                }

                foreach (var function in existingFunctions.Where(item => !functionCodes.Contains(item.Code)))
                {
                    function.Deprecated = true;
                }

                foreach (var permission in existingPermissions.Where(item => !permissionCodes.Contains(item.Code)))
                {
                    permission.Deprecated = true;
                }
            }, cancellationToken);
        }

        private IReadOnlyCollection<AppPermissionCatalog> LoadCatalogs()
        {
            var catalogs = providers.SelectMany(provider =>
                provider.GetCatalogs() ?? throw new InvalidOperationException(
                    $"{provider.GetType().Name} returned a null permission catalog collection."))
                .ToArray();
            var duplicateApp = catalogs
                .GroupBy(catalog => catalog.AppCode.Trim(), StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicateApp is not null)
            {
                throw new InvalidOperationException(
                    $"Multiple permission catalog providers declare app '{duplicateApp.Key}'.");
            }

            return catalogs;
        }

        private static AppPermissionCatalog Validate(AppPermissionCatalog catalog)
        {
            if (string.IsNullOrWhiteSpace(catalog.AppCode) ||
                catalog.Functions is null ||
                catalog.Permissions is null)
            {
                throw new InvalidOperationException(
                    IdentityConstants.ValidationMessages.PermissionCatalogRequired);
            }

            var functions = catalog.Functions.ToArray();
            var permissions = catalog.Permissions.ToArray();
            if (functions.Any(entry => string.IsNullOrWhiteSpace(entry.Code) || string.IsNullOrWhiteSpace(entry.NameKey)) ||
                permissions.Any(entry => string.IsNullOrWhiteSpace(entry.Code) || string.IsNullOrWhiteSpace(entry.NameKey)))
            {
                throw new InvalidOperationException($"Permission catalog '{catalog.AppCode}' contains an empty code or name key.");
            }

            if (functions.Select(entry => entry.Code).Distinct(StringComparer.Ordinal).Count() != functions.Length ||
                permissions.Select(entry => entry.Code).Distinct(StringComparer.Ordinal).Count() != permissions.Length)
            {
                throw new InvalidOperationException($"Permission catalog '{catalog.AppCode}' contains duplicate codes.");
            }

            var functionCodes = functions.Select(entry => entry.Code).ToHashSet(StringComparer.Ordinal);
            if (permissions.Any(entry => entry.FunctionCode is not null && !functionCodes.Contains(entry.FunctionCode)))
            {
                throw new InvalidOperationException(
                    $"Permission catalog '{catalog.AppCode}' references an undeclared function.");
            }

            return catalog with
            {
                AppCode = catalog.AppCode.Trim().ToLowerInvariant(),
                Functions = functions,
                Permissions = permissions
            };
        }
    }
}
