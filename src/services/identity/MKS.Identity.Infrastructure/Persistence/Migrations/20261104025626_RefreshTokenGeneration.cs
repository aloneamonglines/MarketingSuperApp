using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MKS.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefreshTokenGeneration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RefreshTokenGeneration",
                schema: "identity",
                table: "UserSessions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshTokenGeneration",
                schema: "identity",
                table: "UserSessions");
        }
    }
}
