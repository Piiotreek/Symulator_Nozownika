using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Symulator_Nozownika.Data;
using Symulator_Nozownika.Services;

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
            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                if (app.Environment.IsDevelopment())
                {
                    // W trybie dev - usuń i utwórz nową bazę (w razie zmian modelu)
                    dbContext.Database.EnsureDeleted();
                    dbContext.Database.EnsureCreated();
                }
                else
                {
                    // W production - zastosuj migracje
                    dbContext.Database.Migrate();
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
                pattern: "{controller=Account}/{action=Login}/{id?}")
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
