using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FishClubAlginet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBiggestCatchMinWeightToCompetitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BiggestCatchMinWeightInGrams",
                table: "Competitions",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BiggestCatchMinWeightInGrams",
                table: "Competitions");
        }
    }
}
