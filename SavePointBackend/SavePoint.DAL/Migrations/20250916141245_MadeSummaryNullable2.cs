using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SavePoint.DAL.Migrations
{
    /// <inheritdoc />
    public partial class MadeSummaryNullable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameCompanies_GameId_CompanyId_Role",
                table: "GameCompanies");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "GameCompanies",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_GameCompanies_GameId_CompanyId",
                table: "GameCompanies",
                columns: new[] { "GameId", "CompanyId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameCompanies_GameId_CompanyId",
                table: "GameCompanies");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "GameCompanies",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_GameCompanies_GameId_CompanyId_Role",
                table: "GameCompanies",
                columns: new[] { "GameId", "CompanyId", "Role" },
                unique: true);
        }
    }
}
