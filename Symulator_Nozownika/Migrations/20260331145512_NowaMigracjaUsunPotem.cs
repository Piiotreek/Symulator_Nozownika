using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Symulator_Nozownika.Migrations
{
    /// <inheritdoc />
    public partial class NowaMigracjaUsunPotem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_SelectedWeaponId",
                table: "UserAccounts",
                column: "SelectedWeaponId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAccounts_Weapons_SelectedWeaponId",
                table: "UserAccounts",
                column: "SelectedWeaponId",
                principalTable: "Weapons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAccounts_Weapons_SelectedWeaponId",
                table: "UserAccounts");

            migrationBuilder.DropIndex(
                name: "IX_UserAccounts_SelectedWeaponId",
                table: "UserAccounts");
        }
    }
}
