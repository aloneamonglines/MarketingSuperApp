using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MKS.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DurableOutboxInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedAt",
                schema: "identity",
                table: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_InboxMessages_EventId",
                schema: "identity",
                table: "InboxMessages");

            migrationBuilder.RenameColumn(
                name: "Error",
                schema: "identity",
                table: "OutboxMessages",
                newName: "LastError");

            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                schema: "identity",
                table: "OutboxMessages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeadLetteredAt",
                schema: "identity",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EventId",
                schema: "identity",
                table: "OutboxMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LockId",
                schema: "identity",
                table: "OutboxMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LockedUntil",
                schema: "identity",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NextAttemptAt",
                schema: "identity",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "identity",
                table: "OutboxMessages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ProcessedAt",
                schema: "identity",
                table: "InboxMessages",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                schema: "identity",
                table: "InboxMessages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Consumer",
                schema: "identity",
                table: "InboxMessages",
                type: "character varying(400)",
                maxLength: 400,
                nullable: false,
                defaultValue: "legacy");

            migrationBuilder.AddColumn<string>(
                name: "LastError",
                schema: "identity",
                table: "InboxMessages",
                type: "text",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE identity."OutboxMessages"
                SET "EventId" = COALESCE(("Payload"::jsonb ->> 'EventId')::uuid, "Id"),
                    "NextAttemptAt" = COALESCE("ProcessedAt", "OccurredAt"),
                    "Status" = CASE WHEN "ProcessedAt" IS NULL THEN 0 ELSE 2 END;

                UPDATE identity."InboxMessages"
                SET "AttemptCount" = 1
                WHERE "ProcessedAt" IS NOT NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "EventId",
                schema: "identity",
                table: "OutboxMessages",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "NextAttemptAt",
                schema: "identity",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "OutboxDeadLetters",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FailedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxDeadLetters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_EventId",
                schema: "identity",
                table: "OutboxMessages",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_LockId",
                schema: "identity",
                table: "OutboxMessages",
                column: "LockId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_NextAttemptAt_LockedUntil",
                schema: "identity",
                table: "OutboxMessages",
                columns: new[] { "Status", "NextAttemptAt", "LockedUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_EventId_Consumer",
                schema: "identity",
                table: "InboxMessages",
                columns: new[] { "EventId", "Consumer" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxDeadLetters_EventId",
                schema: "identity",
                table: "OutboxDeadLetters",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxDeadLetters_FailedAt",
                schema: "identity",
                table: "OutboxDeadLetters",
                column: "FailedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxDeadLetters",
                schema: "identity");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_EventId",
                schema: "identity",
                table: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_LockId",
                schema: "identity",
                table: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_Status_NextAttemptAt_LockedUntil",
                schema: "identity",
                table: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_InboxMessages_EventId_Consumer",
                schema: "identity",
                table: "InboxMessages");

            migrationBuilder.DropColumn(
                name: "AttemptCount",
                schema: "identity",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "DeadLetteredAt",
                schema: "identity",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "EventId",
                schema: "identity",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "LockId",
                schema: "identity",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "LockedUntil",
                schema: "identity",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "NextAttemptAt",
                schema: "identity",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "identity",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "AttemptCount",
                schema: "identity",
                table: "InboxMessages");

            migrationBuilder.DropColumn(
                name: "Consumer",
                schema: "identity",
                table: "InboxMessages");

            migrationBuilder.DropColumn(
                name: "LastError",
                schema: "identity",
                table: "InboxMessages");

            migrationBuilder.RenameColumn(
                name: "LastError",
                schema: "identity",
                table: "OutboxMessages",
                newName: "Error");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ProcessedAt",
                schema: "identity",
                table: "InboxMessages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedAt",
                schema: "identity",
                table: "OutboxMessages",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_EventId",
                schema: "identity",
                table: "InboxMessages",
                column: "EventId",
                unique: true);
        }
    }
}
