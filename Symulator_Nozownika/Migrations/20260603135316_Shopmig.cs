using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Symulator_Nozownika.Migrations
{
    /// <inheritdoc />
    public partial class Shopmig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Potions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false),
                    EffectStrength = table.Column<int>(type: "int", nullable: false),
                    DurationInSeconds = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Potions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeaponUpgrades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    WeaponId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false),
                    DamageBonus = table.Column<int>(type: "int", nullable: false),
                    CooldownReduction = table.Column<double>(type: "float", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeaponUpgrades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeaponUpgrades_Weapons_WeaponId",
                        column: x => x.WeaponId,
                        principalTable: "Weapons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchasedPotions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PotionId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PricePaid = table.Column<int>(type: "int", nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchasedPotions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchasedPotions_Potions_PotionId",
                        column: x => x.PotionId,
                        principalTable: "Potions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchasedPotions_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchasedWeaponUpgrades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    WeaponUpgradeId = table.Column<int>(type: "int", nullable: false),
                    PricePaid = table.Column<int>(type: "int", nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchasedWeaponUpgrades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchasedWeaponUpgrades_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchasedWeaponUpgrades_WeaponUpgrades_WeaponUpgradeId",
                        column: x => x.WeaponUpgradeId,
                        principalTable: "WeaponUpgrades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Potions",
                columns: new[] { "Id", "Description", "DurationInSeconds", "EffectStrength", "ImageUrl", "Name", "Price" },
                values: new object[,]
                {
                    { 1, "Krótki zastrzyk energii do szybszej rozgrywki.", 30, 10, "/images/scissors.png", "Mała potka energii", 120 },
                    { 2, "Mocniejsze uderzenia przez chwilę.", 45, 20, "/images/dagger.png", "Potka furii", 260 },
                    { 3, "Pomaga utrzymać rytm i serię kliknięć.", 60, 30, "/images/katana.png", "Eliksir skupienia", 400 }
                });

            migrationBuilder.InsertData(
                table: "WeaponUpgrades",
                columns: new[] { "Id", "CooldownReduction", "DamageBonus", "Description", "ImageUrl", "Name", "Price", "WeaponId" },
                values: new object[,]
                {
                    { 1, 0.02, 4, "Lepsza krawędź zwiększa obrażenia kuchennego noża.", "/images/knife.png", "Ostrzenie Kitchen Knife", 300, 1 },
                    { 2, 0.050000000000000003, 6, "Lepszy balans skraca czas odnowienia sztyletu.", "/images/dagger.png", "Wyważenie Dagger", 650, 2 },
                    { 3, 0.080000000000000002, 12, "Wzmocnione ostrze zapewnia dodatkową moc katanie.", "/images/katana.png", "Hartowana Katana", 1800, 9 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchasedPotions_PotionId",
                table: "PurchasedPotions",
                column: "PotionId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchasedPotions_UserId_PotionId",
                table: "PurchasedPotions",
                columns: new[] { "UserId", "PotionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchasedWeaponUpgrades_UserId_WeaponUpgradeId",
                table: "PurchasedWeaponUpgrades",
                columns: new[] { "UserId", "WeaponUpgradeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchasedWeaponUpgrades_WeaponUpgradeId",
                table: "PurchasedWeaponUpgrades",
                column: "WeaponUpgradeId");

            migrationBuilder.CreateIndex(
                name: "IX_WeaponUpgrades_WeaponId",
                table: "WeaponUpgrades",
                column: "WeaponId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchasedPotions");

            migrationBuilder.DropTable(
                name: "PurchasedWeaponUpgrades");

            migrationBuilder.DropTable(
                name: "Potions");

            migrationBuilder.DropTable(
                name: "WeaponUpgrades");
        }
    }
}
