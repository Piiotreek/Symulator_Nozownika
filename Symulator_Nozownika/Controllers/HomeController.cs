using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult HighScores()
        {
            var topScores = _context.UserAccounts
                .OrderByDescending(u => u.HighScore)
                .Take(10)
                .Select(u => new { u.UserName, u.HighScore })
                .ToList();

            return View(topScores);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> SaveScore([FromBody] int currentScore)
        {
            // Sprawdzamy czy użytkownik jest zalogowany
            if (!User.Identity.IsAuthenticated)
            {
                // Użytkownik nie jest zalogowany. 
                // Zwracamy informację dla front-endu, żeby wyświetlił pytanie o logowanie.
                return Json(new { success = false, message = "not_logged_in" });
            }

            // Pobranie zalogowanego użytkownika z bazy
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var user = _context.UserAccounts.FirstOrDefault(u => u.Email == userEmail);

            if (user != null)
            {
                // Sprawdzamy czy nowy wynik jest lepszy od zapisanego "Personal Best"
                if (currentScore > user.HighScore)
                {
                    user.HighScore = currentScore;
                    _context.UserAccounts.Update(user);
                    await _context.SaveChangesAsync();
                }
                
                return Json(new { success = true, newHighScore = user.HighScore });
            }

            return BadRequest();
        }
    }
}
