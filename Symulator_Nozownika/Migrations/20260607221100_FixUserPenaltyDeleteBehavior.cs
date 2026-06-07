using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Symulator_Nozownika.Migrations
{
    /// <inheritdoc />
    public partial class FixUserPenaltyDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPenalties_UserReports_RelatedReportId",
                table: "UserPenalties");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPenalties_UserReports_RelatedReportId",
                table: "UserPenalties",
                column: "RelatedReportId",
                principalTable: "UserReports",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPenalties_UserReports_RelatedReportId",
                table: "UserPenalties");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPenalties_UserReports_RelatedReportId",
                table: "UserPenalties",
                column: "RelatedReportId",
                principalTable: "UserReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
