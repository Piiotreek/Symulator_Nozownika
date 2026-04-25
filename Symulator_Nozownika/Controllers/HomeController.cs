using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Symulator_Nozownika.Data;
using Symulator_Nozownika.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace Symulator_Nozownika.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await _context.UserAccounts
                .Include(u => u.Statistics)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            //achievements logic for homepage display
            var allAchievements = await _context.Achievements.ToListAsync();

            var unlockedAchievementIds = await _context.UserAchievements
                .Where(ua => ua.UserAccountId == userId)
                .Select(ua => ua.AchievementId)
                .ToListAsync();

            ViewBag.AllAchievements = allAchievements;
            ViewBag.UnlockedIds = unlockedAchievementIds;
           

            return View(user);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
