using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Symulator_Nozownika.Migrations
{
    /// <inheritdoc />
    public partial class ExpandPotionTypesAndShopBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Potions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "DurationInSeconds", "Name" },
                values: new object[] { "Przyspiesza tempo ataku na kilka sekund przed lub w trakcie rundy.", 6, "Adrenalina" });

            migrationBuilder.UpdateData(
                table: "Potions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "DurationInSeconds", "EffectStrength", "Name" },
                values: new object[] { "Podkręca obrażenia i zwiększa szansę na potężny critical hit x3.", 8, 18, "Furia" });

            migrationBuilder.UpdateData(
                table: "Potions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "DurationInSeconds", "EffectStrength", "Name" },
                values: new object[] { "Każde trafienie może wywołać niestackujący bleed, który dobija cel z czasem.", 10, 12, "Krwawiące Ostrze" });

            migrationBuilder.InsertData(
                table: "Potions",
                columns: new[] { "Id", "Description", "DurationInSeconds", "EffectStrength", "ImageUrl", "Name", "Price" },
                values: new object[] { 4, "Zatrzymuje licznik rundy na 3 sekundy i daje moment na darmowe trafienia.", 3, 3, "/images/sword.png", "Stop-Czas", 520 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Potions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "Potions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "DurationInSeconds", "Name" },
                values: new object[] { "Krótki zastrzyk energii do szybszej rozgrywki.", 30, "Mała potka energii" });

            migrationBuilder.UpdateData(
                table: "Potions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "DurationInSeconds", "EffectStrength", "Name" },
                values: new object[] { "Mocniejsze uderzenia przez chwilę.", 45, 20, "Potka furii" });

            migrationBuilder.UpdateData(
                table: "Potions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "DurationInSeconds", "EffectStrength", "Name" },
                values: new object[] { "Pomaga utrzymać rytm i serię kliknięć.", 60, 30, "Eliksir skupienia" });
        }
    }
}
