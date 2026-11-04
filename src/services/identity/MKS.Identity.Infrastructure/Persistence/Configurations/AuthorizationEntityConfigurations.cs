using MKS.Identity.Domain.Apps.Entities;
using MKS.Identity.Domain.Authorization.Entities;
using MKS.Identity.Domain.MultiTenancy.Entities;
using MKS.Identity.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MKS.Identity.Infrastructure.Persistence.Configurations
{
    /// <summary>Configures application functions.</summary>
    public sealed class AppFunctionEntityConfiguration : IEntityTypeConfiguration<AppFunctionEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<AppFunctionEntity> builder)
        {
            builder.ToTable("AppFunctions");
            builder.Property(function => function.Code).HasMaxLength(160).IsRequired();
            builder.HasAlternateKey(function => new { function.AppId, function.Code });
            builder.HasOne<AppEntity>().WithMany().HasForeignKey(function => function.AppId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    /// <summary>Configures application permissions.</summary>
    public sealed class AppPermissionEntityConfiguration : IEntityTypeConfiguration<AppPermissionEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<AppPermissionEntity> builder)
        {
            builder.ToTable("AppPermissions");
            builder.Property(permission => permission.Code).HasMaxLength(200).IsRequired();
            builder.HasAlternateKey(permission => new { permission.AppId, permission.Code });
            builder.HasOne<AppEntity>().WithMany().HasForeignKey(permission => permission.AppId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<AppFunctionEntity>().WithMany()
                .HasForeignKey(permission => new { permission.AppId, permission.FunctionCode })
                .HasPrincipalKey(function => new { function.AppId, function.Code })
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    /// <summary>Configures authorization roles.</summary>
    public sealed class RoleEntityConfiguration : IEntityTypeConfiguration<RoleEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<RoleEntity> builder)
        {
            builder.ToTable("Roles");
            builder.Property(role => role.MasterStatus).HasConversion<int>();
            builder.HasAlternateKey(role => new { role.AppId, role.Id });
            builder.HasIndex(role => new { role.AppId, role.TenantId, role.Code })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = FALSE");
            builder.HasOne<AppEntity>().WithMany().HasForeignKey(role => role.AppId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<TenantEntity>().WithMany()
                .HasForeignKey(role => new { role.AppId, role.TenantId })
                .HasPrincipalKey(tenant => new { tenant.AppId, tenant.Id })
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    /// <summary>Configures role-to-function links.</summary>
    public sealed class RoleFunctionEntityConfiguration : IEntityTypeConfiguration<RoleFunctionEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<RoleFunctionEntity> builder)
        {
            builder.ToTable("RoleFunctions");
            builder.HasIndex(link => new { link.AppId, link.RoleId, link.FunctionCode }).IsUnique();
            builder.HasOne<RoleEntity>().WithMany()
                .HasForeignKey(link => new { link.AppId, link.RoleId })
                .HasPrincipalKey(role => new { role.AppId, role.Id })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<AppFunctionEntity>().WithMany()
                .HasForeignKey(link => new { link.AppId, link.FunctionCode })
                .HasPrincipalKey(function => new { function.AppId, function.Code })
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    /// <summary>Configures role-to-permission links.</summary>
    public sealed class RolePermissionEntityConfiguration : IEntityTypeConfiguration<RolePermissionEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<RolePermissionEntity> builder)
        {
            builder.ToTable("RolePermissions");
            builder.HasIndex(link => new { link.AppId, link.RoleId, link.PermissionCode }).IsUnique();
            builder.HasOne<RoleEntity>().WithMany()
                .HasForeignKey(link => new { link.AppId, link.RoleId })
                .HasPrincipalKey(role => new { role.AppId, role.Id })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<AppPermissionEntity>().WithMany()
                .HasForeignKey(link => new { link.AppId, link.PermissionCode })
                .HasPrincipalKey(permission => new { permission.AppId, permission.Code })
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    /// <summary>Configures tenant role assignments.</summary>
    public sealed class RoleAssignmentEntityConfiguration : IEntityTypeConfiguration<RoleAssignmentEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<RoleAssignmentEntity> builder)
        {
            builder.ToTable("RoleAssignments");
            builder.HasIndex(assignment => new { assignment.TenantId, assignment.UserId, assignment.RoleId })
                .IsUnique()
                .HasFilter("\"IsActive\" = TRUE AND \"IsDeleted\" = FALSE");
            builder.HasOne<TenantMembershipEntity>().WithMany()
                .HasForeignKey(assignment => new { assignment.TenantId, assignment.UserId, assignment.AppId })
                .HasPrincipalKey(membership => new { membership.TenantId, membership.UserId, membership.AppId })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<RoleEntity>().WithMany()
                .HasForeignKey(assignment => new { assignment.AppId, assignment.RoleId })
                .HasPrincipalKey(role => new { role.AppId, role.Id })
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
