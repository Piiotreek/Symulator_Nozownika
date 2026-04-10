using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Symulator_Nozownika.Migrations
{
    /// <inheritdoc />
    public partial class AddFavouriteWeapons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FavoriteWeapons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    WeaponId = table.Column<int>(type: "int", nullable: false),
                    MarkedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteWeapons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavoriteWeapons_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoriteWeapons_Weapons_WeaponId",
                        column: x => x.WeaponId,
                        principalTable: "Weapons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteWeapons_UserId",
                table: "FavoriteWeapons",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteWeapons_WeaponId",
                table: "FavoriteWeapons",
                column: "WeaponId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavoriteWeapons");
        }
    }
}
