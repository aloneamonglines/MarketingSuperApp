using MKS.Identity.Domain.Auditing.Entities;
using MKS.Identity.Domain.Platform.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MKS.Identity.Infrastructure.Persistence.Configurations
{
    /// <summary>Configures platform role assignments.</summary>
    public sealed class PlatformRoleAssignmentEntityConfiguration : IEntityTypeConfiguration<PlatformRoleAssignmentEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<PlatformRoleAssignmentEntity> builder)
        {
            builder.ToTable("PlatformRoleAssignments");
            builder.HasIndex(assignment => new { assignment.UserId, assignment.RoleCode })
                .IsUnique()
                .HasFilter("\"IsActive\" = TRUE AND \"IsDeleted\" = FALSE");
        }
    }

    /// <summary>Configures break-glass grants.</summary>
    public sealed class BreakGlassGrantEntityConfiguration : IEntityTypeConfiguration<BreakGlassGrantEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<BreakGlassGrantEntity> builder)
        {
            builder.ToTable("BreakGlassGrants");
            builder.Property(grant => grant.Status).HasConversion<int>();
            builder.HasIndex(grant => new { grant.TenantId, grant.UserId, grant.Status });
            builder.Property(grant => grant.Reason).HasMaxLength(2000).IsRequired();
        }
    }

    /// <summary>Configures append-only audit entries.</summary>
    public sealed class SecurityAuditLogEntityConfiguration : IEntityTypeConfiguration<SecurityAuditLogEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<SecurityAuditLogEntity> builder)
        {
            builder.ToTable("SecurityAuditLogs");
            builder.Property(log => log.Action).HasMaxLength(160).IsRequired();
            builder.Property(log => log.EntityType).HasMaxLength(200).IsRequired();
            builder.Property(log => log.Details).HasMaxLength(4000);
            builder.HasIndex(log => log.OccurredAt);
            builder.Metadata.SetIsTableExcludedFromMigrations(false);
        }
    }
}
