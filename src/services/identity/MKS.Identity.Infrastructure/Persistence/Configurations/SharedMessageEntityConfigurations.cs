using MKS.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MKS.Identity.Infrastructure.Persistence.Configurations
{
    /// <summary>Configures integration-event outbox records.</summary>
    public sealed class OutboxMessageEntityConfiguration : IEntityTypeConfiguration<OutboxMessageEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<OutboxMessageEntity> builder)
        {
            builder.ToTable("OutboxMessages", table => table.HasCheckConstraint(
                "CK_OutboxMessages_Scope",
                """("Scope" = 0 AND "AppId" IS NULL AND "TenantId" IS NULL) OR ("Scope" = 1 AND "AppId" IS NOT NULL AND "TenantId" IS NULL) OR ("Scope" = 2 AND "AppId" IS NOT NULL AND "TenantId" IS NOT NULL)"""));
            builder.Property(message => message.EventId).IsRequired();
            builder.Property(message => message.Type).HasMaxLength(400).IsRequired();
            builder.Property(message => message.Payload).IsRequired();
            builder.Property(message => message.LastError);
            builder.HasIndex(message => message.EventId).IsUnique();
            builder.HasIndex(message => new { message.Status, message.NextAttemptAt, message.LockedUntil });
            builder.HasIndex(message => new
            {
                message.Scope,
                message.AppId,
                message.TenantId,
                message.Status,
                message.NextAttemptAt
            });
            builder.HasIndex(message => message.LockId);
        }
    }

    /// <summary>Configures processed integration-event inbox records.</summary>
    public sealed class InboxMessageEntityConfiguration : IEntityTypeConfiguration<InboxMessageEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<InboxMessageEntity> builder)
        {
            builder.ToTable("InboxMessages", table => table.HasCheckConstraint(
                "CK_InboxMessages_Scope",
                """("Scope" = 0 AND "AppId" IS NULL AND "TenantId" IS NULL) OR ("Scope" = 1 AND "AppId" IS NOT NULL AND "TenantId" IS NULL) OR ("Scope" = 2 AND "AppId" IS NOT NULL AND "TenantId" IS NOT NULL)"""));
            builder.Property(message => message.Consumer).HasMaxLength(400).IsRequired();
            builder.Property(message => message.Type).HasMaxLength(400).IsRequired();
            builder.Property(message => message.LastError);
            builder.HasIndex(message => new { message.EventId, message.Consumer }).IsUnique();
            builder.HasIndex(message => new
            {
                message.Scope,
                message.AppId,
                message.TenantId,
                message.EventId,
                message.Consumer
            });
        }
    }

    /// <summary>Configures integration events that exhausted their delivery attempts.</summary>
    public sealed class OutboxDeadLetterEntityConfiguration : IEntityTypeConfiguration<OutboxDeadLetterEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<OutboxDeadLetterEntity> builder)
        {
            builder.ToTable("OutboxDeadLetters", table => table.HasCheckConstraint(
                "CK_OutboxDeadLetters_Scope",
                """("Scope" = 0 AND "AppId" IS NULL AND "TenantId" IS NULL) OR ("Scope" = 1 AND "AppId" IS NOT NULL AND "TenantId" IS NULL) OR ("Scope" = 2 AND "AppId" IS NOT NULL AND "TenantId" IS NOT NULL)"""));
            builder.Property(message => message.Type).HasMaxLength(400).IsRequired();
            builder.Property(message => message.Payload).IsRequired();
            builder.Property(message => message.LastError).IsRequired();
            builder.HasIndex(message => message.EventId);
            builder.HasIndex(message => message.FailedAt);
            builder.HasIndex(message => new
            {
                message.Scope,
                message.AppId,
                message.TenantId,
                message.FailedAt
            });
        }
    }
}
