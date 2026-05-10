using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Symulator_Nozownika.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuestSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Nożownik Dnia");

            migrationBuilder.UpdateData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Szybkie Paluszki");

            migrationBuilder.UpdateData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "British Special");

            migrationBuilder.UpdateData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Nożyce Jak Brzytwa");

            migrationBuilder.UpdateData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Daj mu jeszcze jeden w serce!");

            migrationBuilder.UpdateData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Prawdziwy Londyńczyk");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Klikacz Dnia");

            migrationBuilder.UpdateData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Szybkie Tempo");

            migrationBuilder.UpdateData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Maratończyk");

            migrationBuilder.UpdateData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Strzelec Wyborowy");

            migrationBuilder.UpdateData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Łowca Punktów");

            migrationBuilder.UpdateData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Mistrz Noży");
        }
    }
}
