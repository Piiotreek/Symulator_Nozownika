using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Symulator_Nozownika.Migrations
{
    /// <inheritdoc />
    public partial class achivements_four_to_seven : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Achievements",
                columns: new[] { "Id", "Description", "ImagePath", "Name", "TargetValue", "Type" },
                values: new object[,]
                {
                    { 4, "Zagraj w swoją pierwszą grę.", "/images/achiv/first-game.png", "Pierwsza krew", 1, 0 },
                    { 5, "Kliknij 100 razy w trakcie jednej gry.", "/images/achiv/100-clicks-in-one-game.png", "Szybkie palce", 100, 4 },
                    { 6, "Zdobądź łącznie 1000 kliknięć we wszystkich grach.", "/images/achiv/1000-clicks.png", "Klikacz", 1000, 1 },
                    { 7, "Zdobądź łącznie 3000 kliknięć we wszystkich grach.", "/images/achiv/3000-clicks.png", "Maniak", 3000, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 7);
        }
    }
}
