using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Symulator_Nozownika.Migrations
{
    /// <inheritdoc />
    public partial class ExpandShopUpgrades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DamageBonus", "Description", "ImageUrl", "Name", "Price", "WeaponId" },
                values: new object[] { 2, "Pewniejszy chwyt przyspiesza kolejne cięcia kuchennym nożem.", "/images/knife.png", "Lekki chwyt Kitchen Knife", 380, 1 });

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CooldownReduction", "DamageBonus", "Description", "ImageUrl", "Name", "Price", "WeaponId" },
                values: new object[] { 0.050000000000000003, 6, "Lepszy balans skraca czas odnowienia sztyletu.", "/images/dagger.png", "Wyważenie Dagger", 650, 2 });

            migrationBuilder.InsertData(
                table: "WeaponUpgrades",
                columns: new[] { "Id", "CooldownReduction", "DamageBonus", "Description", "ImageUrl", "Name", "Price", "WeaponId" },
                values: new object[,]
                {
                    { 4, 0.02, 9, "Dodatkowe nacięcia sprawiają, że sztylet rani znacznie mocniej.", "/images/dagger.png", "Ząbkowane ostrze Dagger", 780, 2 },
                    { 5, 0.040000000000000001, 10, "Dodatkowa masa zwiększa siłę każdego zamachu maczetą.", "/images/machete.png", "Cięższy grzbiet Machete", 980, 3 },
                    { 6, 0.080000000000000002, 6, "Nowa rękojeść pozwala szybciej wrócić do pozycji po cięciu.", "/images/machete.png", "Rękojeść rajdowa Machete", 1120, 3 },
                    { 7, 0.050000000000000003, 11, "Dłuższe ostrze tnie czyściej i głębiej.", "/images/sword.png", "Polerowana klinga Sword", 1350, 4 },
                    { 8, 0.089999999999999997, 7, "Lepsza kontrola miecza poprawia tempo ataku.", "/images/sword.png", "Stalowy jelec Sword", 1490, 4 },
                    { 9, 0.040000000000000001, 15, "Dodatkowe kolce wzmacniają brutalność uderzeń toporem.", "/images/axe.png", "Kolczasty topór Axe", 1750, 5 },
                    { 10, 0.10000000000000001, 9, "Lepsze wyważenie pomaga szybciej odzyskać kontrolę nad toporem.", "/images/axe.png", "Przeciwwaga Axe", 1880, 5 },
                    { 11, 0.040000000000000001, 8, "Hartowany grot włóczni lepiej przebija cel.", "/images/spear.png", "Wzmocniony grot Spear", 920, 6 },
                    { 12, 0.080000000000000002, 5, "Lżejszy drzewiec zwiększa szybkość kolejnego pchnięcia.", "/images/spear.png", "Elastyczny drzewiec Spear", 1080, 6 },
                    { 13, 0.050000000000000003, 12, "Masakrycznie ostra stal zwiększa obrażenia tasaka.", "/images/cleaver.png", "Rzeźnicka stal Cleaver", 1420, 7 },
                    { 14, 0.10000000000000001, 7, "Pewniejszy uchwyt skraca czas między kolejnymi zamachami.", "/images/cleaver.png", "Gumowany chwyt Cleaver", 1560, 7 },
                    { 15, 0.050000000000000003, 18, "Cięższy rdzeń buławy robi ogromną różnicę przy trafieniu.", "/images/mace.png", "Żelazny rdzeń Mace", 2200, 8 },
                    { 16, 0.12, 10, "Nowa owijka poprawia rytm i skraca przerwy między ciosami.", "/images/mace.png", "Skórzana owijka Mace", 2380, 8 },
                    { 17, 0.080000000000000002, 12, "Wzmocnione ostrze zapewnia dodatkową moc katanie.", "/images/katana.png", "Hartowana Katana", 1800, 9 },
                    { 18, 0.11, 8, "Precyzyjna pochwa pozwala wrócić do ataku niemal natychmiast.", "/images/katana.png", "Błyskawiczna pochwa Katana", 1940, 9 },
                    { 19, 0.01, 2, "Usztywnienie nożyc poprawia siłę cięcia mimo ich lekkości.", "/images/scissors.png", "Tytanowy nit Scissors", 240, 10 },
                    { 20, 0.029999999999999999, 1, "Nowa sprężyna przyspiesza każde następne kliknięcie nożycami.", "/images/scissors.png", "Sprężyna Scissors", 320, 10 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DamageBonus", "Description", "ImageUrl", "Name", "Price", "WeaponId" },
                values: new object[] { 6, "Lepszy balans skraca czas odnowienia sztyletu.", "/images/dagger.png", "Wyważenie Dagger", 650, 2 });

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CooldownReduction", "DamageBonus", "Description", "ImageUrl", "Name", "Price", "WeaponId" },
                values: new object[] { 0.080000000000000002, 12, "Wzmocnione ostrze zapewnia dodatkową moc katanie.", "/images/katana.png", "Hartowana Katana", 1800, 9 });
        }
    }
}
