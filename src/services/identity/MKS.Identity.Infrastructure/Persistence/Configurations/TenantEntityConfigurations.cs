using MKS.Identity.Domain.MultiTenancy.Entities;
using MKS.Identity.Domain.Apps.Entities;
using MKS.Identity.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MKS.Identity.Infrastructure.Persistence.Configurations
{
    /// <summary>Configures application tenants.</summary>
    public sealed class TenantEntityConfiguration : IEntityTypeConfiguration<TenantEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<TenantEntity> builder)
        {
            builder.ToTable("Tenants");
            builder.Property(tenant => tenant.Code).HasMaxLength(64).IsRequired();
            builder.HasIndex(tenant => new { tenant.AppId, tenant.Code })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = FALSE");
            builder.Property(tenant => tenant.MasterStatus).HasConversion<int>();
            builder.HasAlternateKey(tenant => new { tenant.AppId, tenant.Id });
            builder.HasOne<AppEntity>().WithMany().HasForeignKey(tenant => tenant.AppId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    /// <summary>Configures tenant membership records.</summary>
    public sealed class TenantMembershipEntityConfiguration : IEntityTypeConfiguration<TenantMembershipEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<TenantMembershipEntity> builder)
        {
            builder.ToTable("TenantMemberships");
            builder.Property(membership => membership.Status).HasConversion<int>();
            builder.HasIndex(membership => new { membership.TenantId, membership.UserId })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = FALSE");
            builder.HasAlternateKey(membership => new { membership.TenantId, membership.UserId, membership.AppId });
            builder.HasOne<TenantEntity>().WithMany()
                .HasForeignKey(membership => new { membership.AppId, membership.TenantId })
                .HasPrincipalKey(tenant => new { tenant.AppId, tenant.Id })
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<UserEntity>().WithMany().HasForeignKey(membership => membership.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    /// <summary>Configures tenant administrator assignments.</summary>
    public sealed class TenantAdminAssignmentEntityConfiguration : IEntityTypeConfiguration<TenantAdminAssignmentEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<TenantAdminAssignmentEntity> builder)
        {
            builder.ToTable("TenantAdminAssignments");
            builder.HasIndex(assignment => new { assignment.TenantId, assignment.UserId })
                .IsUnique()
                .HasFilter("\"IsActive\" = TRUE AND \"IsDeleted\" = FALSE");
            builder.HasOne<TenantMembershipEntity>().WithMany()
                .HasForeignKey(assignment => new { assignment.TenantId, assignment.UserId })
                .HasPrincipalKey(membership => new { membership.TenantId, membership.UserId })
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
