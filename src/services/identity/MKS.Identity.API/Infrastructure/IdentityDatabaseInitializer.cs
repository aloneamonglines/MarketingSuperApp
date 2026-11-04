using Microsoft.EntityFrameworkCore;
using MKS.Shared.Application.Contracts.Common.Interfaces;
using MKS.Identity.Domain.Platform.Entities;
using MKS.Identity.Domain.Users.Entities;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Identity.Domain.Shared.Enums;
using MKS.Identity.Infrastructure.Persistence;
using MKS.Identity.Application.Contracts.Authorization.Interfaces;
using MKS.Identity.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MKS.Identity.API.Infrastructure
{
    /// <summary>
    /// Applies database migrations and idempotently seeds the configured platform administrator.
    /// </summary>
    public sealed class IdentityDatabaseInitializer(
        IServiceScopeFactory scopeFactory,
        IOptions<IdentityOptions> identityOptions,
        IOptions<IdentityConnectionStringsOptions> connectionStrings,
        IHostEnvironment environment,
        ILogger<IdentityDatabaseInitializer> logger) : IHostedService
    {
        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = scopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var dbContext = serviceProvider.GetRequiredService<IdentityDbContext>();
            var migrationConnectionString = connectionStrings.Value.IdentityMigration;
            if (string.IsNullOrWhiteSpace(migrationConnectionString) && environment.IsDevelopment())
            {
                migrationConnectionString = connectionStrings.Value.Identity;
            }

            if (string.IsNullOrWhiteSpace(migrationConnectionString))
            {
                throw new InvalidOperationException(
                    IdentityConstants.ConfigurationErrors.IdentityMigrationConnectionStringRequired);
            }

            var migrationOptions = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseNpgsql(migrationConnectionString)
                .Options;
            await using (var migrationContext = new IdentityDbContext(
                migrationOptions,
                serviceProvider.GetRequiredService<ICurrentContext>(),
                serviceProvider.GetRequiredService<ITenantScopeAccessor>()))
            {
                await migrationContext.Database.MigrateAsync(cancellationToken);
            }

            await ValidateRuntimeDatabaseRoleAsync(dbContext, cancellationToken);
            await serviceProvider
                .GetRequiredService<IAppPermissionCatalogSynchronizer>()
                .SynchronizeExistingAsync(cancellationToken);

            var email = identityOptions.Value.SuperAdmin.Email;
            var password = identityOptions.Value.SuperAdmin.Password;
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    IdentityConstants.ConfigurationErrors.SuperAdminCredentialsRequired);
            }

            var existing = await dbContext.UserIdentifiers
                .Where(identifier =>
                    identifier.NormalizedValue == email.Trim().ToLowerInvariant() &&
                    identifier.Type == CIdentifierType.Email)
                .Select(identifier => identifier.UserId)
                .SingleOrDefaultAsync(cancellationToken);
            if (existing != Guid.Empty)
            {
                if (!await dbContext.PlatformRoleAssignments.AnyAsync(
                        assignment => assignment.UserId == existing &&
                                      assignment.RoleCode == IdentityConstants.SuperAdminRoleCode &&
                                      assignment.IsActive,
                        cancellationToken))
                {
                    dbContext.PlatformRoleAssignments.Add(new PlatformRoleAssignmentEntity
                    {
                        UserId = existing,
                        RoleCode = IdentityConstants.SuperAdminRoleCode,
                        IsActive = true
                    });
                    await dbContext.SaveChangesAsync(cancellationToken);
                }

                logger.LogInformation("The configured SuperAdmin account is already seeded.");
                return;
            }

            var user = UserEntity.Create(email, null, null);
            var passwordService = serviceProvider.GetRequiredService<MKS.Identity.Application.Contracts.Common.Interfaces.IPasswordService>();
            var credential = UserCredentialEntity.Create(
                user.Id,
                passwordService.Hash(user.Id, password),
                mustChangePassword: true);
            dbContext.Users.Add(user);
            dbContext.UserCredentials.Add(credential);
            dbContext.PlatformRoleAssignments.Add(new PlatformRoleAssignmentEntity
            {
                UserId = user.Id,
                RoleCode = IdentityConstants.SuperAdminRoleCode,
                IsActive = true
            });
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogWarning("A SuperAdmin account was seeded and must change its password after first sign-in.");
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private static async Task ValidateRuntimeDatabaseRoleAsync(
            IdentityDbContext dbContext,
            CancellationToken cancellationToken)
        {
            const string roleValidation = """
                SELECT NOT role.rolsuper
                       AND NOT role.rolbypassrls
                       AND NOT EXISTS (
                           SELECT 1
                           FROM pg_class table_info
                           JOIN pg_namespace schema_info ON schema_info.oid = table_info.relnamespace
                           WHERE schema_info.nspname = 'identity'
                             AND table_info.relkind IN ('r', 'p')
                             AND table_info.relowner = role.oid)
                       AND NOT EXISTS (
                           SELECT 1
                           FROM pg_class table_info
                           JOIN pg_namespace schema_info ON schema_info.oid = table_info.relnamespace
                           WHERE schema_info.nspname = 'identity'
                             AND table_info.relname IN ('TenantMemberships', 'TenantAdminAssignments', 'RoleAssignments')
                             AND NOT table_info.relrowsecurity)
                FROM pg_roles role
                WHERE role.rolname = current_user
                """;
            var connection = dbContext.Database.GetDbConnection();
            await using var command = connection.CreateCommand();
            command.CommandText = roleValidation;
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            var result = await command.ExecuteScalarAsync(cancellationToken);
            if (result is not true)
            {
                throw new InvalidOperationException(
                    "The runtime Identity database role must not be a superuser, have BYPASSRLS, own identity tables, or connect without tenant RLS enabled.");
            }
        }
    }
}
