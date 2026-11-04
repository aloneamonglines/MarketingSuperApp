using MKS.Identity.Domain.Apps.Entities;
using MKS.Identity.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MKS.Identity.Infrastructure.Persistence.Configurations
{
    /// <summary>Configures applications.</summary>
    public sealed class AppEntityConfiguration : IEntityTypeConfiguration<AppEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<AppEntity> builder)
        {
            builder.ToTable("Apps");
            builder.Property(app => app.Code).HasMaxLength(64).IsRequired();
            builder.HasIndex(app => app.Code).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
            builder.Property(app => app.MasterStatus).HasConversion<int>();
            builder.HasAlternateKey(app => app.Id);
        }
    }

    /// <summary>Configures application administrator assignments.</summary>
    public sealed class AppAdminAssignmentEntityConfiguration : IEntityTypeConfiguration<AppAdminAssignmentEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<AppAdminAssignmentEntity> builder)
        {
            builder.ToTable("AppAdminAssignments");
            builder.HasIndex(assignment => new { assignment.AppId, assignment.UserId })
                .IsUnique()
                .HasFilter("\"IsActive\" = TRUE AND \"IsDeleted\" = FALSE");
            builder.HasOne<AppEntity>().WithMany().HasForeignKey(assignment => assignment.AppId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<UserEntity>().WithMany().HasForeignKey(assignment => assignment.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
