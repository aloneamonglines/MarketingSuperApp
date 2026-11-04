using Microsoft.EntityFrameworkCore;
using MKS.Identity.Infrastructure.Persistence.Configurations;
using MKS.Identity.Domain.Apps.Entities;
using MKS.Identity.Domain.Auditing.Entities;
using MKS.Identity.Domain.Authorization.Entities;
using MKS.Identity.Domain.MultiTenancy.Entities;
using MKS.Identity.Domain.Platform.Entities;
using MKS.Identity.Domain.Users.Entities;
using MKS.Shared.Application.Contracts.Common.Interfaces;
using MKS.Shared.Infrastructure.Persistence;
using OpenIddict.EntityFrameworkCore;

namespace MKS.Identity.Infrastructure.Persistence
{
    /// <summary>
    /// Stores the Identity, application, tenant, authorization, audit, and OpenIddict models.
    /// </summary>
    public sealed class IdentityDbContext(
        DbContextOptions<IdentityDbContext> options,
        ICurrentContext currentContext,
        MKS.Shared.Application.Contracts.Common.Interfaces.ITenantScopeAccessor tenantScope)
        : DbContext(options), ITenantFilterContext
    {
        /// <inheritdoc />
        public Guid CurrentTenantId => tenantScope.TenantId ?? currentContext.TenantId ?? Guid.Empty;

        /// <summary>Gets global users.</summary>
        public DbSet<UserEntity> Users => Set<UserEntity>();
        /// <summary>Gets login identifiers.</summary>
        public DbSet<UserIdentifierEntity> UserIdentifiers => Set<UserIdentifierEntity>();
        /// <summary>Gets password credentials.</summary>
        public DbSet<UserCredentialEntity> UserCredentials => Set<UserCredentialEntity>();
        /// <summary>Gets revocable user sessions.</summary>
        public DbSet<UserSessionEntity> UserSessions => Set<UserSessionEntity>();
        /// <summary>Gets applications.</summary>
        public DbSet<AppEntity> Apps => Set<AppEntity>();
        /// <summary>Gets application administrator assignments.</summary>
        public DbSet<AppAdminAssignmentEntity> AppAdminAssignments => Set<AppAdminAssignmentEntity>();
        /// <summary>Gets tenants.</summary>
        public DbSet<TenantEntity> Tenants => Set<TenantEntity>();
        /// <summary>Gets tenant memberships.</summary>
        public DbSet<TenantMembershipEntity> TenantMemberships => Set<TenantMembershipEntity>();
        /// <summary>Gets tenant administrator assignments.</summary>
        public DbSet<TenantAdminAssignmentEntity> TenantAdminAssignments => Set<TenantAdminAssignmentEntity>();
        /// <summary>Gets application functions.</summary>
        public DbSet<AppFunctionEntity> AppFunctions => Set<AppFunctionEntity>();
        /// <summary>Gets application permissions.</summary>
        public DbSet<AppPermissionEntity> AppPermissions => Set<AppPermissionEntity>();
        /// <summary>Gets authorization roles.</summary>
        public DbSet<RoleEntity> Roles => Set<RoleEntity>();
        /// <summary>Gets role-function links.</summary>
        public DbSet<RoleFunctionEntity> RoleFunctions => Set<RoleFunctionEntity>();
        /// <summary>Gets role-permission links.</summary>
        public DbSet<RolePermissionEntity> RolePermissions => Set<RolePermissionEntity>();
        /// <summary>Gets role assignments.</summary>
        public DbSet<RoleAssignmentEntity> RoleAssignments => Set<RoleAssignmentEntity>();
        /// <summary>Gets platform role assignments.</summary>
        public DbSet<PlatformRoleAssignmentEntity> PlatformRoleAssignments => Set<PlatformRoleAssignmentEntity>();
        /// <summary>Gets break-glass grants.</summary>
        public DbSet<BreakGlassGrantEntity> BreakGlassGrants => Set<BreakGlassGrantEntity>();
        /// <summary>Gets append-only security audit entries.</summary>
        public DbSet<SecurityAuditLogEntity> SecurityAuditLogs => Set<SecurityAuditLogEntity>();
        /// <summary>Gets outbox records.</summary>
        public DbSet<OutboxMessageEntity> OutboxMessages => Set<OutboxMessageEntity>();
        /// <summary>Gets outbox dead-letter records.</summary>
        public DbSet<OutboxDeadLetterEntity> OutboxDeadLetters => Set<OutboxDeadLetterEntity>();
        /// <summary>Gets inbox records.</summary>
        public DbSet<InboxMessageEntity> InboxMessages => Set<InboxMessageEntity>();

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("identity");
            modelBuilder.UseOpenIddict<Guid>();
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
            modelBuilder.ApplyIdentityGlobalQueryFilters(this);
            base.OnModelCreating(modelBuilder);
        }
    }
}
