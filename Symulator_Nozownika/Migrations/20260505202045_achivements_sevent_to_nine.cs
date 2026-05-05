using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Symulator_Nozownika.Migrations
{
    /// <inheritdoc />
    public partial class achivements_sevent_to_nine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "ImagePath", "TargetValue" },
                values: new object[] { "Zdobądź łącznie 300 kliknięć we wszystkich grach.", "/images/achiv/300-clicks.png", 300 });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "ImagePath", "Name", "TargetValue" },
                values: new object[] { "Zdobądź łącznie 500 kliknięć we wszystkich grach.", "/images/achiv/500-clicks.png", "Wprawiony Klikacz", 500 });

            migrationBuilder.InsertData(
                table: "Achievements",
                columns: new[] { "Id", "Description", "ImagePath", "Name", "TargetValue", "Type" },
                values: new object[,]
                {
                    { 8, "Zdobądź łącznie 1000 kliknięć we wszystkich grach.", "/images/achiv/1000-clicks.png", "Maniak", 1000, 1 },
                    { 9, "Zdobądź łącznie 3000 kliknięć we wszystkich grach.", "/images/achiv/3000-clicks.png", "3000 GWIAZD!", 3000, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "ImagePath", "TargetValue" },
                values: new object[] { "Zdobądź łącznie 1000 kliknięć we wszystkich grach.", "/images/achiv/1000-clicks.png", 1000 });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "ImagePath", "Name", "TargetValue" },
                values: new object[] { "Zdobądź łącznie 3000 kliknięć we wszystkich grach.", "/images/achiv/3000-clicks.png", "Maniak", 3000 });
        }
    }
}
