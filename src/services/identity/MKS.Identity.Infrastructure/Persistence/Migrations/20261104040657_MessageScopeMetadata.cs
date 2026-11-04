using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MKS.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MessageScopeMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AppId",
                schema: "identity",
                table: "OutboxMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Scope",
                schema: "identity",
                table: "OutboxMessages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "identity",
                table: "OutboxMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AppId",
                schema: "identity",
                table: "OutboxDeadLetters",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Scope",
                schema: "identity",
                table: "OutboxDeadLetters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "identity",
                table: "OutboxDeadLetters",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AppId",
                schema: "identity",
                table: "InboxMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Scope",
                schema: "identity",
                table: "InboxMessages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "identity",
                table: "InboxMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE identity."OutboxMessages"
                SET "AppId" = NULLIF("Payload"::jsonb ->> 'AppId', '')::uuid,
                    "TenantId" = NULLIF("Payload"::jsonb ->> 'TenantId', '')::uuid;

                UPDATE identity."OutboxMessages"
                SET "Scope" = CASE
                    WHEN "TenantId" IS NOT NULL THEN 2
                    WHEN "AppId" IS NOT NULL THEN 1
                    ELSE 0
                END;

                UPDATE identity."OutboxDeadLetters" AS dead_letter
                SET "AppId" = outbox."AppId",
                    "TenantId" = outbox."TenantId",
                    "Scope" = outbox."Scope"
                FROM identity."OutboxMessages" AS outbox
                WHERE dead_letter."EventId" = outbox."EventId";

                UPDATE identity."InboxMessages" AS inbox
                SET "AppId" = outbox."AppId",
                    "TenantId" = outbox."TenantId",
                    "Scope" = outbox."Scope"
                FROM identity."OutboxMessages" AS outbox
                WHERE inbox."EventId" = outbox."EventId";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Scope_AppId_TenantId_Status_NextAttemptAt",
                schema: "identity",
                table: "OutboxMessages",
                columns: new[] { "Scope", "AppId", "TenantId", "Status", "NextAttemptAt" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_OutboxMessages_Scope",
                schema: "identity",
                table: "OutboxMessages",
                sql: "(\"Scope\" = 0 AND \"AppId\" IS NULL AND \"TenantId\" IS NULL) OR (\"Scope\" = 1 AND \"AppId\" IS NOT NULL AND \"TenantId\" IS NULL) OR (\"Scope\" = 2 AND \"AppId\" IS NOT NULL AND \"TenantId\" IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxDeadLetters_Scope_AppId_TenantId_FailedAt",
                schema: "identity",
                table: "OutboxDeadLetters",
                columns: new[] { "Scope", "AppId", "TenantId", "FailedAt" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_OutboxDeadLetters_Scope",
                schema: "identity",
                table: "OutboxDeadLetters",
                sql: "(\"Scope\" = 0 AND \"AppId\" IS NULL AND \"TenantId\" IS NULL) OR (\"Scope\" = 1 AND \"AppId\" IS NOT NULL AND \"TenantId\" IS NULL) OR (\"Scope\" = 2 AND \"AppId\" IS NOT NULL AND \"TenantId\" IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_Scope_AppId_TenantId_EventId_Consumer",
                schema: "identity",
                table: "InboxMessages",
                columns: new[] { "Scope", "AppId", "TenantId", "EventId", "Consumer" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_InboxMessages_Scope",
                schema: "identity",
                table: "InboxMessages",
                sql: "(\"Scope\" = 0 AND \"AppId\" IS NULL AND \"TenantId\" IS NULL) OR (\"Scope\" = 1 AND \"AppId\" IS NOT NULL AND \"TenantId\" IS NULL) OR (\"Scope\" = 2 AND \"AppId\" IS NOT NULL AND \"TenantId\" IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_Scope_AppId_TenantId_Status_NextAttemptAt",
                schema: "identity",
                table: "OutboxMessages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OutboxMessages_Scope",
                schema: "identity",
                table: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_OutboxDeadLetters_Scope_AppId_TenantId_FailedAt",
                schema: "identity",
                table: "OutboxDeadLetters");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OutboxDeadLetters_Scope",
                schema: "identity",
                table: "OutboxDeadLetters");

            migrationBuilder.DropIndex(
                name: "IX_InboxMessages_Scope_AppId_TenantId_EventId_Consumer",
                schema: "identity",
                table: "InboxMessages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InboxMessages_Scope",
                schema: "identity",
                table: "InboxMessages");

            migrationBuilder.DropColumn(
                name: "AppId",
                schema: "identity",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "Scope",
                schema: "identity",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "identity",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "AppId",
                schema: "identity",
                table: "OutboxDeadLetters");

            migrationBuilder.DropColumn(
                name: "Scope",
                schema: "identity",
                table: "OutboxDeadLetters");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "identity",
                table: "OutboxDeadLetters");

            migrationBuilder.DropColumn(
                name: "AppId",
                schema: "identity",
                table: "InboxMessages");

            migrationBuilder.DropColumn(
                name: "Scope",
                schema: "identity",
                table: "InboxMessages");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "identity",
                table: "InboxMessages");
        }
    }
}
