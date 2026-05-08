using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FishClubAlginet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompetitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Competitions_LeagueId",
                table: "Competitions");

            migrationBuilder.AlterColumn<string>(
                name: "Subspecialty",
                table: "Competitions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Competitions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Competitions",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Planned");

            migrationBuilder.CreateTable(
                name: "CompetitionResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompetitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FishermanId = table.Column<int>(type: "int", nullable: false),
                    AssignedSpotNumber = table.Column<int>(type: "int", nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsValidated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DidAttend = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    WeightInGrams = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BiggestCatchWeight = table.Column<int>(type: "int", nullable: true),
                    Points = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    Ranking = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdateUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetitionResults_Competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompetitionResults_Fishermen_FishermanId",
                        column: x => x.FishermanId,
                        principalTable: "Fishermen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Competitions_LeagueId_CompetitionNumber",
                table: "Competitions",
                columns: new[] { "LeagueId", "CompetitionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionResults_CompetitionId_FishermanId",
                table: "CompetitionResults",
                columns: new[] { "CompetitionId", "FishermanId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionResults_CompetitionId_SpotNumber",
                table: "CompetitionResults",
                columns: new[] { "CompetitionId", "AssignedSpotNumber" },
                unique: true,
                filter: "[AssignedSpotNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionResults_FishermanId",
                table: "CompetitionResults",
                column: "FishermanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompetitionResults");

            migrationBuilder.DropIndex(
                name: "IX_Competitions_LeagueId_CompetitionNumber",
                table: "Competitions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Competitions");

            migrationBuilder.AlterColumn<string>(
                name: "Subspecialty",
                table: "Competitions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Competitions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_Competitions_LeagueId",
                table: "Competitions",
                column: "LeagueId");
        }
    }
}
