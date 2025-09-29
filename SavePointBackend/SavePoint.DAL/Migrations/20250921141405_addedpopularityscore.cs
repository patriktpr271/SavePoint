using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SavePoint.DAL.Migrations
{
    /// <inheritdoc />
    public partial class addedpopularityscore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Popularities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalGameId = table.Column<long>(type: "bigint", nullable: false),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PopularityScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PopularityType = table.Column<long>(type: "bigint", nullable: false),
                    ExternalId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Popularities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Popularities_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Popularities_ExternalId",
                table: "Popularities",
                column: "ExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_Popularities_GameId",
                table: "Popularities",
                column: "GameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Popularities");
        }
    }
}
