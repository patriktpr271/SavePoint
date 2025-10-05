using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SavePoint.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "UserLists",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "UserLists",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "DefaultListType",
                table: "UserLists",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DownvoteCount",
                table: "UserLists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "UserLists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "UserLists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UpvoteCount",
                table: "UserLists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VoteScore",
                table: "UserLists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserListVotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsUpvote = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserListVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserListVotes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserListVotes_UserLists_UserListId",
                        column: x => x.UserListId,
                        principalTable: "UserLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserListVotes_UserId",
                table: "UserListVotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserListVotes_UserListId_UserId",
                table: "UserListVotes",
                columns: new[] { "UserListId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserListVotes");

            migrationBuilder.DropColumn(
                name: "DefaultListType",
                table: "UserLists");

            migrationBuilder.DropColumn(
                name: "DownvoteCount",
                table: "UserLists");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "UserLists");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "UserLists");

            migrationBuilder.DropColumn(
                name: "UpvoteCount",
                table: "UserLists");

            migrationBuilder.DropColumn(
                name: "VoteScore",
                table: "UserLists");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "UserLists",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "UserLists",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}
