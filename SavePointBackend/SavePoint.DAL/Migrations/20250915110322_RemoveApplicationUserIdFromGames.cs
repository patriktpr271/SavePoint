using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SavePoint.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveApplicationUserIdFromGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_AspNetUsers_ApplicationUserId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Platforms_ExternalId",
                table: "Platforms");

            migrationBuilder.DropIndex(
                name: "IX_Genres_ExternalId",
                table: "Genres");

            migrationBuilder.DropIndex(
                name: "IX_Games_ApplicationUserId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_ExternalId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Companies_ExternalId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Games");

            migrationBuilder.AlterColumn<long>(
                name: "ExternalId",
                table: "Platforms",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "ExternalId",
                table: "Genres",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "ExternalId",
                table: "Games",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "ExternalId",
                table: "Companies",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateIndex(
                name: "IX_Platforms_ExternalId",
                table: "Platforms",
                column: "ExternalId",
                unique: true,
                filter: "[ExternalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Genres_ExternalId",
                table: "Genres",
                column: "ExternalId",
                unique: true,
                filter: "[ExternalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Games_ExternalId",
                table: "Games",
                column: "ExternalId",
                unique: true,
                filter: "[ExternalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_ExternalId",
                table: "Companies",
                column: "ExternalId",
                unique: true,
                filter: "[ExternalId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Platforms_ExternalId",
                table: "Platforms");

            migrationBuilder.DropIndex(
                name: "IX_Genres_ExternalId",
                table: "Genres");

            migrationBuilder.DropIndex(
                name: "IX_Games_ExternalId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Companies_ExternalId",
                table: "Companies");

            migrationBuilder.AlterColumn<long>(
                name: "ExternalId",
                table: "Platforms",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ExternalId",
                table: "Genres",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ExternalId",
                table: "Games",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Games",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ExternalId",
                table: "Companies",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Platforms_ExternalId",
                table: "Platforms",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Genres_ExternalId",
                table: "Genres",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_ApplicationUserId",
                table: "Games",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_ExternalId",
                table: "Games",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_ExternalId",
                table: "Companies",
                column: "ExternalId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_AspNetUsers_ApplicationUserId",
                table: "Games",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
