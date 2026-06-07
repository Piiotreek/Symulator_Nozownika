using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Symulator_Nozownika.Migrations
{
    /// <inheritdoc />
    public partial class PolishUiAndWeapons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Ostrzenie noża kuchennego");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Lekki chwyt noża kuchennego");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Wyważenie sztyletu");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Ząbkowane ostrze sztyletu");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Cięższy grzbiet maczety");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Rajdowa rękojeść maczety");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Polerowana klinga miecza");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "Stalowy jelec miecza");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 9,
                column: "Name",
                value: "Kolczaste ostrze topora");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 10,
                column: "Name",
                value: "Przeciwwaga topora");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 11,
                column: "Name",
                value: "Wzmocniony grot włóczni");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 12,
                column: "Name",
                value: "Elastyczny drzewiec włóczni");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 13,
                column: "Name",
                value: "Rzeźnicka stal tasaka");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 14,
                column: "Name",
                value: "Gumowany chwyt tasaka");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 15,
                column: "Name",
                value: "Żelazny rdzeń buławy");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 16,
                column: "Name",
                value: "Skórzana owijka buławy");

            migrationBuilder.UpdateData(
                table: "Weapons",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Nóż kuchenny");

            migrationBuilder.UpdateData(
                table: "Weapons",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Sztylet");

            migrationBuilder.UpdateData(
                table: "Weapons",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Maczeta");

            migrationBuilder.UpdateData(
                table: "Weapons",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Miecz");

            migrationBuilder.UpdateData(
                table: "Weapons",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Topór");

            migrationBuilder.UpdateData(
                table: "Weapons",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Włócznia");

            migrationBuilder.UpdateData(
                table: "Weapons",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Tasak");

            migrationBuilder.UpdateData(
                table: "Weapons",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "Buława");

            migrationBuilder.UpdateData(
                table: "Weapons",
                keyColumn: "Id",
                keyValue: 10,
                column: "Name",
                value: "Nożyczki");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Ostrzenie Kitchen Knife");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Lekki chwyt Kitchen Knife");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Wyważenie Dagger");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Ząbkowane ostrze Dagger");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Cięższy grzbiet Machete");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Rękojeść rajdowa Machete");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Polerowana klinga Sword");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "Stalowy jelec Sword");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 9,
                column: "Name",
                value: "Kolczasty topór Axe");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 10,
                column: "Name",
                value: "Przeciwwaga Axe");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 11,
                column: "Name",
                value: "Wzmocniony grot Spear");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 12,
                column: "Name",
                value: "Elastyczny drzewiec Spear");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 13,
                column: "Name",
                value: "Rzeźnicka stal Cleaver");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 14,
                column: "Name",
                value: "Gumowany chwyt Cleaver");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 15,
                column: "Name",
                value: "Żelazny rdzeń Mace");

            migrationBuilder.UpdateData(
                table: "WeaponUpgrades",
                keyColumn: "Id",
                keyValue: 16,
                column: "Name",
                value: "Skórzana owijka Mace");

            migrationBuilder.UpdateData(
                table: "Weapons",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Kitchen Knife");

            migrationBuilder.UpdateData(
                table: "Weapons",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Dagger");

            migrationBuilder.UpdateData(
                table: "Weapons",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Machete");

            migrationBuilder.UpdateData(
                table: "Weapons",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Sword");

            migrationBuilder.UpdateData(
                table: "Weapons",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Axe");

            migrationBuilder.UpdateData(
                table: "Weapons",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Spear");

            migrationBuilder.UpdateData(
                table: "Weapons",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Cleaver");

            migrationBuilder.UpdateData(
                table: "Weapons",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "Mace");

            migrationBuilder.UpdateData(
                table: "Weapons",
                keyColumn: "Id",
                keyValue: 10,
                column: "Name",
                value: "Scissors");
        }
    }
}
