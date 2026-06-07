using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Symulator_Nozownika.Migrations
{
    /// <inheritdoc />
    public partial class FixUserReportDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageReports_UserAccounts_ReviewedByAdminId",
                table: "MessageReports");

            migrationBuilder.DropForeignKey(
                name: "FK_UserReports_UserAccounts_ReviewedByAdminId",
                table: "UserReports");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReports_UserAccounts_ReviewedByAdminId",
                table: "MessageReports",
                column: "ReviewedByAdminId",
                principalTable: "UserAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserReports_UserAccounts_ReviewedByAdminId",
                table: "UserReports",
                column: "ReviewedByAdminId",
                principalTable: "UserAccounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageReports_UserAccounts_ReviewedByAdminId",
                table: "MessageReports");

            migrationBuilder.DropForeignKey(
                name: "FK_UserReports_UserAccounts_ReviewedByAdminId",
                table: "UserReports");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReports_UserAccounts_ReviewedByAdminId",
                table: "MessageReports",
                column: "ReviewedByAdminId",
                principalTable: "UserAccounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserReports_UserAccounts_ReviewedByAdminId",
                table: "UserReports",
                column: "ReviewedByAdminId",
                principalTable: "UserAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
