using Microsoft.EntityFrameworkCore;
using Symulator_Nozownika.Models;


namespace Symulator_Nozownika.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<Weapon> Weapons { get; set; }
        public DbSet<HighScore> HighScores { get; set; }
        public DbSet<FavoriteWeapon> FavoriteWeapons { get; set; }
        public DbSet<UserStatistics> UserStatistics { get; set; }
        public DbSet<SavedScore> SavedScores { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<CoinWallet> CoinWallets { get; set; }
        public DbSet<PurchasedWeapon> PurchasedWeapons { get; set; }
        public DbSet<Potion> Potions { get; set; }
        public DbSet<PurchasedPotion> PurchasedPotions { get; set; }
        public DbSet<WeaponUpgrade> WeaponUpgrades { get; set; }
        public DbSet<PurchasedWeaponUpgrade> PurchasedWeaponUpgrades { get; set; }
        public DbSet<Club> Clubs { get; set; }
        public DbSet<ClubMember> ClubMembers { get; set; }
        public DbSet<ClubMessage> ClubMessages { get; set; }

        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }

        public DbSet<Quest> Quests { get; set; }
        public DbSet<UserQuestProgress> UserQuestProgresses { get; set; }

        public DbSet<DemoLockout> DemoLockouts { get; set; }

        public DbSet<MessageReport> MessageReports { get; set; }
        public DbSet<UserReport> UserReports { get; set; }
        public DbSet<UserPenalty> UserPenalties { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserAccount>()
            .HasOne(u => u.Club)
            .WithMany() 
            .HasForeignKey(u => u.ClubId)
            .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserAccount>()
            .HasIndex(u => u.UserName)
            .IsUnique();

            modelBuilder.Entity<UserAccount>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Club>()
            .HasIndex(c => c.Name)
            .IsUnique();

            modelBuilder.Entity<Club>()
                .HasOne(c => c.Owner)
                .WithMany()
                .HasForeignKey(c => c.OwnerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ClubMember>()
                .HasOne(cm => cm.Club)
                .WithMany(c => c.Members)
                .HasForeignKey(cm => cm.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClubMember>()
                .HasOne(cm => cm.User)
                .WithMany()
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ClubMessage>()
                .HasOne(cm => cm.Club)
                .WithMany(c => c.Messages)
                .HasForeignKey(cm => cm.ClubId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClubMessage>()
                .HasOne(cm => cm.User)
                .WithMany()
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<HighScore>()
                .HasOne(h => h.UserAccount)
                .WithMany()
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserStatistics>()
                .HasOne(us => us.User)
                .WithOne(u => u.Statistics)
                .HasForeignKey<UserStatistics>(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserStatistics>()
                .HasIndex(us => us.UserId)
                .IsUnique();

            modelBuilder.Entity<Level>()
                .HasOne(l => l.User)
                .WithOne(u => u.Level)
                .HasForeignKey<Level>(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Level>()
                .HasIndex(l => l.UserId)
                .IsUnique();

            modelBuilder.Entity<CoinWallet>()
                .HasOne(cw => cw.User)
                .WithOne(u => u.CoinWallet)
                .HasForeignKey<CoinWallet>(cw => cw.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CoinWallet>()
                .HasIndex(cw => cw.UserId)
                .IsUnique();

            modelBuilder.Entity<PurchasedWeapon>()
                .HasOne(pw => pw.User)
                .WithMany(u => u.PurchasedWeapons)
                .HasForeignKey(pw => pw.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchasedWeapon>()
                .HasOne(pw => pw.Weapon)
                .WithMany()
                .HasForeignKey(pw => pw.WeaponId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchasedWeapon>()
                .HasIndex(pw => new { pw.UserId, pw.WeaponId })
                .IsUnique();

            modelBuilder.Entity<PurchasedPotion>()
                .HasOne(pp => pp.User)
                .WithMany(u => u.PurchasedPotions)
                .HasForeignKey(pp => pp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchasedPotion>()
                .HasOne(pp => pp.Potion)
                .WithMany()
                .HasForeignKey(pp => pp.PotionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchasedPotion>()
                .HasIndex(pp => new { pp.UserId, pp.PotionId })
                .IsUnique();

            modelBuilder.Entity<WeaponUpgrade>()
                .HasOne(wu => wu.Weapon)
                .WithMany()
                .HasForeignKey(wu => wu.WeaponId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchasedWeaponUpgrade>()
                .HasOne(pwu => pwu.User)
                .WithMany(u => u.PurchasedWeaponUpgrades)
                .HasForeignKey(pwu => pwu.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchasedWeaponUpgrade>()
                .HasOne(pwu => pwu.WeaponUpgrade)
                .WithMany()
                .HasForeignKey(pwu => pwu.WeaponUpgradeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchasedWeaponUpgrade>()
                .HasIndex(pwu => new { pwu.UserId, pwu.WeaponUpgradeId })
                .IsUnique();

            modelBuilder.Entity<SavedScore>()
                .HasOne(ss => ss.User)
                .WithMany()
                .HasForeignKey(ss => ss.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserQuestProgress>()
                .HasOne(uqp => uqp.User)
                .WithMany()
                .HasForeignKey(uqp => uqp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserQuestProgress>()
                .HasOne(uqp => uqp.Quest)
                .WithMany()
                .HasForeignKey(uqp => uqp.QuestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserQuestProgress>()
                .HasIndex(uqp => new { uqp.UserId, uqp.QuestId })
                .IsUnique();

            modelBuilder.Entity<Quest>().HasData(
                new Quest
                {
                    Id = 1,
                    Name = "Nożownik Dnia",
                    Description = "Kliknij 50 razy w trakcie dzisiejszej sesji.",
                    QuestType = QuestType.Clicks,
                    TargetValue = 50,
                    RewardCoins = 10,
                    RewardXp = 25,
                    ResetsAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc)
                },
                new Quest
                {
                    Id = 2,
                    Name = "Szybkie Paluszki",
                    Description = "Kliknij 150 razy w trakcie dzisiejszej sesji.",
                    QuestType = QuestType.Clicks,
                    TargetValue = 150,
                    RewardCoins = 25,
                    RewardXp = 60,
                    ResetsAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc)
                },
                new Quest
                {
                    Id = 3,
                    Name = "British Special",
                    Description = "Kliknij 300 razy w trakcie dzisiejszej sesji.",
                    QuestType = QuestType.Clicks,
                    TargetValue = 300,
                    RewardCoins = 50,
                    RewardXp = 120,
                    ResetsAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc)
                },
                new Quest
                {
                    Id = 4,
                    Name = "Nożyce Jak Brzytwa",
                    Description = "Zdobądź 500 punktów w ciągu dnia.",
                    QuestType = QuestType.Score,
                    TargetValue = 500,
                    RewardCoins = 15,
                    RewardXp = 40,
                    ResetsAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc)
                },
                new Quest
                {
                    Id = 5,
                    Name = "Daj mu jeszcze jeden w serce!",
                    Description = "Zdobądź 1500 punktów w ciągu dnia.",
                    QuestType = QuestType.Score,
                    TargetValue = 1500,
                    RewardCoins = 35,
                    RewardXp = 90,
                    ResetsAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc)
                },
                new Quest
                {
                    Id = 6,
                    Name = "Prawdziwy Londyńczyk",
                    Description = "Zdobądź 3000 punktów w ciągu dnia.",
                    QuestType = QuestType.Score,
                    TargetValue = 3000,
                    RewardCoins = 75,
                    RewardXp = 200,
                    ResetsAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc)
                });

            modelBuilder.Entity<Weapon>().HasData(
                new Weapon { Id = 1, Name = "Kitchen Knife", Damage = 10, Cooldown = 0.4, ImageUrl = "/images/knife.png" },
                new Weapon { Id = 2, Name = "Dagger", Damage = 25, Cooldown = 0.6, ImageUrl = "/images/dagger.png" },
                new Weapon { Id = 3, Name = "Machete", Damage = 45, Cooldown = 0.9, ImageUrl = "/images/machete.png" },
                new Weapon { Id = 4, Name = "Sword", Damage = 55, Cooldown = 1.1, ImageUrl = "/images/sword.png" },
                new Weapon { Id = 5, Name = "Axe", Damage = 70, Cooldown = 1.5, ImageUrl = "/images/axe.png" },
                new Weapon { Id = 6, Name = "Spear", Damage = 40, Cooldown = 0.7, ImageUrl = "/images/spear.png" },
                new Weapon { Id = 7, Name = "Cleaver", Damage = 60, Cooldown = 1.3, ImageUrl = "/images/cleaver.png" },
                new Weapon { Id = 8, Name = "Mace", Damage = 90, Cooldown = 2.0, ImageUrl = "/images/mace.png" },
                new Weapon { Id = 9, Name = "Katana", Damage = 50, Cooldown = 0.5, ImageUrl = "/images/katana.png" },
                new Weapon { Id = 10, Name = "Scissors", Damage = 2, Cooldown = 0.1, ImageUrl = "/images/scissors.png" }
            );

            modelBuilder.Entity<Potion>().HasData(
                new Potion
                {
                    Id = 1,
                    Name = "Mała potka energii",
                    Description = "Krótki zastrzyk energii do szybszej rozgrywki.",
                    Price = 120,
                    EffectStrength = 10,
                    DurationInSeconds = 30,
                    ImageUrl = "/images/scissors.png"
                },
                new Potion
                {
                    Id = 2,
                    Name = "Potka furii",
                    Description = "Mocniejsze uderzenia przez chwilę.",
                    Price = 260,
                    EffectStrength = 20,
                    DurationInSeconds = 45,
                    ImageUrl = "/images/dagger.png"
                },
                new Potion
                {
                    Id = 3,
                    Name = "Eliksir skupienia",
                    Description = "Pomaga utrzymać rytm i serię kliknięć.",
                    Price = 400,
                    EffectStrength = 30,
                    DurationInSeconds = 60,
                    ImageUrl = "/images/katana.png"
                }
            );

            modelBuilder.Entity<WeaponUpgrade>().HasData(
                new WeaponUpgrade
                {
                    Id = 1,
                    Name = "Ostrzenie Kitchen Knife",
                    Description = "Lepsza krawędź zwiększa obrażenia kuchennego noża.",
                    WeaponId = 1,
                    Price = 300,
                    DamageBonus = 4,
                    CooldownReduction = 0.02,
                    ImageUrl = "/images/knife.png"
                },
                new WeaponUpgrade
                {
                    Id = 2,
                    Name = "Wyważenie Dagger",
                    Description = "Lepszy balans skraca czas odnowienia sztyletu.",
                    WeaponId = 2,
                    Price = 650,
                    DamageBonus = 6,
                    CooldownReduction = 0.05,
                    ImageUrl = "/images/dagger.png"
                },
                new WeaponUpgrade
                {
                    Id = 3,
                    Name = "Hartowana Katana",
                    Description = "Wzmocnione ostrze zapewnia dodatkową moc katanie.",
                    WeaponId = 9,
                    Price = 1800,
                    DamageBonus = 12,
                    CooldownReduction = 0.08,
                    ImageUrl = "/images/katana.png"
                }
            );

            //Seed for Achievements and UserAchievements
            modelBuilder.Entity<UserAchievement>()
                .HasIndex(ua => new { ua.UserAccountId, ua.AchievementId })
                .IsUnique();


            modelBuilder.Entity<Achievement>().HasData(
                new Achievement
                {
                    Id = 1,
                    Name = "Nowicjusz",
                    Description = "Zdobądź łącznie 2000 punktów.",
                    ImagePath = "/images/achiv/2000.png",
                    Type = AchievementType.TotalScore,
                    TargetValue = 2000
                },
                new Achievement
                {
                    Id = 2,
                    Name = "Doświadczony",
                    Description = "Zdobądź łącznie 5000 punktów.",
                    ImagePath = "/images/achiv/5000.png",
                    Type = AchievementType.TotalScore,
                    TargetValue = 5000
                },
                new Achievement
                {
                    Id = 3,
                    Name = "Weteran",
                    Description = "Zdobądź łącznie 10000 punktów.",
                    ImagePath = "/images/achiv/10000.png",
                    Type = AchievementType.TotalScore,
                    TargetValue = 10000
                },
                new Achievement
                {
                    Id = 4,
                    Name = "Pierwsza krew",
                    Description = "Zagraj w swoją pierwszą grę.",
                    ImagePath = "/images/achiv/first-game.png",
                    Type = AchievementType.FirstGame,
                    TargetValue = 1
                },
                new Achievement
                {
                    Id = 5,
                    Name = "Szybkie palce",
                    Description = "Kliknij 100 razy w trakcie jednej gry.",
                    ImagePath = "/images/achiv/100-clicks-in-one-game.png",
                    Type = AchievementType.SingleGameClicks,
                    TargetValue = 100
                },
                new Achievement
                {
                    Id = 6,
                    Name = "Klikacz",
                    Description = "Zdobądź łącznie 300 kliknięć we wszystkich grach.",
                    ImagePath = "/images/achiv/300-clicks.png",
                    Type = AchievementType.TotalClicks,
                    TargetValue = 300
                },
                new Achievement
                {
                    Id = 7,
                    Name = "Wprawiony Klikacz",
                    Description = "Zdobądź łącznie 500 kliknięć we wszystkich grach.",
                    ImagePath = "/images/achiv/500-clicks.png", 
                    Type = AchievementType.TotalClicks,
                    TargetValue = 500
                },
                new Achievement
                {
                    Id = 8,
                    Name = "Maniak",
                    Description = "Zdobądź łącznie 1000 kliknięć we wszystkich grach.",
                    ImagePath = "/images/achiv/1000-clicks.png",
                    Type = AchievementType.TotalClicks,
                    TargetValue = 1000
                },
                new Achievement
                {
                    Id = 9,
                    Name = "3000 GWIAZD!",
                    Description = "Zdobądź łącznie 3000 kliknięć we wszystkich grach.",
                    ImagePath = "/images/achiv/3000-clicks.png",
                    Type = AchievementType.TotalClicks,
                    TargetValue = 3000
                });

            // Seed a debug user account with wallet and level
            modelBuilder.Entity<UserAccount>().HasData(
                new UserAccount
                {
                    Id = 9999,
                    FirstName = "Debug",
                    LastName = "Account",
                    Email = "debug@example.local",
                    Country = "Poland",
                    UserName = "debug",
                    Password = "debugdebug1",
                    CreatedAt = new DateTime(2025, 1, 1)
                }
            );
            
            modelBuilder.Entity<UserStatistics>().HasData(
                new UserStatistics
                {
                    Id = 9999,
                    UserId = 9999,
                    TotalGamesPlayed = 0,
                    TotalScore = 5500,
                    TotalClicks = 0,
                    HighestScore = 0,
                    TotalPlayTime = TimeSpan.Zero,
                    CurrentStreak = 0,
                    LongestStreak = 0,
                    LastPlayedAt = new DateTime(2025, 1, 1)
                }
            );
            
            modelBuilder.Entity<Level>().HasData(
                new Level
                {
                    Id = 9999,
                    UserId = 9999,
                    CurrentLevel = 10,
                    CurrentLevelThreshold = 5040,
                    NextLevelThreshold = 6000,
                    TotalScoreSnapshot = 5500,
                    UpdatedAt = new DateTime(2025, 1, 1)
                }
            );
            
            modelBuilder.Entity<CoinWallet>().HasData(
                new CoinWallet
                {
                    Id = 9999,
                    UserId = 9999,
                    Balance = 9999,
                    UpdatedAt = new DateTime(2025, 1, 1)
                }
            );

            // Configuration for MessageReport
            modelBuilder.Entity<MessageReport>()
                .HasOne(mr => mr.ReportedMessage)
                .WithMany()
                .HasForeignKey(mr => mr.ReportedMessageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MessageReport>()
                .HasOne(mr => mr.ReportedByUser)
                .WithMany()
                .HasForeignKey(mr => mr.ReportedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MessageReport>()
                .HasOne(mr => mr.ReviewedByAdmin)
                .WithMany()
                .HasForeignKey(mr => mr.ReviewedByAdminId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configuration for UserReport
            modelBuilder.Entity<UserReport>()
                .HasOne(ur => ur.ReportedUser)
                .WithMany()
                .HasForeignKey(ur => ur.ReportedUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserReport>()
                .HasOne(ur => ur.ReportedByUser)
                .WithMany()
                .HasForeignKey(ur => ur.ReportedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserReport>()
                .HasOne(ur => ur.ReviewedByAdmin)
                .WithMany()
                .HasForeignKey(ur => ur.ReviewedByAdminId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configuration for UserPenalty
            modelBuilder.Entity<UserPenalty>()
                .HasOne(up => up.User)
                .WithMany()
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserPenalty>()
                .HasOne(up => up.Admin)
                .WithMany()
                .HasForeignKey(up => up.AdminId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserPenalty>()
                .HasOne(up => up.RelatedReport)
                .WithMany()
                .HasForeignKey(up => up.RelatedReportId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}