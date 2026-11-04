using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MKS.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UserSessionBreakGlassForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_BreakGlassGrantId",
                schema: "identity",
                table: "UserSessions",
                column: "BreakGlassGrantId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSessions_BreakGlassGrants_BreakGlassGrantId",
                schema: "identity",
                table: "UserSessions",
                column: "BreakGlassGrantId",
                principalSchema: "identity",
                principalTable: "BreakGlassGrants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSessions_BreakGlassGrants_BreakGlassGrantId",
                schema: "identity",
                table: "UserSessions");

            migrationBuilder.DropIndex(
                name: "IX_UserSessions_BreakGlassGrantId",
                schema: "identity",
                table: "UserSessions");
        }
    }
}
