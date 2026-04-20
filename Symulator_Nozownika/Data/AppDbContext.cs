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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            modelBuilder.Entity<SavedScore>()
                .HasOne(ss => ss.User)
                .WithMany()
                .HasForeignKey(ss => ss.UserId)
                .OnDelete(DeleteBehavior.Cascade);

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
        }
    }
}