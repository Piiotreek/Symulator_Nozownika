using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Symulator_Nozownika.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestNameAndSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Quests",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Quests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "Quests",
                columns: new[] { "Id", "Description", "Name", "QuestType", "ResetsAt", "RewardCoins", "RewardXp", "TargetValue" },
                values: new object[,]
                {
                    { 1, "Kliknij 50 razy w trakcie dzisiejszej sesji.", "Klikacz Dnia", 0, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), 10, 25, 50 },
                    { 2, "Kliknij 150 razy w trakcie dzisiejszej sesji.", "Szybkie Tempo", 0, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), 25, 60, 150 },
                    { 3, "Kliknij 300 razy w trakcie dzisiejszej sesji.", "Maratończyk", 0, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), 50, 120, 300 },
                    { 4, "Zdobądź 500 punktów w ciągu dnia.", "Strzelec Wyborowy", 1, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), 15, 40, 500 },
                    { 5, "Zdobądź 1500 punktów w ciągu dnia.", "Łowca Punktów", 1, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), 35, 90, 1500 },
                    { 6, "Zdobądź 3000 punktów w ciągu dnia.", "Mistrz Noży", 1, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), 75, 200, 3000 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Quests",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Quests");
        }
    }
}
