using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SavePoint.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemovePlatformIdFromGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Platforms_PlatformId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_PlatformId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "PlatformId",
                table: "Games");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PlatformId",
                table: "Games",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_PlatformId",
                table: "Games",
                column: "PlatformId");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Platforms_PlatformId",
                table: "Games",
                column: "PlatformId",
                principalTable: "Platforms",
                principalColumn: "Id");
        }
    }
}
