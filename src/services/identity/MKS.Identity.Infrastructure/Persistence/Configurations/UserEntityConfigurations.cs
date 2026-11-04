using MKS.Identity.Domain.Platform.Entities;
using MKS.Identity.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MKS.Identity.Infrastructure.Persistence.Configurations
{
    /// <summary>Configures persisted user accounts.</summary>
    public sealed class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.ToTable("Users");
            builder.Property(user => user.MasterStatus).HasConversion<int>();
            builder.HasIndex(user => user.MasterStatus);
            builder.Ignore(user => user.Identifiers);
            builder.HasMany<UserIdentifierEntity>("_identifiers")
                .WithOne()
                .HasForeignKey(identifier => identifier.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    /// <summary>Configures unique user login identifiers.</summary>
    public sealed class UserIdentifierEntityConfiguration : IEntityTypeConfiguration<UserIdentifierEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<UserIdentifierEntity> builder)
        {
            builder.ToTable("UserIdentifiers");
            builder.Property(identifier => identifier.Type).HasConversion<int>();
            builder.Property(identifier => identifier.Value).HasMaxLength(320).IsRequired();
            builder.Property(identifier => identifier.NormalizedValue).HasMaxLength(320).IsRequired();
            builder.HasIndex(identifier => identifier.NormalizedValue)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = FALSE");
            builder.HasIndex(identifier => new { identifier.UserId, identifier.Type });
        }
    }

    /// <summary>Configures user password credentials.</summary>
    public sealed class UserCredentialEntityConfiguration : IEntityTypeConfiguration<UserCredentialEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<UserCredentialEntity> builder)
        {
            builder.ToTable("UserCredentials");
            builder.HasIndex(credential => credential.UserId).IsUnique();
            builder.HasOne<UserEntity>().WithOne().HasForeignKey<UserCredentialEntity>(credential => credential.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    /// <summary>Configures revocable user sessions.</summary>
    public sealed class UserSessionEntityConfiguration : IEntityTypeConfiguration<UserSessionEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<UserSessionEntity> builder)
        {
            builder.ToTable("UserSessions");
            builder.Property(session => session.DeviceName).HasMaxLength(200);
            builder.HasIndex(session => new { session.UserId, session.RevokedAt });
            builder.HasOne<UserEntity>().WithMany().HasForeignKey(session => session.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<BreakGlassGrantEntity>().WithMany()
                .HasForeignKey(session => session.BreakGlassGrantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
