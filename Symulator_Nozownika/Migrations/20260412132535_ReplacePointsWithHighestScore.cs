using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Symulator_Nozownika.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePointsWithHighestScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAccounts_UserStatistics_StatisticsId",
                table: "UserAccounts");

            migrationBuilder.DropIndex(
                name: "IX_UserAccounts_StatisticsId",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "Points",
                table: "UserStatistics");

            migrationBuilder.DropColumn(
                name: "TotalDeaths",
                table: "UserStatistics");

            migrationBuilder.DropColumn(
                name: "StatisticsId",
                table: "UserAccounts");

            migrationBuilder.RenameColumn(
                name: "TotalKills",
                table: "UserStatistics",
                newName: "HighestScore");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HighestScore",
                table: "UserStatistics",
                newName: "TotalKills");

            migrationBuilder.AddColumn<int>(
                name: "Points",
                table: "UserStatistics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalDeaths",
                table: "UserStatistics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatisticsId",
                table: "UserAccounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_StatisticsId",
                table: "UserAccounts",
                column: "StatisticsId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAccounts_UserStatistics_StatisticsId",
                table: "UserAccounts",
                column: "StatisticsId",
                principalTable: "UserStatistics",
                principalColumn: "Id");
        }
    }
}
