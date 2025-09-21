using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SavePoint.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddedRoleForCompanies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameCompanies_GameId_CompanyId",
                table: "GameCompanies");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "GameCompanies",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_GameCompanies_GameId_CompanyId_Role",
                table: "GameCompanies",
                columns: new[] { "GameId", "CompanyId", "Role" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameCompanies_GameId_CompanyId_Role",
                table: "GameCompanies");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "GameCompanies");

            migrationBuilder.CreateIndex(
                name: "IX_GameCompanies_GameId_CompanyId",
                table: "GameCompanies",
                columns: new[] { "GameId", "CompanyId" },
                unique: true);
        }
    }
}
