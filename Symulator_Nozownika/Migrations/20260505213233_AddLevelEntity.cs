using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Symulator_Nozownika.Migrations
{
    /// <inheritdoc />
    public partial class AddLevelEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Levels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CurrentLevel = table.Column<int>(type: "int", nullable: false),
                    CurrentLevelThreshold = table.Column<int>(type: "int", nullable: false),
                    NextLevelThreshold = table.Column<int>(type: "int", nullable: false),
                    TotalScoreSnapshot = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Levels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Levels_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Levels_UserId",
                table: "Levels",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Levels");
        }
    }
}
