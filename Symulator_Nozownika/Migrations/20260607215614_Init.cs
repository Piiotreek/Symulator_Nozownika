using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Symulator_Nozownika.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    TargetValue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DemoLockouts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemoLockouts", x => x.Id);
                });

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
                name: "Quests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    QuestType = table.Column<int>(type: "int", nullable: false),
                    TargetValue = table.Column<int>(type: "int", nullable: false),
                    RewardCoins = table.Column<int>(type: "int", nullable: false),
                    RewardXp = table.Column<int>(type: "int", nullable: false),
                    ResetsAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quests", x => x.Id);
                });

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
                name: "ClubMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClubId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClubMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClubId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clubs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    OwnerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaxMembers = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clubs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDemo = table.Column<bool>(type: "bit", nullable: false),
                    DemoGamesPlayed = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    SelectedWeaponId = table.Column<int>(type: "int", nullable: true),
                    ClubId = table.Column<int>(type: "int", nullable: true),
                    AvatarPath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAccounts_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserAccounts_Weapons_SelectedWeaponId",
                        column: x => x.SelectedWeaponId,
                        principalTable: "Weapons",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CoinWallets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Balance = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoinWallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoinWallets_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "HighScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlayerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CountryFlag = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HighScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HighScores_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "MessageReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportedMessageId = table.Column<int>(type: "int", nullable: false),
                    ReportedByUserId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReviewedByAdminId = table.Column<int>(type: "int", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdminNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageReports_ClubMessages_ReportedMessageId",
                        column: x => x.ReportedMessageId,
                        principalTable: "ClubMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageReports_UserAccounts_ReportedByUserId",
                        column: x => x.ReportedByUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MessageReports_UserAccounts_ReviewedByAdminId",
                        column: x => x.ReviewedByAdminId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                name: "PurchasedWeapons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    WeaponId = table.Column<int>(type: "int", nullable: false),
                    PricePaid = table.Column<int>(type: "int", nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchasedWeapons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchasedWeapons_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchasedWeapons_Weapons_WeaponId",
                        column: x => x.WeaponId,
                        principalTable: "Weapons",
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

            migrationBuilder.CreateTable(
                name: "SavedScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    AchievedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SavedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedScores_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAchievements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserAccountId = table.Column<int>(type: "int", nullable: false),
                    AchievementId = table.Column<int>(type: "int", nullable: false),
                    UnlockedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAchievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAchievements_Achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "Achievements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAchievements_UserAccounts_UserAccountId",
                        column: x => x.UserAccountId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserQuestProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    QuestId = table.Column<int>(type: "int", nullable: false),
                    CurrentValue = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsRewardClaimed = table.Column<bool>(type: "bit", nullable: false),
                    ProgressDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuestProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserQuestProgresses_Quests_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserQuestProgresses_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportedUserId = table.Column<int>(type: "int", nullable: false),
                    ReportedByUserId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReviewedByAdminId = table.Column<int>(type: "int", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdminNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserReports_UserAccounts_ReportedByUserId",
                        column: x => x.ReportedByUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserReports_UserAccounts_ReportedUserId",
                        column: x => x.ReportedUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserReports_UserAccounts_ReviewedByAdminId",
                        column: x => x.ReviewedByAdminId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TotalGamesPlayed = table.Column<int>(type: "int", nullable: false),
                    TotalScore = table.Column<int>(type: "int", nullable: false),
                    TotalClicks = table.Column<int>(type: "int", nullable: false),
                    HighestScore = table.Column<int>(type: "int", nullable: false),
                    TotalPlayTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    CurrentStreak = table.Column<int>(type: "int", nullable: false),
                    LongestStreak = table.Column<int>(type: "int", nullable: false),
                    LastPlayedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserStatistics_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPenalties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AdminId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RelatedReportId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPenalties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPenalties_UserAccounts_AdminId",
                        column: x => x.AdminId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserPenalties_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPenalties_UserReports_RelatedReportId",
                        column: x => x.RelatedReportId,
                        principalTable: "UserReports",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Achievements",
                columns: new[] { "Id", "Description", "ImagePath", "Name", "TargetValue", "Type" },
                values: new object[,]
                {
                    { 1, "Zdobądź łącznie 2000 punktów.", "/images/achiv/2000.png", "Nowicjusz", 2000, 2 },
                    { 2, "Zdobądź łącznie 5000 punktów.", "/images/achiv/5000.png", "Doświadczony", 5000, 2 },
                    { 3, "Zdobądź łącznie 10000 punktów.", "/images/achiv/10000.png", "Weteran", 10000, 2 },
                    { 4, "Zagraj w swoją pierwszą grę.", "/images/achiv/first-game.png", "Pierwsza krew", 1, 0 },
                    { 5, "Kliknij 100 razy w trakcie jednej gry.", "/images/achiv/100-clicks-in-one-game.png", "Szybkie palce", 100, 4 },
                    { 6, "Zdobądź łącznie 300 kliknięć we wszystkich grach.", "/images/achiv/300-clicks.png", "Klikacz", 300, 1 },
                    { 7, "Zdobądź łącznie 500 kliknięć we wszystkich grach.", "/images/achiv/500-clicks.png", "Wprawiony Klikacz", 500, 1 },
                    { 8, "Zdobądź łącznie 1000 kliknięć we wszystkich grach.", "/images/achiv/1000-clicks.png", "Maniak", 1000, 1 },
                    { 9, "Zdobądź łącznie 3000 kliknięć we wszystkich grach.", "/images/achiv/3000-clicks.png", "3000 GWIAZD!", 3000, 1 }
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
                table: "Quests",
                columns: new[] { "Id", "Description", "Name", "QuestType", "ResetsAt", "RewardCoins", "RewardXp", "TargetValue" },
                values: new object[,]
                {
                    { 1, "Kliknij 50 razy w trakcie dzisiejszej sesji.", "Nożownik Dnia", 0, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), 10, 25, 50 },
                    { 2, "Kliknij 150 razy w trakcie dzisiejszej sesji.", "Szybkie Paluszki", 0, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), 25, 60, 150 },
                    { 3, "Kliknij 300 razy w trakcie dzisiejszej sesji.", "British Special", 0, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), 50, 120, 300 },
                    { 4, "Zdobądź 500 punktów w ciągu dnia.", "Nożyce Jak Brzytwa", 1, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), 15, 40, 500 },
                    { 5, "Zdobądź 1500 punktów w ciągu dnia.", "Daj mu jeszcze jeden w serce!", 1, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), 35, 90, 1500 },
                    { 6, "Zdobądź 3000 punktów w ciągu dnia.", "Prawdziwy Londyńczyk", 1, new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), 75, 200, 3000 }
                });

            migrationBuilder.InsertData(
                table: "UserAccounts",
                columns: new[] { "Id", "AvatarPath", "ClubId", "Country", "CreatedAt", "DemoGamesPlayed", "Email", "FirstName", "IsDemo", "LastName", "Password", "Role", "SelectedWeaponId", "UserName" },
                values: new object[] { 9999, null, null, "Poland", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, "debug@example.local", "Debug", false, "Account", "debugdebug1", 0, null, "debug" });

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

            migrationBuilder.InsertData(
                table: "CoinWallets",
                columns: new[] { "Id", "Balance", "UpdatedAt", "UserId" },
                values: new object[] { 9999, 9999, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 9999 });

            migrationBuilder.InsertData(
                table: "Levels",
                columns: new[] { "Id", "CurrentLevel", "CurrentLevelThreshold", "NextLevelThreshold", "TotalScoreSnapshot", "UpdatedAt", "UserId" },
                values: new object[] { 9999, 10, 5040, 6000, 5500, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 9999 });

            migrationBuilder.InsertData(
                table: "UserStatistics",
                columns: new[] { "Id", "CurrentStreak", "HighestScore", "LastPlayedAt", "LongestStreak", "TotalClicks", "TotalGamesPlayed", "TotalPlayTime", "TotalScore", "UserId" },
                values: new object[] { 9999, 0, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, 0, 0, new TimeSpan(0, 0, 0, 0, 0), 5500, 9999 });

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
                name: "IX_ClubMembers_ClubId",
                table: "ClubMembers",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubMembers_UserId",
                table: "ClubMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubMessages_ClubId",
                table: "ClubMessages",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubMessages_UserId",
                table: "ClubMessages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Clubs_Name",
                table: "Clubs",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clubs_OwnerId",
                table: "Clubs",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_CoinWallets_UserId",
                table: "CoinWallets",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteWeapons_UserId",
                table: "FavoriteWeapons",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteWeapons_WeaponId",
                table: "FavoriteWeapons",
                column: "WeaponId");

            migrationBuilder.CreateIndex(
                name: "IX_HighScores_UserId",
                table: "HighScores",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Levels_UserId",
                table: "Levels",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageReports_ReportedByUserId",
                table: "MessageReports",
                column: "ReportedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReports_ReportedMessageId",
                table: "MessageReports",
                column: "ReportedMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReports_ReviewedByAdminId",
                table: "MessageReports",
                column: "ReviewedByAdminId");

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
                name: "IX_PurchasedWeapons_UserId_WeaponId",
                table: "PurchasedWeapons",
                columns: new[] { "UserId", "WeaponId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchasedWeapons_WeaponId",
                table: "PurchasedWeapons",
                column: "WeaponId");

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
                name: "IX_SavedScores_UserId",
                table: "SavedScores",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_ClubId",
                table: "UserAccounts",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_Email",
                table: "UserAccounts",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_SelectedWeaponId",
                table: "UserAccounts",
                column: "SelectedWeaponId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_UserName",
                table: "UserAccounts",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_AchievementId",
                table: "UserAchievements",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_UserAccountId_AchievementId",
                table: "UserAchievements",
                columns: new[] { "UserAccountId", "AchievementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPenalties_AdminId",
                table: "UserPenalties",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPenalties_RelatedReportId",
                table: "UserPenalties",
                column: "RelatedReportId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPenalties_UserId",
                table: "UserPenalties",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuestProgresses_QuestId",
                table: "UserQuestProgresses",
                column: "QuestId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuestProgresses_UserId_QuestId",
                table: "UserQuestProgresses",
                columns: new[] { "UserId", "QuestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_ReportedByUserId",
                table: "UserReports",
                column: "ReportedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_ReportedUserId",
                table: "UserReports",
                column: "ReportedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_ReviewedByAdminId",
                table: "UserReports",
                column: "ReviewedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStatistics_UserId",
                table: "UserStatistics",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeaponUpgrades_WeaponId",
                table: "WeaponUpgrades",
                column: "WeaponId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClubMembers_Clubs_ClubId",
                table: "ClubMembers",
                column: "ClubId",
                principalTable: "Clubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClubMembers_UserAccounts_UserId",
                table: "ClubMembers",
                column: "UserId",
                principalTable: "UserAccounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClubMessages_Clubs_ClubId",
                table: "ClubMessages",
                column: "ClubId",
                principalTable: "Clubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClubMessages_UserAccounts_UserId",
                table: "ClubMessages",
                column: "UserId",
                principalTable: "UserAccounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Clubs_UserAccounts_OwnerId",
                table: "Clubs",
                column: "OwnerId",
                principalTable: "UserAccounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAccounts_Clubs_ClubId",
                table: "UserAccounts");

            migrationBuilder.DropTable(
                name: "ClubMembers");

            migrationBuilder.DropTable(
                name: "CoinWallets");

            migrationBuilder.DropTable(
                name: "DemoLockouts");

            migrationBuilder.DropTable(
                name: "FavoriteWeapons");

            migrationBuilder.DropTable(
                name: "HighScores");

            migrationBuilder.DropTable(
                name: "Levels");

            migrationBuilder.DropTable(
                name: "MessageReports");

            migrationBuilder.DropTable(
                name: "PurchasedPotions");

            migrationBuilder.DropTable(
                name: "PurchasedWeapons");

            migrationBuilder.DropTable(
                name: "PurchasedWeaponUpgrades");

            migrationBuilder.DropTable(
                name: "SavedScores");

            migrationBuilder.DropTable(
                name: "UserAchievements");

            migrationBuilder.DropTable(
                name: "UserPenalties");

            migrationBuilder.DropTable(
                name: "UserQuestProgresses");

            migrationBuilder.DropTable(
                name: "UserStatistics");

            migrationBuilder.DropTable(
                name: "ClubMessages");

            migrationBuilder.DropTable(
                name: "Potions");

            migrationBuilder.DropTable(
                name: "WeaponUpgrades");

            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropTable(
                name: "UserReports");

            migrationBuilder.DropTable(
                name: "Quests");

            migrationBuilder.DropTable(
                name: "Clubs");

            migrationBuilder.DropTable(
                name: "UserAccounts");

            migrationBuilder.DropTable(
                name: "Weapons");
        }
    }
}
