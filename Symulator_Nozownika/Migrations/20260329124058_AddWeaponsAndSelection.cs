using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Symulator_Nozownika.Migrations
{
    /// <inheritdoc />
    public partial class AddWeaponsAndSelection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SelectedWeaponId",
                table: "UserAccounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Weapons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Damage = table.Column<int>(type: "int", nullable: false),
                    Cooldown = table.Column<double>(type: "float", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Weapons", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Weapons",
                columns: new[] { "Id", "Cooldown", "Damage", "ImageUrl", "Name" },
                values: new object[,]
                {
                    { 1, 0.40000000000000002, 10, "/images/knife.png", "Kitchen Knife" },
                    { 2, 0.59999999999999998, 25, "/images/dagger.png", "Dagger" },
                    { 3, 0.90000000000000002, 45, "/images/machete.png", "Machete" },
                    { 4, 1.1000000000000001, 55, "/images/sword.png", "Sword" },
                    { 5, 1.5, 70, "/images/axe.png", "Axe" },
                    { 6, 0.69999999999999996, 40, "/images/spear.png", "Spear" },
                    { 7, 1.3, 60, "/images/cleaver.png", "Cleaver" },
                    { 8, 2.0, 90, "/images/mace.png", "Mace" },
                    { 9, 0.5, 50, "/images/katana.png", "Katana" },
                    { 10, 0.10000000000000001, 2, "/images/scissors.png", "Scissors" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Weapons");

            migrationBuilder.DropColumn(
                name: "SelectedWeaponId",
                table: "UserAccounts");
        }
    }
}
