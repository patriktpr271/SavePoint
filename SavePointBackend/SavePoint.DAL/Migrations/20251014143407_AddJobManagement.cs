using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SavePoint.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddJobManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportJobRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecordsProcessed = table.Column<int>(type: "int", nullable: false),
                    RecordsAdded = table.Column<int>(type: "int", nullable: false),
                    RecordsUpdated = table.Column<int>(type: "int", nullable: false),
                    RecordsFailed = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ErrorDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJobRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImportStatistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalRecords = table.Column<int>(type: "int", nullable: false),
                    LastSuccessfulImport = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastIGDBUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextScheduledImport = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AutoImportEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ImportFrequencyDays = table.Column<int>(type: "int", nullable: false),
                    AverageImportDuration = table.Column<TimeSpan>(type: "time", nullable: true),
                    SuccessfulImports = table.Column<int>(type: "int", nullable: false),
                    FailedImports = table.Column<int>(type: "int", nullable: false),
                    Configuration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportStatistics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobRuns_JobType_StartedAt",
                table: "ImportJobRuns",
                columns: new[] { "JobType", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobRuns_Status",
                table: "ImportJobRuns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ImportStatistics_DataType",
                table: "ImportStatistics",
                column: "DataType",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportJobRuns");

            migrationBuilder.DropTable(
                name: "ImportStatistics");
        }
    }
}
