using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MKS.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameAuditLogToSecurityAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "AuditLogs",
                schema: "identity",
                newName: "SecurityAuditLogs");

            migrationBuilder.Sql("""
                ALTER TABLE identity."SecurityAuditLogs"
                    RENAME CONSTRAINT "PK_AuditLogs" TO "PK_SecurityAuditLogs";
                """);

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_OccurredAt",
                schema: "identity",
                table: "SecurityAuditLogs",
                newName: "IX_SecurityAuditLogs_OccurredAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_SecurityAuditLogs_OccurredAt",
                schema: "identity",
                table: "SecurityAuditLogs",
                newName: "IX_AuditLogs_OccurredAt");

            migrationBuilder.Sql("""
                ALTER TABLE identity."SecurityAuditLogs"
                    RENAME CONSTRAINT "PK_SecurityAuditLogs" TO "PK_AuditLogs";
                """);

            migrationBuilder.RenameTable(
                name: "SecurityAuditLogs",
                schema: "identity",
                newName: "AuditLogs");
        }
    }
}
