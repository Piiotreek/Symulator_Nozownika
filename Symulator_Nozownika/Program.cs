using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.Cookies;
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
            // Register report service
            builder.Services.AddScoped<IReportService, ReportService>();
            // Register demo service
            builder.Services.AddScoped<IDemoService, DemoService>();
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
