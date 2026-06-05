using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Symulator_Nozownika.Data;
using Symulator_Nozownika.Services;
using Symulator_Nozownika.Models;

namespace Symulator_Nozownika
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            // Register Razor Pages so pages using @page and asp-page work (Admin pages)
            builder.Services.AddRazorPages();
            //building authentication services using cookie authentication scheme
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            //building dependency injection for achievement service
            builder.Services.AddScoped<IAchievementService, AchievementService>();
            builder.Services.AddSingleton<ILevelService, LevelService>();
            builder.Services.AddScoped<IQuestService, QuestService>();
            //building dependency injection for club services
            builder.Services.AddScoped<IClubService, ClubService>();
            // Register browser service as singleton to prevent concurrent download/file-lock errors
            builder.Services.AddSingleton<IBrowserService, BrowserService>();
            // Register report service
            builder.Services.AddScoped<IReportService, ReportService>();
            // Register demo service
            builder.Services.AddScoped<IDemoService, DemoService>();
            // Register report management service
            builder.Services.AddScoped<IReportManagementService, ReportManagementService>();

            // Register penalty expiration service
            builder.Services.AddScoped<IPenaltyExpirationService, PenaltyExpirationService>();
            builder.Services.AddHostedService<PenaltyExpirationBackgroundService>();

            // Add Authorization with custom policies
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IAuthorizationHandler, CsvExportAuthorizationHandler>();
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("CanExportCsv", policy =>
                    policy.AddRequirements(new CsvExportRequirement()));

                options.AddPolicy("CanExportClubCsv", policy =>
                    policy.AddRequirements(new CsvExportRequirement()));

                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Apply migrations to ensure schema exists (safe for dev and test).
                try
                {
                    dbContext.Database.Migrate();
                }
                catch
                {
                    // If migrations fail, continue — seeding may still fail but we don't want to crash the app here.
                }

                // Ensure debug account exists (create or update). Run in all environments so dev login works.
                try
                {
                    using var tx = dbContext.Database.BeginTransaction();

                    var debugUser = dbContext.UserAccounts
                        .Include(u => u.CoinWallet)
                        .Include(u => u.Level)
                        .Include(u => u.Statistics)
                        .FirstOrDefault(u => u.UserName == "debug");

                    if (debugUser == null)
                    {
                        debugUser = new UserAccount
                        {
                            FirstName = "Debug",
                            LastName = "Account",
                            Email = "debug@example.local",
                            Country = "Poland",
                            UserName = "debug",
                            Password = "debugdebug1",
                            CreatedAt = DateTime.UtcNow
                        };

                        dbContext.UserAccounts.Add(debugUser);
                        dbContext.SaveChanges();
                    }
                    else
                    {
                        // Ensure password is known value for debugging
                        debugUser.Password = "debugdebug1";
                        debugUser.Email = debugUser.Email ?? "debug@example.local";
                        dbContext.UserAccounts.Update(debugUser);
                        dbContext.SaveChanges();
                    }

                    // Ensure CoinWallet
                    var wallet = dbContext.CoinWallets.FirstOrDefault(c => c.UserId == debugUser.Id);
                    if (wallet == null)
                    {
                        wallet = new CoinWallet
                        {
                            UserId = debugUser.Id,
                            Balance = 9999,
                            UpdatedAt = DateTime.UtcNow
                        };
                        dbContext.CoinWallets.Add(wallet);
                    }
                    else
                    {
                        wallet.Balance = 9999;
                        wallet.UpdatedAt = DateTime.UtcNow;
                        dbContext.CoinWallets.Update(wallet);
                    }

                    // Ensure Statistics
                    var stats = dbContext.UserStatistics.FirstOrDefault(s => s.UserId == debugUser.Id);
                    if (stats == null)
                    {
                        stats = new UserStatistics
                        {
                            UserId = debugUser.Id,
                            TotalGamesPlayed = 0,
                            TotalScore = 5500,
                            TotalClicks = 0,
                            HighestScore = 0,
                            TotalPlayTime = TimeSpan.Zero,
                            CurrentStreak = 0,
                            LongestStreak = 0,
                            LastPlayedAt = DateTime.UtcNow
                        };
                        dbContext.UserStatistics.Add(stats);
                    }
                    else
                    {
                        stats.TotalScore = 5500;
                        dbContext.UserStatistics.Update(stats);
                    }

                    // Ensure Level
                    var level = dbContext.Levels.FirstOrDefault(l => l.UserId == debugUser.Id);
                    var levelService = scope.ServiceProvider.GetRequiredService<ILevelService>();

                    if (level == null)
                    {
                        level = new Level
                        {
                            UserId = debugUser.Id,
                        };
                        levelService.SyncLevel(level, stats.TotalScore);
                        dbContext.Levels.Add(level);
                    }
                    else
                    {
                        levelService.SyncLevel(level, stats.TotalScore);
                        dbContext.Levels.Update(level);
                    }

                    // Ensure admin account exists (create or update)
                    var adminUser = dbContext.UserAccounts
                        .Include(u => u.CoinWallet)
                        .Include(u => u.Level)
                        .Include(u => u.Statistics)
                        .FirstOrDefault(u => u.UserName == "admin");

                    if (adminUser == null)
                    {
                        adminUser = new UserAccount
                        {
                            FirstName = "Administrator",
                            LastName = "Account",
                            Email = "admin@example.local",
                            Country = "Poland",
                            UserName = "admin",
                            Password = "admin123456",
                            Role = UserRole.Admin,
                            CreatedAt = DateTime.UtcNow
                        };

                        dbContext.UserAccounts.Add(adminUser);
                        dbContext.SaveChanges();
                    }
                    else
                    {
                        // Ensure admin role and password are set correctly
                        adminUser.Role = UserRole.Admin;
                        adminUser.Password = "admin123456";
                        adminUser.Email = adminUser.Email ?? "admin@example.local";
                        dbContext.UserAccounts.Update(adminUser);
                        dbContext.SaveChanges();
                    }

                    // Ensure admin CoinWallet
                    var adminWallet = dbContext.CoinWallets.FirstOrDefault(c => c.UserId == adminUser.Id);
                    if (adminWallet == null)
                    {
                        adminWallet = new CoinWallet
                        {
                            UserId = adminUser.Id,
                            Balance = 9999,
                            UpdatedAt = DateTime.UtcNow
                        };
                        dbContext.CoinWallets.Add(adminWallet);
                    }
                    else
                    {
                        adminWallet.Balance = 9999;
                        adminWallet.UpdatedAt = DateTime.UtcNow;
                        dbContext.CoinWallets.Update(adminWallet);
                    }

                    // Ensure admin Statistics
                    var adminStats = dbContext.UserStatistics.FirstOrDefault(s => s.UserId == adminUser.Id);
                    if (adminStats == null)
                    {
                        adminStats = new UserStatistics
                        {
                            UserId = adminUser.Id,
                            TotalGamesPlayed = 0,
                            TotalScore = 5500,
                            TotalClicks = 0,
                            HighestScore = 0,
                            TotalPlayTime = TimeSpan.Zero,
                            CurrentStreak = 0,
                            LongestStreak = 0,
                            LastPlayedAt = DateTime.UtcNow
                        };
                        dbContext.UserStatistics.Add(adminStats);
                    }
                    else
                    {
                        adminStats.TotalScore = 5500;
                        dbContext.UserStatistics.Update(adminStats);
                    }

                    // Ensure admin Level
                    var adminLevel = dbContext.Levels.FirstOrDefault(l => l.UserId == adminUser.Id);

                    if (adminLevel == null)
                    {
                        adminLevel = new Level
                        {
                            UserId = adminUser.Id,
                        };
                        levelService.SyncLevel(adminLevel, adminStats.TotalScore);
                        dbContext.Levels.Add(adminLevel);
                    }
                    else
                    {
                        levelService.SyncLevel(adminLevel, adminStats.TotalScore);
                        dbContext.Levels.Update(adminLevel);
                    }

                    // Ensure debug2 account exists (create or update)
                    var debug2User = dbContext.UserAccounts
                        .Include(u => u.CoinWallet)
                        .Include(u => u.Level)
                        .Include(u => u.Statistics)
                        .FirstOrDefault(u => u.UserName == "debug2");

                    if (debug2User == null)
                    {
                        debug2User = new UserAccount
                        {
                            FirstName = "Debug2",
                            LastName = "Account",
                            Email = "debug2@example.local",
                            Country = "Poland",
                            UserName = "debug2",
                            Password = "debugdebug2",
                            CreatedAt = DateTime.UtcNow
                        };

                        dbContext.UserAccounts.Add(debug2User);
                        dbContext.SaveChanges();
                    }
                    else
                    {
                        // Ensure password is known value for debugging
                        debug2User.Password = "debugdebug2";
                        debug2User.Email = debug2User.Email ?? "debug2@example.local";
                        dbContext.UserAccounts.Update(debug2User);
                        dbContext.SaveChanges();
                    }

                    // Ensure debug2 CoinWallet (2000 coins)
                    var debug2Wallet = dbContext.CoinWallets.FirstOrDefault(c => c.UserId == debug2User.Id);
                    if (debug2Wallet == null)
                    {
                        debug2Wallet = new CoinWallet
                        {
                            UserId = debug2User.Id,
                            Balance = 2000,
                            UpdatedAt = DateTime.UtcNow
                        };
                        dbContext.CoinWallets.Add(debug2Wallet);
                    }
                    else
                    {
                        debug2Wallet.Balance = 2000;
                        debug2Wallet.UpdatedAt = DateTime.UtcNow;
                        dbContext.CoinWallets.Update(debug2Wallet);
                    }

                    // Ensure debug2 Statistics (level 11, score for level 11)
                    var debug2Stats = dbContext.UserStatistics.FirstOrDefault(s => s.UserId == debug2User.Id);
                    // Calculate score needed for level 11
                    int scoreForLevel11 = levelService.GetTotalScoreThresholdForLevel(11);
                    if (debug2Stats == null)
                    {
                        debug2Stats = new UserStatistics
                        {
                            UserId = debug2User.Id,
                            TotalGamesPlayed = 0,
                            TotalScore = scoreForLevel11,
                            TotalClicks = 0,
                            HighestScore = 0,
                            TotalPlayTime = TimeSpan.Zero,
                            CurrentStreak = 0,
                            LongestStreak = 0,
                            LastPlayedAt = DateTime.UtcNow
                        };
                        dbContext.UserStatistics.Add(debug2Stats);
                    }
                    else
                    {
                        debug2Stats.TotalScore = scoreForLevel11;
                        dbContext.UserStatistics.Update(debug2Stats);
                    }

                    // Ensure debug2 Level (level 11)
                    var debug2Level = dbContext.Levels.FirstOrDefault(l => l.UserId == debug2User.Id);

                    if (debug2Level == null)
                    {
                        debug2Level = new Level
                        {
                            UserId = debug2User.Id,
                        };
                        levelService.SyncLevel(debug2Level, scoreForLevel11);
                        dbContext.Levels.Add(debug2Level);
                    }
                    else
                    {
                        levelService.SyncLevel(debug2Level, scoreForLevel11);
                        dbContext.Levels.Update(debug2Level);
                    }

                    dbContext.SaveChanges();
                    tx.Commit();
                }
                catch
                {
                    // ignore seed errors
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


            app.UseHttpsRedirection();
            app.UseRouting();
            // Enable authentication and authorization middleware
            app.UseAuthentication();

            app.UseAuthorization();

            app.MapStaticAssets();
            // Map Razor Pages endpoints so links using asp-page work (e.g. Admin pages)
            app.MapRazorPages();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=StartView}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}

namespace Symulator_Nozownika.Services
{
    public interface IClubService
    {
        // Dodaj tutaj metody, które powinny być zaimplementowane przez ClubService
    }

    public class ClubService : IClubService
    {
        // Implementacja metod z interfejsu IClubService
    }
}
